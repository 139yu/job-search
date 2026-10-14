using Commons.Enums;
using Commons.Logging;
using HalconDotNet;
using Vision.Enums;
using Vision.Events;

namespace Vision.ImageProvider;

/// <summary>
/// 帧分发器。所有图像提供器共用的分发规则：
/// <para>
/// <b>除最后一个订阅者之外，其余每个订阅者都拿到一份独立的图像拷贝。</b>
/// 订阅者负责释放自己收到的那一份。
/// </para>
/// <para>
/// 这样做的原因：订阅者之间共享同一个 HImage 时，先释放的一方会让另一方
/// 访问已释放的非托管内存——表现为随机崩溃，极难定位。
/// 只有一个订阅者时不做任何拷贝。
/// </para>
/// <para>
/// 规则收在这一个类里，是为了以后接入算法通路时不需要再改各提供器。
/// </para>
/// </summary>
internal static class FrameDistributor
{
    private static readonly NLog.Logger _logger = Log.For(LogModule.Camera, typeof(FrameDistributor));

    /// <summary>
    /// 把一帧图像分发给 <paramref name="handler"/> 的全部订阅者。
    /// </summary>
    /// <returns>
    /// 所有权是否已经移交。
    /// <para>返回 false（没有任何订阅者）时，<b>调用方必须自己释放</b> <paramref name="image"/>。</para>
    /// <para>返回 true 时，调用方不得再访问或释放该图像。</para>
    /// </returns>
    internal static bool Dispatch(EventHandler<FrameReadyEventArgs>? handler,
        object sender,
        HImage image,
        ImageSourceEnum imageSource)
    {
        var handlers = handler?.GetInvocationList();
        if (handlers is null || handlers.Length == 0)
            return false;
        for (var i = 0; i < handlers.Length; i++)
        {
            var frame = i == handlers.Length - 1 ? image : image.CopyImage();
            var args = new FrameReadyEventArgs(frame, imageSource);
            try
            {
                ((EventHandler<FrameReadyEventArgs>)handlers[i]).Invoke(sender,args);
            }
            catch (Exception e)
            {
                _logger.Error(e,"订阅方处理图像帧失败");
                frame.Dispose();
            }
        }

        return true;
    }
}