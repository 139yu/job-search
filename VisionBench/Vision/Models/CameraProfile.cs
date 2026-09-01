namespace Vision.Models;

public class CameraProfile
{
    public CameraInfo Info { get; set; } = new();
    public CameraParam Param { get; set; } = new();
}