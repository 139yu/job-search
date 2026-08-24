using Commons.Base;
using Commons.Enums;
using MvCameraControl;
using Vision.Base;
using Vision.Models;

namespace Vision.Camera;

public class HikVisionCamera : ICameraDevice
{
    public HikVisionCamera(CameraInfo cameraInfo, CameraParam cameraParam)
    {
        IsInitialized = false;
        IsGrabbing = false;
        IsConnected = false;
        CameraInfo = cameraInfo;
        CameraParam = cameraParam;
    }
    public bool IsOpen { get; private set; }
    public bool IsInitialized { get; private set; }
    public bool IsGrabbing { get; private set; }
    public bool IsConnected { get; private set; }
    public CameraInfo CameraInfo { get; }
    public CameraParam CameraParam { get; }
    public void Open()
    {
        throw new NotImplementedException();
    }

    public void Init()
    {
        throw new NotImplementedException();
    }

    public void Close()
    {
        throw new NotImplementedException();
    }

    public void SetExposure(int exposure)
    {
        throw new NotImplementedException();
    }

    public void SetGain(int gain)
    {
        throw new NotImplementedException();
    }

    public void StartAcquisition()
    {
        throw new NotImplementedException();
    }

    public void StopAcquisition()
    {
        throw new NotImplementedException();
    }

    public void ClearFrame()
    {
        throw new NotImplementedException();
    }

    public bool TryGrabFrame()
    {
        throw new NotImplementedException();
    }

    public bool TryGetFrame(out object frame)
    {
        throw new NotImplementedException();
    }
}
