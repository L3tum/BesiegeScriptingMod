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
                    foreach (FileInfo info in DI.GetDirectories().First(info => info.Name.Equals("PythonScripts")).GetFiles())
                    {
                        PythonScripts.Add(info.Name, info);
                    }
                }
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/LuaScripts"))
                {
                    foreach (FileInfo fileInfo in DI.GetDirectories().First(info => info.Name.Equals("LuaScripts")).GetFiles())
                    {
                        LuaScripts.Add(fileInfo.Name, fileInfo);
                    }
                }
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/BrainfuckScripts"))
                {
                    foreach (FileInfo fileInfo in DI.GetDirectories().First(info => info.Name.Equals("BrainfuckScripts")).GetFiles())
                    {
                        BrainfuckScripts.Add(fileInfo.Name, fileInfo);
                    }
                }
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/CSharpScripts"))
                {
                    foreach (FileInfo fileInfo in DI.GetDirectories().First(info => info.Name.Equals("CSharpScripts")).GetFiles())
                    {
                        CSharpScripts.Add(fileInfo.Name, fileInfo);
                    }
                }
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/ChefScripts"))
                {
                    foreach (FileInfo fileInfo in DI.GetDirectories().First(info => info.Name.Equals("ChefScripts")).GetFiles())
                    {
                        ChefScripts.Add(fileInfo.Name, fileInfo);
                    }
                }
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/JavaScriptScripts"))
                {
                    foreach (FileInfo fileInfo in DI.GetDirectories().First(info => info.Name.Equals("JavaScriptScripts")).GetFiles())
                    {
                        JavaScriptScripts.Add(fileInfo.Name, fileInfo);
                    }
                }
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/JavaScripts"))
                {
                    foreach (FileInfo fileInfo in DI.GetDirectories().First(info => info.Name.Equals("BrainfuckScripts")).GetFiles())
                    {
                        BrainfuckScripts.Add(fileInfo.Name, fileInfo);
                    }
                }
            }
        }

        public void OnUnload()
        {
            
        }
    }
}
