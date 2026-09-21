using Commons.Base;
using CommonUI.Base;
using CommonUI.Helper;
using MainApp.Models;
using Vision.Base;
using Vision.Camera;
using Vision.Enums;
using Vision.Factory;
using Vision.Models;
using Vision.Service;

namespace MainApp.ViewModels.Menu;

public class CameraSettingDialogViewModel : IBaseDialogAware
{
    public List<CameraBrand> CameraBrands { get; set; } = new List<CameraBrand>();
    public DelegateCommand FindCameraCommand { get; set; }

    private CameraBrand _selectedCameraBrand;
    public CameraBrand SelectedCameraBrand
    {
        get => _selectedCameraBrand;
        set { 
            SetProperty(ref _selectedCameraBrand, value); 
            FindCameraCommand.RaiseCanExecuteChanged();
            
        }
    }

    public IReadOnlyCollection<StationProfile> StationList { get; set; }
    private List<CameraInfo> _cameraList;

    public List<CameraInfo> CameraList
    {
        get => _cameraList;
        set
        {
            SetProperty(ref _cameraList, value);
        }
    }

    private ICameraStationService _cameraStationService;

    public CameraSettingDialogViewModel(ICameraStationService cameraStationService)
    {
        _cameraStationService = cameraStationService;
        StationList = _cameraStationService.GetStations();
        Init();
    }

    public DelegateCommand DisposeDialogCommand { get; set; }
    public string Title { get; set; } = "相机设置";

    private void Init()
    {
        CameraBrands.Add(new CameraBrand()
        {
            CameraName = "海康相机",
            CameraType = CameraEnum.HikVision
        });
        FindCameraCommand = new DelegateCommand(DoFindCamera, () => SelectedCameraBrand != null);
    }

    private void DoFindCamera()
    {
        try
        {
            var cameraEnumerator = CameraEnumeratorFactory.Instance.GetCameraEnumerator(SelectedCameraBrand.CameraType);
            CameraList = cameraEnumerator.ListAvailable();
            GrowlHelper.Success("枚举相机成功！");
        }
        catch (Exception e)
        {
            GrowlHelper.Error(e.Message);
        }
    }
}