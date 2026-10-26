using System.IO;
using Commons;
using Commons.Enums;
using Commons.Logging;
using Vision.Calibration;

namespace Vision.Config;

public class JsonCalibrationStore: ICalibrationStore
{
    private static readonly NLog.Logger Logger = Log.For<JsonCalibrationStore>(LogModule.App);
    private readonly string _directory;
    private readonly string _metadataPath;
    private readonly object _gate = new();

    public JsonCalibrationStore():this(Path.Combine(AppConstants.ConfigPath,"calib"))
    {
        
    }

    public JsonCalibrationStore(string directory)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        _metadataPath = Path.Combine(directory, "calibration.json");
    }
    public CalibrationSnapshot? Load()
    {
        lock (_gate)
        {
            if (!File.Exists(_metadataPath))
                return null;
            var json = File.ReadAllText(_metadataPath);
        }
    }

    public void Save(CalibrationSnapshot snapshot)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }
}