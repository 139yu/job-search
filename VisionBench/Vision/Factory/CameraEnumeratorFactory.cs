using Vision.Base;
using Vision.Enums;
using Vision.Manager;

namespace Vision.Factory;

public class CameraEnumeratorFactory
{
    public readonly static CameraEnumeratorFactory Instance;

    static CameraEnumeratorFactory()
    {
        Instance = new CameraEnumeratorFactory();
    }
    private CameraEnumeratorFactory()
    {
        
    }
    public ICameraEnumerator GetCameraEnumerator(CameraEnum cameraType)
    {
        switch (cameraType)
        {
            case CameraEnum.HikVision:
                return new HikVisionEnumerator();
            default:
                throw new NotImplementedException("Unknown camera type: " + cameraType);
        }
    }
}