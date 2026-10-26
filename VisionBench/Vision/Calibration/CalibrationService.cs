using Commons.Enums;
using Commons.Logging;

namespace Vision.Calibration;

public class CalibrationService : ICalibrationService
{
    private static readonly NLog.Logger Logger = Log.For<CalibrationService>(LogModule.App);
    private readonly ICalibrationStore _store;
    private readonly object _gate = new();
    private CalibrationSnapshot _current = CalibrationSnapshot.None;
    public event EventHandler<CalibrationSnapshot>? Changed;

    public CalibrationService(ICalibrationStore store)
    {
        _store = store;
    }

    public CalibrationSnapshot Current
    {
        get
        {
            lock (_gate)
            {
                return _current;
            }
        }
    }
    public CalibrationSnapshot Load()
    {
        CalibrationSnapshot? loaded;
        try
        {
            loaded = _store.Load();
        }
        catch (Exception e)
        {
            Logger.Error(e, "加载标定快照失败，按未标定处理");
            loaded = null;
        }

        if (loaded is null)
        {
            Publish(CalibrationSnapshot.None);
            return CalibrationSnapshot.None;
        }

        try
        {
            loaded.Validate();
        }
        catch (Exception e)
        {
            Logger.Error(e, $"标定快照非法（version={loaded.Version}），按未标定处理");
            Publish(CalibrationSnapshot.None);
            return CalibrationSnapshot.None;
        }
        Publish(loaded);
        Logger.Info($"加载标定快照：kind={loaded.Kind} version={loaded.Version}");
        return loaded;
    }

    public void Replace(CalibrationSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        snapshot.Validate();
        _store.Save(snapshot);
        Publish(snapshot);
        Logger.Info($"替换标定快照：kind={snapshot.Kind} version={snapshot.Version}");
    }

    public void Clear()
    {
        _store.Clear();
        Publish(CalibrationSnapshot.None);
        Logger.Info("已清除标定");
    }

    private void Publish(CalibrationSnapshot snapshot)
    {
        lock (_gate)
        {
            _current = snapshot;
        }
        Changed?.Invoke(this,snapshot);
    }
}