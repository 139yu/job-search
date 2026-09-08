using Vision.Base;
using Vision.Camera;
using Vision.Models;

namespace Vision.Service;

public class CameraStationService: ICameraStationService
{
    public ICameraDevice GetCamera(StationEnum station)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<StationProfile> GetStations()
    {
        throw new NotImplementedException();
    }

    public void OpenCamera()
    {
        throw new NotImplementedException();
    }

    public void CloseCamera()
    {
        throw new NotImplementedException();
    }

    public void Reload()
    {
        throw new NotImplementedException();
    }
}