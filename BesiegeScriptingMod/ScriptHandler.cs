using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using spaar.ModLoader;
using spaar.ModLoader.UI;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BesiegeScriptingMod
{
    internal class ScriptHandler : MonoBehaviour
    {
        private readonly int _winId = spaar.ModLoader.Util.GetWindowID();
        private readonly int chooseWinID = spaar.ModLoader.Util.GetWindowID();
        private Vector2 _areaLoc1;
        private string _name = "";
        private Vector2 _refPos;
        private Vector2 _scrollPos = new Vector2(0, Mathf.Infinity);
        private Vector2 _windowLoc = new Vector2(Screen.width - 400.0f, Screen.height - 100.0f);
        private Rect _winRect;
        private bool _addingRefs;
        public readonly Dictionary<string, Script> BrainfuckScripts = new Dictionary<string, Script>();
        public readonly Dictionary<string, Script> ChefScripts = new Dictionary<string, Script>();
        public readonly Dictionary<string, Script> JavaScriptScripts = new Dictionary<string, Script>();
        public readonly Dictionary<string, Script> CSharpScripts = new Dictionary<string, Script>();
        public readonly Dictionary<string, Script> LuaScripts = new Dictionary<string, Script>();
        public readonly Dictionary<string, Script> PythonScripts = new Dictionary<string, Script>();
        public readonly Dictionary<string, Script> OokScripts = new Dictionary<string, Script>();
        private readonly Dictionary<GameObject, Color> _gos = new Dictionary<GameObject, Color>();

        public readonly Dictionary<Tuple<string, GameObject>, Component> AddedScripts =
            new Dictionary<Tuple<string, GameObject>, Component>();

        private string _cSauce = "";
        private bool _displayC;
        public string Ide = "";
        private String lastIDE = "";
        private bool _ideSelection;
        private bool _isLoading;
        private bool _isOpen;
        private bool _stopScript;
        private bool _chooseObject;
        private bool _showObjects;
        private bool showGOOptions;
        private bool cs;
        private bool js;
        private bool py;
        private bool lua;
        private bool bf;
        private bool ook;
        private string _refs;
        private string _sauce = "";
        private GUIStyle _toggleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _headlineStyle;
        private Key Key;
        private Key selecting;

        public void Start()
        {
            _gos.Add(gameObject, Color.clear);
            _refs += Application.dataPath + "/Mods/SpaarModLoader.dll" + Util.getNewLine();
            _refs += Application.dataPath + "/Managed/Assembly-UnityScript.dll" + Util.getNewLine();
            _refs += Application.dataPath + "/Managed/Assembly-UnityScript-firstpass.dll" + Util.getNewLine();
            _refs += Application.dataPath + "/Managed/Assembly-CSharp.dll" + Util.getNewLine();
            _refs += Application.dataPath + "/Managed/Assembly-CSharp-firstpass.dll" + Util.getNewLine();
            _refs += Application.dataPath + "/Managed/UnityEngine.dll" + Util.getNewLine();
            _refs += Application.dataPath + "/Managed/System.dll";

            _winRect = new Rect(100.0f, 20.0f, _windowLoc.x, _windowLoc.y);
            if (!Directory.Exists(Application.dataPath + "/Mods/Scripts"))
            {
                Debug.Log("[ScriptingMod]:Installation incomplete!");
            }
            else
            {
                var di = new DirectoryInfo(Application.dataPath + "/Mods/Scripts");

                #region Python
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/PythonScripts"))
                {
                    foreach (var info in di.GetDirectories().First(info => info.Name.Equals("PythonScripts")).GetFiles())
                    {
                        if (info.Extension.Equals(".py"))
                        {
                            if (!File.Exists(Application.dataPath + "/Mods/Scripts/PythonScripts/" + info.Name.Replace(".py", "") + ".ref"))
                            {
                                TextWriter tw = File.CreateText(Application.dataPath + "/Mods/Scripts/PythonScripts/" + info.Name.Replace(".py", "") + ".ref");
                                tw.Write(_refs);
                                tw.Close();
                            }
                            PythonScripts.Add(info.Name.Replace(".py", ""),
                                new Script(info.FullName,
                                    Application.dataPath + "/Mods/Scripts/PythonScripts/" +
                                    info.Name.Replace(".py", "") +
                                    ".ref"));
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(Application.dataPath + "/Mods/Scripts/PythonScripts");
                }
                #endregion

                #region Lua
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/LuaScripts"))
                {
                    foreach (
                        var fileInfo in
                            di.GetDirectories().First(info => info.Name.Equals("LuaScripts")).GetFiles())
                    {
                        if (fileInfo.Extension.Equals(".lua"))
                        {
                            if (!File.Exists(Application.dataPath + "/Mods/Scripts/LuaScripts/" + fileInfo.Name.Replace(".lua", "") + ".ref"))
                            {
                                TextWriter tw = File.CreateText(Application.dataPath + "/Mods/Scripts/LuaScripts/" + fileInfo.Name.Replace(".lua", "") + ".ref");
                                tw.Write(_refs);
                                tw.Close();
                            }
                            LuaScripts.Add(fileInfo.Name.Replace(".lua", ""),
                                new Script(fileInfo.FullName,
                                    Application.dataPath + "/Mods/Scripts/LuaScripts/" +
                                    fileInfo.Name.Replace(".lua", "") +
                                    ".ref"));
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(Application.dataPath + "/Mods/Scripts/LuaScripts");
                }
                #endregion

                #region Brainfuck
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/BrainfuckScripts"))
                {
                    foreach (
                        var fileInfo in
                            di.GetDirectories().First(info => info.Name.Equals("BrainfuckScripts")).GetFiles())
                    {
                        if (fileInfo.Extension.Equals(".bf"))
                        {
                            if (!File.Exists(Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + fileInfo.Name.Replace(".bf", "") + ".ref"))
                            {
                                TextWriter tw = File.CreateText(Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + fileInfo.Name.Replace(".bf", "") + ".ref");
                                tw.Write(_refs);
                                tw.Close();
                            }
                            BrainfuckScripts.Add(fileInfo.Name.Replace(".bf", ""),
                                new Script(fileInfo.FullName,
                                    Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" +
                                    fileInfo.Name.Replace(".bf", "") +
                                    ".ref"));
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(Application.dataPath + "/Mods/Scripts/BrainfuckScripts");
                }
                #endregion

                #region Ook
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/OokScripts"))
                {
                    foreach (
                        var fileInfo in
                            di.GetDirectories().First(info => info.Name.Equals("OokScripts")).GetFiles())
                    {
                        if (fileInfo.Extension.Equals(".ook"))
                        {
                            if (!File.Exists(Application.dataPath + "/Mods/Scripts/OokScripts/" + fileInfo.Name.Replace(".ook", "") + ".ref"))
                            {
                                TextWriter tw = File.CreateText(Application.dataPath + "/Mods/Scripts/OokScripts/" + fileInfo.Name.Replace(".ook", "") + ".ref");
                                tw.Write(_refs);
                                tw.Close();
                            }
                            OokScripts.Add(fileInfo.Name.Replace(".ook", ""),
                                new Script(fileInfo.FullName,
                                    Application.dataPath + "/Mods/Scripts/OokScripts/" +
                                    fileInfo.Name.Replace(".ook", "") +
                                    ".ref"));
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(Application.dataPath + "/Mods/Scripts/OokScripts");
                }
                #endregion

                #region CSharp
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/CSharpScripts"))
                {
                    var dii = new DirectoryInfo(Application.dataPath + "/Mods/Scripts/CSharpScripts");
                    foreach (var fileInfo in dii.GetFiles())
                    {
                        if (fileInfo.Extension.Equals(".cs"))
                        {
                            if (!File.Exists(Application.dataPath + "/Mods/Scripts/CSharpScripts/" + fileInfo.Name.Replace(".cs", "") + ".ref"))
                            {
                                TextWriter tw = File.CreateText(Application.dataPath + "/Mods/Scripts/CSharpScripts/" + fileInfo.Name.Replace(".cs", "") + ".ref");
                                tw.Write(_refs);
                                tw.Close();
                            }
                            CSharpScripts.Add(fileInfo.Name.Replace(".cs", ""),
                                new Script(fileInfo.FullName,
                                    Application.dataPath + "/Mods/Scripts/CSharpScripts/" +
                                    fileInfo.Name.Replace(".cs", "") +
                                    ".ref"));
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(Application.dataPath + "/Mods/Scripts/CSharpScripts");
                }
                #endregion

                #region Chef
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/ChefScripts"))
                {
                    foreach (
                        var fileInfo in
                            di.GetDirectories().First(info => info.Name.Equals("ChefScripts")).GetFiles())
                    {
                        if (fileInfo.Extension.Equals(".recipe"))
                        {
                            if (!File.Exists(Application.dataPath + "/Mods/Scripts/ChefScripts/" + fileInfo.Name.Replace(".recipe", "") + ".ref"))
                            {
                                TextWriter tw = File.CreateText(Application.dataPath + "/Mods/Scripts/ChefScripts/" + fileInfo.Name.Replace(".recipe", "") + ".ref");
                                tw.Write(_refs);
                                tw.Close();
                            }
                            ChefScripts.Add(fileInfo.Name.Replace(".recipe", ""),
                                new Script(fileInfo.FullName,
                                    Application.dataPath + "/Mods/Scripts/ChefScripts/" +
                                    fileInfo.Name.Replace(".recipe", "") +
                                    ".ref"));
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(Application.dataPath + "/Mods/Scripts/ChefScripts");
                }
                #endregion

                #region JavaScript
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/JavaScriptScripts"))
                {
                    foreach (
                        var fileInfo in
                            di.GetDirectories().First(info => info.Name.Equals("JavaScriptScripts")).GetFiles())
                    {
                        if (fileInfo.Extension.Equals(".js"))
                        {
                            if (!File.Exists(Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + fileInfo.Name.Replace(".js", "") + ".ref"))
                            {
                                TextWriter tw = File.CreateText(Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + fileInfo.Name.Replace(".js", "") + ".ref");
                                tw.Write(_refs);
                                tw.Close();
                            }
                            JavaScriptScripts.Add(fileInfo.Name.Replace(".js", ""),
                                new Script(fileInfo.FullName,
                                    Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" +
                                    fileInfo.Name.Replace(".js", "") +
                                    ".ref"));
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(Application.dataPath + "/Mods/Scripts/JavaScriptScripts");
                }
                #endregion
            }
            _areaLoc1 = new Vector2(_windowLoc.x - _windowLoc.x/100.0f, _windowLoc.y/2/2);
            UpdateSauce();
        }

        public void SetKeys(Key key, Key key2)
        {
            Key = key;
            selecting = key2;
        }

        public void OnGUI()
        {
            if (_isOpen)
            {
                if (Event.current.keyCode == KeyCode.Return)
                {
                    _scrollPos.y = Mathf.Infinity;
                }
                GUI.skin = ModGUI.Skin;
                _toggleStyle = new GUIStyle(GUI.skin.button) {onNormal = Elements.Buttons.Red.hover};

                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    margin =
                        new RectOffset(left: 10, right: 5, top: GUI.skin.textField.margin.top,
                            bottom: GUI.skin.textField.margin.bottom),
                    border = GUI.skin.textField.border,
                    alignment = GUI.skin.textField.alignment
                };
                _headlineStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleLeft,
                    border = _labelStyle.border,
                    fontStyle = FontStyle.Bold
                };

                _winRect = GUILayout.Window(_winId, _winRect,
                    Func,
                    "Scripting Mod by Mortimer");
            }
            else if (_chooseObject)
            {
                GUI.skin = ModGUI.Skin;
                _headlineStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleLeft,
                    border = _labelStyle.border,
                    fontStyle = FontStyle.Bold
                };
                GUILayout.Window(chooseWinID, new Rect(0, 20.0f, 100.0f, 100.0f), Func2, "Choosing Objects");
            }
        }

        private void Func2(int id)
        {
            GUILayout.Label("Choosing GameObjects. Press " + Key.Modifier + " + " + Key.Trigger +
                            " to confirm selection.", _headlineStyle);
        }

        private void Func(int id)
        {
            //GUILayout.BeginArea(new Rect(_windowLoc.x/100.0f, _windowLoc.x/100.0f, _areaLoc1.x, _areaLoc1.y));

            #region Buttons

            #region IDE

            GUILayout.BeginHorizontal(GUI.skin.box);
            _ideSelection = GUILayout.Toggle(_ideSelection, "IDE", _toggleStyle);
            if (_ideSelection)
            {
                lastIDE = Ide;
                cs = GUILayout.Toggle(cs, "CSharp", _toggleStyle);
                if (cs)
                {
                    js = false;
                    py = false;
                    lua = false;
                    bf = false;
                    ook = false;
                    Ide = "CSharp";
                }
                py = GUILayout.Toggle(py, "Python", _toggleStyle);
                if (py)
                {
                    cs = false;
                    js = false;
                    lua = false;
                    bf = false;
                    ook = false;
                    Ide = "Python";
                }
                js = GUILayout.Toggle(js, "UnityScript", _toggleStyle);
                if (js)
                {
                    cs = false;
                    py = false;
                    lua = false;
                    bf = false;
                    ook = false;
                    Ide = "JavaScript";
                }
                lua = GUILayout.Toggle(lua, "Lua", _toggleStyle);
                if (lua)
                {
                    cs = false;
                    py = false;
                    js = false;
                    bf = false;
                    ook = false;
                    Ide = "Lua";
                }
                bf = GUILayout.Toggle(bf, "Brainfuck", _toggleStyle);
                if (bf)
                {
                    cs = false;
                    py = false;
                    js = false;
                    lua = false;
                    ook = false;
                    Ide = "Brainfuck";
                }
                ook = GUILayout.Toggle(ook, "Ook", _toggleStyle);
                if (ook)
                {
                    cs = false;
                    py = false;
                    js = false;
                    lua = false;
                    bf = false;
                    Ide = "Ook";
                }
                if (!lastIDE.Equals(Ide))
                {
                    UpdateSauce();
                    _ideSelection = false;
                }
                /*
                if (GUILayout.Button("Java"))
                {
                    Ide = "Java";
                }
                if (GUILayout.Button("Chef"))
                {
                    Ide = "Chef";
                }
                */
            }
            GUILayout.EndHorizontal();

            #endregion

            #region GOOptions

            GUILayout.BeginHorizontal(GUI.skin.box);
            showGOOptions = GUILayout.Toggle(showGOOptions, "GameObject Options", _toggleStyle);
            if (showGOOptions)
            {
                _chooseObject = GUILayout.Toggle(_chooseObject, "Choose GameObject", _toggleStyle);
                if (_chooseObject)
                {
                    _isOpen = false;
                }
                _showObjects = GUILayout.Toggle(_showObjects, "Show selected GOs", _toggleStyle);
                if (GUILayout.Button("Add Default GO"))
                {
                    if (!_gos.Keys.Contains(gameObject))
                    {
                        _gos.Add(gameObject, Color.black);
                    }
                }
            }
            GUILayout.EndHorizontal();

            #endregion

            GUILayout.BeginHorizontal(GUI.skin.box);

            #region Execute

            if (GUILayout.Button("Execute"))
            {
                Execution();
            }

            #endregion Execute

            #region Convert

            if (!Ide.Equals("CSharp") && !Ide.Equals("Lua") && !Ide.Equals("Python") && !Ide.Equals("Chef") &&
                !Ide.Equals("Java"))
            {
                if (GUILayout.Button("Convert"))
                {
                    Convertion();
                }
                _displayC = GUILayout.Toggle(_displayC, "Display C# Source", _toggleStyle);
            }

            #endregion

            //var last = _isSaving;
            //_isSaving = GUILayout.Toggle(_isSaving, "Save", GUI.skin.button);

            _stopScript = GUILayout.Toggle(_stopScript, "Stop Script", _toggleStyle);
            if (_stopScript)
            {
                _isLoading = false;
                _addingRefs = false;
                _chooseObject = false;
            }

            _isLoading = GUILayout.Toggle(_isLoading, "Load", _toggleStyle);

            if (_isLoading)
            {
                _stopScript = false;
                _addingRefs = false;
                _chooseObject = false;
            }

            if (!Ide.Equals("Lua"))
            {
                _addingRefs = GUILayout.Toggle(_addingRefs, "Add references", _toggleStyle);
            }

            _isOpen = GUILayout.Toggle(_isOpen, "Open/Close Editor", _toggleStyle);
            //GUILayout.EndArea();

            GUILayout.EndHorizontal();

            #endregion

            if (!_addingRefs && !_isLoading && !_stopScript && !_chooseObject && !_showObjects)
            {
                GUILayout.Label("Source Code Editor", _headlineStyle);
                _scrollPos = GUILayout.BeginScrollView(_scrollPos);
                if (_displayC)
                {
                    _cSauce = GUILayout.TextArea(_cSauce);
                }
                else
                {
                    _sauce = GUILayout.TextArea(_sauce);
                }
                GUILayout.EndScrollView();
            }
            else if (_addingRefs)
            {
                GUILayout.Label("Reference Editor", _headlineStyle);
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label("Enter the name of your script: ", _labelStyle, GUILayout.Width(200.0f));
                _name = GUILayout.TextField(_name, GUILayout.Width(100.0f));
                GUILayout.EndHorizontal();
                _refPos = GUILayout.BeginScrollView(_refPos);
                _refs = GUILayout.TextArea(_refs);
                GUILayout.EndScrollView();
            }

            else if (_stopScript)
            {
                GUILayout.Label("Stop Scripts", _headlineStyle);
                _scrollPos = GUILayout.BeginScrollView(_scrollPos);
                if (AddedScripts.Count > 0)
                {
                    List<Tuple<String, GameObject>> list = new List<Tuple<String, GameObject>>();
                    for (int i = 0; i < AddedScripts.Count; i++)
                    {
                        if (AddedScripts.Values.ElementAt(i) == null) continue;
                        if (
                            GUILayout.Button(AddedScripts.Keys.ElementAt(i).First + ", GameObject:" +
                                             AddedScripts.Keys.ElementAt(i).Second.name))
                        {
                            Destroy(AddedScripts.Values.ElementAt(i));
                            list.Add(AddedScripts.Keys.ElementAt(i));
                        }
                    }
                    if (list.Count > 0)
                    {
                        foreach (Tuple<string, GameObject> tuple in list)
                        {
                            AddedScripts.Remove(tuple);
                        }
                    }
                }
                GUILayout.EndScrollView();
            }

            else if (_showObjects)
            {
                GUILayout.Label("Selected GameObjects", _headlineStyle);
                _scrollPos = GUILayout.BeginScrollView(_scrollPos);
                if (_gos.Count > 0)
                {
                    List<GameObject> goss =
                        _gos.Keys.Where(go => GUILayout.Button("Name: " + go.name + ", Tag: " + go.tag)).ToList();
                    foreach (GameObject o in goss)
                    {
                        _gos.Remove(o);
                    }
                }
                else
                {
                    GUILayout.Label("No GameObjects Selected!");
                }
                GUILayout.EndScrollView();
            }

                #region saving[OBSOLETE]
            /*
            else if (!_addingRefs && _isSaving && !_isLoading)
            {
                _scrollPos = GUILayout.BeginScrollView(_scrollPos);
                foreach (
                    var fileInfo in
                        CSharpScripts.Where(
                            fileInfo =>
                                fileInfo.Value.sourceFile.Contains("TempScripts") && GUILayout.Button(fileInfo.Key)))
                {
                    File.Copy(fileInfo.Value.sourceFile,
                        Application.dataPath + "/Mods/Scripts/CSharpScripts/" + fileInfo.Key + ".cs", true);
                    File.Copy(fileInfo.Value.refFile,
                        Application.dataPath + "/Mods/Scripts/CSharpScripts/" + fileInfo.Key + ".ref", true);
                }
                foreach (
                    var fileInfo in
                        JavaScriptScripts.Where(
                            fileInfo =>
                                fileInfo.Value.sourceFile.Contains("TempScripts") && GUILayout.Button(fileInfo.Key)))
                {
                    File.Copy(fileInfo.Value.sourceFile,
                        Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + fileInfo.Key + ".js", true);
                    File.Copy(fileInfo.Value.refFile,
                        Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + fileInfo.Key + ".ref", true);
                }
                foreach (
                    var fileInfo in
                        LuaScripts.Where(
                            fileInfo =>
                                fileInfo.Value.sourceFile.Contains("TempScripts") && GUILayout.Button(fileInfo.Key)))
                {
                    File.Copy(fileInfo.Value.sourceFile,
                        Application.dataPath + "/Mods/Scripts/LuaScripts/" + fileInfo.Key + ".lua", true);
                    File.Copy(fileInfo.Value.refFile,
                        Application.dataPath + "/Mods/Scripts/LuaScripts/" + fileInfo.Key + ".ref", true);
                }
                foreach (
                    var fileInfo in
                        PythonScripts.Where(
                            fileInfo =>
                                fileInfo.Value.sourceFile.Contains("TempScripts") && GUILayout.Button(fileInfo.Key)))
                {
                    File.Copy(fileInfo.Value.sourceFile,
                        Application.dataPath + "/Mods/Scripts/PythonScripts/" + fileInfo.Key + ".py", true);
                    File.Copy(fileInfo.Value.refFile,
                        Application.dataPath + "/Mods/Scripts/PythonScripts/" + fileInfo.Key + ".ref", true);
                }
                foreach (
                    var fileInfo in
                        BrainfuckScripts.Where(
                            fileInfo =>
                                fileInfo.Value.sourceFile.Contains("TempScripts") && GUILayout.Button(fileInfo.Key)))
                {
                    File.Copy(fileInfo.Value.sourceFile,
                        Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + fileInfo.Key + ".bf", true);
                    File.Copy(fileInfo.Value.refFile,
                        Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + fileInfo.Key + ".ref", true);
                }
                foreach (
                    var fileInfo in
                        ChefScripts.Where(
                            fileInfo =>
                                fileInfo.Value.sourceFile.Contains("TempScripts") && GUILayout.Button(fileInfo.Key)))
                {
                    File.Copy(fileInfo.Value.sourceFile,
                        Application.dataPath + "/Mods/Scripts/ChefScripts/" + fileInfo.Key + ".recipe", true);
                    File.Copy(fileInfo.Value.refFile,
                        Application.dataPath + "/Mods/Scripts/ChefScripts/" + fileInfo.Key + ".ref", true);
                }
                GUILayout.EndScrollView();
            }
            */
                #endregion

                #region loading

            else if (_isLoading)
            {
                _scrollPos = GUILayout.BeginScrollView(_scrollPos);
                switch (Ide)
                {
                    case "CSharp":
                    {
                        foreach (var cSharpScript in CSharpScripts)
                        {
                            if (GUILayout.Button(cSharpScript.Key))
                            {
                                Loadtion(cSharpScript);
                            }
                        }
                        break;
                    }
                    case "Lua":
                    {
                        foreach (var cSharpScript in LuaScripts)
                        {
                            if (GUILayout.Button(cSharpScript.Key))
                            {
                                Loadtion(cSharpScript);
                            }
                        }
                        break;
                    }
                    case "Python":
                    {
                        foreach (var cSharpScript in PythonScripts)
                        {
                            if (GUILayout.Button(cSharpScript.Key))
                            {
                                Loadtion(cSharpScript);
                            }
                        }
                        break;
                    }
                    case "JavaScript":
                    {
                        foreach (var cSharpScript in JavaScriptScripts)
                        {
                            if (GUILayout.Button(cSharpScript.Key))
                            {
                                Loadtion(cSharpScript);
                            }
                        }
                        break;
                    }
                    case "Brainfuck":
                    {
                        foreach (var cSharpScript in BrainfuckScripts)
                        {
                            if (GUILayout.Button(cSharpScript.Key))
                            {
                                Loadtion(cSharpScript);
                            }
                        }
                        break;
                    }
                    case "Chef":
                    {
                        foreach (var cSharpScript in ChefScripts)
                        {
                            if (GUILayout.Button(cSharpScript.Key))
                            {
                                Loadtion(cSharpScript);
                            }
                        }
                        break;
                    }
                    case "Ook":
                    {
                        foreach (var cSharpScript in OokScripts)
                        {
                            if (GUILayout.Button(cSharpScript.Key))
                            {
                                Loadtion(cSharpScript);
                            }
                        }
                        break;
                    }
                }
                GUILayout.EndScrollView();
            }

            #endregion

            GUI.DragWindow();
        }

        public void Update()
        {
            if (Key.Pressed())
                _isOpen = !_isOpen;
            if (!_chooseObject) return;
            if (selecting.Pressed())
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null)
                    {
                        GameObject go = hit.transform.parent.gameObject;
                        if (_gos.ContainsKey(go))
                        {
                            _gos.Remove(go);
                        }
                        else
                        {
                            _gos.Add(go,
                                go.GetComponent<Renderer>()
                                    ? go.GetComponent<Renderer>().material.color
                                    : Color.clear);
                        }
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.Mouse2))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null)
                    {
                        if (Input.GetKey(KeyCode.LeftControl))
                        {
                            GameObject go = hit.transform.parent.gameObject;
                            _gos.Add(go,
                                go.GetComponent<Renderer>()
                                    ? go.GetComponent<Renderer>().material.color
                                    : Color.clear);
                        }
                        else
                        {
                            _gos.Clear();
                            GameObject go = hit.transform.parent.gameObject;
                            _gos.Add(go,
                                go.GetComponent<Renderer>()
                                    ? go.GetComponent<Renderer>().material.color
                                    : Color.clear);
                        }
                    }
                }
            }
            if (_gos.Count > 0)
            {
                foreach (KeyValuePair<GameObject, Color> go in _gos.Where(go => !go.Value.Equals(Color.clear)))
                {
                    go.Key.GetComponent<Renderer>().material.color = Color.red;
                }
            }
            if (Key.Pressed())
            {
                if (_gos.Count > 0)
                {
                    foreach (KeyValuePair<GameObject, Color> go in _gos.Where(go => !go.Value.Equals(Color.clear)))
                    {
                        go.Key.GetComponent<Renderer>().material.color = go.Value;
                    }
                }
                _chooseObject = false;
                _isOpen = true;
            }
        }

        public void OnUnload()
        {
            if (AddedScripts.Count > 0)
            {
                List<Tuple<String, GameObject>> list = new List<Tuple<String, GameObject>>();
                for (int i = 0; i < AddedScripts.Count; i++)
                {
                    if (AddedScripts.Values.ElementAt(i) != null)
                    {
                        Destroy(AddedScripts.Values.ElementAt(i));
                        list.Add(AddedScripts.Keys.ElementAt(i));
                    }
                }
                if (list.Count > 0)
                {
                    foreach (Tuple<string, GameObject> tuple in list)
                    {
                        AddedScripts.Remove(tuple);
                    }
                }
            }
        }

        public void Execution()
        {
            if (_name.Equals(""))
            {
                Debug.LogError("No name specified!");
            }
            if (AddedScripts.Count > 0)
            {
                List<Tuple<String, GameObject>> list = new List<Tuple<string, GameObject>>();
                for (int i = 0; i < AddedScripts.Count; i++)
                {
                    if (AddedScripts.Values.ElementAt(i) != null)
                    {
                        if (AddedScripts.Keys.ElementAt(i).First.Equals(_name))
                        {
                            Destroy(AddedScripts.Values.ElementAt(i));
                            list.Add(AddedScripts.Keys.ElementAt(i));
                        }
                    }
                }
                if (list.Count > 0)
                {
                    foreach (Tuple<string, GameObject> tuple in list)
                    {
                        AddedScripts.Remove(tuple);
                    }
                }
            }
            switch (Ide)
            {
                    #region CSharp

                case "CSharp":
                {
                    var reffs = Util.splitStringAtNewline(_refs);
                    String finalSauce = Util.getMethodsWithClass(_sauce, _name, reffs);
                    TextWriter tw =
                        new StreamWriter(Application.dataPath + "/Mods/Scripts/CSharpScripts/" + _name + ".cs",
                            false);
                    tw.Write(_sauce);
                    tw.Close();
                    TextWriter tt = new StreamWriter(Application.dataPath + "/Mods/Scripts/temp.cs");
                    tt.Write(finalSauce);
                    tt.Close();
                    TextWriter t =
                        new StreamWriter(Application.dataPath + "/Mods/Scripts/CSharpScripts/" + _name + ".ref",
                            false);
                    t.Write(_refs);
                    t.Close();

                    if (!CSharpScripts.ContainsKey(_name))
                    {
                        CSharpScripts.Add(_name,
                            new Script(Application.dataPath + "/Mods/Scripts/CSharpScripts/" + _name + ".cs",
                                Application.dataPath + "/Mods/Scripts/CSharpScripts/" + _name + ".ref"));
                    }
                    var assInfo = Util.Compile(new FileInfo(Application.dataPath + "/Mods/Scripts/temp.cs"), reffs,
                        _name);
                    File.Delete(Application.dataPath + "/Mods/Scripts/temp.cs");
                    String assinfoSauce = "";
                    using (TextReader tr = new FileInfo(assInfo).OpenText())
                    {
                        assinfoSauce = tr.ReadToEnd();
                    }
                    foreach (var go in _gos.Keys)
                    {
                        if (go != null)
                        {
                            go.AddComponent<PythonBehaviour>();
                            var python = go.GetComponent<PythonBehaviour>();
                            python.SourceCodening(assinfoSauce, reffs);
                            python.Awakening(_name);
                            AddedScripts.Add(new Tuple<string, GameObject>(_name, go), python);
                        }
                    }
                    File.Delete(assInfo);
                    break;
                }

                    #endregion

                    #region Lua

                case "Lua":
                {
                    TextWriter tw =
                        new StreamWriter(Application.dataPath + "/Mods/Scripts/LuaScripts/" + _name + ".lua",
                            false);
                    tw.Write(_sauce);
                    tw.Close();

                    TextWriter t =
                        new StreamWriter(Application.dataPath + "/Mods/Scripts/LuaScripts/" + _name + ".ref", false);
                    t.Write(_refs);
                    t.Close();

                    if (!LuaScripts.ContainsKey(_name))
                    {
                        LuaScripts.Add(_name,
                            new Script(Application.dataPath + "/Mods/Scripts/LuaScripts/" + _name + ".lua",
                                Application.dataPath + "/Mods/Scripts/LuaScripts/" + _name + ".ref"));
                    }

                    foreach (var go in _gos.Keys)
                    {
                        if (go != null)
                        {
                            go.AddComponent<PythonBehaviour>();
                            var python = go.GetComponent<PythonBehaviour>();
                            python.SourceCodening(_sauce, Util.splitStringAtNewline(_refs));
                            python.Awakening(_name);
                            AddedScripts.Add(new Tuple<string, GameObject>(_name, go), python);
                        }
                    }
                    break;
                }

                    #endregion

                    #region Python

                case "Python":
                {
                    String[] refs = Util.splitStringAtNewline(_refs);
                    String finalSauce = Util.getMethodsWithClassPython(_sauce, _name, refs);

                    TextWriter tw =
                        new StreamWriter(Application.dataPath + "/Mods/Scripts/PythonScripts/" + _name + ".py",
                            false);
                    tw.Write(_sauce);
                    tw.Close();


                    TextWriter t =
                        new StreamWriter(Application.dataPath + "/Mods/Scripts/PythonScripts/" + _name + ".ref",
                            false);
                    t.Write(_refs);
                    t.Close();

                    if (!PythonScripts.ContainsKey(_name))
                    {
                        PythonScripts.Add(_name,
                            new Script(Application.dataPath + "/Mods/Scripts/PythonScripts/" + _name + ".py",
                                Application.dataPath + "/Mods/Scripts/PythonScripts/" + _name + ".ref"));
                    }
                    String[] reffs = Util.splitStringAtNewline(_refs);
                    foreach (var go in _gos.Keys)
                    {
                        if (go != null)
                        {
                            go.AddComponent<PythonBehaviour>();
                            var python = go.GetComponent<PythonBehaviour>();
                            python.SourceCodening(finalSauce, reffs);
                            python.Awakening(_name);
                            AddedScripts.Add(new Tuple<string, GameObject>(_name, go), python);
                        }
                    }
                    break;
                }

                    #endregion

                    #region JS

                case "JavaScript":
                {
                    if (!_cSauce.Equals(""))
                    {
                        TextWriter tw =
                            new StreamWriter(
                                Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + _name + ".js",
                                false);
                        tw.Write(_sauce);
                        tw.Close();

                        TextWriter t =
                            new StreamWriter(
                                Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + _name + ".ref",
                                false);
                        t.Write(_refs);
                        t.Close();

                        if (!JavaScriptScripts.ContainsKey(_name))
                        {
                            JavaScriptScripts.Add(_name,
                                new Script(
                                    Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + _name + ".js",
                                    Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + _name + ".ref"));
                        }

                        if (File.Exists(Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + _name + ".dll"))
                        {
                            File.Delete(Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + _name + ".dll");
                        }
                        var reffs = Util.splitStringAtNewline(_refs);
                        String finalSauce = Util.getMethodsWithClass(_cSauce, _name, reffs);
                        TextWriter ts =
                            new StreamWriter(
                                Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + _name + ".cs",
                                false);
                        tw.Write(finalSauce);
                        tw.Close();

                        File.Delete(Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + _name + ".cs");

                        var assInfo =
                            Util.Compile(
                                new FileInfo(Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + _name +
                                             ".cs"),
                                reffs, _name);
                        String assinfoSauce = "";
                        using (TextReader tr = new FileInfo(assInfo).OpenText())
                        {
                            assinfoSauce = tr.ReadToEnd();
                        }
                        foreach (var go in _gos.Keys)
                        {
                            if (go != null)
                            {
                                go.AddComponent<PythonBehaviour>();
                                var python = go.GetComponent<PythonBehaviour>();
                                python.SourceCodening(assinfoSauce, reffs);
                                python.Awakening(_name);
                                AddedScripts.Add(new Tuple<string, GameObject>(_name, go), python);
                            }
                        }
                        File.Delete(assInfo);
                    }
                    else
                    {
                        Debug.LogError("Please convert your source before executing it!");
                    }
                    break;
                }

                    #endregion

                    #region BF

                case "Brainfuck":
                {
                    if (!_cSauce.Equals(""))
                    {
                        TextWriter tw =
                            new StreamWriter(
                                Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + _name + ".bf",
                                false);
                        tw.Write(_sauce);
                        tw.Close();
                        TextWriter t =
                            new StreamWriter(Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + _name + ".ref",
                                false);
                        t.Write(_refs);
                        t.Close();
                        if (!BrainfuckScripts.ContainsKey(_name))
                        {
                            BrainfuckScripts.Add(_name,
                                new Script(
                                    Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + _name + ".bf",
                                    Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + _name + ".ref"));
                        }
                        var reffs = Util.splitStringAtNewline(_refs);
                        String finalSauce = Util.getMethodsWithClass(_cSauce, _name, reffs);
                        TextWriter ts =
                            new StreamWriter(
                                Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + _name + ".cs",
                                false);
                        tw.Write(finalSauce);
                        tw.Close();
                        var assInfo =
                            Util.Compile(
                                new FileInfo(Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + _name +
                                             ".cs"),
                                reffs, _name);
                        File.Delete(Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + _name + ".cs");
                        String assinfoSauce = "";
                        using (TextReader tr = new FileInfo(assInfo).OpenText())
                        {
                            assinfoSauce = tr.ReadToEnd();
                        }
                        foreach (var go in _gos.Keys)
                        {
                            if (go != null)
                            {
                                go.AddComponent<PythonBehaviour>();
                                var python = go.GetComponent<PythonBehaviour>();
                                python.SourceCodening(assinfoSauce, reffs);
                                python.Awakening(_name);
                                AddedScripts.Add(new Tuple<string, GameObject>(_name, go), python);
                            }
                        }
                        File.Delete(assInfo);
                    }
                    else
                    {
                        Debug.LogError("Please convert your source before executing it!");
                    }
                    break;
                }

                    #endregion

                    #region Ook

                case "Ook":
                {
                    if (!_cSauce.Equals(""))
                    {
                        TextWriter tw =
                            new StreamWriter(
                                Application.dataPath + "/Mods/Scripts/OokScripts/" + _name + ".ook",
                                false);
                        tw.Write(_sauce);
                        tw.Close();
                        TextWriter t =
                            new StreamWriter(Application.dataPath + "/Mods/Scripts/OokScripts/" + _name + ".ref",
                                false);
                        t.Write(_refs);
                        t.Close();
                        if (!OokScripts.ContainsKey(_name))
                        {
                            OokScripts.Add(_name,
                                new Script(
                                    Application.dataPath + "/Mods/Scripts/OokScripts/" + _name + ".ook",
                                    Application.dataPath + "/Mods/Scripts/OokScripts/" + _name + ".ref"));
                        }
                        var reffs = Util.splitStringAtNewline(_refs);
                        String finalSauce = Util.getMethodsWithClass(_cSauce, _name, reffs);
                        TextWriter ts =
                            new StreamWriter(
                                Application.dataPath + "/Mods/Scripts/OokScripts/" + _name + ".cs",
                                false);
                        tw.Write(finalSauce);
                        tw.Close();
                        var assInfo =
                            Util.Compile(
                                new FileInfo(Application.dataPath + "/Mods/Scripts/OokScripts/" + _name +
                                             ".cs"),
                                reffs, _name);
                        File.Delete(Application.dataPath + "/Mods/Scripts/OokScripts/" + _name + ".cs");
                        String assinfoSauce = "";
                        using (TextReader tr = new FileInfo(assInfo).OpenText())
                        {
                            assinfoSauce = tr.ReadToEnd();
                        }
                        foreach (var go in _gos.Keys)
                        {
                            if (go != null)
                            {
                                go.AddComponent<PythonBehaviour>();
                                var python = go.GetComponent<PythonBehaviour>();
                                python.SourceCodening(assinfoSauce, reffs);
                                python.Awakening(_name);
                                AddedScripts.Add(new Tuple<string, GameObject>(_name, go), python);
                            }
                        }
                        File.Delete(assInfo);
                    }
                    else
                    {
                        Debug.LogError("Please convert your source before executing it!");
                    }
                    break;
                }

                    #endregion

                    #region Chef

                case "Chef":
                {
                    TextWriter tw =
                        new StreamWriter(Application.dataPath + "/Mods/Scripts/ChefScripts/" + _name + ".recipe",
                            false);
                    tw.Write(_sauce);
                    tw.Close();

                    TextWriter t =
                        new StreamWriter(Application.dataPath + "/Mods/Scripts/ChefScripts" + _name + ".ref", false);
                    t.Write(_refs);
                    t.Close();

                    ChefScripts.Add(_name,
                        new Script(Application.dataPath + "/Mods/Scripts/ChefScripts/" + _name + ".recipe",
                            Application.dataPath + "/Mods/Scripts/ChefScripts" + _name + ".ref"));
                    var chef =
                        new Chef.Chef(Application.dataPath + "/Mods/Scripts/ChefScripts/" + _name + ".recipe");
                    break;
                }

                    #endregion

                    #region Default

                default:
                {
                    Debug.LogError("Please select an IDE!");
                    break;
                }

                    #endregion

                    #region Java[OBSOLETE]

                    /*
                case "Java":
                {
                    TextWriter tw =
                        new StreamWriter(Application.dataPath + "/Mods/Scripts/TempScripts/" + _name + ".java",
                            false);
                    tw.Write(_sauce);
                    tw.Close();
                    JavaScripts.Add(_name, Application.dataPath + "/Mods/Scripts/TempScripts/" + _name + ".java");
                    var reffs = Regex.Split(_refs, Util.getNewLine());
                    var finalRefs = new string[reffs.Length];
                    var javaRefs = new string[reffs.Length];
                    foreach (var reff in reffs)
                    {
                        var refffs = reff.Split('/');
                        finalRefs[finalRefs.Length] = refffs[refffs.Length - 1];
                    }
                    for (var i = 0; i < finalRefs.Length; i++)
                    {
                        var reffff = finalRefs[i].Replace(".dll", ".jar");
                        if (File.Exists(resourcePath + "/JavaStubs/" + reffff)) continue;
                        Process.Start(
                            new ProcessStartInfo(resourcePath + "/ikvm-7.2.4630.5/bin/ikvmstub.exe",
                                reffs[i]));
                        File.Copy(reffs[i].Replace(".dll", ".jar"), resourcePath + "/JavaStubs/" + reffff);
                        javaRefs[i] = resourcePath + "/JavaStubs/" + reffff + " ";
                    }
                    Util.compileJava(_name, JavaScripts[_name], javaRefs);
                    if (!CompiledScripts.ContainsKey(_name))
                    {
                        CompiledScripts.Add(_name,
                            Application.dataPath + "/Mods/Scripts/TempScripts/" + _name + ".dll");
                    }
                    Mono.Cecil.AssemblyDefinition ad = AssemblyFactory.GetAssembly(CompiledScripts[_name]);
                    ModuleDefinition md = ad.MainModule;
                    Type t = null;
                    foreach (TypeDefinition typeDefinition in md.Types)
                    {
                        if (!typeDefinition.IsAutoClass && typeDefinition.IsClass)
                        {
                            t = typeDefinition.GetType();
                        }
                    }
                    if (t != null)
                    {
                        AddedScripts.Add(_name, gameObject.AddComponent(t));
                    }
                    else
                    {
                        UnityEngine.Debug.Log("No type found in assembly! Are you sure you declared a class?");
                    }
                    break;
                }
                */

                    #endregion
            }
        }

        public void Convertion()
        {
            switch (Ide)
            {
                case "JavaScript":
                {
                    _cSauce = Util.ConvertJSToC(_sauce, "JavaScriptClass");
                    _displayC = true;
                    break;
                }
                case "Brainfuck":
                {
                    var bfb = new OokAndBrainfuckBehaviour(_sauce, true);
                    bfb.runBF();
                    _cSauce = bfb.sauce;
                    _displayC = true;
                    break;
                }
                case "Ook":
                {
                    var bfb = new OokAndBrainfuckBehaviour(_sauce, false);
                    bfb.runOok();
                    _cSauce = bfb.sauce;
                    _displayC = true;
                    break;
                }
            }
        }

        public void Loadtion(KeyValuePair<String, Script> script)
        {
            var f = new FileInfo(script.Value.sourceFile);
            using (TextReader tr = f.OpenText())
            {
                _sauce = tr.ReadToEnd();
                _name = script.Key;
            }
            using (TextReader trr = new FileInfo(script.Value.refFile).OpenText())
            {
                _refs = trr.ReadToEnd();
            }
        }

        public void UpdateSauce()
        {
            switch (Ide)
            {
                case "CSharp":
                {
                    using (
                        TextReader tr =
                            new StreamReader(Application.dataPath + "/Mods/Scripts/StandardScripts/CSharp.cs"))
                    {
                        _sauce = tr.ReadToEnd();
                    }
                    break;
                }
                case "Lua":
                {
                    using (
                        TextReader tr =
                            new StreamReader(Application.dataPath + "/Mods/Scripts/StandardScripts/Lua.lua"))
                    {
                        _sauce = tr.ReadToEnd();
                    }
                    break;
                }
                case "Python":
                {
                    using (
                        TextReader tr =
                            new StreamReader(Application.dataPath + "/Mods/Scripts/StandardScripts/Python.py"))
                    {
                        _sauce = tr.ReadToEnd();
                    }
                    break;
                }
                case "JavaScript":
                {
                    using (
                        TextReader tr =
                            new StreamReader(Application.dataPath + "/Mods/Scripts/StandardScripts/JavaScript.js"))
                    {
                        _sauce = tr.ReadToEnd();
                    }
                    break;
                }
                case "Brainfuck":
                {
                    using (
                        TextReader tr =
                            new StreamReader(Application.dataPath + "/Mods/Scripts/StandardScripts/Brainfuck.bf"))
                    {
                        _sauce = tr.ReadToEnd();
                    }
                    break;
                }
                case "Ook":
                {
                    using (
                        TextReader tr =
                            new StreamReader(Application.dataPath + "/Mods/Scripts/StandardScripts/Ook.ook"))
                    {
                        _sauce = tr.ReadToEnd();
                    }
                    break;
                }
            }
        }
    }
}