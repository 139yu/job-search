using System.Threading.Channels;
using Commons.Base;
using Commons.Enums;
using Commons.Logging;
using MvCameraControl;
using Vision.Base;
using Vision.Enums;
using Vision.Manager;
using Vision.Models;

namespace Vision.Camera;

public class HikVisionCamera : ICameraDevice
{
    private readonly object _lock = new object();

    readonly DeviceTLayerType enumTLayerType = DeviceTLayerType.MvGigEDevice
                                               | DeviceTLayerType.MvUsbDevice
                                               | DeviceTLayerType.MvGenTLGigEDevice
                                               | DeviceTLayerType.MvGenTLCXPDevice
                                               | DeviceTLayerType.MvGenTLCameraLinkDevice
                                               | DeviceTLayerType.MvGenTLXoFDevice;

    private static readonly NLog.Logger Logger = Log.For<HikVisionCamera>(LogModule.Camera);

    private readonly Channel<CameraFrame> channel = Channel.CreateBounded<CameraFrame>(new BoundedChannelOptions(10)
    {
        FullMode = BoundedChannelFullMode.DropOldest
    });

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
            throw new BusinessException<VisionError>(VisionError.InvalidParams,
                VisionError.InvalidParams.GetMessage("CameraInfo"));
        }

        if (IsConnected) return;
        var ret = 0;
        List<IDeviceInfo> deviceInfoList;
        ret = DeviceEnumerator.EnumDevices(enumTLayerType, out deviceInfoList);
        if (ret != MvError.MV_OK)
        {
            throw new BusinessException<VisionError>(VisionError.EnumDeviceFailure,
                VisionError.EnumDeviceFailure.GetMessage(ret.ToString()));
        }

        if (deviceInfoList == null || deviceInfoList.Count == 0)
        {
            throw new BusinessException<VisionError>(VisionError.DeviceNotFound,
                VisionError.DeviceNotFound.GetMessage(ret.ToString()));
        }

        var target = deviceInfoList.FirstOrDefault(d => d.SerialNumber.Equals(CameraInfo.SerialNum));
        if (target == null)
        {
            throw new BusinessException<VisionError>(VisionError.DeviceNotExits,
                VisionError.DeviceNotExits.GetMessage(CameraInfo.CameraName));
        }

        device = DeviceFactory.CreateDevice(target);
        ret = device.Open();
        if (ret != MvError.MV_OK)
        {
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
                throw new BusinessException<VisionError>(VisionError.OpenFailed,
                    VisionError.SetPacketSizeFailed.GetMessage(ret.ToString()));
            }
            else
            {
                ret = device.Parameters.SetIntValue("GevSCPSPacketSize", (long)optionPacketSize);
                if (ret != MvError.MV_OK)
                {
                    throw new BusinessException<VisionError>(VisionError.OpenFailed,
                        VisionError.GetPacketSizeFailed.GetMessage(ret.ToString()));
                }
            }
        }
        IsConnected = true;
    }

    public void Init()
    {
        if (CameraParam == null)
        {
            throw new BusinessException<VisionError>(VisionError.InvalidParams,
                VisionError.InvalidParams.GetMessage("CameraParam"));
        }

        if (!IsConnected)
        {
            throw new BusinessException<VisionError>(VisionError.InitFailed,
                VisionError.InitFailed.GetMessage("相机未连接"));
        }

        if (IsInitialized) return;
        device.Parameters.SetBoolValue("ReverseX", CameraParam.ReverseX);
        device.Parameters.SetBoolValue("ReverseY", CameraParam.ReverseY);
        // 关闭自动曝光
        device.Parameters.SetEnumValue("ExposureAuto", 0);
        // 关闭自动增益
        device.Parameters.SetEnumValue("GainAuto", 0);

        device.Parameters.SetFloatValue("Gain", CameraParam.Gain);
        device.Parameters.SetFloatValue("ExposureTime", CameraParam.ExposureTime);
        var ret = device.Parameters.SetEnumValueByString("TriggerMode", "Off");
        if (ret != MvError.MV_OK)
            throw new BusinessException(VisionError.SetCameraParamFailed,
                VisionError.SetCameraParamFailed.GetMessage("TriggerMode"));
        device.StreamGrabber.SetImageNodeNum(5);
        device.StreamGrabber.FrameGrabedEventEx += OnFrameGrabbed;
        IsInitialized = true;
    }

    private void OnFrameGrabbed(object? sender, FrameGrabbedEventArgs e)
    {
        if (!IsConnected || !IsInitialized) return;
        lock (_lock)
        {
            try
            {
                var frameOut = e.FrameOut;
                var flag = TryBuildFrame(frameOut,out CameraFrame frame);
                if(flag)
                    channel.Writer.TryWrite(frame);
                device.StreamGrabber.FreeImageBuffer(frameOut);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
            }
        }
    }

    public void Close()
    {
        try
        {
            IsGrabbing = false;
            IsConnected = false;
            device.StreamGrabber.FrameGrabedEventEx -= OnFrameGrabbed;
            device.Close();
            device.Dispose();
        }
        catch (Exception e)
        {
        }
    }

    public void ApplyExposure()
    {
        device.Parameters.SetFloatValue("ExposureTime", CameraParam.ExposureTime);
    }

    public void ApplyGain()
    {
        device.Parameters.SetFloatValue("Gain", CameraParam.Gain);
    }

    public void StartAcquisition()
    {
        try
        {
            if (IsGrabbing) return;
            if (!IsConnected || !IsInitialized)
                throw new BusinessException(VisionError.StartGarbFailed, "相机未连接或初始化");
            var ret = device.StreamGrabber.StartGrabbing();
            if (ret != MvError.MV_OK)
                throw new BusinessException(VisionError.StartGarbFailed, ret.ToString());
            IsGrabbing = true;
        }
        catch (Exception e)
        {
            IsGrabbing = false;
            throw;
        }
    }

    public void StopAcquisition()
    {
        if (IsGrabbing)
        {
            var ret = device.StreamGrabber.StopGrabbing();
            if (ret != MvError.MV_OK)
                throw new BusinessException(VisionError.StopGarbFailed, ret.ToString());
            IsGrabbing = false;
        }
    }

    public void ClearFrame()
    {
        try
        {
            while (channel.Reader.TryRead(out _))
            {
            }
        }
        catch (Exception e)
        {
        }
    }

    public void StartSingleGarb()
    {
    }

    public CameraFrame TryGetFrame()
    {
        try
        {
            channel.Reader.TryRead(out CameraFrame frame);
            return frame;
        }
        catch (Exception e)
        {
            throw;
        }
    }

    private bool TryBuildFrame(IFrameOut frameOut, out CameraFrame frame)
    {
        ImageLayoutEnum imageLayout;
        var image = frameOut.Image;
        byte[] data;
        switch (image.PixelType)
        {
            case MvGvspPixelType.PixelType_Gvsp_Mono8:
                imageLayout = ImageLayoutEnum.Gray8;
                data = image.PixelData;
                break;
            case MvGvspPixelType.PixelType_Gvsp_Mono10:
            case MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
            case MvGvspPixelType.PixelType_Gvsp_Mono12:
            case MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
            case MvGvspPixelType.PixelType_Gvsp_Mono16:
                imageLayout = ImageLayoutEnum.Gray16;
                data = ConvertPixelTo(image, MvGvspPixelType.PixelType_Gvsp_Mono16);
                break;
            case MvGvspPixelType.PixelType_Gvsp_BGR8_Packed:
                imageLayout = ImageLayoutEnum.Bgr8;
                data = image.PixelData;
                break;
            case MvGvspPixelType.PixelType_Gvsp_RGB8_Packed:
            case MvGvspPixelType.PixelType_Gvsp_BayerGR8:
            case MvGvspPixelType.PixelType_Gvsp_BayerGB8:
            case MvGvspPixelType.PixelType_Gvsp_BayerBG8:
                imageLayout = ImageLayoutEnum.Bgr8;
                data = ConvertPixelTo(image, MvGvspPixelType.PixelType_Gvsp_BGR8_Packed);
                break;
            default:
                Logger.Error($"暂不支持像素格式：{image.PixelType}");
                frame = null;
                return false;
        }

        frame = new CameraFrame()
        {
            Width = (int)image.Width,
            Height = (int)image.Height,
            ImageData = data,
            ImageLayout = imageLayout,
        };
        return true;
    }

    private byte[] ConvertPixelTo(IImage image,MvGvspPixelType destType)
    {
        var conv = device.PixelTypeConverter;
        ulong size = conv.GetBufferSizeForConvert(destType, image.Width, image.Height);
        byte[] dest = new byte[size];
        ulong actual ;
        conv.ConvertPixelType(image, dest, out actual, destType);
        return dest;
    }
}