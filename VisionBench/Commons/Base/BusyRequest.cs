namespace Commons.Base;

public sealed class BusyRequest
{
    public string Title { get; set; } = "请稍后";
    public string Message { get; set; } = string.Empty;
    public bool Cancellable { get; set; } = true;
    /// <summary>
    /// 超过此时长仍未完成才显示窗口，避免短操作闪窗。
    /// </summary>
    public int ShowDelayMs { get; set; } = 300;

    public int CancelTimeout { get; set; } = 5000;

    public static BusyRequest CancellableRequest(string message, string title = "请稍后") => new()
    {
        Message = message, Title = title, Cancellable = true
    };
    public static BusyRequest BlockingRequest(string message, string title = "请稍后") => new()
    {
        Message = message, Title = title, Cancellable = false
    };
}