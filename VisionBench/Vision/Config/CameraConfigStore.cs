using System.IO;
using Commons;
using Commons.Helper;
using Vision.Base;
using Vision.Models;

namespace Vision.Config;

public class CameraConfigStore : ICameraConfigStore
{
    private string storePath = Path.Combine(AppConstants.ConfigPath,"CameraConfig.json");
    private readonly object _lock = new();
    public List<CameraProfile> LoadProfiles()
    {
        if(!File.Exists(storePath))
            return new List<CameraProfile>();
        var json = File.ReadAllText(storePath);
        return JsonUtil.FromJson<List<CameraProfile>>(json);
    }

    public void SaveProfiles(List<CameraProfile> profiles)
    {
        lock (_lock)
        {
            var dir = Path.GetDirectoryName(storePath);
            if(!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var tmp = storePath + ".tmp";
            var json = JsonUtil.ToJson(profiles);
            File.WriteAllText(tmp,json);
            File.Move(tmp, storePath);
        }
    }

}