using Commons.Base;
using Vision.Base;
using Vision.Camera;
using Vision.Models;
using Vision.Service;

namespace MainApp.ViewModels.Menu;

public class CameraSettingDialogViewModel : IBaseDialogAware
{
    public IReadOnlyCollection<StationProfile> StationList;
    private ICameraStationService _cameraStationService;
    public CameraSettingDialogViewModel(ICameraStationService cameraStationService)
    {
        _cameraStationService = cameraStationService;
        StationList = _cameraStationService.GetStations();
    }

    public DelegateCommand DisposeDialogCommand { get; set; }
    public string Title { get; set; } = "相机设置";

    public bool CanCloseDialog()
    {
        return true;
    }

    public void OnDialogClosed()
    {
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
    }

    public DialogCloseListener RequestClose { get; }
}