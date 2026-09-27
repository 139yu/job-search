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
    private List<StationProfile> _stationProfiles = new List<StationProfile>();
    private Dictionary<CameraStateEnum,ICameraDevice> _cameraDevices = new Dictionary<CameraStateEnum,ICameraDevice>();
    public CameraStationService(ICameraConfigStore  cameraConfigStore)
    {
        _cameraConfigStore =  cameraConfigStore; 
    }
    public IReadOnlyCollection<StationProfile> LoadStations()
    {
        _stationProfiles = _cameraConfigStore.LoadStations() ?? new  List<StationProfile>();
        foreach (StationEnum value in Enum.GetValues<StationEnum>())
        {
            if(_stationProfiles.All(x => x.StationName != value))
                _stationProfiles.Add(new StationProfile()
                {
                    StationName = value
                });
        }
        return _stationProfiles;
    }

    public void SaveStations()
    {
        throw new NotImplementedException();
    }

    public bool OpenStation(StationEnum station)
    {
        throw new NotImplementedException();
    }

    public void CloseStation(StationEnum station)
    {
        throw new NotImplementedException();
    }

    public bool IsStationOnline(StationEnum station)
    {
        throw new NotImplementedException();
    }

    public ICameraDevice GetCamera(StationEnum station)
    {
        throw new NotImplementedException();
    }

    public void BindStation(StationEnum station, CameraInfo cameraInfo)
    {
        throw new NotImplementedException();
    }

    public void UnBindStation(StationEnum station)
    {
        throw new NotImplementedException();
    }
}