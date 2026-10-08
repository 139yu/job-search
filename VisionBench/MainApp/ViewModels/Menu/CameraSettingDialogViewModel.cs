using System.Collections.ObjectModel;
using Commons.Base;
using Commons.Enums;
using Commons.Logging;
using CommonUI.Base;
using CommonUI.Helper;
using CommonUI.Service;
using MainApp.Models;
using Vision.Base;
using Vision.Camera;
using Vision.Enums;
using Vision.Factory;
using Vision.Models;
using Vision.Service;

namespace MainApp.ViewModels.Menu;

public class CameraSettingDialogViewModel : BaseDialogAware
{
    private static NLog.Logger _logger = Log.For<CameraSettingDialogViewModel>(LogModule.App);
    private IMessageDialogService _messageDialogService;
    private ICameraStationService _cameraStationService;
    private IBusyService _busyService;
    
    
    public DelegateCommand FindCameraCommand { get; set; }
    public DelegateCommand<CameraItem> BindCameraCommand { get; set; }
    public DelegateCommand<CameraItem> UnBindCameraCommand { get; set; }
    public DelegateCommand SaveCommand { get; set; }

    public CameraSettingDialogViewModel(ICameraStationService cameraStationService,
        IMessageDialogService messageDialogService,
        IBusyService busyService)
    {
        _cameraStationService = cameraStationService;
        _messageDialogService = messageDialogService;
        _busyService = busyService;
        Init();
    }

    public string Title { get; set; } = "相机设置";
    public List<CameraBrand> CameraBrands { get; set; } = new List<CameraBrand>();

    private CameraBrand? _selectedCameraBrand;

    public CameraBrand? SelectedCameraBrand
    {
        get => _selectedCameraBrand;
        set
        {
            SetProperty(ref _selectedCameraBrand, value);
            FindCameraCommand.RaiseCanExecuteChanged();
        }
    }

    private StationProfile? _selectedStation;

    public StationProfile? SelectedStation
    {
        get => _selectedStation;
        set
        {
            SetProperty(ref _selectedStation, value);
            RefreshRowStates();
            BindCameraCommand.RaiseCanExecuteChanged();
        }
    }
    
    private ObservableCollection<StationProfile> _stationList;

    public ObservableCollection<StationProfile> StationList
    {
        get
        {
            if (_stationList is null)
                _stationList = new ObservableCollection<StationProfile>();
            return _stationList;
        }
        set => SetProperty(ref _stationList, value);
        
    }
    private ObservableCollection<CameraItem> _cameraItems;

    public ObservableCollection<CameraItem> CameraItems
    {
        get
        { 
            if(_cameraItems is null)
                _cameraItems = new ObservableCollection<CameraItem>();
            return _cameraItems;
        }
        set => SetProperty(ref _cameraItems, value);
    }

    private void Init()
    {
        StationList = new ObservableCollection<StationProfile>(_cameraStationService.GetStations());
        CameraBrands.Add(new CameraBrand()
        {
            CameraName = "海康相机",
            CameraType = CameraEnum.HikVision
        });
        FindCameraCommand = new DelegateCommand(DoFindCamera, () => SelectedCameraBrand != null);
        BindCameraCommand = new DelegateCommand<CameraItem>((arg) => _ = DoBindCameraCommand(arg),
            (arg) => SelectedStation != null);
        UnBindCameraCommand = new DelegateCommand<CameraItem>( (obj) =>  _ = DoUnBindCameraCommand(obj));
        SaveCommand = new DelegateCommand(DoSaveCommand);
    }

    private void DoSaveCommand()
    {
        try
        {
            _cameraStationService.SaveStations();
            if(SelectedStation != null)
                _cameraStationService.ApplyCameraParams(SelectedStation.StationName, SelectedStation.CameraParam);
            GrowlHelper.Success("保存成功");
        }
        catch (Exception e)
        {
            _logger.Error(e,"保存失败");
            GrowlHelper.Error(e.Message);
        }
    }

    private async Task DoBindCameraCommand(CameraItem obj)
    {
        try
        {
            if (SelectedStation is null)
                return;
            //当前工位已绑定
            if (SelectedStation.IsBound)
            {
                var res = await _messageDialogService.ConfirmAsync("当前工位已绑定相机，是否覆盖？");
                if (!res)
                    return;
                _cameraStationService.UnBindStation(SelectedStation.StationName);
            }

            //当前相机已被绑定
            var bindTarget = StationList.FirstOrDefault(s => s.IsBound && s.SerialNum == obj.Camera.SerialNum);
            if (bindTarget is not null)
            {
                var res = await _messageDialogService.ConfirmAsync($"当前相机已绑工位:{bindTarget.StationName}，是否替换？");
                if (!res)
                    return;
                _cameraStationService.UnBindStation(bindTarget.StationName);
            }

            _cameraStationService.BindStation(SelectedStation.StationName, obj.Camera);
            _cameraStationService.ReadCameraParams(SelectedStation.StationName,out var cameraParam);
            SelectedStation.CameraParam = cameraParam;
            GrowlHelper.Success("绑定成功");
        }
        catch (Exception e)
        {
            _logger.Error(e,"绑定相机失败");
            await _messageDialogService.ErrorAsync(e.Message);
        }

        ReLoadStations();
        RefreshRowStates();
    }

    private async void DoFindCamera()
    {
        try
        {

            var cameraEnumerator = CameraEnumeratorFactory.Instance.GetCameraEnumerator(SelectedCameraBrand.CameraType);
            var list = await _busyService.RunAsync(BusyRequest.CancellableRequest("正在枚举相机设备..."),
                (progress, cts) => cameraEnumerator.ListAvailable());
            foreach (var cameraInfo in list)
            {
                var cameraItem = new CameraItem(cameraInfo);
                cameraItem.IsBoundToStation = SelectedStation is not null && cameraItem.Matches(SelectedStation);
                CameraItems.Add(cameraItem);
            }

            GrowlHelper.Success("枚举相机成功！");
        }
        catch (Exception e)
        {
            GrowlHelper.Error(e.Message);
        }
    }

    private void RefreshRowStates()
    {
        foreach (var cameraItem in CameraItems)
        {
            cameraItem.IsBoundToStation = SelectedStation is not null && cameraItem.Matches(SelectedStation);
        }
    }

    private async Task DoUnBindCameraCommand(CameraItem obj)
    {
        try
        {
            if (SelectedStation is null || !SelectedStation.IsBound)
                return;
            var res = await _messageDialogService.ConfirmAsync($"是否确认解除工位[{SelectedStation.StationName}]相机绑定？");
            if (!res)
                return;
            _cameraStationService.UnBindStation(SelectedStation.StationName);
            ReLoadStations();
        }
        catch (Exception e)
        {
            _logger.Error(e,"解除绑定失败！");
            await _messageDialogService.ErrorAsync(e.Message);
        }
    }

    private void ReLoadStations()
    {
        StationList = new ObservableCollection<StationProfile>(_cameraStationService.GetStations());
        if (SelectedStation != null && StationList.Count > 0)
        {
            var target = StationList.FirstOrDefault(s => s.SerialNum != null &&
                s.SerialNum.Equals(SelectedStation.SerialNum) && s.CameraType == SelectedStation.CameraType);
            SelectedStation = target;
        }
    }
    
    
}