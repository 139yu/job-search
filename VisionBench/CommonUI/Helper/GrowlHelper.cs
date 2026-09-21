using HandyControl.Controls;
using HandyControl.Data;

namespace CommonUI.Helper;

public class GrowlHelper
{
    #region Info

    public static void Info(string message, double waitTime = GrowlOptions.DefaultWaitTime,
        GrowlLocation location = GrowlLocation.MainTop) =>
        Info(GrowlOptions.Create(message, waitTime, location));

    public static void Info(GrowlOptions options) =>
        RunOnUI.Run(() => Show(GrowlKind.Info, options));

    public static void InfoGlobal(string message, double waitTime = GrowlOptions.DefaultWaitTime) =>
        InfoGlobal(GrowlOptions.Create(message, waitTime, GrowlLocation.Global));

    public static void InfoGlobal(GrowlOptions options) =>
        RunOnUI.Run(() => ShowGlobal(GrowlKind.Info, options));

    #endregion

    #region Success

    public static void Success(string message, double waitTime = GrowlOptions.DefaultWaitTime,
        GrowlLocation location = GrowlLocation.MainTop) =>
        Success(GrowlOptions.Create(message, waitTime, location));

    public static void Success(GrowlOptions options) =>
        RunOnUI.Run(() => Show(GrowlKind.Success, options));

    public static void SuccessGlobal(string message, double waitTime = GrowlOptions.DefaultWaitTime) =>
        SuccessGlobal(GrowlOptions.Create(message, waitTime, GrowlLocation.Global));

    public static void SuccessGlobal(GrowlOptions options) =>
        RunOnUI.Run(() => ShowGlobal(GrowlKind.Success, options));

    #endregion

    #region Warning

    public static void Warning(string message, double waitTime = GrowlOptions.DefaultWaitTime,
        GrowlLocation location = GrowlLocation.MainTop) =>
        Warning(GrowlOptions.Create(message, waitTime, location));

    public static void Warning(GrowlOptions options) =>
        RunOnUI.Run(() => Show(GrowlKind.Warning, options));

    public static void WarningGlobal(string message, double waitTime = GrowlOptions.DefaultWaitTime) =>
        WarningGlobal(GrowlOptions.Create(message, waitTime, GrowlLocation.Global));

    public static void WarningGlobal(GrowlOptions options) =>
        RunOnUI.Run(() => ShowGlobal(GrowlKind.Warning, options));

    #endregion

    #region Error

    public static void Error(string message, double waitTime = GrowlOptions.DefaultWaitTime,
        GrowlLocation location = GrowlLocation.MainTop) =>
        Error(GrowlOptions.Create(message, waitTime, location));

    public static void Error(GrowlOptions options) =>
        RunOnUI.Run(() => Show(GrowlKind.Error, options));

    public static void ErrorGlobal(string message, double waitTime = GrowlOptions.DefaultWaitTime) =>
        ErrorGlobal(GrowlOptions.Create(message, waitTime, GrowlLocation.Global));

    public static void ErrorGlobal(GrowlOptions options) =>
        RunOnUI.Run(() => ShowGlobal(GrowlKind.Error, options));

    #endregion

    #region Fatal

    public static void Fatal(string message, double waitTime = GrowlOptions.DefaultWaitTime,
        GrowlLocation location = GrowlLocation.MainTop) =>
        Fatal(GrowlOptions.Create(message, waitTime, location));

    public static void Fatal(GrowlOptions options) =>
        RunOnUI.Run(() => Show(GrowlKind.Fatal, options));

    public static void FatalGlobal(string message, double waitTime = GrowlOptions.DefaultWaitTime) =>
        FatalGlobal(GrowlOptions.Create(message, waitTime, GrowlLocation.Global));

    public static void FatalGlobal(GrowlOptions options) =>
        RunOnUI.Run(() => ShowGlobal(GrowlKind.Fatal, options));

    #endregion

    #region Ask

    public static void Ask(string message, Func<bool, bool> callback,
        double waitTime = GrowlOptions.DefaultWaitTime,
        GrowlLocation location = GrowlLocation.MainTop) =>
        Ask(GrowlOptions.Create(message, waitTime, location), callback);

    public static void Ask(GrowlOptions options, Func<bool, bool> callback) =>
        RunOnUI.Run(() => ShowAsk(GrowlKind.Ask, options, callback, global: false));

