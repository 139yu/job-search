using Commons.Base;
using Commons.Enums;
using Commons.Logging;
using Vision.Base;

namespace Vision.Service;

public class CameraService
{
    private ICameraConfigStore _cameraConfigStore;

    public CameraService(ICameraConfigStore cameraConfigStore)
    {
        _cameraConfigStore = cameraConfigStore;
    }

    public bool OpenCamera()
    {
        try
        {
            var cameraProfiles = _cameraConfigStore.LoadProfiles();
            if (cameraProfiles == null || cameraProfiles.Count == 0)
            {
                throw new BusinessException(VisionError.DeviceNotExits, "未配置相机");
            }

            return true;
        }
        catch (BusinessException ex)
        {
            
            return false;
        }
    }
    
}