﻿#region usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BesiegeScriptingMod.Extensions;
using BesiegeScriptingMod.LibrariesForScripts;
using BesiegeScriptingMod.Util;
using spaar.ModLoader;
using spaar.ModLoader.UI;
using UnityEngine;
using Component = UnityEngine.Component;

#endregion

namespace BesiegeScriptingMod
{
    internal class ScriptHandler : MonoBehaviour
    {
        private Texture2D text;
        private readonly int _blockInfoId = spaar.ModLoader.Util.GetWindowID();
        private readonly int _chooseWinId = spaar.ModLoader.Util.GetWindowID();
        private readonly Dictionary<GameObject, Tuple<Renderer, Color>> _gos = new Dictionary<GameObject, Tuple<Renderer, Color>>();
        private readonly int _helpId = spaar.ModLoader.Util.GetWindowID();
        private readonly Dictionary<string, Language> _languages = new Dictionary<string, Language>();
        private readonly int _winId = spaar.ModLoader.Util.GetWindowID();
        private readonly int _planExID = spaar.ModLoader.Util.GetWindowID();
        private bool _addingRefs;
        private bool _bInfo;
        private Rect _bInfoRect;
        private GameObject _block;
        private Key _blockInfo;
        private bool _chooseObject;
        private Rect _chooseRect = new Rect(0, 20.0f, 100.0f, 100.0f);

        public string CSauce = "";
        private GUIStyle _defaultLabel;
        private GUIStyle _headlineStyle;
        private bool _ideSelection;
        private bool _isLoading;
        private bool _isOpen;
        private bool _options;
        private bool _plannedExec;
        private Key _key;
        private GUIStyle _labelStyle;

        private string _name = "";
        private Vector2 _refPos;
        private string _refs;
        private string _sauce = "";
        private Vector2 _scrollPos = new Vector2(0, Mathf.Infinity);
        private Key _selecting;
        private Key _settingsGui;
        private bool _showGoOptions;
        private bool _showObjects;
        private bool _stopScript;
        private GUIStyle _toggleStyle;

        private Dictionary<Tuple<string, GameObject>, Component> _addedScripts =
            new Dictionary<Tuple<string, GameObject>, Component>();

        private Rect _plannedExecRect = new Rect(200.0f, 200.0f, 100.0f, 200.0f);
        private readonly Dictionary<PlannedExecEnum, PlannedExecutionWrapper> plannedExecution = new Dictionary<PlannedExecEnum, PlannedExecutionWrapper>(); 
        private readonly Dictionary<PlannedExecEnum, bool> enums = new Dictionary<PlannedExecEnum, bool>();
        private Key key1 = new Key(KeyCode.None, KeyCode.None);
        private bool lastKey;
        private static KeyCode[] SpecialKeys =
    {
      KeyCode.LeftControl, KeyCode.RightControl, KeyCode.LeftShift,
      KeyCode.RightShift, KeyCode.LeftAlt, KeyCode.RightAlt,
      KeyCode.Backspace, KeyCode.Mouse0, KeyCode.Mouse1,
      KeyCode.Mouse2, KeyCode.Mouse3, KeyCode.Mouse4,
      KeyCode.Mouse5, KeyCode.Mouse6, KeyCode.Alpha0,
      KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
      KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6,
      KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9,
      KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4,
      KeyCode.F5, KeyCode.F6, KeyCode.F7, KeyCode.F8,
      KeyCode.F9, KeyCode.F10, KeyCode.F11, KeyCode.F12,
      KeyCode.F13, KeyCode.F14, KeyCode.F15,
    };

        public bool DisplayC;
        public string Ide = "";
        internal string LastTooltip = "";

