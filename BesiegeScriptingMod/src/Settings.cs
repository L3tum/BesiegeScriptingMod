#region usings

using System.IO;
using System.Xml.Serialization;
using UnityEngine;

#endregion

namespace BesiegeScriptingMod
{
    public static class Settings
    {
        public static Rect WinSize;
        public static bool useAPI;
        private static readonly string SettingsPath = Application.dataPath + "/Mods/Scripts/settings.xml";
        public static bool Tutorial { get; internal set; }
        public static string LastIde { get; internal set; }

        public static void Save()
        {
            SaveSettings ss = new SaveSettings(WinSize, LastIde, useAPI);
            new XmlSerializer(typeof (SaveSettings)).Serialize(new StreamWriter(SettingsPath, false), ss);
        }

        public static void Load()
        {
            if (File.Exists(SettingsPath))
            {
                SaveSettings ss = (SaveSettings) new XmlSerializer(typeof (SaveSettings)).Deserialize(new StreamReader(SettingsPath));
                WinSize = ss.WinSize;
                LastIde = ss.LastIde;
                useAPI = ss.useAPI;
                Tutorial = true;
            }
            else
            {
                Vector2 windowLoc = new Vector2(Screen.width - 1200.0f, Screen.height - 400.0f);
                WinSize = new Rect(100.0f, 20.0f, windowLoc.x, windowLoc.y);
                Tutorial = true;
                useAPI = false;
                LastIde = "";
            }
        }
    }

    public class SaveSettings
    {
        public string LastIde;
        public bool useAPI;
        public Rect WinSize;

        public SaveSettings(Rect winSize, string lastIde, bool useApi)
        {
            WinSize = winSize;
            LastIde = lastIde;
            useAPI = useApi;
        }

        public SaveSettings()
        {
        }
    }
}