    public static void AskGlobal(string message, Func<bool, bool> callback,
        double waitTime = GrowlOptions.DefaultWaitTime) =>
        AskGlobal(GrowlOptions.Create(message, waitTime, GrowlLocation.Global), callback);

    public static void AskGlobal(GrowlOptions options, Func<bool, bool> callback) =>
        RunOnUI.Run(() => ShowAsk(GrowlKind.Ask, options, callback, global: true));

    #endregion

    #region Clear

    public static void Clear() => RunOnUI.Run(() => Growl.Clear());

    public static void Clear(GrowlLocation location)
    {
        if (location == GrowlLocation.Global)
        {
            ClearGlobal();
            return;
        }
        RunOnUI.Run(() => Growl.Clear(location.ToString()));
    }

    public static void ClearGlobal() => RunOnUI.Run(() => Growl.ClearGlobal());

    #endregion

    private static void Show(GrowlKind kind, GrowlOptions options)
    {
        if (options.Location == GrowlLocation.Global)
        {
            ShowGlobal(kind, options);
            return;
        }

        var info = BuildGrowlInfo(options);
        InvokeGrowl(kind, info, false);
    }

    private static void ShowGlobal(GrowlKind kind, GrowlOptions options)
    {
        var info = BuildGrowlInfo(options);
        info.Token = null;
        InvokeGrowl(kind, info, true);
    }

    private static void ShowAsk(GrowlKind kind, GrowlOptions options, Func<bool, bool> callback, bool global)
    {
        if (kind != GrowlKind.Ask)
            throw new ArgumentException("Ask handler requires GrowlKind.Ask.", nameof(kind));
        if (global)
        {
            var info = BuildGrowlInfo(options);
            info.Token = null;
            info.ActionBeforeClose = callback;
            Growl.AskGlobal(info);
            return;
        }

        if (options.Location == GrowlLocation.Global)
        {
            ShowAsk(kind, options, callback, true);
            return;
        }

        var token = options.Location.ToString();
        if (!string.IsNullOrEmpty(options.ConfirmStr) || !string.IsNullOrEmpty(options.CancelStr)
                                                      || options.StaysOpen || options.ShowDateTime
                                                      || Math.Abs(options.WaitTime - GrowlOptions.DefaultWaitTime) >
                                                      double.Epsilon)
        {
            var info  = BuildGrowlInfo(options);
            info.ActionBeforeClose = callback;
            Growl.Ask(info);
        }
        else
        {
            Growl.Ask(options.Message,callback,token);
        }
    }

    private static GrowlOptions CloneOptions(GrowlOptions source, GrowlLocation location) =>
        new()
        {
            Message = source.Message,
            WaitTime = source.WaitTime,
            Location = location,
            ShowDateTime = source.ShowDateTime,
            StaysOpen = source.StaysOpen,
            Title = source.Title,
            ConfirmStr = source.ConfirmStr,
            CancelStr = source.CancelStr,
            ActionBeforeClose = source.ActionBeforeClose
        };

    private static GrowlInfo BuildGrowlInfo(GrowlOptions options)
    {
        var message = options.Message;
        var title = options.Title;
        if (!string.IsNullOrEmpty(title))
            message = $"{title}\n{message}";
        var info = new GrowlInfo()
        {
            Message = message,
            WaitTime = (int)options.WaitTime,
            ShowDateTime = options.ShowDateTime,
            StaysOpen = options.StaysOpen,
        };
        if (options.Location != GrowlLocation.Global)
            info.Token = options.Location.ToString();
        if (!string.IsNullOrEmpty(options.ConfirmStr))
            info.ConfirmStr = options.ConfirmStr;
        if (!string.IsNullOrEmpty(options.CancelStr))
            info.CancelStr = options.CancelStr;
        if (options.ActionBeforeClose != null)
            info.ActionBeforeClose = options.ActionBeforeClose;
        return info;
    }


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
    MainTop,

    /// <summary>
    /// 桌面级
    /// </summary>
    Global,
    DialogTop
}

public class GrowlOptions
{
    public const double DefaultWaitTime = 3;
    public string Message { get; set; }
    public double WaitTime { get; set; } = DefaultWaitTime;
    public GrowlLocation Location { get; set; } = GrowlLocation.MainTop;

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
        GrowlLocation location = GrowlLocation.MainTop) =>
        new()
        {
            Message = message,
            WaitTime = waitTime,
            Location = location
        };
}