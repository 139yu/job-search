using System.Threading.Channels;
using Commons.Base;
using Commons.Enums;
using Commons.Logging;
using MvCameraControl;
using Vision.Base;
using Vision.Enums;
using Vision.Events;
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

    public HikVisionCamera(CameraInfo cameraInfo)
    {
        CameraInfo = cameraInfo;
    }

    private IDevice device = null;
    private CameraStateEnum _state = CameraStateEnum.Disconnected;

    public CameraStateEnum State
    {
        get => _state;
        private set
        {
            if (_state == value)
                return;
            var oldState = _state;
            _state = value;
            var args = new StateChangedEventArgs(oldState, _state);
            this.StateChanged?.Invoke(this, args);
        }
    }

    public event EventHandler<StateChangedEventArgs>? StateChanged;

    public CameraInfo CameraInfo { get; }


    public void Open()
    {
        if (CameraInfo == null)
        {
            throw new BusinessException<VisionError>(VisionError.InvalidParams,
                VisionError.InvalidParams.GetMessage("CameraInfo"));
        }

        if (State != CameraStateEnum.Disconnected)
            return;
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

        try
        {
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

            State = CameraStateEnum.Connected;
        }
        catch (Exception e)
        {
            Close();
            throw;
        }
    }

    public void Init()
    {
        if (State == CameraStateEnum.Disconnected)
        {
            throw new BusinessException<VisionError>(VisionError.InitFailed,
                VisionError.InitFailed.GetMessage("相机未连接"));
        }

        if (State != CameraStateEnum.Connected) return;

        var ret = device.Parameters.SetEnumValueByString("TriggerMode", "Off");
        if (ret != MvError.MV_OK)
            throw new BusinessException(VisionError.SetCameraParamFailed,
                VisionError.SetCameraParamFailed.GetMessage("TriggerMode"));
        device.StreamGrabber.SetImageNodeNum(5);
        device.StreamGrabber.FrameGrabedEventEx += OnFrameGrabbed;
        State = CameraStateEnum.Ready;
    }

    private void OnFrameGrabbed(object? sender, FrameGrabbedEventArgs e)
    {
        if (State is CameraStateEnum.Grabbing)
        {
            lock (_lock)
            {
                try
                {
                    var frameOut = e.FrameOut;
                    var flag = TryBuildFrame(frameOut, out CameraFrame frame);
                    if (flag)
                        channel.Writer.TryWrite(frame);
                    device.StreamGrabber.FreeImageBuffer(frameOut);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception.Message);
                }
            }
        }
    }

    public void Close()
    {
        try
        {
            State = CameraStateEnum.Disconnected;
            if (device != null)
            {
                device.StreamGrabber.FrameGrabedEventEx -= OnFrameGrabbed;
                device.Close();
                device.Dispose();
            }
        }
        catch (Exception e)
        {
        }
    }

    public void StartAcquisition()
    {
        if (State == CameraStateEnum.Grabbing) return;
        if (State != CameraStateEnum.Ready)
            throw new BusinessException(VisionError.StartGarbFailed, "相机未连接或初始化");
        var ret = device.Parameters.SetEnumValueByString("AcquisitionMode", "Continuous");
        if (ret != MvError.MV_OK)
            throw new BusinessException<VisionError>(VisionError.StartGarbFailed, ret.ToString());
        ret = device.StreamGrabber.StartGrabbing();
        if (ret != MvError.MV_OK)
            throw new BusinessException(VisionError.StartGarbFailed, ret.ToString());
        State = CameraStateEnum.Grabbing;
    }

    public void StopAcquisition()
    {
        if (State != CameraStateEnum.Grabbing) return;
        {
            var ret = device.StreamGrabber.StopGrabbing();
            if (ret != MvError.MV_OK)
                throw new BusinessException(VisionError.StopGarbFailed, ret.ToString());
            State = CameraStateEnum.Ready;
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
        if (State != CameraStateEnum.Grabbing && State != CameraStateEnum.Ready)
        {
            throw new BusinessException(VisionError.StartGarbFailed, "相机未初始化");
        }

        var ret = device.Parameters.SetEnumValueByString("AcquisitionMode", "Software");
        if (ret != MvError.MV_OK)
            throw new BusinessException(VisionError.StartGarbFailed, ret.ToString());
        ret = device.Parameters.SetCommandValue("TriggerSoftware");
        if (ret != MvError.MV_OK)
            throw new BusinessException(VisionError.TriggerSoftwareFail, ret.ToString());
        State = CameraStateEnum.Ready;
    }

    public bool TryGetFrame(out CameraFrame frame)
    {
        return channel.Reader.TryRead(out frame);
    }

    public void ApplyParams(CameraParam? p)
    {
        if (p == null)
            return;
        try
        {
            var grabFlag = State == CameraStateEnum.Grabbing;
            if (grabFlag)
                throw new BusinessException(VisionError.InvalidState, "相机采集中，不可更改参数");
            SetIfWritable("ReverseX", p.ReverseX);
            SetIfWritable("ReverseY", p.ReverseY);
            ApplyLiveParams(p);
        }
        catch (Exception e)
        {
            Logger.Error(e,"下发相机参数失败！");
            throw;
        }
    }

    public void ApplyLiveParams(CameraParam? p)
    {
        if (p == null)
            return;
        SetIfWritable("ExposureAuto", 0);
        SetIfWritable("GainAuto", 0);
        SetIfWritable("Gain", p.Gain);
        SetIfWritable("ExposureTime", p.ExposureTime);
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

    private byte[] ConvertPixelTo(IImage image, MvGvspPixelType destType)
    {
        var conv = device.PixelTypeConverter;
        ulong size = conv.GetBufferSizeForConvert(destType, image.Width, image.Height);
        byte[] dest = new byte[size];
        ulong actual;
        conv.ConvertPixelType(image, dest, out actual, destType);
        return dest;
    }

    private bool CanWrite(string node)
    {
        if (device.Parameters.GetNodeAccessMode(node, out var mode) != MvError.MV_OK)
            return false;
        return mode is XmlAccessMode.RW or XmlAccessMode.WO;
    }
    private bool CanRead(string node)
    {
        if (device.Parameters.GetNodeAccessMode(node, out var mode) != MvError.MV_OK)
            return false;
        return mode is XmlAccessMode.RW or XmlAccessMode.RO;
    }
    private void SetIfWritable(string node, bool value)
    {
        if (!CanWrite(node))
        {
            Logger.Warn($"节点[{node}]当前不可写，已跳过");
            return;
        }

        device.Parameters.SetBoolValue(node, value);
    }
    private void SetIfWritable(string node, float value)
    {
        if (!CanWrite(node))
        {
            Logger.Warn($"节点[{node}]当前不可写，已跳过");
            return;
        }

        device.Parameters.SetFloatValue(node, value);
    }
    private void SetIfWritable(string node, int value)
    {
        if (!CanWrite(node))
        {
            Logger.Warn($"节点[{node}]当前不可写，已跳过");
            return;
        }

        device.Parameters.SetIntValue(node, value);
    }
    private void SetIfWritable(string node, string value)
    {
        if (!CanWrite(node))
        {
            Logger.Warn($"节点[{node}]当前不可写，已跳过");
            return;
        }

        device.Parameters.SetStringValue(node, value);
    }
    private bool ReadBool(string node)
    {
        try
        {
            if (!CanRead(node))
                throw new BusinessException(VisionError.InvalidParams, $"节点[{node}]当前不可读");
            device.Parameters.GetBoolValue(node, out var value);
            return value;
        }
        catch (Exception e)
        {
            Logger.Warn(e,$"读取节点[{node}]失败");
            return false;
        }
    }
    private int ReadInt(string node)
    {
        try
        {
            if (!CanRead(node))
                throw new BusinessException(VisionError.InvalidParams, $"节点[{node}]当前不可读");
            device.Parameters.GetIntValue(node, out var value);
            return (int)value.CurValue;
        }
        catch (Exception e)
        {
            Logger.Warn(e, $"读取节点[{node}]失败");
            return -1;
        }
    }
    private string ReadString(string node)
    {
        try
        {
            if (!CanRead(node))
                throw new BusinessException(VisionError.InvalidParams, $"节点[{node}]当前不可读");
            device.Parameters.GetStringValue(node, out var value);
            return value.CurValue;
        }
        catch (Exception e)
        {
            Logger.Warn(e, $"读取节点[{node}]失败");
            return string.Empty;
        }
    }
    private float ReadFloat(string node)
    {
        try
        {
            if (!CanRead(node))
                throw new BusinessException(VisionError.InvalidParams, $"节点[{node}]当前不可读");
            device.Parameters.GetFloatValue(node, out var value);
            return (float)value.CurValue;
        }
        catch (Exception e)
        {
            Logger.Warn(e, $"读取节点[{node}]失败");
            return -1;
        }
    }
    public void ReadCameraParams(CameraParam? cameraParam)
    {
        if (cameraParam == null)
            cameraParam = new CameraParam();
        cameraParam.ExposureTime = ReadInt("ExposureTime");
        cameraParam.Gain = ReadFloat("Gain");
        cameraParam.ReverseX = ReadBool("ReverseX");
        cameraParam.ReverseY = ReadBool("ReverseY");
        cameraParam.ImageWidth = ReadInt("Width");
        cameraParam.ImageHeight = ReadInt("Height");
    }
}