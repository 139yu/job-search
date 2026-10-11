using Commons;
using Commons.Enums;
using Commons.Logging;
using System.Windows;
using Commons.Events;
using CommonUI.Helper;
using Vision.Base;
using Vision.Enums;
using Vision.Events;
using Vision.Models;

namespace MainApp.ViewModels
{
    public class MainWindowViewModel : BindableBase, IDestructible
    {
        private static readonly NLog.Logger _logger = Log.For<MainWindowViewModel>(LogModule.App);
        private IRegionManager _regionManager;
        private IEventAggregator _eventAggregator;
        private ICameraStationService _cameraStationService;
        public DelegateCommand CloseCommand { get; set; }
        public DelegateCommand ApplyParamsCommand { get; set; }
        public DelegateCommand StartGrabCommand { get; set; }
        public DelegateCommand StopGrabCommand { get; set; }

        public MainWindowViewModel(IRegionManager regionManager,
            ICameraStationService cameraStationService,
            IEventAggregator eventAggregator)
        {
            _logger.Debug("程序启动");
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            _cameraStationService = cameraStationService;

            _cameraStationService.StationStateChanged += OnStationStateChanged;

            CloseCommand = new DelegateCommand(DoCloseCommand);
            ApplyParamsCommand = new DelegateCommand(DoApplyParamsCommand, () => CanStartGrab);
            StartGrabCommand = new DelegateCommand(DoStartGrabCommand, () => CanStartGrab);
            StopGrabCommand = new DelegateCommand(DoStopGrabCommand, () => CanStopGrab);
            _cameraStationService.Initialize();
            _eventAggregator.GetEvent<AppLoadedEvent>().Subscribe(AppLoaded);

            MainStation = _cameraStationService.GetStation(StationEnum.MainCamera);
        }


        public bool CanStartGrab => StationState == StationStateEnum.Connected;
        public bool CanStopGrab => StationState == StationStateEnum.Grabbing;

        private StationProfile _mainStation;

        public StationProfile MainStation
        {
            get => _mainStation;
            set => SetProperty(ref _mainStation, value);
        }

        private StationStateEnum _stationState = StationStateEnum.Unbound;

        public StationStateEnum StationState
        {
            get => _stationState;
            set
            {
                SetProperty(ref _stationState, value);
                RaisePropertyChanged(nameof(ConnectionStateText));
                RaisePropertyChanged(nameof(IsCameraConnected));
                RaisePropertyChanged(nameof(CanStartGrab));
                RaisePropertyChanged(nameof(CanStopGrab));
                StartGrabCommand.RaiseCanExecuteChanged();
                StopGrabCommand.RaiseCanExecuteChanged();
                ApplyParamsCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsCameraConnected => StationState == StationStateEnum.Connected || StationState == StationStateEnum.Grabbing;

        public string ConnectionStateText
        {
            get
            {
                switch (StationState)
                {
                    case StationStateEnum.Unbound:
                        return "未绑定";
                    case StationStateEnum.Connected:
                    case StationStateEnum.Grabbing:
                        return "已连接";
                    case StationStateEnum.Offline:
                        return "离线";
                    default:
                        return "连接失败";
                }
            }
        }

        public string Title { get; } = "Nobody";


        private void DoCloseCommand()
        {
            if (Application.Current.MainWindow != null) Application.Current.MainWindow.Close();
        }


        private void OnStationStateChanged(object? sender, StationStateChangedEventArgs e)
        {
            if (e.StationName != StationEnum.MainCamera)
                return;
            RunOnUI.Run(() => StationState = e.NewState);
        }

        private void DoApplyParamsCommand()
        {
            try
            {
                _cameraStationService.ApplyCameraParams(MainStation.StationName, MainStation.CameraParam);
                GrowlHelper.Success("下发参数成功");
            }
            catch (Exception e)
            {
                _logger.Error(e, "下发相机参数失败");
                GrowlHelper.Error(e.Message);
            }
        }

        private void DoStartGrabCommand()
        {
            if (StationState == StationStateEnum.Grabbing)
            {
                GrowlHelper.Success("已开启实时采集");
                return;
            }

            if (StationState == StationStateEnum.Connected)
            {
                try
                {
                    var device = _cameraStationService.GetCamera(MainStation.StationName);
                    device.StartAcquisition();
                    GrowlHelper.Success("开启实时采集");
                }
                catch (Exception e)
                {
                    GrowlHelper.Error(e.Message);
                    _logger.Error(e, "开启采集失败");
                }
            }
        }

        private void DoStopGrabCommand()
        {
            if (StationState != StationStateEnum.Grabbing)
            {
                GrowlHelper.Success("相机未在采集中");
                return;
            }

            try
            {
                var device = _cameraStationService.GetCamera(MainStation.StationName);
                device.StopAcquisition();
                GrowlHelper.Success("实时采集已关闭");
            }
            catch (Exception e)
            {
                GrowlHelper.Error(e.Message);
                _logger.Error(e, "关闭采集失败");
            }
        }

        private void AppLoaded()
        {
            _regionManager.RequestNavigate(RegionConstants.HalconRegion, "HwView");
            _regionManager.RequestNavigate(RegionConstants.MainMenuRegion, "MainMenuView");
        }

        public void ApplyLiveParams()
        {
            try
            {
                _cameraStationService.ApplyCameraLiveParams(MainStation.StationName, MainStation.CameraParam);
            }
            catch (Exception e)
            {
                _logger.Error(e, "下发实时参数失败");
                GrowlHelper.Error(e.Message);
            }
        }

        public void Destroy()
        {
            _cameraStationService.StationStateChanged -= OnStationStateChanged;
            _eventAggregator.GetEvent<AppLoadedEvent>().Unsubscribe(AppLoaded);
        }
    }
}