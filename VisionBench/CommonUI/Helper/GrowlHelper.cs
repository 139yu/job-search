using HandyControl.Controls;
using HandyControl.Data;

namespace CommonUI.Helper;

public class GrowlHelper
{
    #region Info

    public static void Info(string message, double waitTime = GrowlOptions.DefaultWaitTime,
        GrowlLocation location = GrowlLocation.MainCenter)
    {
    }

    public static void Info(GrowlOptions growlOptions)
    {
    }

    #endregion

    private static void InvokeGrowl(GrowlKind kind, GrowlInfo info, bool global)
    {
        if (global)
        {
            switch (kind)
            {
                case GrowlKind.Success: Growl.SuccessGlobal(info); break;
                case GrowlKind.Info: Growl.InfoGlobal(info); break;
                case GrowlKind.Warning: Growl.WarningGlobal(info); break;
                case GrowlKind.Error: Growl.ErrorGlobal(info); break;
                case GrowlKind.Fatal: Growl.FatalGlobal(info); break;
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }

            return;
        }

        switch (kind)
        {
            case GrowlKind.Success: Growl.Success(info); break;
            case GrowlKind.Info: Growl.Info(info); break;
            case GrowlKind.Warning: Growl.Warning(info); break;
            case GrowlKind.Error: Growl.Error(info); break;
            case GrowlKind.Fatal: Growl.Fatal(info); break;
            default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }
    }
}

public enum GrowlKind
{
    Success,
    Info,
    Warning,
    Error,
    Fatal,
    Ask
}

public enum GrowlLocation
{
    MainCenter,
    MainTopCenter,

    /// <summary>
    /// 桌面级
    /// </summary>
    Global,
    DialogTopCenter
}

public class GrowlOptions
{
    public const double DefaultWaitTime = 3;
    public string Message { get; set; }
    public double WaitTime { get; set; } = DefaultWaitTime;
    public GrowlLocation Location { get; set; } = GrowlLocation.MainCenter;

    public bool ShowDateTime { get; set; }

    public bool StaysOpen { get; set; }

    public string? ConfirmStr { get; set; }

    public string? CancelStr { get; set; }

    /// <summary>关闭前回调；返回 true 则关闭通知。</summary>
    public Func<bool, bool>? ActionBeforeClose { get; set; }

    /// <summary>可选标题（若 Growl 模板支持，可通过 Message 前缀等方式自行组合）。</summary>
    public string? Title { get; set; }

    public static GrowlOptions Create(
        string message,
        double waitTime = DefaultWaitTime,
        GrowlLocation location = GrowlLocation.MainCenter) =>
        new()
        {
            Message = message,
            WaitTime = waitTime,
            Location = location
        };
}