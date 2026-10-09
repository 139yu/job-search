using Commons;
using Commons.Base;
using Commons.Enums;
using Commons.Logging;
using MainApp.Models;
using MainApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Vision.Base;
using Vision.Camera;
using Vision.Enums;
using Vision.Events;
using Vision.Models;
using Vision.Service;

namespace MainApp.ViewModels
{
    public class MainWindowViewModel: BindableBase
    {
        private static readonly NLog.Logger _logger = Log.For<MainWindowViewModel>(LogModule.App);
        private IRegionManager _regionManager;
        private IEventAggregator _eventAggregator;
        private ICameraStationService _cameraStationService;
        public StationProfile MainStation { get; set; }
       
        public MainWindowViewModel(IRegionManager regionManager,
            ICameraStationService cameraStationService,
            IEventAggregator  eventAggregator)
        {
            _logger.Debug("程序启动");
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            _cameraStationService = cameraStationService;
            CloseCommand = new DelegateCommand(DoCloseCommand);
            _eventAggregator.GetEvent<AppLoadedEvent>().Subscribe(AppLoaded);
            _cameraStationService.StationStateChanged += OnCameraStateChanged;
            MainStation = _cameraStationService.GetStation(StationEnum.MainCamera);
        }

        private bool _cameraConnected = false;
        public bool CameraConnected
        {
            get => _cameraConnected;
            set
            {
                SetProperty(ref _cameraConnected, value);
                RaisePropertyChanged(nameof(ConnectionStateText));
            }
        }
        public string ConnectionStateText
        {
            get => CameraConnected ? "已连接" : "未连接";
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


        private void OnCameraStateChanged(object? sender, StationStateChangedEventArgs e)
        {
            if(e.StationName == StationEnum.MainCamera)
            {
                CameraConnected = e.NewState == StationConnectionState.Connected;
            }
        }
        private void AppLoaded()
        {
            _regionManager.RequestNavigate(RegionConstants.HalconRegion, "HwView");
            _regionManager.RequestNavigate(RegionConstants.MainMenuRegion,"MainMenuView");
        }
    }
}
