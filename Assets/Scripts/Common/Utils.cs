using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Utils
{
    public static class Layer
    {
        public static bool Compare(LayerMask myLayerMask, int targetLayer)
        {
            return ((1 << myLayerMask) & targetLayer) != 0;
        }
    }
    
    public static class JsonHandler
    {
        public static JObject ResourceFileLoader(string fileName)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(fileName);
            if (textAsset == null)
            {
                Debug.LogError($"Not found JSON file: Resources/{fileName}");
                return null;
            }
            return JObject.Parse(textAsset.text);
        }

        public static JObject LoadFromString(string jsonString) => JObject.Parse(jsonString);
    }
    
    public static class Environment
    {
        public static bool isDevelopment = true;
    }
}
