namespace ZapExplorer.BusinessLayer;
using Newtonsoft.Json;

public static class Utility
{
    public static T DeepClone<T>(this T self)
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.None
        };
        var serialized = JsonConvert.SerializeObject(self, settings);
        return JsonConvert.DeserializeObject<T>(serialized, settings);
    }
}

