using Vision.Enums;
using Vision.Events;
using Vision.Models;

namespace Vision.Base;

/// <summary>
/// 工位与相机的装配服务。职责边界只有三件：
/// <list type="bullet">
///   <item>加载与保存工位配置；</item>
///   <item>按工位绑定、打开、关闭相机；</item>
///   <item>作为工位连接状态的<b>唯一权威</b>，通过 <see cref="StationStateChanged"/> 广播变化。</item>
/// </list>
/// <para>
/// 本接口不负责图像采集与分发——采集由 <see cref="IImageProvider"/> 承担。
/// 需要帧的模块不要从这里取设备自行轮询。
/// </para>
/// </summary>
public interface ICameraStationService : IDisposable
{
    /// <summary>工位状态变化的唯一通知出口。所有变化都会在这里发出，且保证不重复。</summary>
    event EventHandler<StationStateChangedEventArgs> StationStateChanged;

    /// <summary>已注册的相机品牌，供 UI 展示。新增品牌后此列表自动生效。</summary>
    IReadOnlyList<ICameraProvider> GetAvailableBrands();

    /// <summary>枚举指定品牌下当前可用的相机设备。</summary>
    List<CameraInfo> ListAvailable(CameraEnum cameraType);

    /// <summary>加载工位配置。首次运行会补齐全部默认工位。只处理配置，不碰设备。</summary>
    void LoadStations();

    List<StationProfile> GetStations();

    /// <summary>取工位配置。工位不存在时抛出。</summary>
    StationProfile GetStation(StationEnum station);

    /// <summary>按配置打开全部已绑定工位，并广播各自的状态。</summary>
    void Initialize();

    void SaveStations();

    /// <summary>打开工位。返回打开后的状态；失败不抛异常，通过返回状态与事件表达。</summary>
    StationStateEnum OpenStation(StationEnum station);

    /// <summary>关闭工位并解除运行期装配，状态回到 <see cref="StationState.Unbound"/>。</summary>
    void CloseStation(StationEnum station);

    StationStateEnum GetStationState(StationEnum station);

    /// <summary>取工位当前挂载的设备。工位未连接时抛出 <c>BusinessException</c>。</summary>
    ICameraDevice GetCamera(StationEnum station);

    void BindStation(StationEnum station, CameraInfo cameraInfo);

    void UnBindStation(StationEnum station);

    void ApplyCameraParams(StationEnum station, CameraParam cameraParam);

    void ApplyCameraLiveParams(StationEnum station, CameraParam cameraParam);

    void ReadCameraParams(StationEnum station, out CameraParam cameraParam);
}