        public void Start()
        {
            Game.OnSimulationToggle += GameOnOnSimulationToggle;
            Game.OnBlockPlaced += GameOnOnBlockPlaced;
            Game.OnBlockRemoved += GameOnOnBlockRemoved;
            Game.OnLevelWon += GameOnOnLevelWon;
            Settings.Load();
            SettingsGUI sg = gameObject.AddComponent<SettingsGUI>();
            sg.SetKey(_settingsGui);
            LoadLanguages();
            enums.Add(PlannedExecEnum.OnMachineDestroyed, false);
            enums.Add(PlannedExecEnum.OnSimulationStart, false);
            enums.Add(PlannedExecEnum.OnSimulationStop, false);
            enums.Add(PlannedExecEnum.OnBlockPlaced, false);
            enums.Add(PlannedExecEnum.OnBlockRemoved, false);
            enums.Add(PlannedExecEnum.OnKeyPress, false);
            enums.Add(PlannedExecEnum.OnLevelLoaded, false);

            _gos.Add(gameObject, new Tuple<Renderer, Color>(null, Color.clear));
            _refs += Application.dataPath + "/Mods/SpaarModLoader.dll" + Util.Util.getNewLine();
            _refs += Application.dataPath + "/Managed/Assembly-UnityScript.dll" + Util.Util.getNewLine();
            _refs += Application.dataPath + "/Managed/Assembly-UnityScript-firstpass.dll" + Util.Util.getNewLine();
            _refs += Application.dataPath + "/Managed/Assembly-CSharp.dll" + Util.Util.getNewLine();
            _refs += Application.dataPath + "/Managed/Assembly-CSharp-firstpass.dll" + Util.Util.getNewLine();
            _refs += Application.dataPath + "/Managed/UnityEngine.dll" + Util.Util.getNewLine();
            _refs += Application.dataPath + "/Managed/System.dll";

            _languages.Add("CSharp", new Language("CSharp", false, "cs"));
            _languages.Add("Python", new Language("Python", false, "py"));
            _languages.Add("Lua", new Language("Lua", false, "lua"));
            _languages.Add("Brainfuck", new Language("Brainfuck", true, "bf"));
            _languages.Add("Ook", new Language("Ook", true, "ook"));
            _languages.Add("UnityScript", new Language("UnityScript", true, "us"));
            _languages.Add("TrumpScript", new Language("TrumpScript", true, "ts"));
            UpdateSauce();
            text = new Texture2D(0, 0);
            var bytes = File.ReadAllBytes(Application.dataPath + "/Mods/Scripts/Resource/UI/background.png");
            text.LoadImage(bytes);
        }
        public void SetKeys(Key key, Key key2, Key key3, Key key4)
        {
            _key = key;
            _selecting = key2;
            _settingsGui = key3;
            _blockInfo = key4;
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
                GUI.skin.window.normal.background = text;
                GUI.skin.textArea.richText = true;
                _toggleStyle = new GUIStyle(GUI.skin.button) {onNormal = Elements.Buttons.Red.hover};

                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    margin =
                        new RectOffset(10, 5, GUI.skin.textField.margin.top, GUI.skin.textField.margin.bottom),
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

                Settings.WinSize = GUILayout.Window(_winId, Settings.WinSize,
                    Func,
                    "Scripting Mod by Mortimer");
            }
            else if (_chooseObject)
            {
                GUI.skin = ModGUI.Skin;
                GUI.skin.window.normal.background = text;
                _headlineStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleLeft,
                    border = _labelStyle.border,
                    fontStyle = FontStyle.Bold
                };
                _chooseRect = GUILayout.Window(_chooseWinId, _chooseRect, Func2, "Choosing Objects");
            }


