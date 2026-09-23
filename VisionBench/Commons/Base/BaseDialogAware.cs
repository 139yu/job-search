namespace Commons.Base;

public abstract class BaseDialogAware :BindableBase, IDialogAware
{
    public string Title { get; set; }
    public virtual bool CanCloseDialog()
    {
        return true;
    }

    public virtual void OnDialogClosed()
    {
    }

    public virtual void OnDialogOpened(IDialogParameters parameters)
    {
    }

    public DialogCloseListener RequestClose { get; }
}