using Vision.Enums;
using Vision.Events;
using Vision.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Vision.Base;

/// <summary>
/// 相机设备接口。
/// 定义所有相机设备（如海康、巴斯勒等）统一的操作契约，
/// 上层业务通过本接口与具体相机实现解耦。
///
/// <para><b>失败约定：</b>命令型操作（Open/Init/Close/采集控制/参数设置）失败时抛异常，
/// 不通过返回值表达结果；轮询型操作（Try*）失败返回 false，不抛异常。</para>
///
/// <para><b>像素格式约定：</b>设备必须把厂商 SDK 特有的像素格式在本层消化掉，
/// 保证 <see cref="CameraFrame.ImageLayout"/> 只可能是
/// <c>Gray8 / Gray16 / Bgr8</c> 三者之一。
/// 换言之，上层拿到的帧永远不需要知道相机厂商用的是什么格式；
/// 从 <c>CameraFrame</c> 转成可显示的图像由 <c>CameraFrameConverter</c> 统一负责，
/// 本接口的实现里不得出现"转换成显示格式"的代码。</para>
///
/// <para><b>帧来源约定：</b>设备是"拉"模型——采集到的帧进内部队列，
/// 由 <see cref="TryGetFrame"/> 取走。要"推"给多个消费者，请通过
/// <see cref="IImageProvider"/> 而不是直接轮询本接口。</para>
/// </summary>
public interface ICameraDevice
{
    CameraStateEnum State { get; }
    event EventHandler<StateChangedEventArgs> StateChanged;

    /// <summary>
    /// 相机基本信息（品牌、型号、序列号等）。
    /// </summary>
    CameraInfo CameraInfo { get; }

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
    void StartSingleGarb();

    /// <summary>
    /// 尝试获取最近抓取到的图像帧
    /// </summary>
    /// <returns></returns>
    bool TryGetFrame(out CameraFrame frame);

    void ApplyParams(CameraParam? cameraParam);
    void ApplyLiveParams(CameraParam? cameraParam);
    void ReadCameraParams(CameraParam? cameraParam);
}