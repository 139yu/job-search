using Vision.Models;

namespace Vision.Base;

public interface ICameraConfigStore
{
    List<StationProfile> LoadStations();
    void SaveStations(List<StationProfile> profiles);
}
