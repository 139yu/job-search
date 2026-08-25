using Commons.Base;
using Commons.Enums;
using Commons.Logging;
using MvCameraControl;
using Vision.Base;
using Vision.Manager;
using Vision.Models;

namespace Vision.Camera;

public class HikVisionCamera : ICameraDevice
{
    readonly DeviceTLayerType enumTLayerType = DeviceTLayerType.MvGigEDevice 
                                               | DeviceTLayerType.MvUsbDevice
                                               | DeviceTLayerType.MvGenTLGigEDevice 
                                               | DeviceTLayerType.MvGenTLCXPDevice 
                                               | DeviceTLayerType.MvGenTLCameraLinkDevice 
                                               | DeviceTLayerType.MvGenTLXoFDevice;

    private static readonly NLog.Logger Logger = Log.For<HikVisionCamera>(LogModule.Camera);
    public HikVisionCamera(CameraInfo cameraInfo, CameraParam cameraParam)
    {
        IsInitialized = false;
        IsGrabbing = false;
        IsConnected = false;
        CameraInfo = cameraInfo;
        CameraParam = cameraParam;
    }
    private IDevice device = null;
    public bool IsInitialized { get; private set; }
    public bool IsGrabbing { get; private set; }
    public bool IsConnected { get; private set; }
    public CameraInfo CameraInfo { get; }
    public CameraParam CameraParam { get; }

    public void Open()
    {
        if (CameraInfo == null)
        {
            Logger.Error(VisionError.InvalidParams.GetMessage("CameraInfo"));
            throw new BusinessException<VisionError>(VisionError.InvalidParams,
                VisionError.InvalidParams.GetMessage("CameraInfo"));
        }
        if (IsConnected) return;
        var ret = 0;
        List<IDeviceInfo> deviceInfoList;
        DeviceEnumerator.EnumDevices(enumTLayerType, out deviceInfoList);
        if (ret != MvError.MV_OK)
        {
            Logger.Error(VisionError.EnumDeviceFailure.GetMessage(ret.ToString()));
            throw new BusinessException<VisionError>(VisionError.EnumDeviceFailure,
                VisionError.EnumDeviceFailure.GetMessage(ret.ToString()));
        }

        if (deviceInfoList == null || deviceInfoList.Count == 0)
        {
            Logger.Error(VisionError.DeviceNotFound.GetMessage());
            throw new BusinessException<VisionError>(VisionError.EnumDeviceFailure,
                VisionError.EnumDeviceFailure.GetMessage(ret.ToString()));
        }

        var target = deviceInfoList.FirstOrDefault(d => d.SerialNumber.Equals(CameraInfo.CameraName));
        if (device == null)
        {
            Logger.Error(VisionError.DeviceNotExits.GetMessage());
            throw new BusinessException<VisionError>(VisionError.DeviceNotExits,
                VisionError.DeviceNotExits.GetMessage(CameraInfo.CameraName));
        }
        device = DeviceFactory.CreateDevice(target);
        ret = device.Open();
        if (ret != MvError.MV_OK)
        {
            Logger.Error(VisionError.OpenFailed.GetMessage(ret.ToString()));
            throw new BusinessException<VisionError>(VisionError.OpenFailed,
                VisionError.OpenFailed.GetMessage(ret.ToString()));
        }

        if (device is IGigEDevice)
        {
            IGigEDevice gigEDevice = device as IGigEDevice;

            // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
            int optionPacketSize;
            ret = gigEDevice.GetOptimalPacketSize(out optionPacketSize);
            if (ret != MvError.MV_OK)
            {
                Logger.Error(VisionError.SetPacketSizeFailed.GetMessage(ret.ToString()));
                throw new BusinessException<VisionError>(VisionError.OpenFailed,
                    VisionError.SetPacketSizeFailed.GetMessage(ret.ToString()));
            }
            else
            {
                ret = device.Parameters.SetIntValue("GevSCPSPacketSize", (long)optionPacketSize);
                if (ret != MvError.MV_OK)
                {
                    Logger.Error(VisionError.GetPacketSizeFailed.GetMessage(ret.ToString()));
                    throw new BusinessException<VisionError>(VisionError.OpenFailed,
                        VisionError.GetPacketSizeFailed.GetMessage(ret.ToString()));
                }
            }

            IsConnected = true;
        }
    }

    public void Init()
    {
        
    }

    public void Close()
    {
        
    }

    public void SetExposure(int exposure)
    {
        
    }

    public void SetGain(int gain)
    {
        
    }

    public void StartAcquisition()
    {
        
    }

    public void StopAcquisition()
    {
        
    }

    public void ClearFrame()
    {
        
    }

    public bool TryGrabFrame()
    {
        throw new NotImplementedException();
    }

    public bool TryGetFrame(out object frame)
    {
        throw new NotImplementedException();
    }
}