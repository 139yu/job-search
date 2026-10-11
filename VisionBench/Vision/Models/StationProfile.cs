using Vision.Enums;

namespace Vision.Models;

public class StationProfile : BindableBase
{
    private StationEnum _stationName;

    public StationEnum StationName
    {
        get => _stationName;
        set => SetProperty(ref _stationName, value);
    }

    private CameraEnum? _cameraType;

    public CameraEnum? CameraType
    {
        get => _cameraType;
        set
        {
            if (SetProperty(ref _cameraType, value))
                RaisePropertyChanged(nameof(IsBound));
        }
    }

    private string? _serialNum;

    public string? SerialNum
    {
        get => _serialNum;
        set
        {
            if (SetProperty(ref _serialNum, value))
                RaisePropertyChanged(nameof(IsBound));
        }
    }

    private CameraParam _cameraParam = new();

    public CameraParam CameraParam
    {
        get => _cameraParam;
        set => SetProperty(ref _cameraParam, value);
    }

    public bool IsBound => CameraType is not null && SerialNum is not null;
}