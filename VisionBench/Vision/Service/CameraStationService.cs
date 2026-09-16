using Commons.Base;
using Commons.Enums;
using Commons.Logging;
using Vision.Base;
using Vision.Camera;
using Vision.Enums;
using Vision.Models;

namespace Vision.Service;

public class CameraStationService : ICameraStationService
{
    private ICameraConfigStore _cameraConfigStore;
    private List<StationProfile> _stationProfiles;
    private readonly Dictionary<StationEnum, ICameraDevice> cameraDict = new();
    private static readonly NLog.Logger Logger = Log.For<CameraStationService>(LogModule.Camera);

    public CameraStationService(ICameraConfigStore cameraConfigStore)
    {
        _cameraConfigStore = cameraConfigStore;
    }

    public ICameraDevice GetCamera(StationEnum station)
    {
        if (cameraDict.ContainsKey(station))
        {
            var cameraDevice = cameraDict[station];
            if (cameraDevice == null || cameraDevice.State == CameraStateEnum.Disconnected)
                throw new BusinessException("Cannot get camera for station " + station);
            return cameraDevice;
        }

        throw new BusinessException("Cannot get camera for station " + station);
    }

    public IReadOnlyCollection<StationProfile> GetStations()
    {
        // 没有初始化设备功能，暂时代替
        OpenCamera();
        if (_stationProfiles != null && _stationProfiles.Any())
        {
            return _stationProfiles;
        }
        return new List<StationProfile>();
    }

    public void OpenCamera()
    {
        CloseCamera();
        _stationProfiles = _cameraConfigStore.LoadStations();
        if (_stationProfiles == null || !_stationProfiles.Any())
        {
            _stationProfiles = new List<StationProfile>();
            foreach (var value in Enum.GetValues(typeof(StationEnum)))
            {
                _stationProfiles.Add(new StationProfile()
                {
                    StationName = (StationEnum)value
                });
            }
        }
        else
        {
            foreach (var item in _stationProfiles)
            {
                if (item.Camera == null)
                    continue;
                var cameraProfile = item.Camera;

                ICameraDevice cameraDevice = null;
                try
                {
                    cameraDevice = CameraFactory.Instance.Create(cameraProfile);
                    cameraDevice.Open();
                    cameraDevice.Init();
                    cameraDict[item.StationName] = cameraDevice;
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"{item.StationName}打开失败：{e.Message}");
                    if (cameraDevice != null)
                    {
                        cameraDevice.Close();
                        cameraDevice = null;
                    }
                }
            }
        }
    }

    public void CloseCamera()
    {
        if (cameraDict.Any())
        {
            foreach (var item in cameraDict)
            {
                var cameraDevice = item.Value;
                if (cameraDevice != null && cameraDevice.State != CameraStateEnum.Disconnected)
                    cameraDevice.Close();
            }
        }

        cameraDict.Clear();
    }
}