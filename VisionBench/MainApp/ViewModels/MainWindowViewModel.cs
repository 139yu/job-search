using Commons;
using Commons.Enums;
using Commons.Logging;
using System.Windows;
using Commons.Events;
using CommonUI.Helper;
using CommonUI.Service;
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
        private IMessageDialogService _messageDialogService;
        private readonly IImageProvider _cameraImageProvider;

        public DelegateCommand CloseCommand { get; set; }
        public DelegateCommand ApplyParamsCommand { get; set; }
        public DelegateCommand StartGrabCommand { get; set; }
        public DelegateCommand SingleGrabCommand { get; set; }
        public DelegateCommand StopGrabCommand { get; set; }

        public MainWindowViewModel(IRegionManager regionManager,
            ICameraStationService cameraStationService,
            IMessageDialogService messageDialogService,
            IEventAggregator eventAggregator)
        {
            _logger.Debug("程序启动");
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
            _cameraStationService = cameraStationService;
            _messageDialogService = messageDialogService;
            _cameraStationService.StationStateChanged += OnStationStateChanged;

            CloseCommand = new DelegateCommand(DoCloseCommand);
            ApplyParamsCommand = new DelegateCommand(DoApplyParamsCommand, () => CanStartGrab);
            StartGrabCommand = new DelegateCommand(DoStartGrabCommand, () => CanStartGrab);
            StopGrabCommand = new DelegateCommand(DoStopGrabCommand, () => CanStopGrab);
            SingleGrabCommand = new DelegateCommand(DoSingleGrabCommand, () => CanSingleGrab);
            _cameraStationService.Initialize();
            _cameraImageProvider = _cameraStationService.GetImageProvider(StationEnum.MainCamera);
            _eventAggregator.GetEvent<AppLoadedEvent>().Subscribe(AppLoaded);

            MainStation = _cameraStationService.GetStation(StationEnum.MainCamera);
        }



        public bool CanStartGrab => StationState == StationStateEnum.Connected;
        public bool CanStopGrab => StationState == StationStateEnum.Grabbing;
        public bool CanSingleGrab => StationState == StationStateEnum.Connected;

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
                RaisePropertyChanged(nameof(CanSingleGrab));
                StartGrabCommand.RaiseCanExecuteChanged();
                StopGrabCommand.RaiseCanExecuteChanged();
                ApplyParamsCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsCameraConnected =>
            StationState == StationStateEnum.Connected || StationState == StationStateEnum.Grabbing;

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
            _cameraStationService.SaveStations();
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
            try
            {
                _cameraImageProvider.Start();
                GrowlHelper.Success("开启实时采集");
            }
            catch (Exception e)
            {
                _logger.Error(e,"开启采集失败");
                _messageDialogService.ErrorAsync("开采集失败");
            }
        }
        
        private void DoSingleGrabCommand()
        {
            try
            {
                _cameraStationService.StartSingleGrab(StationEnum.MainCamera);
                GrowlHelper.Success("单帧采集成功");
            }
            catch (Exception e)
            {
                _logger.Error(e,"单帧采集失败");
                _messageDialogService.ErrorAsync("单帧集失败");
            }
        }

        private void DoStopGrabCommand()
        {
            try
            {
                _cameraImageProvider.Stop();
                GrowlHelper.Success("停止采集成功");
            }
            catch (Exception e)
            {
                _logger.Error(e,"停止采集失败");
                _messageDialogService.ErrorAsync("停止集失败");
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