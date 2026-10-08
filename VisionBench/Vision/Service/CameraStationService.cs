using Commons.Base;
using Commons.Enums;
using Commons.Logging;
using Vision.Base;
using Vision.Camera;
using Vision.Enums;
using Vision.Factory;
using Vision.Models;

namespace Vision.Service;

public class CameraStationService : ICameraStationService
{
    private static readonly NLog.Logger _logger = Log.For<CameraStationService>(LogModule.Camera);
    private ICameraConfigStore _cameraConfigStore;
    private List<StationProfile> _stationProfiles = new();
    private readonly Dictionary<StationEnum,ICameraDevice?> _cameraDict = new();
    public CameraStationService(ICameraConfigStore  cameraConfigStore)
    {
        _cameraConfigStore =  cameraConfigStore; 
    }
    public void LoadStations()
    {
        _stationProfiles = _cameraConfigStore.LoadStations() ?? new();
        // 同步后续代码中新增的相机工位
        foreach (StationEnum value in Enum.GetValues<StationEnum>())
        {
            if(_stationProfiles.All(x => x.StationName != value))
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
                OpenStation(profile.StationName);
            }
            catch (Exception e)
            {
                _logger.Error(e,$"初始化工位[{profile.StationName}]失败");
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
            device.ApplyParams(profile.CameraParam);
            _cameraDict[station] = device;
            return StationConnectionState.Connected;
        }
        catch (Exception e)
        {
           CloseQuietly(device);
           _cameraDict.Remove(station);
           _logger.Error(e,$"打开工位[{station}]失败");
           return StationConnectionState.Failed;
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
        var profile = FindProfile(station);
        if(profile is null || !profile.IsBound)
            return StationConnectionState.Unbound;
        if (!_cameraDict.TryGetValue(station, out var device) || device is null)
            return StationConnectionState.Offline;
        return device.State == CameraStateEnum.Disconnected ? StationConnectionState.Offline : StationConnectionState.Connected;
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
        var target  = FindProfile(station);
        if(target is null)
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
            _logger.Error(e,"关闭相机失败");
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
}