using Commons.Base;
using Commons.Enums;
using Commons.Logging;
using Vision.Base;
using Vision.Camera;
using Vision.Enums;
using Vision.Events;
using Vision.Factory;
using Vision.Models;

namespace Vision.Service;

public class CameraStationService : ICameraStationService
{
    private static readonly NLog.Logger _logger = Log.For<CameraStationService>(LogModule.Camera);
    private ICameraConfigStore _cameraConfigStore;
    private List<StationProfile> _stationProfiles = new();
    private readonly Dictionary<StationEnum, ICameraDevice?> _cameraDict = new();
    private readonly Dictionary<StationEnum, StationConnectionState> _states = new();

    public CameraStationService(ICameraConfigStore cameraConfigStore)
    {
        _cameraConfigStore = cameraConfigStore;
    }

    public event EventHandler<StationStateChangedEventArgs>? StationStateChanged;

    public List<CameraInfo> ListAvailable(CameraEnum cameraType)
    {
        var cameraEnumerator = CameraEnumeratorFactory.Instance.GetCameraEnumerator(cameraType);
        return cameraEnumerator.ListAvailable();
    }

    public void LoadStations()
    {
        _stationProfiles = _cameraConfigStore.LoadStations() ?? new();
        // 同步后续代码中新增的相机工位
        foreach (StationEnum value in Enum.GetValues<StationEnum>())
        {
            if (_stationProfiles.All(x => x.StationName != value))
                _stationProfiles.Add(new StationProfile()
                {
                    StationName = value
                });
        }
    }

    public List<StationProfile> GetStations()
    {
        if (_stationProfiles is null || _stationProfiles.Count == 0)
        {
            LoadStations();
        }

        return _stationProfiles;
    }

    public void Initialize()
    {
        LoadStations();
        foreach (var profile in _stationProfiles.Where(p => p.IsBound))
        {
            try
            {
                if (profile.IsBound)
                    OpenStation(profile.StationName);
                else
                    SetState(profile.StationName, StationConnectionState.Unbound);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"初始化工位[{profile.StationName}]失败");
            }
        }
    }

    public void SaveStations()
    {
        _cameraConfigStore.SaveStations(_stationProfiles);
    }

    public StationConnectionState OpenStation(StationEnum station)
    {
        CloseStation(station);
        var profile = FindProfile(station);
        if (profile is null || !profile.IsBound)
            return StationConnectionState.Unbound;
        ICameraDevice? device = null;
        try
        {
            var cameraInfo = ResolveCameraInfo(profile);
            if (cameraInfo is null)
            {
                _logger.Error($"{station}相机离线");
                return StationConnectionState.Offline;
            }

            device = CameraFactory.Instance.Create(cameraInfo);
            device.Open();
            device.Init();
            device.ReadCameraParams(profile.CameraParam);
            device.StateChanged += OnDeviceStateChanged;
            _cameraDict[station] = device;
            return SetState(station, StationConnectionState.Connected);
        }
        catch (Exception e)
        {
            CloseQuietly(device);
            _cameraDict.Remove(station);
            _logger.Error(e, $"打开工位[{station}]失败");
            return SetState(station, StationConnectionState.Failed);
        }
    }

    public void CloseStation(StationEnum station)
    {
        if (!_cameraDict.TryGetValue(station, out var device))
            return;
        _cameraDict.Remove(station);
        CloseQuietly(device);
    }

    public StationConnectionState GetStationState(StationEnum station)
    {
        _states.TryGetValue(station, out var state);
        if(state == null)
            return StationConnectionState.Unbound;
        return state;
    }

    public ICameraDevice GetCamera(StationEnum station)
    {
        var stationState = GetStationState(station);
        if (stationState != StationConnectionState.Connected)
        {
            throw new BusinessException($"工位[{station}]未连接");
        }

        return _cameraDict[station];
    }

    public void BindStation(StationEnum station, CameraInfo cameraInfo)
    {
        var profile = FindProfile(station);
        if (profile is null)
            throw new BusinessException($"工位[{station}]不存在");
        CloseStation(station);
        profile.CameraType = cameraInfo.CameraType;
        profile.SerialNum = cameraInfo.SerialNum;
        OpenStation(station);
    }

    public void UnBindStation(StationEnum station)
    {
        var target = FindProfile(station);
        if (target is null)
            throw new BusinessException($"工位[{station}]不存在");
        CloseStation(station);
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

    private StationProfile? FindProfile(StationEnum station)
    {
        return _stationProfiles?.FirstOrDefault(x => x.StationName == station);
    }

    private CameraInfo? ResolveCameraInfo(StationProfile profile)
    {
        if (profile.CameraType is null || profile.SerialNum is null)
            return null;
        var enumerator = CameraEnumeratorFactory.Instance.GetCameraEnumerator(profile.CameraType);
        return enumerator
            .ListAvailable()
            .FirstOrDefault(c => c.SerialNum == profile.SerialNum);
    }

    private static void CloseQuietly(ICameraDevice? cameraDevice)
    {
        if (cameraDevice is null)
            return;
        try
        {
            if (cameraDevice.State != CameraStateEnum.Disconnected)
            {
                cameraDevice.Close();
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "关闭相机失败");
        }
    }

    public void ReadCameraParams(StationEnum station, out CameraParam cameraParam)
    {
        var camera = GetCamera(station);
        cameraParam = new CameraParam();
        camera.ReadCameraParams(cameraParam);
    }

    public StationProfile GetStation(StationEnum station) =>
        _stationProfiles.FirstOrDefault(x => x.StationName == station);

    private StationConnectionState SetState(StationEnum station, StationConnectionState newState)
    {
        var oldState = _states.TryGetValue(station, out var old) ? old : StationConnectionState.Unbound;
        if (oldState == newState)
            return newState;
        _states[station] = newState;
        var args = new StationStateChangedEventArgs(station, oldState, newState);
        StationStateChanged?.Invoke(this, args);
        return newState;
    }

    private void OnDeviceStateChanged(object? sender, StateChangedEventArgs args)
    {
        if (sender is not ICameraDevice device)
            return;
        // 只关心相机是否断开连接
        if (args.NewState != CameraStateEnum.Disconnected)
            return;
        var pair = _cameraDict.FirstOrDefault(kv => ReferenceEquals(kv.Value, device));
        if (pair.Value == null)
            return;
        var station = pair.Key;
        _cameraDict.Remove(station);
        device.StateChanged -= OnDeviceStateChanged;
        SetState(station, StationConnectionState.Offline);
        ThreadPool.QueueUserWorkItem(_ => CloseQuietly(device));
    }
}