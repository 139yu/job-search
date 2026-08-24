using Vision.Models;

namespace Vision.Base;

/// <summary>
/// 相机管理器接口。
/// 负责枚举系统上已连接/可用的相机设备。
/// </summary>
public interface ICameraManager
{
    /// <summary>
    /// 枚举当前系统上所有可用的相机设备。
    /// </summary>
    /// <returns>可用相机信息列表；没有可用相机时返回空列表。</returns>
    List<CameraInfo> ListAvailable();
}