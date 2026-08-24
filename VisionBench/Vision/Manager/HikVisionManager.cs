using System.Windows.Documents;
using MvCameraControl;
using Vision.Base;
using Vision.Enums;
using Vision.Models;

namespace Vision.Manager;

public class HikVisionManager : ICameraManager
{
    readonly DeviceTLayerType enumTLayerType = DeviceTLayerType.MvGigEDevice | DeviceTLayerType.MvUsbDevice
                                                                             | DeviceTLayerType.MvGenTLGigEDevice |
                                                                             DeviceTLayerType.MvGenTLCXPDevice |
                                                                             DeviceTLayerType.MvGenTLCameraLinkDevice |
                                                                             DeviceTLayerType.MvGenTLXoFDevice;


    public List<CameraInfo> ListAvailable()
    {
        List<IDeviceInfo> deviceInfoList = new List<IDeviceInfo>();
        int ret = DeviceEnumerator.EnumDevices(enumTLayerType, out deviceInfoList);
        if (ret != MvError.MV_OK)
        {
            return null;
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