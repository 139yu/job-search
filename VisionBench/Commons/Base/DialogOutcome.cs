namespace Commons.Base;

public sealed class DialogOutcome
{
    public ButtonResult Result { get; init; } = ButtonResult.None;
    public string ButtonKey { get; init; }
    public IDialogParameters? Parameters { get; set; }
    public bool Is(ButtonResult result) => Result == result; 
}