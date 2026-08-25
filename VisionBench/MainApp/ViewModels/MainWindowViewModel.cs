using MainApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Commons;
using Commons.Base;
using Commons.Enums;
using Commons.Logging;
using MainApp.Views;

namespace MainApp.ViewModels
{
    public class MainWindowViewModel: BindableBase
    {
        private static readonly NLog.Logger _logger = Log.For<MainWindowViewModel>(LogModule.App);
        private IRegionManager _regionManager;
        private IEventAggregator _eventAggregator;
        public MainWindowViewModel(IRegionManager regionManager,IEventAggregator  eventAggregator)
        {
            _logger.Debug("程序启动");
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            CloseCommand = new DelegateCommand(DoCloseCommand);
            _eventAggregator.GetEvent<AppLoadedEvent>().Subscribe(AppLoaded);
        }


   
        private string title = "Nobody";

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public DelegateCommand CloseCommand { get; set; }
        public DelegateCommand LoadedCommand { get; set; }
   
        private void DoCloseCommand()
        {
            Application.Current.MainWindow.Close();
        }

        
        
        private void AppLoaded()
        {
            _regionManager.RequestNavigate(RegionConstants.HalconRegion, "HwView");
            _regionManager.RequestNavigate(RegionConstants.MainMenuRegion,"MainMenuView");
        }
    }
}
