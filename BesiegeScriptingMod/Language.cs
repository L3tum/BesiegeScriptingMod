using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IronPython.Runtime;
using UnityEngine;

namespace BesiegeScriptingMod
{
    class Language
    {
        public bool selected;

        public String name { get; }

        private readonly String _folder;
        public readonly Dictionary<string, Script> Scripts = new Dictionary<string, Script>();
        public bool needsConvertion { get; }
        private readonly String _defaultFile;
        private readonly String _extension;

        public Language(String name, bool needsConvertion, String extension)
        {
            this.name = name;
            _folder = Application.dataPath + "/Mods/Scripts/" + name + "Scripts";
            this.needsConvertion = needsConvertion;
            this._defaultFile = Application.dataPath + "/Mods/Scripts/StandardScripts/" + name + "." + extension;
            this._extension = extension;
        }

        public void InitializeScripts()
        {
            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(_folder);
                foreach (FileInfo fileInfo in di.GetFiles())
                {
                    if (!fileInfo.Extension.Equals("ref"))
                    {
                        if (!File.Exists(fileInfo.FullName.Replace(_extension, "ref")))
                        {
                            using (TextWriter tw = File.CreateText(fileInfo.FullName.Replace(_extension, "ref")))
                            {
                                String refs = "";
                                refs += Application.dataPath + "/Mods/SpaarModLoader.dll" + Util.getNewLine();
                                refs += Application.dataPath + "/Managed/Assembly-UnityScript.dll" + Util.getNewLine();
                                refs += Application.dataPath + "/Managed/Assembly-UnityScript-firstpass.dll" + Util.getNewLine();
                                refs += Application.dataPath + "/Managed/Assembly-CSharp.dll" + Util.getNewLine();
                                refs += Application.dataPath + "/Managed/Assembly-CSharp-firstpass.dll" + Util.getNewLine();
                                refs += Application.dataPath + "/Managed/UnityEngine.dll" + Util.getNewLine();
                                refs += Application.dataPath + "/Managed/System.dll";
                                tw.Write(refs);
                                tw.Close();
                            }
                        }
                        Scripts.Add(fileInfo.Name.Replace("." + _extension, ""), new Script(fileInfo.FullName, fileInfo.FullName.Replace(_extension, "ref")));
                    }
                }
            }
        }

        public String LoadDefaultFileSource()
        {
            String source;
            using (TextReader tr = new StreamReader(_defaultFile))
            {
                source = tr.ReadToEnd();
                tr.Close();
            }
            return source;
        }

        public void SaveSourceToFile(String sauce, String name)
        {
            using (TextWriter tw = new StreamWriter(_folder + "/" + name + "." +_extension))
            {
                tw.Write(sauce);
                tw.Close();
            }
        }
    }
}
