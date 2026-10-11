using System.Collections.ObjectModel;
using Commons.Base;
using Commons.Enums;
using Commons.Events;
using Commons.Logging;
using CommonUI.Base;
using CommonUI.Helper;
using CommonUI.Service;
using MainApp.Models;
using Vision.Base;
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
    public IReadOnlyList<ICameraProvider> CameraProviders { get; set; }
    private ICameraProvider? _cameraProvider;

    public ICameraProvider? CameraProvider
    {
        get => _cameraProvider;
        set
        {
            SetProperty(ref _cameraProvider, value);
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
        CameraProviders = _cameraStationService.GetAvailableBrands();
        FindCameraCommand = new DelegateCommand(DoFindCamera, () => CameraProvider != null);
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
            
            GrowlHelper.Success("保存成功");
        }
        catch (Exception e)
        {
            _logger.Error(e,"保存失败");
            GrowlHelper.Error(e.Message);
        }
        if (SelectedStation is null)
            return;
        if (_cameraStationService.GetStationState(SelectedStation.StationName)
            is not (StationStateEnum.Connected or StationStateEnum.Grabbing))
            return;

        try
        {
            _cameraStationService.ApplyCameraParams(SelectedStation.StationName,
                SelectedStation.CameraParam);
        }
        catch (Exception e)
        {
            _logger.Error(e, $"下发工位[{SelectedStation.StationName}]参数失败");
            GrowlHelper.Warning($"配置已保存，但参数下发失败：{e.Message}");
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
            if (_cameraStationService.GetStationState(SelectedStation.StationName) is StationStateEnum.Connected
                or StationStateEnum.Grabbing)
            {
                _cameraStationService.ReadCameraParams(SelectedStation.StationName,out var cameraParam);
                SelectedStation.CameraParam = cameraParam;
            }
            GrowlHelper.Success("绑定成功");
        }
        catch (Exception e)
        {
            _logger.Error(e,"绑定相机失败");
            await _messageDialogService.ErrorAsync(e.Message);
        }

        RefreshRowStates();
    }

    private async void DoFindCamera()
    {
        try
        {
            if (CameraProvider is null)
                return;
            var cameraInfos = _cameraStationService.ListAvailable(CameraProvider.Brand);
            foreach (var cameraInfo in cameraInfos)
            {
                CameraItems.Add(new CameraItem(cameraInfo));
            }
            RefreshRowStates();
            if(CameraItems.Count == 0)
                GrowlHelper.Info("未找到相机设备");
            else
                GrowlHelper.Success($"找到{CameraItems.Count}台相机");
        }
        catch (Exception e)
        {
            _logger.Error(e,"枚举相机失败");
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
        }
        catch (Exception e)
        {
            _logger.Error(e,"解除绑定失败！");
            await _messageDialogService.ErrorAsync(e.Message);
        }
    }

    
}