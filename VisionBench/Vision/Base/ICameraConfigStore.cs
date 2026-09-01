using Vision.Models;

namespace Vision.Base;

public interface ICameraConfigStore
{
    List<CameraProfile> LoadProfiles();
    void SaveProfiles(List<CameraProfile> profiles);
}