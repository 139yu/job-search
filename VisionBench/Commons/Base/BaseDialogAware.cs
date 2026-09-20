namespace Commons.Base;

public abstract class IBaseDialogAware :BindableBase, IDialogAware
{
    public string Title { get; set; }
    public bool CanCloseDialog()
    {
        return true;
    }

    public void OnDialogClosed()
    {
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
    }

    public DialogCloseListener RequestClose { get; }
}