using Commons.Enums;

namespace Vision;

public static class CameraMessageHelper
{
    private static readonly Dictionary<CameraResult,string> _messages = new()
    {
        { CameraResult.OpenFailed, "相机打开失败: {0}"},
        { CameraResult.DeviceNotFound, "设备不存在" },
        { CameraResult.InitFailed, "初始化失败：{0}" },
        { CameraResult.InvalidParams, "参数非法：{0}" },
    };

    public static string GetMessage(this CameraResult result)
    {
        var msg = _messages.TryGetValue(result, out var value) ? value : _messages[result];
        return msg;
    }
    public static string GetMessage(this CameraResult result,params string[] args)
    {
        var msg = _messages.TryGetValue(result, out var value) ? value : _messages[result];
        return args.Length == 0 ? msg : string.Format(msg, args);
    }
}