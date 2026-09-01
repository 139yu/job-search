using Commons.Base;
using Vision.Base;
using Vision.Enums;
using Vision.Models;

namespace Vision.Camera;

public class CameraFactory : ICameraFactory
{
    public ICameraDevice Create(CameraInfo cameraInfo, CameraParam cameraParam)
    {
        switch (cameraInfo.CameraType)
        {
            case CameraEnum.HikVision:
                return new HikVisionCamera(cameraInfo, cameraParam);
            default:
                throw new BusinessException("Unknown camera type");
        }
    }
}