using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FileIndexer
{
    public static class Utility
    {
        private static Dictionary<string, string> settings = new Dictionary<string, string>();

        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        public static string ResolvePath(string parsedPath)
        {
            // To resolve paths platform-independently, deconstruct path into parts, then resolve each part individually
            char separator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? '\\' : '/';
            string[] parts = parsedPath.Split(separator);
            var resolvedParts = new List<string>();


            foreach (string part in parts)
            {
                if (part == "" || part == ".")
                    continue;

                if (part == "..")
                {
                    if (resolvedParts.Count == 0)
                        throw new ArgumentException("Invalid parsed path");
                    resolvedParts.RemoveAt(resolvedParts.Count - 1);
                }

                else
                    resolvedParts.Add(part);
            }
            return string.Join(separator, resolvedParts);
        }

        public static bool LoadSettings(string path)
        {
            Dictionary<string, string>? settings = new();
            path = ResolvePath(path);

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                return true;
            }

            return false;
        }

        public static string GetSetting(string key)
        {
            if (settings.TryGetValue(key, out string value))
                return value;

            else
                return "";
        }

        public static bool SetSetting(string key, string value)
        {
            if (settings.ContainsKey(key))
            {
                settings[key] = value;
                return true;
            }

            else
                return false;
        }

        public static bool WriteSettings(string path)
        {
            path = ResolvePath(path);

            if (settings.Count <= 0)
                return false;

            else
            {
                string json = JsonConvert.SerializeObject(settings);
                File.WriteAllText(path, json);
                return true;
            }
        }
    }
}
