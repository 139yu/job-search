using Commons.Base;
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
    private IMessageDialogService _messageDialogService;
    private ICameraStationService _cameraStationService;
    private IBusyService _busyService;

    public CameraSettingDialogViewModel(ICameraStationService cameraStationService,
        IMessageDialogService messageDialogService,
        IBusyService busyService)
    {
        _cameraStationService = cameraStationService;
        _messageDialogService = messageDialogService;
        _busyService = busyService;
        Init();
    }

    public DelegateCommand DisposeDialogCommand { get; set; }
    public string Title { get; set; } = "相机设置";
    public List<CameraBrand> CameraBrands { get; set; } = new List<CameraBrand>();
    public DelegateCommand FindCameraCommand { get; set; }
    public DelegateCommand<CameraInfo> BindCameraCommand { get; set; }

    private CameraBrand _selectedCameraBrand;

    public CameraBrand SelectedCameraBrand
    {
        get => _selectedCameraBrand;
        set
        {
            SetProperty(ref _selectedCameraBrand, value);
            FindCameraCommand.RaiseCanExecuteChanged();
        }
    }

    private StationProfile _selectedStation;

    public StationProfile SelectedStation
    {
        get => _selectedStation;
        set
        {
            SetProperty(ref _selectedStation, value);
            RefreshRowStates();
        }
    }

    public IReadOnlyCollection<StationProfile> StationList { get; set; }
    private List<CameraItem> _cameraItems;

    public List<CameraItem> CameraItems
    {
        get
        { 
            if(_cameraItems is null)
                _cameraItems = new List<CameraItem>();
            return _cameraItems;
        }
        set { SetProperty(ref _cameraItems, value); }
    }

    private void Init()
    {
        StationList = _cameraStationService.GetStations();
        CameraBrands.Add(new CameraBrand()
        {
            CameraName = "海康相机",
            CameraType = CameraEnum.HikVision
        });
        FindCameraCommand = new DelegateCommand(DoFindCamera, () => SelectedCameraBrand != null);
        BindCameraCommand = new DelegateCommand<CameraInfo>(DoBindCameraCommand);
    }

    private void DoBindCameraCommand(CameraInfo obj)
    {
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
}