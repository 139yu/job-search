using System.Collections.ObjectModel;
using Commons.Base;
using MainApp.Models;

namespace MainApp.ViewModels;

public class MainMenuViewModel: RegionBaseViewModel
{
    private IDialogService _dialogService;
    public MainMenuViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService; 
        MenuItems = new ObservableCollection<MenuItem>
        {
            new MenuItem { Title = "相机设置", NavigationPath = "CameraSetting" },
            new MenuItem { Title = "运动设置", NavigationPath = "MotionSetting" },
        };
        MenuClickCommand = new DelegateCommand<string>(DoMenuClickCommand);
    }


    private ObservableCollection<MenuItem> menuItems;

    public ObservableCollection<MenuItem> MenuItems
    {
        get { return menuItems; }
        set { menuItems = value; }
    }
    
    public DelegateCommand<string> MenuClickCommand { get; set; }
    
    
    private void DoMenuClickCommand(string navigationPath)
    {
        switch (navigationPath)
        {
            case  "CameraSetting":
                _dialogService.ShowDialog("CameraSettingDialog");
                break;
            case  "MotionSetting":
                break;
        }
    }
}