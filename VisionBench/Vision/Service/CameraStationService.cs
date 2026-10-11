using Commons.Base;
using Commons.Enums;
using Commons.Logging;
using Vision.Base;
using Vision.Enums;
using Vision.Events;
using Vision.Factory;
using Vision.Models;

namespace Vision.Service;

public class CameraStationService : ICameraStationService
{
    private static readonly NLog.Logger _logger = Log.For<CameraStationService>(LogModule.Camera);
    private ICameraConfigStore _cameraConfigStore;

    /// <summary>
    /// 工位运行时记录。
    /// 设备引用与状态必须同步变化，因此刻意放在同一个对象里——
    /// 分成两个字典正是此前"状态说已连接、设备其实已摘掉"的根源。
    /// </summary>
    private sealed class StationRuntime
    {
        public ICameraDevice? Device { get; set; }
        public StationStateEnum State { get; set; } = StationStateEnum.Unbound;
    }

    private List<StationProfile> _stationProfiles = new();
    private readonly Dictionary<StationEnum, StationRuntime> _runtime = new();
    public event EventHandler<StationStateChangedEventArgs>? StationStateChanged;

    public CameraStationService(ICameraConfigStore cameraConfigStore)
    {
        _cameraConfigStore = cameraConfigStore;
    }


    public void LoadStations()
    {
        _stationProfiles = _cameraConfigStore.LoadStations() ?? new();
        // 同步后续代码中新增的相机工位
        foreach (StationEnum value in Enum.GetValues<StationEnum>())
        {
            if (_stationProfiles.All(x => x.StationName != value))
                _stationProfiles.Add(new StationProfile() { StationName = value });
        }
    }

    public List<StationProfile> GetStations()
    {
        if (_stationProfiles.Count == 0)
        {
            LoadStations();
        }

        return _stationProfiles;
    }

    public StationProfile GetStation(StationEnum station)
    {
        return FindProfile(station) ?? throw new BusinessException($"工位[{station}]不存在");
    }

    public IReadOnlyList<ICameraProvider> GetAvailableBrands()
    {
        return CameraRegistry.Instance.Providers;
    }

    public List<CameraInfo> ListAvailable(CameraEnum cameraType)
    {
        return CameraRegistry.Instance.GetEnumerator(cameraType).ListAvailable();
    }


