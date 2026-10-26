namespace Vision.Calibration;

public interface ICalibrationStore
{
    CalibrationSnapshot? Load();
    void Save(CalibrationSnapshot snapshot);
    void Clear();
}