using Vision.Camera;
using Vision.Models;

namespace Vision.Base;

public interface ICameraStationService
{
    ICameraDevice GetCamera(StationEnum station);
    IReadOnlyList<StationProfile> GetStations();
    void OpenCamera();
    void CloseCamera();
}