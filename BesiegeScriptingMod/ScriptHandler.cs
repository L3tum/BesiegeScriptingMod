using System;
using System.Collections.Generic;
using System.IO;
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
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/PythonScripts"))
                {
                    foreach (string file in Directory.get(Application.dataPath + "/Mods/Scripts/PythonScripts"))
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
