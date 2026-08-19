using Commons.Base;

namespace MainApp.ViewModels;

public class CameraSettingDialogViewModel: IBaseDialogAware
{
    public CameraSettingDialogViewModel()
    {
    }

    public DelegateCommand DisposeDialogCommand { get; set; }
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