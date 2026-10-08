using Vision.Camera;
using Vision.Enums;
using Vision.Models;

namespace Vision.Base;
/// <summary>
/// 工位与相机的装配服务。
/// 负责三件事：加载工位配置、按工位打开/关闭相机、维护工位与相机的绑定关系。
/// </summary>
public interface ICameraStationService
{
    /// <summary>
    /// 加载工位配置。首次运行（配置文件不存在或为空）时会补齐全部默认工位。
    /// <para><b>本方法只处理配置，不碰任何设备。</b></para>
    /// </summary>
    void LoadStations();
    List<StationProfile> GetStations();
    StationProfile GetStation(StationEnum station);
    void Initialize();
    void SaveStations();
    StationConnectionState OpenStation(StationEnum station);
    void CloseStation(StationEnum station);
    StationConnectionState GetStationState(StationEnum station);
    ICameraDevice GetCamera(StationEnum station);
    void BindStation(StationEnum station,CameraInfo cameraInfo);
    void UnBindStation(StationEnum station);
    void ApplyCameraParams(StationEnum station,CameraParam cameraParam);
    void ApplyCameraLiveParams(StationEnum station,CameraParam cameraParam);
    void ReadCameraParams(StationEnum station, out CameraParam cameraParam);
}