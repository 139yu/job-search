namespace Vision.Calibration;

public interface ICalibrationService
{

    /// <summary>当前快照；未标定时为 <see cref="CalibrationSnapshot.None"/>，永不为 null。</summary>
    CalibrationSnapshot Current { get; }

    /// <summary>快照被替换或清空后触发。参数是新快照。订阅方需自行切回 UI 线程。</summary>
    event EventHandler<CalibrationSnapshot>? Changed;

    /// <summary>从存储加载并设为当前快照。返回加载结果（失败时为 None）。</summary>
    CalibrationSnapshot Load();

    /// <summary>校验并替换当前快照，同时落盘。非法快照抛异常且不改变现状。</summary>
    void Replace(CalibrationSnapshot snapshot);

    /// <summary>清空标定：删除存储并回到 None。</summary>
    void Clear();
}