using Commons.Enums;

namespace Commons.Base;

public sealed class DialogRequest
{
    public string Title { get; set; } = "提示";
    public string Message { get; set; }= string.Empty;
    public IReadOnlyList<DialogButtonModel> Buttons { get; set; } = Array.Empty<DialogButtonModel>();
    /// <summary>
    /// 点击标题栏X时回传的结果
    /// </summary>
    public ButtonResult CloseRequest { get; set; } = ButtonResult.Cancel;

    public static DialogRequest Confirm(string message,string title = "提示")
    {
        return new DialogRequest()
        {
            Title = title,
            Message = message,
            Buttons = new []
            {
                new DialogButtonModel(){Content = "取消",Result = ButtonResult.Cancel,Semantic = DialogButtonSemantic.Secondary,IsCancel = true},
                new DialogButtonModel(){Content = "确认",Result = ButtonResult.OK,Semantic = DialogButtonSemantic.Primary,IsDefault = true},
            }
        };
    }
    public static DialogRequest Danger(string message,string title = "警告")
    {
        return new DialogRequest()
        {
            Title = title,
            Message = message,
            Buttons = new []
            {
                new DialogButtonModel(){Content = "取消",Result = ButtonResult.Cancel,Semantic = DialogButtonSemantic.Secondary,IsCancel = true},
                new DialogButtonModel(){Content = "删除",Result = ButtonResult.OK,Semantic = DialogButtonSemantic.Danger,IsDefault = true},
            }
        };
    }
    
}