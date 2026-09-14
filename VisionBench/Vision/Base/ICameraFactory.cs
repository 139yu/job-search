using Vision.Models;

namespace Vision.Base;

public interface ICameraFactory
{
    public ICameraDevice Create(CameraProfile profile);
}