using Vision.Models;

namespace MainApp.Models;

public class CameraItem : BindableBase
{
    public CameraInfo Camera { get; }

    public CameraItem(CameraInfo camera)
    {
        Camera = camera;
    }

    public bool Matches(StationProfile profile) =>
        profile.IsBound && profile.CameraType == Camera.CameraType
                        && profile.SerialNum == Camera.SerialNum;

    private bool _isBoundToStation;

    public bool IsBoundToStation
    {
        get => _isBoundToStation;
        set
        {
            if (SetProperty(ref _isBoundToStation, value))
                return;
            RaisePropertyChanged(nameof(ShowBindButton));
            RaisePropertyChanged(nameof(ShowUnBindButton));
        }
    }

    public bool ShowBindButton => !IsBoundToStation;
    public bool ShowUnBindButton => IsBoundToStation;
}