using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using spaar.ModLoader;
using spaar.ModLoader.UI;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BesiegeScriptingMod
{
    internal class ScriptHandler : MonoBehaviour
    {
        //Window stuff
        private readonly int _winId = spaar.ModLoader.Util.GetWindowID();
        private readonly int _chooseWinId = spaar.ModLoader.Util.GetWindowID();
        private readonly int _helpId = spaar.ModLoader.Util.GetWindowID();
        private Vector2 _refPos;
        private Vector2 _scrollPos = new Vector2(0, Mathf.Infinity);
        private Vector2 _windowLoc = new Vector2(Screen.width - 1200.0f, Screen.height - 400.0f);
        public Rect WinRect;
        private Rect _chooseRect = new Rect(0, 20.0f, 100.0f, 100.0f);
        public bool _displayC;
        private bool _addingRefs;
        private GUIStyle _toggleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _headlineStyle;
        private GUIStyle _defaultLabel;
        private bool _ideSelection;
        private bool _isLoading;
        private bool _isOpen;
        private bool _stopScript;
        private bool _chooseObject;
        private bool _showObjects;
        private bool _showGoOptions;

        private string _name = "";
        private readonly Dictionary<GameObject, Color> _gos = new Dictionary<GameObject, Color>();
        private readonly Dictionary<String, Language> _languages = new Dictionary<string, Language>(); 
        public readonly Dictionary<Tuple<string, GameObject>, Component> AddedScripts =
            new Dictionary<Tuple<string, GameObject>, Component>();

        public string _cSauce = "";
        public string Ide = "";
        private string _lastIde = "";
        private string _refs;
        private string _sauce = "";
        private Key _key;
        private Key _selecting;
        private string _lastTooltip = "";

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

            WinRect = new Rect(100.0f, 20.0f, _windowLoc.x, _windowLoc.y);

            _languages.Add("CSharp", new Language("CSharp", false, "cs"));
            _languages.Add("Python", new Language("Python", false, "py"));
            _languages.Add("Lua", new Language("Lua", false, "lua"));
            _languages.Add("Brainfuck", new Language("Brainfuck", true, "bf"));
            _languages.Add("Ook", new Language("Ook", true, "ook"));
            _languages.Add("UnityScript", new Language("UnityScript", true, "us"));
            _languages.Add("TrumpScript", new Language("TrumpScript", true, "ts"));
            

            foreach (var language in _languages.Values)
            {
                language.InitializeScripts();
            }

            UpdateSauce();
        }

        public void SetKeys(Key key, Key key2)
        {
            _key = key;
            _selecting = key2;
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
                GUI.skin.textArea.richText = true;
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

                WinRect = GUILayout.Window(_winId, WinRect,
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
                _chooseRect = GUILayout.Window(_chooseWinId, _chooseRect, Func2, "Choosing Objects");
            }
            if (_lastTooltip != "")
            {
                GUI.skin = ModGUI.Skin;
                var background = GUI.skin.label.normal.background;
                _defaultLabel = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 24,
                    normal = new GUIStyleState() {background = background, textColor = Color.white}
                };
                GUILayout.Window(_helpId, new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y + 10.0f, 100.0f, 50.0f), HelpFunc, "ToolTip");
            }
        }
        private void HelpFunc(int id)
        {
            GUILayout.Label(_lastTooltip, _defaultLabel);
        }

        private void Func2(int id)
        {
            GUILayout.Label(@"Choosing GameObjects. Press " + _selecting.Trigger + @" while hovering over an object to select it. Hold " + _selecting.Modifier + @" to select multiple.
Press " + _key.Modifier + @" + " + _key.Trigger + @" to confirm selection.", _headlineStyle);
            GUI.DragWindow();
        }

        private void Func(int id)
        {
            //GUILayout.BeginArea(new Rect(_windowLoc.x/100.0f, _windowLoc.x/100.0f, _areaLoc1.x, _areaLoc1.y));

            #region Buttons


            #region EditorToggles

            GUILayout.BeginHorizontal(GUI.skin.box);

            if (Ide != "")
            {
                #region Execute

                if (GUILayout.Button(new GUIContent("Execute", "Execute your Script. It will be attached to every selected GameObject")))
                {
                    Execution();
                }

                #endregion Execute

                #region Convert

                if (_languages[Ide].needsConvertion)
                {
                    if (GUILayout.Button(new GUIContent("Convert", "Convert your Source Code into C# code. This is necessary in order to execute it")))
                    {
                        Convertion();
                    }
                    _displayC = GUILayout.Toggle(_displayC, new GUIContent("Display C# Source", "You can either display your original source, or the converted source"), _toggleStyle);
                }

                #endregion
            }

            //var last = _isSaving;
            //_isSaving = GUILayout.Toggle(_isSaving, "Save", GUI.skin.button);

            _stopScript = GUILayout.Toggle(_stopScript,
                new GUIContent("Stop Script", "Shows you a list of running Scripts. If you press the Left Mousebutton while hovering over one, it will be destroyed"), _toggleStyle);
            if (_stopScript)
            {
                _isLoading = false;
                _addingRefs = false;
                _chooseObject = false;
                _showObjects = false;
            }

            _isLoading = GUILayout.Toggle(_isLoading, new GUIContent("Load", "Shows you a list of available Scripts. The standard Scripts are not included"), _toggleStyle);

            if (_isLoading)
            {
                _stopScript = false;
                _addingRefs = false;
                _chooseObject = false;
                _showObjects = false;
            }

            if (Ide != "")
            {
                _addingRefs = GUILayout.Toggle(_addingRefs, new GUIContent("Add references", "Let's you define the libraries you are using in your Script. The most common ones are predefined"),
                    _toggleStyle);
                if (_addingRefs)
                {
                    _isLoading = false;
                    _chooseObject = false;
                    _stopScript = false;
                    _showObjects = false;
                }
            }

            _isOpen = GUILayout.Toggle(_isOpen, new GUIContent("Open/Close Editor", "Opens/Closes the whole editor window"), _toggleStyle);
            //GUILayout.EndArea();

            GUILayout.EndHorizontal();

            #endregion


            #region Menus

            GUILayout.BeginHorizontal();
            _ideSelection = GUILayout.Toggle(_ideSelection, new GUIContent("IDE", "Select a programming language"), _toggleStyle);
            if (_ideSelection)
            {
                _showGoOptions = false;
            }

            _showGoOptions = GUILayout.Toggle(_showGoOptions, new GUIContent("GameObject Options", "Diplays GameObject options"), _toggleStyle);
            if (_showGoOptions)
            {
                _ideSelection = false;
            }
            GUILayout.EndHorizontal();
            #endregion 

            #region SubMenus
            if (_ideSelection)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                _lastIde = Ide;
                Ide = "";
                foreach (var value in _languages.Values)
                {
                    value.selected = GUILayout.Toggle(value.selected, value.name, _toggleStyle);
                    if (value.selected)
                    {
                        foreach (var value1 in _languages.Values)
                        {
                            value1.selected = false;
                        }
                        Ide = value.name;
                    }
                }
                if (!_lastIde.Equals(Ide))
                {
                    UpdateSauce();
                    _cSauce = "";
                    _displayC = false;
                    _ideSelection = false;
                }
                GUILayout.EndHorizontal();

            }
            else if (_showGoOptions)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                _chooseObject = GUILayout.Toggle(_chooseObject, new GUIContent("Choose GameObject(s)", "Select the GameObject(s) in the game the Script should be attached to"), _toggleStyle);
                if (_chooseObject)
                {
                    _isOpen = false;
                    _stopScript = false;
                    _addingRefs = false;
                    _showObjects = false;
                    _isLoading = false;
                }
                _showObjects = GUILayout.Toggle(_showObjects, new GUIContent("Show Selected", "Shows you the selected GameObject(s)"), _toggleStyle);
                if (_showObjects)
                {
                    _stopScript = false;
                    _addingRefs = false;
                    _chooseObject = false;
                    _isLoading = false;
                }
                if (GUILayout.Button(new GUIContent("Add Default", "Add the Default GameObject to the list of selected GameObjects")))
                {
                    if (!_gos.Keys.Contains(gameObject))
                    {
                        _gos.Add(gameObject, Color.black);
                    }
                }
                GUILayout.EndHorizontal();
            }
            #endregion 

            #endregion

            if (!_addingRefs && !_isLoading && !_stopScript && !_chooseObject && !_showObjects)
            {
                GUILayout.Label(Ide.Equals("") ? "Help" : "Source Code Editor", _headlineStyle);
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
                GUILayout.Label("Enter the Name of your script: ", _labelStyle, GUILayout.Width(200.0f));
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
                                fileInfo.Value.sourceFile.Contains("TempScripts") && GUILayout.Button(fileInfo._key)))
                {
                    File.Copy(fileInfo.Value.sourceFile,
                        Application.dataPath + "/Mods/Scripts/CSharpScripts/" + fileInfo._key + ".cs", true);
                    File.Copy(fileInfo.Value.refFile,
                        Application.dataPath + "/Mods/Scripts/CSharpScripts/" + fileInfo._key + ".ref", true);
                }
                foreach (
                    var fileInfo in
                        JavaScriptScripts.Where(
                            fileInfo =>
                                fileInfo.Value.sourceFile.Contains("TempScripts") && GUILayout.Button(fileInfo._key)))
                {
                    File.Copy(fileInfo.Value.sourceFile,
                        Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + fileInfo._key + ".js", true);
                    File.Copy(fileInfo.Value.refFile,
                        Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + fileInfo._key + ".ref", true);
                }
                foreach (
                    var fileInfo in
                        LuaScripts.Where(
                            fileInfo =>
                                fileInfo.Value.sourceFile.Contains("TempScripts") && GUILayout.Button(fileInfo._key)))
                {
                    File.Copy(fileInfo.Value.sourceFile,
                        Application.dataPath + "/Mods/Scripts/LuaScripts/" + fileInfo._key + ".lua", true);
                    File.Copy(fileInfo.Value.refFile,
                        Application.dataPath + "/Mods/Scripts/LuaScripts/" + fileInfo._key + ".ref", true);
                }
                foreach (
                    var fileInfo in
                        PythonScripts.Where(
                            fileInfo =>
                                fileInfo.Value.sourceFile.Contains("TempScripts") && GUILayout.Button(fileInfo._key)))
                {
                    File.Copy(fileInfo.Value.sourceFile,
                        Application.dataPath + "/Mods/Scripts/PythonScripts/" + fileInfo._key + ".py", true);
                    File.Copy(fileInfo.Value.refFile,
                        Application.dataPath + "/Mods/Scripts/PythonScripts/" + fileInfo._key + ".ref", true);
                }
                foreach (
                    var fileInfo in
                        BrainfuckScripts.Where(
                            fileInfo =>
                                fileInfo.Value.sourceFile.Contains("TempScripts") && GUILayout.Button(fileInfo._key)))
                {
                    File.Copy(fileInfo.Value.sourceFile,
                        Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + fileInfo._key + ".bf", true);
                    File.Copy(fileInfo.Value.refFile,
                        Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + fileInfo._key + ".ref", true);
                }
                foreach (
                    var fileInfo in
                        ChefScripts.Where(
                            fileInfo =>
                                fileInfo.Value.sourceFile.Contains("TempScripts") && GUILayout.Button(fileInfo._key)))
                {
                    File.Copy(fileInfo.Value.sourceFile,
                        Application.dataPath + "/Mods/Scripts/ChefScripts/" + fileInfo._key + ".recipe", true);
                    File.Copy(fileInfo.Value.refFile,
                        Application.dataPath + "/Mods/Scripts/ChefScripts/" + fileInfo._key + ".ref", true);
                }
                GUILayout.EndScrollView();
            }
            */
                #endregion

                #region loading

            else if (_isLoading)
            {
                _scrollPos = GUILayout.BeginScrollView(_scrollPos);
                List<String> toRemove = new List<string>();
                foreach (var script in _languages[Ide].Scripts)
                {
                    if (GUILayout.Button(script.Key))
                    {
                        if (Event.current.button == 2)
                        {
                            toRemove.Add(script.Key);
                        }
                        else
                        {
                            Loadtion(script);
                        }
                    }
                }
                GUILayout.EndScrollView();
            }

            #endregion

            _lastTooltip = GUI.tooltip;
            GUI.DragWindow();
        }

        public void Update()
        {
            if (_key.Pressed())
                _isOpen = !_isOpen;
            if (!_chooseObject) return;
            if (_selecting.Pressed())
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null)
                    {
                        Transform current = hit.transform;
                        while (current.parent.name != "Building Machine" && current.parent.name != "Simulation Machine")
                        {
                            current = current.parent;
                        }
                        GameObject go = current.gameObject;
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
                        Transform current = hit.transform;
                        while (current.parent.name != "Building Machine" && current.parent.name != "Simulation Machine")
                        {
                            current = current.parent;
                        }
                        GameObject go = current.gameObject;
                        _gos.Clear();
                        _gos.Add(go,
                            go.GetComponent<Renderer>()
                                ? go.GetComponent<Renderer>().material.color
                                : Color.clear);
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
            if (_key.Pressed())
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
            if (Regex.Matches(_name, @"[a-zA-Z]").Count == 0)
            {
                Debug.LogError("No Name specified!");
                return;
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
                    _languages[Ide].SaveSourceToFile(_sauce, _name);
                    TextWriter tt = new StreamWriter(Application.dataPath + "/Mods/Scripts/temp.cs");
                    tt.Write(finalSauce);
                    tt.Close();
                    TextWriter t =
                        new StreamWriter(Application.dataPath + "/Mods/Scripts/CSharpScripts/" + _name + ".ref",
                            false);
                    t.Write(_refs);
                    t.Close();

                    if (!_languages[Ide].Scripts.ContainsKey(_name))
                    {
                            _languages[Ide].Scripts.Add(_name,
                            new Script(Application.dataPath + "/Mods/Scripts/CSharpScripts/" + _name + ".cs",
                                Application.dataPath + "/Mods/Scripts/CSharpScripts/" + _name + ".ref"));
                    }
                    var assInfo = Util.Compile(new FileInfo(Application.dataPath + "/Mods/Scripts/temp.cs"), reffs,
                        _name);
                    File.Delete(Application.dataPath + "/Mods/Scripts/temp.cs");
                    foreach (var go in _gos.Keys)
                    {
                        if (go != null)
                        {
                            go.AddComponent<PythonBehaviour>();
                            var python = go.GetComponent<PythonBehaviour>();
                            python.SourceCodening(assInfo, reffs);
                            if (!python.Awakening(_name))
                            {
                                Destroy(python);
                            }
                            else
                            {
                                AddedScripts.Add(new Tuple<string, GameObject>(_name, go), python);
                            }
                        }
                    }
                    break;
                }

                    #endregion

                    #region Lua

                case "Lua":
                {
                        _languages[Ide].SaveSourceToFile(_sauce, _name);

                        TextWriter t =
                        new StreamWriter(Application.dataPath + "/Mods/Scripts/LuaScripts/" + _name + ".ref", false);
                    t.Write(_refs);
                    t.Close();

                    if (!_languages[Ide].Scripts.ContainsKey(_name))
                    {
                            _languages[Ide].Scripts.Add(_name,
                            new Script(Application.dataPath + "/Mods/Scripts/LuaScripts/" + _name + ".lua",
                                Application.dataPath + "/Mods/Scripts/LuaScripts/" + _name + ".ref"));
                    }

                    foreach (var go in _gos.Keys)
                    {
                        if (go != null)
                        {
                            go.AddComponent<LuaBehaviour>();
                            var luaa = go.GetComponent<LuaBehaviour>();
                            luaa.SourceCodening(_sauce);
                            if (!luaa.Awakening())
                            {
                                Destroy(luaa);
                            }
                            else
                            {
                                AddedScripts.Add(new Tuple<string, GameObject>(_name, go), luaa);
                            }
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

                        _languages[Ide].SaveSourceToFile(_sauce, _name);


                        TextWriter t =
                        new StreamWriter(Application.dataPath + "/Mods/Scripts/PythonScripts/" + _name + ".ref",
                            false);
                    t.Write(_refs);
                    t.Close();

                    if (!_languages[Ide].Scripts.ContainsKey(_name))
                    {
                            _languages[Ide].Scripts.Add(_name,
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
                            if (!python.Awakening(_name))
                            {
                                Destroy(python);
                            }
                            else
                            {
                                AddedScripts.Add(new Tuple<string, GameObject>(_name, go), python);
                            }
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
                            _languages[Ide].SaveSourceToFile(_sauce, _name);

                            TextWriter t =
                            new StreamWriter(
                                Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + _name + ".ref",
                                false);
                        t.Write(_refs);
                        t.Close();

                        if (!_languages[Ide].Scripts.ContainsKey(_name))
                        {
                                _languages[Ide].Scripts.Add(_name,
                                new Script(
                                    Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + _name + ".js",
                                    Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + _name + ".ref"));
                        }
                        var reffs = Util.splitStringAtNewline(_refs);
                        String finalSauce = Util.getMethodsWithClass(_cSauce, _name, reffs);
                        TextWriter streamWriter =
                            new StreamWriter(
                                Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + _name + ".cs", false);
                        streamWriter.Write(finalSauce);
                        streamWriter.Close();

                        var assInfo =
                            Util.Compile(
                                new FileInfo(Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + _name +
                                             ".cs"),
                                reffs, _name);

                        File.Delete(Application.dataPath + "/Mods/Scripts/JavaScriptScripts/" + _name + ".cs");
                        foreach (var go in _gos.Keys)
                        {
                            if (go != null)
                            {
                                go.AddComponent<PythonBehaviour>();
                                var python = go.GetComponent<PythonBehaviour>();
                                python.SourceCodening(assInfo, reffs);
                                if (!python.Awakening(_name))
                                {
                                    Destroy(python);
                                }
                                else
                                {
                                    AddedScripts.Add(new Tuple<string, GameObject>(_name, go), python);
                                }
                            }
                        }
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
                            _languages[Ide].SaveSourceToFile(_sauce, _name);
                            TextWriter t =
                            new StreamWriter(Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + _name + ".ref",
                                false);
                        t.Write(_refs);
                        t.Close();
                        if (!_languages[Ide].Scripts.ContainsKey(_name))
                        {
                                _languages[Ide].Scripts.Add(_name,
                                new Script(
                                    Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + _name + ".bf",
                                    Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + _name + ".ref"));
                        }
                        var reffs = Util.splitStringAtNewline(_refs);
                        String finalSauce = Util.getMethodsWithClass(_cSauce, _name, reffs);
                        TextWriter streamWriter =
                            new StreamWriter(
                                Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + _name + ".cs",
                                false);
                            streamWriter.Write(finalSauce);
                            streamWriter.Close();
                        var assInfo =
                            Util.Compile(
                                new FileInfo(Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + _name +
                                             ".cs"),
                                reffs, _name);
                        File.Delete(Application.dataPath + "/Mods/Scripts/BrainfuckScripts/" + _name + ".cs");
                        foreach (var go in _gos.Keys)
                        {
                            if (go != null)
                            {
                                go.AddComponent<PythonBehaviour>();
                                var python = go.GetComponent<PythonBehaviour>();
                                python.SourceCodening(assInfo, reffs);
                                if (!python.Awakening(_name))
                                {
                                    Destroy(python);
                                }
                                else
                                {
                                    AddedScripts.Add(new Tuple<string, GameObject>(_name, go), python);
                                }
                            }
                        }
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
                            _languages[Ide].SaveSourceToFile(_sauce, _name);
                            TextWriter t =
                            new StreamWriter(Application.dataPath + "/Mods/Scripts/OokScripts/" + _name + ".ref",
                                false);
                        t.Write(_refs);
                        t.Close();
                        if (!_languages[Ide].Scripts.ContainsKey(_name))
                        {
                                _languages[Ide].Scripts.Add(_name,
                                new Script(
                                    Application.dataPath + "/Mods/Scripts/OokScripts/" + _name + ".ook",
                                    Application.dataPath + "/Mods/Scripts/OokScripts/" + _name + ".ref"));
                        }
                        var reffs = Util.splitStringAtNewline(_refs);
                        String finalSauce = Util.getMethodsWithClass(_cSauce, _name, reffs);
                        TextWriter streamWriter =
                            new StreamWriter(
                                Application.dataPath + "/Mods/Scripts/OokScripts/" + _name + ".cs",
                                false);
                            streamWriter.Write(finalSauce);
                            streamWriter.Close();
                        var assInfo =
                            Util.Compile(
                                new FileInfo(Application.dataPath + "/Mods/Scripts/OokScripts/" + _name +
                                             ".cs"),
                                reffs, _name);
                        File.Delete(Application.dataPath + "/Mods/Scripts/OokScripts/" + _name + ".cs");
                        foreach (var go in _gos.Keys)
                        {
                            if (go != null)
                            {
                                go.AddComponent<PythonBehaviour>();
                                var python = go.GetComponent<PythonBehaviour>();
                                python.SourceCodening(assInfo, reffs);
                                if (!python.Awakening(_name))
                                {
                                    Destroy(python);
                                }
                                else
                                {
                                    AddedScripts.Add(new Tuple<string, GameObject>(_name, go), python);
                                }
                            }
                        }
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
                        _languages[Ide].SaveSourceToFile(_sauce, _name);

                        TextWriter t =
                        new StreamWriter(Application.dataPath + "/Mods/Scripts/ChefScripts" + _name + ".ref", false);
                    t.Write(_refs);
                    t.Close();

                        _languages[Ide].Scripts.Add(_name,
                        new Script(Application.dataPath + "/Mods/Scripts/ChefScripts/" + _name + ".recipe",
                            Application.dataPath + "/Mods/Scripts/ChefScripts" + _name + ".ref"));
                    var chef =
                        new Chef.Chef(Application.dataPath + "/Mods/Scripts/ChefScripts/" + _name + ".recipe");
                    break;
                }

                    #endregion

                    #region TrumpScript

                case "TrumpScript":
                {
                    if (!_cSauce.Equals(""))
                    {
                        var reffs = Util.splitStringAtNewline(_refs);
                        String finalSauce = Util.getMethodsWithClass(_cSauce, _name, reffs);
                            _languages[Ide].SaveSourceToFile(_sauce, _name);
                            TextWriter tt = new StreamWriter(Application.dataPath + "/Mods/Scripts/temp.cs");
                        tt.Write(finalSauce);
                        tt.Close();
                        TextWriter t =
                            new StreamWriter(Application.dataPath + "/Mods/Scripts/TrumpScripts/" + _name + ".ref",
                                false);
                        t.Write(_refs);
                        t.Close();

                        if (!_languages[Ide].Scripts.ContainsKey(_name))
                        {
                                _languages[Ide].Scripts.Add(_name,
                                new Script(Application.dataPath + "/Mods/Scripts/TrumpScripts/" + _name + ".ts",
                                    Application.dataPath + "/Mods/Scripts/TrumpScripts/" + _name + ".ref"));
                        }
                        var assInfo = Util.Compile(new FileInfo(Application.dataPath + "/Mods/Scripts/temp.cs"), reffs,
                            _name);
                        File.Delete(Application.dataPath + "/Mods/Scripts/temp.cs");
                        foreach (var go in _gos.Keys)
                        {
                            if (go != null)
                            {
                                go.AddComponent<PythonBehaviour>();
                                var python = go.GetComponent<PythonBehaviour>();
                                python.SourceCodening(assInfo, reffs);
                                if (!python.Awakening(_name))
                                {
                                    Destroy(python);
                                }
                                else
                                {
                                    AddedScripts.Add(new Tuple<string, GameObject>(_name, go), python);
                                }
                            }
                        }
                    }
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
                case "UnityScript":
                {
                    _cSauce = Util.ConvertJSToC(_sauce, "UnityScriptClass");
                    _displayC = true;
                    break;
                }
                case "Brainfuck":
                {
                    var bfb = gameObject.AddComponent<OokAndBrainfuckBehaviour>();
                    bfb.init(_sauce, true);
                    StartCoroutine_Auto(bfb.runBF());
                    break;
                }
                case "Ook":
                {
                        var bfb = gameObject.AddComponent<OokAndBrainfuckBehaviour>();
                        bfb.init(_sauce, false);
                        StartCoroutine_Auto(bfb.runOok());
                    break;
                }
                case "TrumpScript":
                {
                    _cSauce = new TrumpScript.TrumpScript().Convert(_sauce);
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
            if (!String.IsNullOrEmpty(script.Value.refFile))
            {
                using (TextReader trr = new FileInfo(script.Value.refFile).OpenText())
                {
                    _refs = trr.ReadToEnd();
                }
            }
        }

        public void UpdateSauce()
        {
            if (!Ide.Equals(""))
            {
                _sauce = _languages[Ide].LoadDefaultFileSource();
            }
            else {
                    _sauce = "<size=18><color=#ff0000ff>\t\t\t\t\tHow to write a Script and execute it</color></size>" + Util.getNewLine() +
                             "<size=16>1. Select an IDE (you can hover your mouse over any button to see a more detailed description)</size>" + Util.getNewLine() +
                             "<size=16>2. Press <color=#ffa500ff>Add References</color> and input a Name for your script. This is important during compilation. Additionally, you can already input more references, which your script will be using.</size>" +
                             Util.getNewLine() +
                             "<size=16>3. Press <color=#ffa500ff>Add References</color> again to get back to the source code editor. Now you can write your script. <color=#ff0000ff>DO NOT SWITCH YOUR IDE AFTER WRITING YOUR SOURCE CODE. It would be overridden.</color> When you're done, proceed with <b>Point 4</b></size>" +
                             Util.getNewLine() +
                             "<size=16>4. Press <color=#ffa500ff>GameObject Options</color>. A submenu should show up with <color=#ffa500ff>Choose GameObject(s)</color>, <color=#ffa500ff>Show Selected</color> and <color=#ffa500ff>Add Default</color>. </size>" +
                             Util.getNewLine() +
                             "<size=14>\t4.1. <color=#ffa500ff>Choose GameObject(s)</color> Let's you select the GameObjects your Script should be attached to when you execute it. If you press it, the editor window should be invisible and a help message with further instructions should pop up in the top-left corner. </size>" +
                             Util.getNewLine() +
                             "<size=14>\t4.2. <color=#ffa500ff>Show Selected</color> Let's you see the selected GameObject(s). You can click on one to remove it.</size>" + Util.getNewLine() +
                             "<size=14>\t4.3. <color=#ffa500ff>Add Default</color> Adds the Default GameObject to the List of selected GameObjects. It is the GameObject, which the ScriptingMod is attached to and is named <color=#ffa500ff>MortimersScriptingMod</color></size>" +
                             Util.getNewLine() +
                             "<size=16>5. <color=#ffa500ff>Execute</color> compiles the script and attaches it to the selected GameObject(s). </size>" + Util.getNewLine() +
                             "<size=14><b>\t5.1. Special cases for the IDEs <color=#ffa500ff>Brainfuck</color>, <color=#ffa500ff>Ook</color> and <color=#ffa500ff>UnityScript</color>. In order for them to work, you have to press <color=#ffa500ff>Convert</color> first. This converts your source code into C# Source Code. This happens in order for you to be able to fix any issues that came up with the convertion.</b></size>" +
                             Util.getNewLine() +
                             "<size=16>6. <color=#ffa500ff>Stop Script</color> displays a List of currently running Scripts. Press the left MouseButton while hovering over one to destroy it. <color=#ffa500ff>OnDestroy</color> will be called.</size>" +
                             Util.getNewLine() +
                             "<size=16>7. <color=#ffa500ff>Load</color> displays a List of saved Scripts. Scripts are saved when you execute them, in order to reduce data garbage on your harddrive with non-working prototypes of your Script. Click on one and the Script will be loaded into the <color=#ffa500ff>Source Code Editor</color>.</size>" +
                             Util.getNewLine() +
                             "<size=16>8. <color=#ffa500ff>Display C# Source</color> will let you switch between your original Source Code and the converted one. Only available for <color=#ffa500ff>Brainfuck</color>, <color=#ffa500ff>Ook</color> and <color=#ffa500ff>UnityScript</color></size>" +
                             Util.getNewLine() +
                             "<size=14><i>Additionally, if you want to resize the window, you can enter ScaleX [value] or ScaleY[value] into the console</i></size>" + Util.getNewLine() +
                             "<size=14><i>Deleting a Script is done by right clicking it in the 'Load' screen</i></size>";
            }
            _name = "";
        }
    }
}