using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Commons.Helper;

public static class JsonUtil
{
    private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
    {
        Formatting = Formatting.Indented,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        Converters = {new StringEnumConverter()}
    };
    public static string ToJson<T>(T obj) => JsonConvert.SerializeObject(obj,_settings);
    public static T? FromJson<T>(string json) => JsonConvert.DeserializeObject<T>(json, _settings);
}