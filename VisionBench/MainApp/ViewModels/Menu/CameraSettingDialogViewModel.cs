using Commons.Base;
using Vision.Base;
using Vision.Camera;
using Vision.Models;
using Vision.Service;

namespace MainApp.ViewModels.Menu;

public class CameraSettingDialogViewModel : IBaseDialogAware
{
    public IReadOnlyList<StationProfile> StationProfiles;
    private ICameraStationService _cameraStationService;
    public CameraSettingDialogViewModel(ICameraStationService cameraStationService)
    {
        _cameraStationService = cameraStationService;
        StationProfiles = _cameraStationService.GetStations();
        StationProfiles = StationProfiles == null ? new List<StationProfile>() : StationProfiles;
        if (StationProfiles.Count == 0)
        {
            var stations = Enum.GetValues(typeof(StationEnum));
            foreach (var station in stations)
            {
                StationProfiles.Append(new StationProfile()
                {
                    StationName = (StationEnum)station
                });
            }
        }
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