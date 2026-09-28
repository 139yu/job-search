namespace Vision.Enums;

public enum StationConnectionState
{
    /// <summary>该工位尚未绑定相机。</summary>
    Unbound,

    /// <summary>已绑定，且相机已成功连接。</summary>
    Connected,

    /// <summary>
    /// 已绑定，但设备不在线——配置里有序列号，硬件上枚举不到。
    /// 通常是没插、没上电、或网线没接。
    /// </summary>
    Offline,

    /// <summary>
    /// 已绑定，设备也在线，但打开失败。
    /// 通常是被别的程序占用、或初始化（下发参数）失败。
    /// </summary>
    Failed
}