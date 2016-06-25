using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BesiegeScriptingMod.Brainfuck;
using BesiegeScriptingMod.Lua;
using BesiegeScriptingMod.Ook;
using BesiegeScriptingMod.Python;
using BesiegeScriptingMod.Util;
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
        private readonly String _extensionDot;
        private delegate bool ExecuteDeg(String refs, String sauce, List<GameObject> gos, String sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts);

        private delegate String ConvertDeg(String Sauce, String sname, String refs);

        private readonly ExecuteDeg _executeDeg;
        private readonly ConvertDeg _convertDeg;

        public Language(String name, bool needsConvertion, String extension, MethodInfo executeMethod = null, MethodInfo convertMethod = null)
        {
            this.name = name;
            _folder = Application.dataPath + "/Mods/Scripts/" + name + "Scripts";
            this.needsConvertion = needsConvertion;
            this._defaultFile = Application.dataPath + "/Mods/Scripts/StandardScripts/" + name + "." + extension;
            this._extension = extension;
            _extensionDot = "." + _extension;
            if (executeMethod == null)
            {
                _executeDeg = (ExecuteDeg) Delegate.CreateDelegate(typeof (ExecuteDeg), typeof (Language).GetMethod("Execute" + name));
            }
            else
            {
                _executeDeg = (ExecuteDeg)Delegate.CreateDelegate(typeof(ExecuteDeg), executeMethod);
            }
            if (needsConvertion)
            {
                if (convertMethod != null)
                {
                    _convertDeg = (ConvertDeg) Delegate.CreateDelegate(typeof (ConvertDeg), convertMethod);
                }
                else
                {
                    _convertDeg = (ConvertDeg) Delegate.CreateDelegate(typeof (ConvertDeg), typeof (Language).GetMethod("Convert" + name));
                }
            }
            InitializeScripts();
        }

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
                                String refs = "";
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

        /// <summary>
        /// Saves the source code of the Script to a file
        /// </summary>
        /// <param name="sauce">Source code as passed to your method</param>
        /// <param name="sname">Name of the Script</param>
        public void SaveSourceToFile(String sauce, String sname)
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
        public void SaveRefToFile(String refs, String sname)
        {
            TextWriter t = new StreamWriter(_folder + "/" + sname + ".ref", false);
            t.Write(refs);
            t.Close();
        }

        public bool Execute(String refs, String sauce, List<GameObject> gos, String sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
        {
            return _executeDeg.Invoke(refs, sauce, gos, sname, ref addedScripts);
        }

        private bool ExecuteCSharp(String refs, String sauce, List<GameObject> gos, String sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
        {
            SaveSourceToFile(sauce, sname);
            SaveRefToFile(refs, sname);
            AddScript(sname);

            var reffs = Util.Util.splitStringAtNewline(refs);
            String finalSauce = Util.Util.getMethodsWithClass(sauce, sname, reffs);

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
                        MonoBehaviour.Destroy(python);
                    }
                    else
                    {
                        addedScripts.Add(new Tuple<string, GameObject>(sname, go), python);
                    }
                }
            }
            return true;
        }

        private bool ExecuteUnityScript(String refs, String sauce, List<GameObject> gos, String sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
        {
            SaveSourceToFile(sauce, sname);
            SaveRefToFile(refs, sname);
            AddScript(sname);

            var reffs = Util.Util.splitStringAtNewline(refs);
            String finalSauce = Util.Util.getMethodsWithClass(sauce, sname, reffs);

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
                        MonoBehaviour.Destroy(python);
                    }
                    else
                    {
                        addedScripts.Add(new Tuple<string, GameObject>(sname, go), python);
                    }
                }
            }
            return true;
        }

        private bool ExecuteLua(String refs, String sauce, List<GameObject> gos, String sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
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
                        MonoBehaviour.Destroy(luaa);
                    }
                    else
                    {
                        addedScripts.Add(new Tuple<string, GameObject>(sname, go), luaa);
                    }
                }
            }
            return true;
        }

        private bool ExecutePython(String refs, String sauce, List<GameObject> gos, String sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
        {
            SaveSourceToFile(sauce, sname);
            SaveRefToFile(refs, sname);

            String[] reffs = Util.Util.splitStringAtNewline(refs);
            String finalSauce = Util.Util.getMethodsWithClassPython(sauce, sname, reffs);

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
                        MonoBehaviour.Destroy(python);
                    }
                    else
                    {
                        addedScripts.Add(new Tuple<string, GameObject>(sname, go), python);
                    }
                }
            }
            return true;
        }

        private bool ExecuteBrainfuck(String refs, String sauce, List<GameObject> gos, String sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
        {
            SaveSourceToFile(sauce, sname);
            SaveRefToFile(refs, sname);

            AddScript(sname);

            var reffs = Util.Util.splitStringAtNewline(refs);
            String finalSauce = Util.Util.getMethodsWithClass(sauce, sname, reffs);

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
                        MonoBehaviour.Destroy(python);
                    }
                    else
                    {
                        addedScripts.Add(new Tuple<string, GameObject>(sname, go), python);
                    }
                }
            }
            return true;
        }

        private bool ExecuteOok(String refs, String sauce, List<GameObject> gos, String sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
        {
            SaveSourceToFile(sauce, sname);
            SaveRefToFile(refs, sname);

            AddScript(sname);

            var reffs = Util.Util.splitStringAtNewline(refs);
            String finalSauce = Util.Util.getMethodsWithClass(sauce, sname, reffs);

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
                        MonoBehaviour.Destroy(python);
                    }
                    else
                    {
                        addedScripts.Add(new Tuple<string, GameObject>(sname, go), python);
                    }
                }
            }
            return true;
        }

        private bool ExecuteChef(String refs, String sauce, List<GameObject> gos, String sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
        {
            SaveSourceToFile(sauce, sname);
            SaveRefToFile(refs, sname);
            AddScript(sname);

            var chef = new Chef.Chef(_folder + "/" + sname + _extensionDot);
            chef.bake();
            return true;
        }

        private bool ExecuteTrumpScript(String refs, String sauce, List<GameObject> gos, String sname, ref Dictionary<Tuple<string, GameObject>, Component> addedScripts)
        {
            SaveSourceToFile(sauce, sname);
            SaveRefToFile(refs, sname);
            AddScript(sname);

            var reffs = Util.Util.splitStringAtNewline(refs);
            String finalSauce = Util.Util.getMethodsWithClass(sauce, sname, reffs);

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
                        MonoBehaviour.Destroy(python);
                    }
                    else
                    {
                        addedScripts.Add(new Tuple<string, GameObject>(sname, go), python);
                    }
                }
            }
            return true;
        }

        public String Convert(String Sauce, String sname, String refs)
        {
            return _convertDeg.Invoke(Sauce, sname, refs);
        }

        private String ConvertUnityScript(String Sauce, String sname, String refs)
        {
            return Util.Util.ConvertJSToC(Sauce, "UnityScriptClass");
        }

        private String ConvertBrainfuck(String Sauce, String sname, String refs)
        {
            GameObject gameObject = GameObject.Find("MortimersScriptingMod");
            var bfb = gameObject.AddComponent<BrainfuckBehaviour>();
            bfb.init(Sauce);
            return gameObject.GetComponent<ScriptHandler>()._cSauce;
        }

        private String ConvertOok(String Sauce, String sname, String refs)
        {
            GameObject gameObject = GameObject.Find("MortimersScriptingMod");
            var bfb = gameObject.AddComponent<OokBehaviour>();
            bfb.init(Sauce);
            return gameObject.GetComponent<ScriptHandler>()._cSauce;
        }

        private String ConvertTrumpScript(String Sauce, String sname, String refs)
        {
            return new TrumpScript.TrumpScript().Convert(Sauce);
        }

        /// <summary>
        /// Adds the Script with the defined $sname to the Dictionary 'Scripts'
        /// </summary>
        /// <param name="sname">Name of the Script</param>
        public void AddScript(String sname)
        {
            if (!Scripts.ContainsKey(sname))
            {
                Scripts.Add(sname, new Script(_folder + "/" + sname + _extensionDot, _folder + "/" + sname + ".ref"));
            }
        }
    }
}
