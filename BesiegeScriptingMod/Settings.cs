using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace BesiegeScriptingMod
{
    public static class Settings
    {
        public static Rect WinSize;
        public static bool Tutorial { get; internal set; }
        public static String LastIde { get; internal set; }
        private static readonly String SettingsPath = Application.dataPath + "/Mods/Scripts/settings.json";

        public static void Save()
        {
            SaveSettings ss = new SaveSettings(WinSize, LastIde);
            String json = JsonConvert.SerializeObject(ss);
            using (TextWriter tw = new StreamWriter(SettingsPath, false))
            {
                tw.Write(json);
                tw.Close();
            }
        }

        public static void Load()
        {
            if (File.Exists(SettingsPath))
            {

                String json;
                using (TextReader tr = new StreamReader(SettingsPath))
                {
                    json = tr.ReadToEnd();
                    tr.Close();
                }
                SaveSettings ss = JsonConvert.DeserializeObject<SaveSettings>(json);
                WinSize = ss.WinSize;
                LastIde = ss.LastIde;
                Tutorial = true;
            }
            else
            {
                Vector2 windowLoc = new Vector2(Screen.width - 1200.0f, Screen.height - 400.0f);
                WinSize = new Rect(100.0f, 20.0f, windowLoc.x, windowLoc.y);
                Tutorial = true;
                LastIde = "Lua";
            }
        }
    }

    internal class SaveSettings
    {
        public Rect WinSize;
        public readonly String LastIde;
        public SaveSettings(Rect winSize, String lastIde)
        {
            this.WinSize = winSize;
            this.LastIde = lastIde;
        }
    }
}
