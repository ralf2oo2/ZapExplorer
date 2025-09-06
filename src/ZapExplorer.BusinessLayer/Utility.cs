namespace ZapExplorer.BusinessLayer;
using Newtonsoft.Json;

public static class Utility
{
    public static T DeepClone<T>(this T self)
    {
        var serialized = JsonConvert.SerializeObject(self);
        return JsonConvert.DeserializeObject<T>(serialized);
    }
}

