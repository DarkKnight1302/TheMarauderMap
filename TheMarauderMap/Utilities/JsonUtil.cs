using Newtonsoft.Json;

namespace TheMarauderMap.Utilities
{
    public static class JsonUtil
    {
        public static string SerializeObject(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        public static T DeSerialize<T>(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                return JsonConvert.DeserializeObject<T>(json);
            }

            return default;
        }
    }
}
