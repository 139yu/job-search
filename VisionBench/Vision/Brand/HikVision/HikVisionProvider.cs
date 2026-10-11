using Vision.Base;
using Vision.Enums;
using Vision.Models;

namespace Vision.Brand.HikVision;

public class HikVisionProvider : ICameraProvider
{
    public CameraEnum Brand { get; } = CameraEnum.HikVision;
    public string DisplayName { get; } = "海康相机";

    public ICameraEnumerator CameraEnumerator()
    {
        return new HikVisionEnumerator();
    }

    public ICameraDevice Create(CameraInfo cameraInfo)
    {
        return new HikVisionCamera(cameraInfo);
    }
}