            if (LastTooltip != "")
            {
                GUI.skin = ModGUI.Skin;
                GUI.skin.window.normal.background = text;
                var background = GUI.skin.label.normal.background;
                _defaultLabel = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 24,
                    normal = new GUIStyleState {background = background, textColor = Color.white}
                };
                GUILayout.Window(_helpId, new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y + 10.0f, 100.0f, 50.0f), HelpFunc, "ToolTip");
            }

            if (_bInfo)
            {
                GUI.skin = ModGUI.Skin;
                GUI.skin.window.normal.background = text;
                _bInfoRect = GUILayout.Window(_blockInfoId, _bInfoRect, BlockInfo, "Block Info");
            }
            if (_plannedExec)
            {
                GUI.skin = ModGUI.Skin;
                GUI.skin.window.normal.background = text;
                _plannedExecRect = GUILayout.Window(_planExID, _plannedExecRect, PlannedExecFunc, "Planned Execution");

                if (lastKey && !enums[PlannedExecEnum.OnKeyPress])
                {
                    _plannedExecRect = new Rect(_plannedExecRect.x, _plannedExecRect.y, 100.0f, 200.0f);
                }
                lastKey = enums[PlannedExecEnum.OnKeyPress];
            }
        }

        private void PlannedExecFunc(int id)
        {
            GUILayout.Label("Triggers", new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
            });
            GUILayout.BeginHorizontal();
            enums[PlannedExecEnum.OnBlockPlaced] = GUILayout.Toggle(enums[PlannedExecEnum.OnBlockPlaced], new GUIContent("On Block Placed", "Selects 'OnBlockPlaced' as a Trigger for the Script"), _toggleStyle);
            enums[PlannedExecEnum.OnBlockRemoved] = GUILayout.Toggle(enums[PlannedExecEnum.OnBlockRemoved], new GUIContent("On Block Removed", "Selects 'OnBlockRemoved' as a Trigger for the Script"), _toggleStyle);
            enums[PlannedExecEnum.OnKeyPress] = GUILayout.Toggle(enums[PlannedExecEnum.OnKeyPress], new GUIContent("On Key Press", "Selects 'On Key Press' as a Trigger for the Script"), _toggleStyle);
            enums[PlannedExecEnum.OnLevelLoaded] = GUILayout.Toggle(enums[PlannedExecEnum.OnLevelLoaded], new GUIContent("On Level Loaded", "Selects 'On Level Loaded' as a Trigger for the Script"), _toggleStyle);
            enums[PlannedExecEnum.OnMachineDestroyed] = GUILayout.Toggle(enums[PlannedExecEnum.OnMachineDestroyed], new GUIContent("On Machine Destroyed", "Selects 'On Machine Destroyed' as a Trigger for the Script"), _toggleStyle);
            enums[PlannedExecEnum.OnSimulationStart] = GUILayout.Toggle(enums[PlannedExecEnum.OnSimulationStart], new GUIContent("On Simulation Start", "Selects 'On Simulation Start' as a Trigger for the Script"), _toggleStyle);
            enums[PlannedExecEnum.OnSimulationStop] = GUILayout.Toggle(enums[PlannedExecEnum.OnSimulationStop], new GUIContent("On Simulation Stop", "Selects 'On Simulation Stop' as a Trigger for the Script"), _toggleStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(25.0f);
            GUILayout.Label("", GUI.skin.horizontalSlider);

            if (enums[PlannedExecEnum.OnKeyPress])
            {
                GUILayout.Space(25.0f);
                GUILayout.Label("Select the Keys for triggering the Script", new GUIStyle(GUI.skin.label) { fontSize = 24, alignment = TextAnchor.MiddleCenter });
                GUILayout.Space(25.0f);
                GUILayout.BeginHorizontal();
                GUILayout.Label(@"Modifier:", new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleRight});
                GUILayout.Label(new GUIContent(key1.Modifier.ToString(), "Modifier"), new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleLeft});
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label(@"Trigger:", new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleRight});
                GUILayout.Label(new GUIContent(key1.Trigger.ToString(), "Trigger"), new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleLeft});
                GUILayout.EndHorizontal();

                if (Event.current.type == EventType.Repaint)
                {
                    if (GUI.tooltip != "")
                    {
                        if (Input.inputString.Length > 0 && !Input.inputString.Contains('\u0008' + ""))
                        {
                            if (GUI.tooltip == "Modifier")
                            {
                                key1.Modifier = (KeyCode)Enum.Parse(typeof(KeyCode), (Input.inputString[0] + "").ToUpper());
                            }
                            else if (GUI.tooltip == "Trigger")
                            {
                                key1.Trigger = (KeyCode)Enum.Parse(typeof(KeyCode), (Input.inputString[0] + "").ToUpper());
                            }
                        }
                        KeyCode keyCode = KeyCode.None;

                        foreach (var key in SpecialKeys)
                        {
                            if (Input.GetKeyDown(key))
                            {
                                keyCode = key;
                                break;
                            }
                        }

                        if (keyCode != KeyCode.None)
                        {
                            if (GUI.tooltip == "Modifier")
                            {
                                key1.Modifier = keyCode == KeyCode.Backspace ? KeyCode.None : keyCode;
                            }
                            else if (GUI.tooltip == "Trigger")
                            {
                                key1.Trigger = keyCode == KeyCode.Backspace ? KeyCode.None : keyCode;
                            }
                        }
                    }
                }
                GUILayout.Space(25.0f);
                GUILayout.Label("", GUI.skin.horizontalSlider);
            }

            GUILayout.Space(25.0f);
            GUILayout.Label(new GUIContent("Saved Scripts", "The Scripts you can plan to execute"), new GUIStyle(GUI.skin.label) { fontSize = 24, alignment = TextAnchor.MiddleCenter });

            foreach (FileInfo fileInfo in new DirectoryInfo(_languages[Ide]._folder).GetFiles("*" + _languages[Ide]._extensionDot))
            {
                if (GUILayout.Button(fileInfo.Name.Replace(_languages[Ide]._extensionDot, "")))
                {
                    foreach (var @enum in enums)
                    {
                        if (@enum.Value)
                        {
                            plannedExecution.Add(@enum.Key, new PlannedExecutionWrapper(_sauce, _refs, _name, _gos.Keys.ToList(), Ide, key1));
                        }
                    }
                }
            }
            GUILayout.Space(25.0f);

            LastTooltip = GUI.tooltip;
            GUI.DragWindow();
        }

        private void BlockInfo(int id)
        {
            BlockBehaviour bb = _block.GetComponent<BlockBehaviour>();
            GUILayout.Label("Name: " + _block.name);
            GUILayout.Label("ID: " + bb.GetBlockID());
            GUILayout.Label("GUID: " + bb.Guid);
            GUILayout.Space(25.0f);

            if (GUILayout.Button(new GUIContent("Copy to Clipboard", "Copies the GUID to the Clipboard")))
            {
                TextEditor te = new TextEditor {content = new GUIContent(bb.Guid.ToString())};
                te.SelectAll();
                te.Copy();
            }

            if (GUILayout.Button(new GUIContent("Close", "Closes the Block Info Window")))
            {
                _bInfo = false;
            }
            LastTooltip = GUI.tooltip;
            GUI.DragWindow();
        }

        private void HelpFunc(int id)
        {
            GUILayout.Label(LastTooltip, _defaultLabel);
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

            _options = GUILayout.Toggle(_options, new GUIContent("Options", "Opens the options sub-menu"), _toggleStyle);
            if (_options)
            {
                _showGoOptions = false;
                _ideSelection = false;
            }

            _ideSelection = GUILayout.Toggle(_ideSelection, new GUIContent("IDE[" + Ide + "]", "Select a programming language"), _toggleStyle);
            if (_ideSelection)
            {
                _showGoOptions = false;
                _options = false;
            }

            _showGoOptions = GUILayout.Toggle(_showGoOptions, new GUIContent("GameObject Options", "Diplays GameObject options"), _toggleStyle);
            if (_showGoOptions)
            {
                _ideSelection = false;
                _options = false;
            }

            _isOpen = GUILayout.Toggle(_isOpen, new GUIContent("Open/Close Editor", "Opens/Closes the whole editor window"), _toggleStyle);
            //GUILayout.EndArea();

            GUILayout.EndHorizontal();

            #endregion

            #region SubMenus

            if (_ideSelection)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
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
                if (!Settings.LastIde.Equals(Ide))
                {
                    UpdateSauce();
                    CSauce = "";
                    DisplayC = false;
                    _ideSelection = false;
                    Settings.LastIde = Ide;
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
                        _gos.Add(gameObject, new Tuple<Renderer, Color>(null, Color.clear));
                    }
                }
                GUILayout.EndHorizontal();
            }
            else if (_options)
            {
                GUILayout.BeginHorizontal();
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
                        DisplayC = GUILayout.Toggle(DisplayC, new GUIContent("Display C# Source", "You can either display your original source, or the converted source"), _toggleStyle);
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

                _plannedExec = GUILayout.Toggle(_plannedExec, new GUIContent("Plan execution", "Let's you plan the execution of a Script you previously saved"), _toggleStyle);

                if (Ide != "" && _sauce != "" && _name != "" && GUILayout.Button(new GUIContent("Save", "Saves the currently open Script")))
                {
                    _languages[Ide].SaveSourceToFile(_sauce, _name);
                    _languages[Ide].SaveRefToFile(_refs, _name);
                }

                _isLoading = GUILayout.Toggle(_isLoading, new GUIContent("Load", "Shows you a list of available Scripts. The standard Scripts are not included"), _toggleStyle);
                if (_isLoading)
                {
                    _stopScript = false;
                    _addingRefs = false;
                    _chooseObject = false;
                    _showObjects = false;
                }
                GUILayout.EndHorizontal();
            }

            #endregion

            #endregion

            if (!_addingRefs && !_isLoading && !_stopScript && !_chooseObject && !_showObjects)
            {
                GUILayout.Label(Ide.Equals("") ? "Help" : "Source Code Editor", _headlineStyle);
                _scrollPos = GUILayout.BeginScrollView(_scrollPos);
                if (DisplayC)
                {
                    CSauce = GUILayout.TextArea(CSauce);
                }
                else
                {
                    _sauce = GUILayout.TextArea(_sauce);
                    if (Ide.Equals("") && Settings.Tutorial)
                    {
                        if (GUILayout.Button(new GUIContent("Got it!", "Disables the Tutorial and selects Lua as the IDE")))
                        {
                            Ide = "Lua";
                            Settings.Tutorial = false;
                            UpdateSauce();
                        }
                    }
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
                if (_addedScripts.Count > 0)
                {
                    List<Tuple<string, GameObject>> list = new List<Tuple<string, GameObject>>();
                    for (int i = 0; i < _addedScripts.Count; i++)
                    {
                        if (_addedScripts.Values.ElementAt(i) == null) continue;
                        if (
                            GUILayout.Button(_addedScripts.Keys.ElementAt(i).First + ", GameObject:" +
                                             _addedScripts.Keys.ElementAt(i).Second.name))
                        {
                            Destroy(_addedScripts.Values.ElementAt(i));
                            list.Add(_addedScripts.Keys.ElementAt(i));
                        }
                    }
                    if (list.Count > 0)
                    {
                        foreach (Tuple<string, GameObject> tuple in list)
                        {
                            _addedScripts.Remove(tuple);
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
                List<string> toRemove = new List<string>();
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
            LastTooltip = GUI.tooltip;
            GUI.DragWindow();
        }

        public void Update()
        {
            if (!_chooseObject && _key.Pressed())
                _isOpen = !_isOpen;

            if (_blockInfo.Pressed())
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null)
                    {
                        Transform current = hit.transform;
                        while (current.parent.name != "Building Machine" && current.parent.name != "Simulation Machine" && current.parent != null)
                        {
                            current = current.parent;
                        }
                        _block = current.gameObject;
                        _bInfo = true;
                    }
                }
                _bInfoRect = new Rect(Input.mousePosition.x + 50.0f, Screen.height - Input.mousePosition.y + 10.0f, 200.0f, 100.0f);
            }

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
                        while (current.parent.name != "Building Machine" && current.parent.name != "Simulation Machine" && current.parent != null)
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
                                new Tuple<Renderer, Color>(
                                    go.GetComponent<Renderer>() ? go.GetComponent<Renderer>() : go.GetComponentInChildren<Renderer>() ? go.GetComponentInChildren<Renderer>() : null,
                                    go.GetComponent<Renderer>()
                                        ? go.GetComponent<Renderer>().material.color
                                        : go.GetComponentInChildren<Renderer>() ? go.GetComponentInChildren<Renderer>().material.color : Color.clear));
                        }
                    }
                }
            }
            else if (Input.GetKeyDown(_selecting.Trigger))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null)
                    {
                        Transform current = hit.transform;
                        while (current.parent.name != "Building Machine" && current.parent.name != "Simulation Machine" && current.parent != null)
                        {
                            current = current.parent;
                        }
                        GameObject go = current.gameObject;
                        _gos.Clear();
                        _gos.Add(go,
                            new Tuple<Renderer, Color>(
                                go.GetComponent<Renderer>() ? go.GetComponent<Renderer>() : go.GetComponentInChildren<Renderer>() ? go.GetComponentInChildren<Renderer>() : null,
                                go.GetComponent<Renderer>()
                                    ? go.GetComponent<Renderer>().material.color
                                    : go.GetComponentInChildren<Renderer>() ? go.GetComponentInChildren<Renderer>().material.color : Color.clear));
                    }
                }
            }
            if (_gos.Count > 0)
            {
                foreach (KeyValuePair<GameObject, Tuple<Renderer, Color>> go in _gos.Where(go => !go.Value.First.Equals(null)))
                {
                    go.Value.First.material.color = Color.red;
                }
            }
            if (_key.Pressed())
            {
                if (_gos.Count > 0)
                {
                    foreach (KeyValuePair<GameObject, Tuple<Renderer, Color>> go in _gos.Where(go => !go.Value.First.Equals(null)))
                    {
                        go.Value.First.material.color = go.Value.Second;
                    }
                }
                _chooseObject = false;
                _isOpen = true;
            }
        }

        public void OnUnload()
        {
            Settings.Save();
            if (_addedScripts.Count > 0)
            {
                List<Tuple<string, GameObject>> list = new List<Tuple<string, GameObject>>();
                for (int i = 0; i < _addedScripts.Count; i++)
                {
                    if (_addedScripts.Values.ElementAt(i) != null)
                    {
                        Destroy(_addedScripts.Values.ElementAt(i));
                        list.Add(_addedScripts.Keys.ElementAt(i));
                    }
                }
                if (list.Count > 0)
                {
                    foreach (Tuple<string, GameObject> tuple in list)
                    {
                        _addedScripts.Remove(tuple);
                    }
                }
            }
        }

        private void Execution()
        {
            if (Regex.Matches(_name, @"[a-zA-Z]").Count == 0)
            {
                Debug.LogError("No Name specified!");
                return;
            }
            if (_languages[Ide].needsConvertion && CSauce == "")
            {
                Debug.LogError("Please convert your script first!");
                return;
            }
            if (_addedScripts.Count > 0)
            {
                List<Tuple<string, GameObject>> list = new List<Tuple<string, GameObject>>();
                for (int i = 0; i < _addedScripts.Count; i++)
                {
                    if (_addedScripts.Values.ElementAt(i) != null)
                    {
                        if (_addedScripts.Keys.ElementAt(i).First.Equals(_name))
                        {
                            Destroy(_addedScripts.Values.ElementAt(i));
                            list.Add(_addedScripts.Keys.ElementAt(i));
                        }
                    }
                }
                if (list.Count > 0)
                {
                    foreach (Tuple<string, GameObject> tuple in list)
                    {
                        _addedScripts.Remove(tuple);
                    }
                }
            }
            if (!_languages[Ide].Execute(_refs, _languages[Ide].needsConvertion ? CSauce : _sauce, _gos.Keys.ToList(), _name, ref _addedScripts))
            {
                Debug.LogError("Critical error!");
            }

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
                        _addedScripts.Add(_name, gameObject.AddComponent(t));
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

        private void Convertion()
        {
            CSauce = _languages[Ide].Convert(_sauce, _name, _refs);
            DisplayC = true;
        }

        private void Loadtion(KeyValuePair<string, Script> script)
        {
            var f = new FileInfo(script.Value.sourceFile);
            using (TextReader tr = f.OpenText())
            {
                _sauce = tr.ReadToEnd();
                _name = script.Key;
            }
            if (!string.IsNullOrEmpty(script.Value.refFile))
            {
                using (TextReader trr = new FileInfo(script.Value.refFile).OpenText())
                {
                    _refs = trr.ReadToEnd();
                }
            }
        }

        private void UpdateSauce()
        {
            if (!Ide.Equals(""))
            {
                _sauce = _languages[Ide].LoadDefaultFileSource();
            }
            else
            {
                _sauce = "<size=18><color=#ff0000ff>\t\t\t\t\tHow to write a Script and execute it</color></size>" + Util.Util.getNewLine() +
                         "<size=16>1. Select an IDE (you can hover your mouse over any button to see a more detailed description)</size>" + Util.Util.getNewLine() +
                         "<size=16>2. Press <color=#ffa500ff>Add References</color> and input a Name for your script. This is important during compilation. Additionally, you can already input more references, which your script will be using.</size>" +
                         Util.Util.getNewLine() +
                         "<size=16>3. Press <color=#ffa500ff>Add References</color> again to get back to the source code editor. Now you can write your script. <color=#ff0000ff>DO NOT SWITCH YOUR IDE AFTER WRITING YOUR SOURCE CODE. It would be overridden.</color> When you're done, proceed with <b>Point 4</b></size>" +
                         Util.Util.getNewLine() +
                         "<size=16>4. Press <color=#ffa500ff>GameObject Options</color>. A submenu should show up with <color=#ffa500ff>Choose GameObject(s)</color>, <color=#ffa500ff>Show Selected</color> and <color=#ffa500ff>Add Default</color>. </size>" +
                         Util.Util.getNewLine() +
                         "<size=14>\t4.1. <color=#ffa500ff>Choose GameObject(s)</color> Let's you select the GameObjects your Script should be attached to when you execute it. If you press it, the editor window should be invisible and a help message with further instructions should pop up in the top-left corner. </size>" +
                         Util.Util.getNewLine() +
                         "<size=14>\t4.2. <color=#ffa500ff>Show Selected</color> Let's you see the selected GameObject(s). You can click on one to remove it.</size>" + Util.Util.getNewLine() +
                         "<size=14>\t4.3. <color=#ffa500ff>Add Default</color> Adds the Default GameObject to the List of selected GameObjects. It is the GameObject, which the ScriptingMod is attached to and is named <color=#ffa500ff>MortimersScriptingMod</color></size>" +
                         Util.Util.getNewLine() +
                         "<size=16>5. <color=#ffa500ff>Execute</color> compiles the script and attaches it to the selected GameObject(s). </size>" + Util.Util.getNewLine() +
                         "<size=14><b>\t5.1. Special cases for the IDEs <color=#ffa500ff>Brainfuck</color>, <color=#ffa500ff>Ook</color> and <color=#ffa500ff>UnityScript</color>. In order for them to work, you have to press <color=#ffa500ff>Convert</color> first. This converts your source code into C# Source Code. This happens in order for you to be able to fix any issues that came up with the convertion.</b></size>" +
                         Util.Util.getNewLine() +
                         "<size=16>6. <color=#ffa500ff>Stop Script</color> displays a List of currently running Scripts. Press the left MouseButton while hovering over one to destroy it. <color=#ffa500ff>OnDestroy</color> will be called.</size>" +
                         Util.Util.getNewLine() +
                         "<size=16>7. <color=#ffa500ff>Load</color> displays a List of saved Scripts. Scripts are saved when you execute them, in order to reduce data garbage on your harddrive with non-working prototypes of your Script. Click on one and the Script will be loaded into the <color=#ffa500ff>Source Code Editor</color>.</size>" +
                         Util.Util.getNewLine() +
                         "<size=16>8. <color=#ffa500ff>Display C# Source</color> will let you switch between your original Source Code and the converted one. Only available for <color=#ffa500ff>Brainfuck</color>, <color=#ffa500ff>Ook</color> and <color=#ffa500ff>UnityScript</color></size>" +
                         Util.Util.getNewLine() +
                         "<size=14><i>Additionally, if you want to resize the window, you can enter ScaleX [value] or ScaleY[value] into the console</i></size>" + Util.Util.getNewLine() +
                         "<size=14><i>Deleting a Script is done by right clicking it in the 'Load' screen</i></size>";
            }
            _name = "";
        }

        private void LoadLanguages()
        {
            var infos = new DirectoryInfo(Application.dataPath + "/Mods").GetFiles();
            foreach (FileInfo fileInfo in infos)
            {
                if (fileInfo.FullName.EndsWith("scriptextension.dll"))
                {
                    Assembly assembly = Assembly.Load(fileInfo.FullName);
                    var types = assembly.GetExportedTypes();
                    foreach (Type type in types)
                    {
                        if (!type.IsGenericType && type.BaseType == typeof (LanguageExtension))
                        {
                            MethodInfo method = null;
                            foreach (MethodInfo methodInfo in type.GetMethods())
                            {
                                if (methodInfo.Name.Contains("Execute"))
                                {
                                    method = methodInfo;
                                }
                            }
                            var namef = type.GetField("name");
                            var value = (string) namef.GetValue(namef);
                            var needsConf = type.GetField("needsConvertion");
                            var needsCon = (bool) needsConf.GetValue(needsConf);
                            var extensionf = type.GetField("extension");
                            var extension = (string) extensionf.GetValue(extensionf);

                            MethodInfo conMethod = null;
                            if (needsCon)
                            {
                                foreach (var methodInfo in type.GetMethods())
                                {
                                    if (methodInfo.Name.Contains("Convert"))
                                    {
                                        conMethod = methodInfo;
                                    }
                                }
                            }
                            object extensios = assembly.CreateInstance(type.FullName);
                            _languages.Add(value, new Language(value, needsCon, extension, method, conMethod, extensios));
                        }
                    }
                }
            }
        }

        public void OnLevelWasLoaded(int level)
        {
            Besiege.OnLevelWasLoaded();
        }

        private void GameOnOnLevelWon()
        {

        }

        private void GameOnOnBlockRemoved()
        {

        }

        private void GameOnOnBlockPlaced(Transform block)
        {

        }

        private void GameOnOnSimulationToggle(bool simulating)
        {

        }

    }
}