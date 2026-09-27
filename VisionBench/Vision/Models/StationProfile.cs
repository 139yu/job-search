using Vision.Camera;
using Vision.Enums;

namespace Vision.Models;

public class StationProfile
{
    public StationEnum StationName { get; set; }
    public CameraEnum? CameraType { get; set; }
    public string? SerialNum { get; set; }
    public CameraParam CameraParam { get; set; } = new();
    public bool IsBound => CameraType is not null && SerialNum is not null;
    public string BindingKey => IsBound ? $"{CameraType}-{SerialNum}" : string.Empty;
}