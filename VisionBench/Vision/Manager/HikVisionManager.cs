using System.Windows.Documents;
using Commons.Base;
using Commons.Enums;
using Commons.Logging;
using MvCameraControl;
using Vision.Base;
using Vision.Enums;
using Vision.Models;

namespace Vision.Manager;

public class HikVisionManager : ICameraManager
{
    private readonly DeviceTLayerType enumTLayerType = DeviceTLayerType.MvGigEDevice | DeviceTLayerType.MvUsbDevice
                                                                             | DeviceTLayerType.MvGenTLGigEDevice |
                                                                             DeviceTLayerType.MvGenTLCXPDevice |
                                                                             DeviceTLayerType.MvGenTLCameraLinkDevice |
                                                                             DeviceTLayerType.MvGenTLXoFDevice;

    private static NLog.Logger _logger = Log.For<HikVisionManager>(LogModule.Camera);

    public List<CameraInfo> ListAvailable()
    {
        List<IDeviceInfo> deviceInfoList = new List<IDeviceInfo>();
        int ret = DeviceEnumerator.EnumDevices(enumTLayerType, out deviceInfoList);
        if (ret != MvError.MV_OK)
        {
            _logger.Error(VisionError.EnumDeviceFailure.GetMessage(ret.ToString()));
            throw new BusinessException<VisionError>(VisionError.EnumDeviceFailure,
                VisionError.EnumDeviceFailure.GetMessage(ret.ToString()));
        }

        List<CameraInfo> cameraInfos = new List<CameraInfo>();
        foreach (var deviceInfo in deviceInfoList)
        {
            var cameraInfo = new CameraInfo();
            cameraInfo.CameraType = CameraEnum.HikVision;
            cameraInfo.TypeModel = deviceInfo.ModelName;
            cameraInfo.CameraName = deviceInfo.UserDefinedName;
            cameraInfo.InterfaceType = deviceInfo.TLayerType.ToString();
            cameraInfo.SerialNum = deviceInfo.SerialNumber;
            cameraInfos.Add(cameraInfo);
        }
        return cameraInfos;
    }
}