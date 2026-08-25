using MainApp.Views;
using System.Configuration;
using System.Data;
using System.Windows;
using Commons;
using CommonUI.Base;
using MainApp.ViewModels;
using NLog;

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
            ConfigureNLog();

            ViewModelLocationProvider.Register<MainWindow, MainWindowViewModel>();
            ViewModelLocationProvider.Register<HwViewModel, HwViewModel>();

            containerRegistry.RegisterForNavigation<HwView, HwViewModel>();
            containerRegistry.RegisterForNavigation<MainMenuView, MainMenuViewModel>();

            containerRegistry.RegisterDialogWindow<BaseDialog>();
            containerRegistry.RegisterDialog<CameraSettingDialog, CameraSettingDialogViewModel>();
        }

        /// <summary>
        /// 按构建环境切换 NLog 调试输出级别：
        /// Debug 构建开启（Debug+ 到 VS 输出窗口与 Debug/ 目录），Release 关闭。
        /// 需在 ViewModel 使用 Log.For 之前调用（此时 NLog.config 已自动加载）。
        /// </summary>
        private static void ConfigureNLog()
        {
            var config = LogManager.Configuration;
            if (config == null)
            {
                // NLog.config 未加载（缺失或格式错误），此时 NLog 处于空配置状态。
                return;
            }
#if DEBUG
            // 开发期：配置写错立即抛异常，避免静默吞掉导致"没有日志"的假象
            LogManager.ThrowConfigExceptions = true;
            config.Variables["debugLevel"] = "Debug";
#else
            config.Variables["debugLevel"] = "Off";
#endif
            LogManager.ReconfigExistingLoggers();
        }
    }
}