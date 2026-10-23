namespace Vision.Calibration;

public interface ICalibrationService
{
    CalibrationSnapshot Current { get; }
    Task<CalibrationSnapshot> LoadAsync();
    void Replace(CalibrationSnapshot snapshot);
}