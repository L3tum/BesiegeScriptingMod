#region usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BesiegeScriptingMod.Brainfuck;
using BesiegeScriptingMod.Lua;
using BesiegeScriptingMod.Ook;
using BesiegeScriptingMod.Python;
using BesiegeScriptingMod.Util;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace BesiegeScriptingMod
{
    internal class Language
    {
        private readonly ConvertDeg _convertDeg;
        private readonly string _defaultFile;

        private readonly ExecuteDeg _executeDeg;
        internal readonly string _extension;
        internal readonly string _extensionDot;

        internal readonly string _folder;
        public readonly Dictionary<string, Script> Scripts = new Dictionary<string, Script>();
        public bool selected;

        public Language(string name, bool needsConvertion, string extension, [CanBeNull] MethodInfo executeMethod = null, [CanBeNull] MethodInfo convertMethod = null,
            [CanBeNull] object languageextension = null)
        {
            this.name = name;
            _folder = Application.dataPath + "/Mods/Scripts/" + name + "Scripts";
            this.needsConvertion = needsConvertion;
            _defaultFile = Application.dataPath + "/Mods/Scripts/StandardScripts/" + name + "." + extension;
            _extension = extension;
            _extensionDot = "." + _extension;
            if (executeMethod == null)
            {
                _executeDeg = (ExecuteDeg) Delegate.CreateDelegate(typeof (ExecuteDeg), this, typeof (Language).GetMethod("Execute" + name));
            }
            else
            {
                _executeDeg = (ExecuteDeg) Delegate.CreateDelegate(typeof (ExecuteDeg), languageextension, executeMethod);
            }
            if (needsConvertion)
            {
                if (convertMethod != null)
                {
                    _convertDeg = (ConvertDeg) Delegate.CreateDelegate(typeof (ConvertDeg), languageextension, convertMethod);
                }
                else
                {
                    _convertDeg = (ConvertDeg) Delegate.CreateDelegate(typeof (ConvertDeg), this, typeof (Language).GetMethod("Convert" + name));
                }
            }
            InitializeScripts();
        }

        public string name { get; }
        public bool needsConvertion { get; }

        private void InitializeScripts()
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
                                string refs = "";
                                refs += Application.dataPath + "/Mods/SpaarModLoader.dll" + Util.Util.getNewLine();
                                refs += Application.dataPath + "/Managed/Assembly-UnityScript.dll" + Util.Util.getNewLine();
                                refs += Application.dataPath + "/Managed/Assembly-UnityScript-firstpass.dll" + Util.Util.getNewLine();
                                refs += Application.dataPath + "/Managed/Assembly-CSharp.dll" + Util.Util.getNewLine();
                                refs += Application.dataPath + "/Managed/Assembly-CSharp-firstpass.dll" + Util.Util.getNewLine();
                                refs += Application.dataPath + "/Managed/UnityEngine.dll" + Util.Util.getNewLine();
                                refs += Application.dataPath + "/Managed/System.dll";
                                tw.Write(refs);
                                tw.Close();
                            }
                        }
                        Scripts.Add(fileInfo.Name.Replace(_extensionDot, ""), new Script(fileInfo.FullName, fileInfo.FullName.Replace(_extension, "ref")));
                    }
                }
            }
        }

        public string LoadDefaultFileSource()
        {
            string source;
            using (TextReader tr = new StreamReader(_defaultFile))
            {
                source = tr.ReadToEnd();
                tr.Close();
            }
            return source;
        }

        /// <summary>
        /// Saves the source code of the Script to a file
        /// </summary>
        /// <param name="sauce">Source code as passed to your method</param>
        /// <param name="sname">Name of the Script</param>
        public void SaveSourceToFile(string sauce, string sname)
        {
            using (TextWriter tw = new StreamWriter(_folder + "/" + sname + _extensionDot))
            {
                tw.Write(sauce);
                tw.Close();
            }
        }

        /// <summary>
        /// Saves the references that are defined for this Script to a file
        /// </summary>
        /// <param name="refs">References, as passed to your method</param>
        /// <param name="sname">Name of the Script</param>
        public void SaveRefToFile(string refs, string sname)
        {
            TextWriter t = new StreamWriter(_folder + "/" + sname + ".ref", false);
            t.Write(refs);
            t.Close();
        }

        public bool Execute(string refs, string sauce, List<GameObject> gos, string sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
        {
            return _executeDeg.Invoke(refs, sauce, gos, sname, ref addedScripts);
        }

        public bool ExecuteCSharp(string refs, string sauce, List<GameObject> gos, string sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
        {
            SaveSourceToFile(sauce, sname);
            SaveRefToFile(refs, sname);
            AddScript(sname);

            var reffs = Util.Util.splitStringAtNewline(refs);
            string finalSauce = Util.Util.getMethodsWithClass(sauce, sname, reffs);

            TextWriter tt = new StreamWriter(Application.dataPath + "/Mods/Scripts/temp.cs");
            tt.Write(finalSauce);
            tt.Close();

            var assInfo = Util.Util.Compile(new FileInfo(Application.dataPath + "/Mods/Scripts/temp.cs"), reffs, sname);
            File.Delete(Application.dataPath + "/Mods/Scripts/temp.cs");
            List<Component> added = new List<Component>();
            foreach (var go in gos)
            {
                if (go != null)
                {
                    go.AddComponent<PythonBehaviour>();
                    var python = go.GetComponent<PythonBehaviour>();
                    python.SourceCodening(assInfo, reffs);
                    if (!python.Awakening(sname))
                    {
                        Object.Destroy(python);
                    }
                    else
                    {
                        addedScripts.Add(new Tuple<string, GameObject>(sname, go), python);
                    }
                }
            }
            return true;
        }

        public bool ExecuteUnityScript(string refs, string sauce, List<GameObject> gos, string sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
        {
            SaveSourceToFile(sauce, sname);
            SaveRefToFile(refs, sname);
            AddScript(sname);

            var reffs = Util.Util.splitStringAtNewline(refs);
            string finalSauce = Util.Util.getMethodsWithClass(sauce, sname, reffs);

            TextWriter tt = new StreamWriter(Application.dataPath + "/Mods/Scripts/temp.cs");
            tt.Write(finalSauce);
            tt.Close();

            var assInfo = Util.Util.Compile(new FileInfo(Application.dataPath + "/Mods/Scripts/temp.cs"), reffs, sname);
            File.Delete(Application.dataPath + "/Mods/Scripts/temp.cs");
            List<Component> added = new List<Component>();
            foreach (var go in gos)
            {
                if (go != null)
                {
                    go.AddComponent<PythonBehaviour>();
                    var python = go.GetComponent<PythonBehaviour>();
                    python.SourceCodening(assInfo, reffs);
                    if (!python.Awakening(sname))
                    {
                        Object.Destroy(python);
                    }
                    else
                    {
                        addedScripts.Add(new Tuple<string, GameObject>(sname, go), python);
                    }
                }
            }
            return true;
        }

        public bool ExecuteLua(string refs, string sauce, List<GameObject> gos, string sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
        {
            SaveSourceToFile(sauce, sname);
            SaveRefToFile(refs, sname);

            AddScript(sname);

            foreach (var go in gos)
            {
                if (go != null)
                {
                    go.AddComponent<LuaBehaviour>();
                    var luaa = go.GetComponent<LuaBehaviour>();
                    luaa.SourceCodening(sauce);
                    if (!luaa.Awakening())
                    {
                        Object.Destroy(luaa);
                    }
                    else
                    {
                        addedScripts.Add(new Tuple<string, GameObject>(sname, go), luaa);
                    }
                }
            }
            return true;
        }

        public bool ExecutePython(string refs, string sauce, List<GameObject> gos, string sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
        {
            SaveSourceToFile(sauce, sname);
            SaveRefToFile(refs, sname);

            string[] reffs = Util.Util.splitStringAtNewline(refs);
            string finalSauce = Util.Util.getMethodsWithClassPython(sauce, sname, reffs);

            AddScript(sname);

            foreach (var go in gos)
            {
                if (go != null)
                {
                    go.AddComponent<PythonBehaviour>();
                    var python = go.GetComponent<PythonBehaviour>();
                    python.SourceCodening(finalSauce, reffs);
                    if (!python.Awakening(sname))
                    {
                        Object.Destroy(python);
                    }
                    else
                    {
                        addedScripts.Add(new Tuple<string, GameObject>(sname, go), python);
                    }
                }
            }
            return true;
        }

        public bool ExecuteBrainfuck(string refs, string sauce, List<GameObject> gos, string sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
        {
            SaveSourceToFile(sauce, sname);
            SaveRefToFile(refs, sname);

            AddScript(sname);

            var reffs = Util.Util.splitStringAtNewline(refs);
            string finalSauce = Util.Util.getMethodsWithClass(sauce, sname, reffs);

            TextWriter streamWriter = new StreamWriter(Application.dataPath + "/Mods/Scripts/temp.cs", false);
            streamWriter.Write(finalSauce);
            streamWriter.Close();

            var assInfo = Util.Util.Compile(new FileInfo(Application.dataPath + "/Mods/Scripts/temp.cs"), reffs, sname);
            File.Delete(Application.dataPath + "/Mods/Scripts/temp.cs");
            foreach (var go in gos)
            {
                if (go != null)
                {
                    go.AddComponent<PythonBehaviour>();
                    var python = go.GetComponent<PythonBehaviour>();
                    python.SourceCodening(assInfo, reffs);
                    if (!python.Awakening(sname))
                    {
                        Object.Destroy(python);
                    }
                    else
                    {
                        addedScripts.Add(new Tuple<string, GameObject>(sname, go), python);
                    }
                }
            }
            return true;
        }

        public bool ExecuteOok(string refs, string sauce, List<GameObject> gos, string sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
        {
            SaveSourceToFile(sauce, sname);
            SaveRefToFile(refs, sname);

            AddScript(sname);

            var reffs = Util.Util.splitStringAtNewline(refs);
            string finalSauce = Util.Util.getMethodsWithClass(sauce, sname, reffs);

            TextWriter streamWriter = new StreamWriter(Application.dataPath + "/Mods/Scripts/temp.cs", false);
            streamWriter.Write(finalSauce);
            streamWriter.Close();

            var assInfo = Util.Util.Compile(new FileInfo(Application.dataPath + "/Mods/Scripts/temp.cs"), reffs, sname);
            File.Delete(Application.dataPath + "/Mods/Scripts/temp.cs");
            foreach (var go in gos)
            {
                if (go != null)
                {
                    go.AddComponent<PythonBehaviour>();
                    var python = go.GetComponent<PythonBehaviour>();
                    python.SourceCodening(assInfo, reffs);
                    if (!python.Awakening(sname))
                    {
                        Object.Destroy(python);
                    }
                    else
                    {
                        addedScripts.Add(new Tuple<string, GameObject>(sname, go), python);
                    }
                }
            }
            return true;
        }

        public bool ExecuteChef(string refs, string sauce, List<GameObject> gos, string sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
        {
            SaveSourceToFile(sauce, sname);
            SaveRefToFile(refs, sname);
            AddScript(sname);

            var chef = new Chef.Chef(_folder + "/" + sname + _extensionDot);
            chef.bake();
            return true;
        }

        public bool ExecuteTrumpScript(string refs, string sauce, List<GameObject> gos, string sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
        {
            SaveSourceToFile(sauce, sname);
            SaveRefToFile(refs, sname);
            AddScript(sname);

            var reffs = Util.Util.splitStringAtNewline(refs);
            string finalSauce = Util.Util.getMethodsWithClass(sauce, sname, reffs);

            TextWriter tt = new StreamWriter(Application.dataPath + "/Mods/Scripts/temp.cs");
            tt.Write(finalSauce);
            tt.Close();

            var assInfo = Util.Util.Compile(new FileInfo(Application.dataPath + "/Mods/Scripts/temp.cs"), reffs, sname);
            File.Delete(Application.dataPath + "/Mods/Scripts/temp.cs");
            foreach (var go in gos)
            {
                if (go != null)
                {
                    go.AddComponent<PythonBehaviour>();
                    var python = go.GetComponent<PythonBehaviour>();
                    python.SourceCodening(assInfo, reffs);
                    if (!python.Awakening(sname))
                    {
                        Object.Destroy(python);
                    }
                    else
                    {
                        addedScripts.Add(new Tuple<string, GameObject>(sname, go), python);
                    }
                }
            }
            return true;
        }

        public string Convert(string Sauce, string sname, string refs)
        {
            return _convertDeg.Invoke(Sauce, sname, refs);
        }

        public string ConvertUnityScript(string Sauce, string sname, string refs)
        {
            return Util.Util.ConvertJSToC(Sauce, "UnityScriptClass");
        }

        public string ConvertBrainfuck(string Sauce, string sname, string refs)
        {
            GameObject gameObject = GameObject.Find("MortimersScriptingMod");
            var bfb = gameObject.AddComponent<BrainfuckBehaviour>();
            bfb.init(Sauce);
            return gameObject.GetComponent<ScriptHandler>().CSauce;
        }

        public string ConvertOok(string Sauce, string sname, string refs)
        {
            GameObject gameObject = GameObject.Find("MortimersScriptingMod");
            var bfb = gameObject.AddComponent<OokBehaviour>();
            bfb.init(Sauce);
            return gameObject.GetComponent<ScriptHandler>().CSauce;
        }

        public string ConvertTrumpScript(string Sauce, string sname, string refs)
        {
            return new TrumpScript.TrumpScript().Convert(Sauce);
        }

        /// <summary>
        /// Adds the Script with the defined $sname to the Dictionary 'Scripts'
        /// </summary>
        /// <param name="sname">Name of the Script</param>
        public void AddScript(string sname)
        {
            if (!Scripts.ContainsKey(sname))
            {
                Scripts.Add(sname, new Script(_folder + "/" + sname + _extensionDot, _folder + "/" + sname + ".ref"));
            }
        }

        private delegate bool ExecuteDeg(string refs, string sauce, List<GameObject> gos, string sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts);

        private delegate string ConvertDeg(string Sauce, string sname, string refs);
    }
}