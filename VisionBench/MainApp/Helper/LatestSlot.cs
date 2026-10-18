namespace MainApp.Helper;

/// <summary>
/// "只保留最新一项"的槽位。
///
/// <para>
/// 用途：生产端比消费端快时（相机 30fps、界面只能画 20fps），
/// 让新项直接顶替还没被消费的旧项，而不是排队——
/// 排队的结果是"延迟越积越大"，而丢帧的结果只是"画面变卡"，后者是可接受的。
/// </para>
///
/// <para>
/// <b>为什么必须连同释放一起做：</b>本槽位专用于非托管资源（如 Halcon 图像）。
/// 被顶替的项若没人释放，每丢一帧就泄漏一块非托管内存——
/// 这种泄漏不报错，只表现为内存缓慢上涨，是最难查的一类问题。
/// 因此 <see cref="Put"/> 把旧项"交还"给调用方释放，而不是自己悄悄丢进垃圾桶。
/// </para>
///
/// <para>
/// <b>为什么不在锁内释放：</b>Dispose 可能触发任意回调（甚至回到本类），
/// 在锁内释放容易死锁。这里只在锁内换指针，把释放交给调用方在锁外做。
/// </para>
/// </summary>
/// <typeparam name="T">需要显式释放的资源类型。</typeparam>
public sealed class LatestSlot<T> where T : class, IDisposable
{
    private readonly object _gate = new();
    private T? _item;

    /// <summary>
    /// 替代当前项，并返回被替代的项
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public T? Put(T item)
    {
        lock (_gate)
        {
            var dropped = _item;
            _item = item;
            return dropped;
        }
    }

    /// <summary>
    /// 取出当前项，清空槽位
    /// </summary>
    /// <returns></returns>
    public T? Take()
    {
        lock (_gate)
        {
            var item = _item;
            _item = null;
            return item;
        }
    }

    /// <summary>
    /// 查看当前项，只读操作
    /// </summary>
    /// <returns></returns>
    public T? Peek()
    {
        lock (_gate)
        {
            return _item;
        }
    }
}