using Commons.Base;
using Vision.Base;
using Vision.Camera;
using Vision.Enums;
using Vision.Models;

namespace Vision.Factory;

public class CameraFactory
{
    public static readonly  CameraFactory Instance;

    static CameraFactory()
    {
        Instance = new CameraFactory();
    }
    private CameraFactory()
    {
        
    }
    public ICameraDevice Create(CameraInfo cameraInfo,CameraParam cameraParam)
    {
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