    public void Initialize()
    {
        LoadStations();
        foreach (var profile in _stationProfiles)
        {
            if (!profile.IsBound)
            {
                SetState(profile.StationName, StationStateEnum.Unbound);
                continue;
            }

            try
            {
                OpenStation(profile.StationName);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"初始化工位[{profile.StationName}]失败");
            }
        }
    }

    public StationStateEnum OpenStation(StationEnum station)
    {
        DetachDevice(station, StationStateEnum.Unbound);
        var profile = FindProfile(station);
        if (profile is null || !profile.IsBound)
            return StationStateEnum.Unbound;
        ICameraDevice? device = null;
        try
        {
            var cameraInfo = ResolveCameraInfo(profile);
            if (cameraInfo is null)
            {
                _logger.Warn($"工位{station}相机离线");
                return SetState(station, StationStateEnum.Offline);
            }

            device = CameraRegistry.Instance.Create(cameraInfo);
            device.StateChanged += OnDeviceStateChanged;
            device.Open();
            device.Init();
            device.ReadCameraParams(profile.CameraParam);
            return AttachDevice(station, device);
        }
        catch (Exception e)
        {
            _logger.Error(e, $"打开工位{station}相机失败");
            DetachDevice(station, StationStateEnum.Failed);
            CloseQuietly(device);
            return StationStateEnum.Failed;
        }
    }

    public void CloseStation(StationEnum station)
    {
        DetachDevice(station, StationStateEnum.Unbound);
    }

    public StationStateEnum GetStationState(StationEnum station)
    {
        return _runtime.TryGetValue(station, out var runtime) ? runtime.State : StationStateEnum.Unbound;
    }

    public ICameraDevice GetCamera(StationEnum station)
    {
        return Runtime(station).Device ?? throw new BusinessException($"工位{station}未连接");
    }

    public void BindStation(StationEnum station, CameraInfo cameraInfo)
    {
        var profile = FindProfile(station);
        if (profile is null)
            throw new BusinessException($"工位[{station}]不存在");
        DetachDevice(station, StationStateEnum.Unbound);
        profile.CameraType = cameraInfo.CameraType;
        profile.SerialNum = cameraInfo.SerialNum;
        OpenStation(station);
    }

    public void UnBindStation(StationEnum station)
    {
        var target = FindProfile(station);
        if (target is null)
            throw new BusinessException($"工位[{station}]不存在");
        DetachDevice(station, StationStateEnum.Unbound);
        target.SerialNum = null;
        target.CameraType = null;
    }

    public void ApplyCameraParams(StationEnum station, CameraParam cameraParam)
    {
        var cameraDevice = GetCamera(station);
        cameraDevice.ApplyParams(cameraParam);
    }

    public void ApplyCameraLiveParams(StationEnum station, CameraParam cameraParam)
    {
        var cameraDevice = GetCamera(station);
        cameraDevice.ApplyLiveParams(cameraParam);
    }

    public void ReadCameraParams(StationEnum station, out CameraParam cameraParam)
    {
        var camera = GetCamera(station);
        cameraParam = new CameraParam();
        camera.ReadCameraParams(cameraParam);
    }

    private StationRuntime Runtime(StationEnum station)
    {
        if (!_runtime.TryGetValue(station, out var runtime))
        {
            runtime = new StationRuntime();
            _runtime[station] = runtime;
        }

        return runtime;
    }

    private StationStateEnum SetState(StationEnum station, StationStateEnum newState)
    {
        var runtime = Runtime(station);
        if (runtime.State == newState)
            return newState;
        var oldState = runtime.State;
        runtime.State = newState;
        StationStateChanged?.Invoke(this, new StationStateChangedEventArgs(station, oldState, newState));
        return newState;
    }

    private StationStateEnum AttachDevice(StationEnum station, ICameraDevice device)
    {
        Runtime(station).Device = device;
        return SetState(station, StationStateEnum.Connected);
    }

    /// <summary>
    /// 摘除设备并把状态落到 <paramref name="nextState"/>。
    /// <para>
    /// <b>退订必须先于 Close：</b>相机设备在 Close 时会同步抛出 Disconnected 状态变化，
    /// 不退订就会重入回 <see cref="OnDeviceStateChanged"/>。
    /// </para>
    /// </summary>
    /// <param name="deferClose">
    /// true 时把关闭动作丢到线程池。用于设备自身的断开回调——
    /// 回调跑在厂商 SDK 的线程上，在它内部直接 Close 同一台设备有死锁风险。
    /// </param>
    private void DetachDevice(StationEnum station, StationStateEnum nextState, bool deferClose = false)
    {
        var runtime = Runtime(station);
        var device = runtime.Device;
        runtime.Device = null;
        if (device is not null)
        {
            device.StateChanged -= OnDeviceStateChanged;
            if (deferClose)
                ThreadPool.QueueUserWorkItem(_ => CloseQuietly(device));
            else
                CloseQuietly(device);
        }

        SetState(station, nextState);
    }

    private void OnDeviceStateChanged(object? sender, StateChangedEventArgs args)
    {
        if (sender is not ICameraDevice device)
            return;
        if (!TryFindStation(device, out var station))
            return;
        switch (args.NewState)
        {
            case CameraStateEnum.Disconnected:
                _logger.Warn($"工位{station}相机断开连接");
                DetachDevice(station, StationStateEnum.Offline, true);
                break;
            case CameraStateEnum.Connected:
                SetState(station, StationStateEnum.Connected);
                break;
            case CameraStateEnum.Grabbing:
                SetState(station, StationStateEnum.Grabbing);
                break;
        }
    }

    private bool TryFindStation(ICameraDevice device, out StationEnum station)
    {
        foreach (var keyValuePair in _runtime)
        {
            if (ReferenceEquals(keyValuePair.Value.Device, device))
            {
                station = keyValuePair.Key;
                return true;
            }
        }

        station = default;
        return false;
    }

    private StationProfile? FindProfile(StationEnum station)
    {
        return _stationProfiles.FirstOrDefault(x => x.StationName == station);
    }

    private CameraInfo? ResolveCameraInfo(StationProfile profile)
    {
        if (profile.CameraType is null || profile.SerialNum is null)
            return null;
        return CameraRegistry.Instance.GetEnumerator(profile.CameraType)
            .ListAvailable()
            .FirstOrDefault(c => c.SerialNum == profile.SerialNum);
    }
    private static void CloseQuietly(ICameraDevice? device)
    {
        if (device is null)
            return;
        try
        {
            if(device.State != CameraStateEnum.Disconnected)
                device.Close();
        }
        catch (Exception e)
        {
            _logger.Error(e,"关闭相机失败");
        }
    }

    public void SaveStations()
    {
        _cameraConfigStore.SaveStations(_stationProfiles);
    }

    public void Dispose()
    {
        foreach (var station in _runtime.Keys.ToList())
        {
            DetachDevice(station, StationStateEnum.Unbound);
        }
    }
}