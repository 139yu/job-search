using Vision.Models;

namespace Vision.Base;

/// <summary>
/// 相机设备接口。
/// 定义所有相机设备（如海康、巴斯勒等）统一的操作契约，
/// 上层业务通过本接口与具体相机实现解耦。
/// 约定：命令型操作（Open/Init/Close/采集控制/参数设置）失败时抛异常，
/// 不通过返回值表达结果；轮询型操作（Try*）失败返回 false，不抛异常。
/// </summary>
public interface ICameraDevice
{
    /// <summary>
    /// 相机是否已初始化（参数已下发）。
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// 相机是否正在采集。
    /// </summary>
    bool IsGrabbing { get; }

    /// <summary>
    /// 相机连接是否有效。
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 相机基本信息（品牌、型号、序列号等）。
    /// </summary>
    CameraInfo CameraInfo { get; }

    /// <summary>
    /// 相机运行参数（曝光、增益、像素格式、触发模式等）。
    /// </summary>
    CameraParam CameraParam { get; }

    /// <summary>
    /// 打开相机设备，建立与相机的连接。失败抛异常。
    /// </summary>
    void Open();

    /// <summary>
    /// 初始化相机，应用相机运行参数。失败抛异常。
    /// </summary>
    void Init();

    /// <summary>
    /// 关闭相机设备，释放相关资源。失败抛异常。
    /// </summary>
    void Close();

    /// <summary>
    /// 设置曝光时间。失败抛异常。
    /// </summary>
    /// <param name="exposure">曝光时间值（单位由具体相机决定，通常为微秒）。</param>
    void SetExposure(int exposure);

    /// <summary>
    /// 设置增益。失败抛异常。
    /// </summary>
    /// <param name="gain">增益值。</param>
    void SetGain(int gain);

    /// <summary>
    /// 开始图像采集。失败抛异常。
    /// </summary>
    void StartAcquisition();

    /// <summary>
    /// 停止图像采集。失败抛异常。
    /// </summary>
    void StopAcquisition();

    /// <summary>
    /// 清空缓冲区中尚未取走的图像帧。失败抛异常。
    /// </summary>
    void ClearFrame();

    /// <summary>
    /// 尝试同步抓取一帧图像。失败（超时等）返回 false，不抛异常。
    /// </summary>
    bool TryGrabFrame();

    /// <summary>
    /// 尝试获取最近抓取到的图像帧。失败返回 false，不抛异常。
    /// </summary>
    /// <param name="frame">输出参数，抓取到的图像帧对象。</param>
    bool TryGetFrame(out object frame);
}
