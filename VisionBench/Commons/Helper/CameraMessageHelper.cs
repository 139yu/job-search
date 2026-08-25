using Commons.Enums;

namespace Vision;

public static class CameraMessageHelper
{
    private static readonly Dictionary<VisionError,string> _messages = new()
    {
        { VisionError.OpenFailed, "相机打开失败: {0}"},
        { VisionError.DeviceNotFound, "设备不存在" },
        { VisionError.InitFailed, "初始化失败：{0}" },
        { VisionError.InvalidParams, "参数非法：{0}" },
        { VisionError.EnumDeviceFailure, "参枚举设备失败：{0}" },
        { VisionError.DeviceNotExits, "设备不存在：{0}" },
        { VisionError.GetPacketSizeFailed, "Get Packet Size failed!" },
        { VisionError.SetPacketSizeFailed, "Set Packet Size failed!" },
    };

    public static string GetMessage(this VisionError result)
    {
        var msg = _messages.TryGetValue(result, out var value) ? value : _messages[result];
        return msg;
    }
    public static string GetMessage(this VisionError result,params string[] args)
    {
        var msg = _messages.TryGetValue(result, out var value) ? value : _messages[result];
        return args.Length == 0 ? msg : string.Format(msg, args);
    }
}