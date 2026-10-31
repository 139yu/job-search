using System.IO;
using Commons;
using Commons.Enums;
using Commons.Helper;
using Commons.Logging;
using HalconDotNet;
using Vision.Calibration;
using Vision.Enums;

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
            if (string.IsNullOrWhiteSpace(json))
                return null;
            var snapshot = JsonUtil.FromJson<CalibrationSnapshot>(json);
            if (snapshot == null)
                return null;
            if (snapshot.SchemaVersion > CalibrationSnapshot.CurrentSchemaVersion)
                throw new InvalidOperationException($"标定文件 schemaVersion={snapshot.SchemaVersion} 高于程序支持的 {CalibrationSnapshot.CurrentSchemaVersion}");
            return snapshot;
        }
    }

    public void Save(CalibrationSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        lock (_gate)
        {
            Directory.CreateDirectory(_directory);
            var json = JsonUtil.ToJson(snapshot);
            var tmp = _metadataPath + ".tmp";
            File.WriteAllText(tmp, json);
            File.Move(tmp, _metadataPath, true);
            if (snapshot.Kind != CalibrationKindEnum.CameraPlane)
                return;
            var camParPath = Path.Combine(_directory, $"cam_par{ToFileName(snapshot.Version)}.cal");
            var cameraParam = snapshot.CreateCameraParam();
            try
            {
                HOperatorSet.WriteCamPar(cameraParam, camParPath);
            }
            catch (Exception e)
            {
                Logger.Warn(e, $"写入campar文件失败：{camParPath}");
            }
            finally
            {
                cameraParam.Dispose();
            }
        }
    }
    
    public void Clear()
    {
        lock (_gate)
        {
            if(File.Exists(_metadataPath))
                File.Delete(_metadataPath);
            if (!Directory.Exists(_directory))
                return;
            foreach (var file in Directory.GetFiles(_directory,"campar_*.cal"))
            {
                File.Delete(file);
            }
        }
    }

    private static string ToFileName(string version)
    {
        if(string.IsNullOrEmpty(version))
            return "unknown";
        var invalid = Path.GetInvalidFileNameChars();
        var chars = version.Select(c => invalid.Contains(c) ? '-' : c).ToArray();
        return new string(chars);
    }
}