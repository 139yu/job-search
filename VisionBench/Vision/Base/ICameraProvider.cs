using Vision.Enums;
using Vision.Models;

namespace Vision.Base;

/// <summary>
/// 相机品牌扩展点。一个品牌（海康 / 大恒 / Basler …）实现一个 Provider，
/// 由它负责该品牌下"枚举设备"与"创建设备"两件事。
/// <para>
/// <b>新增品牌不允许改动 Vision 库中任何既有文件</b>，只需：
/// 新增一个 Provider 类，并在启动时调用
/// <see cref="Vision.Factory.CameraRegistry.Register"/> 注册一行。
/// </para>
/// </summary>
public interface ICameraProvider
{
    CameraEnum Brand { get; }
    string DisplayName { get; }
    ICameraEnumerator CameraEnumerator();
    ICameraDevice Create(CameraInfo cameraInfo);
}