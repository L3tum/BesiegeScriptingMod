using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BesiegeScriptingMod
{
    class ScriptHandler : MonoBehaviour
    {
        public Dictionary<String, FileInfo> PythonScripts = new Dictionary<string, FileInfo>();
        public Dictionary<String, FileInfo> LuaScripts = new Dictionary<string, FileInfo>();
        public Dictionary<String, FileInfo> BrainfuckScripts = new Dictionary<string, FileInfo>();
        public Dictionary<String, FileInfo> CSharpScripts = new Dictionary<string, FileInfo>();
        public Dictionary<String, FileInfo> ChefScripts = new Dictionary<string, FileInfo>();
        public Dictionary<String, FileInfo> JavaScriptScripts = new Dictionary<string, FileInfo>();
        public Dictionary<String, FileInfo> JavaScripts = new Dictionary<string, FileInfo>();

        public void Start()
        {
            if (!Directory.Exists(Application.dataPath + "/Mods/Scripts"))
            {
                Directory.CreateDirectory(Application.dataPath + "/Mods/Scripts");
            }
            else
            {
                DirectoryInfo DI = new DirectoryInfo(Application.dataPath + "/Mods/Scripts");
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/PythonScripts"))
                {
                    DirectoryInfo DI = new DirectoryInfo(Application.dataPath + "/Mods/Scripts/");
                    foreach (FileInfo info in DI.GetDirectories().First(info => info.Name.Equals("PythonScripts")).GetFiles())
                    {
                        
                    }
                }
            }
        }

        public void OnUnload()
        {
            
        }
    }
}
