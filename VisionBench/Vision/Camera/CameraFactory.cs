using Commons.Base;
using Vision.Base;
using Vision.Enums;
using Vision.Models;

namespace Vision.Camera;

public class CameraFactory : ICameraFactory
{
    public static readonly  CameraFactory Instance;

    static CameraFactory()
    {
        Instance = new CameraFactory();
    }
    private CameraFactory()
    {
        
    }
    public ICameraDevice Create(CameraProfile cameraProfile)
    {
        var cameraInfo = cameraProfile.Info;
        var cameraParam = cameraProfile.Param;
        if(cameraInfo == null || cameraParam == null)
            throw new BusinessException("No camera profile found");
        switch (cameraInfo.CameraType)
        {
            case CameraEnum.HikVision:
                return new HikVisionCamera(cameraInfo, cameraParam);
            default:
                throw new BusinessException("Unknown camera type");
        }
    }
}