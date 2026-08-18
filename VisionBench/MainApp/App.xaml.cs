using MainApp.Views;
using System.Configuration;
using System.Data;
using System.Windows;
using Commons;
using CommonUI.Base;
using MainApp.ViewModels;

namespace MainApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            ViewModelLocationProvider.Register<MainWindow, MainWindowViewModel>();
            ViewModelLocationProvider.Register<HwViewModel, HwViewModel>();
            
            containerRegistry.RegisterForNavigation<HwView, HwViewModel>();
            containerRegistry.RegisterForNavigation<MainMenuView, MainMenuViewModel>();
            
            containerRegistry.RegisterDialogWindow<BaseDialog>();
            containerRegistry.RegisterDialog<CameraSettingDialog,CameraSettingDialogViewModel>();
        }
    }

}
