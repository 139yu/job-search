using Commons.Base;

namespace MainApp.ViewModels;

public class CameraSettingDialogViewModel: IDialogAware
{
    public CameraSettingDialogViewModel()
    {
        
    }

    public string Title { get; set; } = "相机设置";

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