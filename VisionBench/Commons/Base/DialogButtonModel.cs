using Commons.Enums;

namespace Commons.Base;

public sealed class DialogButtonModel
{
    public string Content { get; set; } = string.Empty;
    public ButtonResult Result { get; set; } =  ButtonResult.None;
    public DialogButtonSemantic Semantic { get; set; } = DialogButtonSemantic.Secondary;
    public string? ButtonKey { get; set; }
    /// <summary>
    /// 回车触发
    /// </summary>
    public bool IsDefault { get; set; }
    /// <summary>
    /// Esc触发
    /// </summary>
    public bool IsCancel { get; set; }
}