using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using spaar.ModLoader;
using spaar.ModLoader.UI;
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
        public List<Component> AddedScripts = new List<Component>();
        public bool isOpen = false;
        public Key key = new Key(KeyCode.LeftControl, KeyCode.M);
        public String ide = "CSharp";
        private Vector2 scrollPos = new Vector2(0, Mathf.Infinity);
        public String sauce = "";
        public bool isEdit = false;
        public bool addingRefs = false;
        public String refs; 
        public Vector2 windowLoc = new Vector2(Screen.width - 400.0f, Screen.height - 100.0f);
        public Vector2 areaLoc1;
        public int WinID = spaar.ModLoader.Util.GetWindowID();
        private Rect winRect;
        private String tempRefs;
        private Vector2 refPos;
        private String name = "";

        public void Start()
        {
            refs += ("SpaarModLoader.dll:");
            refs += ("Assembly-UnityScript.dll:");
            refs += ("Assembly-CSharp.dll:");
            refs += ("System.dll:");
            refs += "UnityEngine.dll";
            foreach (string @ref in refs.Split(':'))
            {
                tempRefs += @ref + "\n";
            }
            winRect = new Rect(100.0f, 20.0f, windowLoc.x, windowLoc.y);
            if (!Directory.Exists(Application.dataPath + "/Mods/Scripts"))
            {
                Debug.Log("[ScriptingMod]:Installation incomplete!");
            }
            else
            {
                Directory.CreateDirectory(Application.dataPath + "/Mods/Scripts/TempScripts");
                DirectoryInfo DI = new DirectoryInfo(Application.dataPath + "/Mods/Scripts");
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/PythonScripts"))
                {
                    foreach (
                        FileInfo info in DI.GetDirectories().First(info => info.Name.Equals("PythonScripts")).GetFiles()
                        )
                    {
                        PythonScripts.Add(info.Name, info);
                    }
                }
                else
                {
                    Directory.CreateDirectory(Application.dataPath + "/Mods/Scripts/PythonScripts");
                }
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/LuaScripts"))
                {
                    foreach (
                        FileInfo fileInfo in
                            DI.GetDirectories().First(info => info.Name.Equals("LuaScripts")).GetFiles())
                    {
                        LuaScripts.Add(fileInfo.Name, fileInfo);
                    }
                }
                else
                {
                    Directory.CreateDirectory(Application.dataPath + "/Mods/Scripts/LuaScripts");
                }
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/BrainfuckScripts"))
                {
                    foreach (
                        FileInfo fileInfo in
                            DI.GetDirectories().First(info => info.Name.Equals("BrainfuckScripts")).GetFiles())
                    {
                        BrainfuckScripts.Add(fileInfo.Name, fileInfo);
                    }
                }
                else
                {
                    Directory.CreateDirectory(Application.dataPath + "/Mods/Scripts/BrainfuckScripts");
                }
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/CSharpScripts"))
                {
                    foreach (
                        FileInfo fileInfo in
                            DI.GetDirectories().First(info => info.Name.Equals("CSharpScripts")).GetFiles())
                    {
                        CSharpScripts.Add(fileInfo.Name, fileInfo);
                    }
                }
                else
                {
                    Directory.CreateDirectory(Application.dataPath + "/Mods/Scripts/CSharpScripts");
                }
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/ChefScripts"))
                {
                    foreach (
                        FileInfo fileInfo in
                            DI.GetDirectories().First(info => info.Name.Equals("ChefScripts")).GetFiles())
                    {
                        ChefScripts.Add(fileInfo.Name, fileInfo);
                    }
                }
                else
                {
                    Directory.CreateDirectory(Application.dataPath + "/Mods/Scripts/ChefScripts");
                }
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/JavaScriptScripts"))
                {
                    foreach (
                        FileInfo fileInfo in
                            DI.GetDirectories().First(info => info.Name.Equals("JavaScriptScripts")).GetFiles())
                    {
                        JavaScriptScripts.Add(fileInfo.Name, fileInfo);
                    }
                }
                else
                {
                    Directory.CreateDirectory(Application.dataPath + "/Mods/Scripts/JavaScriptScripts");
                }
                if (Directory.Exists(Application.dataPath + "/Mods/Scripts/JavaScripts"))
                {
                    foreach (
                        FileInfo fileInfo in
                            DI.GetDirectories().First(info => info.Name.Equals("BrainfuckScripts")).GetFiles())
                    {
                        BrainfuckScripts.Add(fileInfo.Name, fileInfo);
                    }
                }
                else
                {
                    Directory.CreateDirectory(Application.dataPath + "/Mods/Scripts/JavaScripts");
                }
            }
            key = spaar.ModLoader.Keybindings.AddKeybinding("Open scripting mod", new Key(KeyCode.LeftControl, KeyCode.M));
            areaLoc1 = new Vector2(windowLoc.x - (windowLoc.x / 100.0f), (windowLoc.y / 2) / 2);
        }

        public void OnGUI()
        {
            GUI.skin = ModGUI.Skin;
            if (isOpen)
            {
                if (Event.current.keyCode == KeyCode.Return)
                {
                    scrollPos.y = Mathf.Infinity;
                }
                winRect = GUILayout.Window(WinID, winRect,
                    Func,
                    "Scripting Mod by Mortimer");
            }
        }

        private void Func(int id)
        {
            //GUILayout.BeginArea(new Rect(windowLoc.x/100.0f, windowLoc.x/100.0f, areaLoc1.x, areaLoc1.y));
            GUILayout.BeginHorizontal(GUI.skin.box);

            if (GUILayout.Button("CSharp"))
            {
                ide = "CSharp";
            }
            if (GUILayout.Button("Java"))
            {
                ide = "Java";
            }
            if (GUILayout.Button("Python"))
            {
                ide = "Python";
            }
            if (GUILayout.Button("Lua"))
            {
                ide = "Lua";
            }
            if (GUILayout.Button("JavaScript"))
            {
                ide = "JavaScript";
            }
            if (GUILayout.Button("Chef"))
            {
                ide = "Chef";
            }
            if (GUILayout.Button("Brainfuck"))
            {
                ide = "Brainfuck";
            }

            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal(GUI.skin.box);

            if (GUILayout.Button("Execute"))
            {
                isEdit = false;
                
                switch (ide)
                {
                    case "CSharp":
                    {
                        if (!addingRefs)
                        {
                            addingRefs = true;
                            break;
                        }
                        if (name.Equals(""))
                        {
                            break;
                        }

                            if (File.Exists(Application.dataPath + "/Mods/Scripts/TempScripts/" + name + ".dll"))
                            {
                                File.Delete(Application.dataPath + "/Mods/Scripts/TempScripts/" + name + ".dll");
                            }
                            refs = tempRefs.Replace("\n", ":");
                        FileStream fs = File.Create(Application.dataPath + "/Mods/Scripts/TempScripts/" + name + ".cs");

                        FileInfo assInfo = Util.Compile(new FileInfo(fs.Name), refs, name);
                        addingRefs = false;
                        isEdit = true;
                        CSharpScripts.Add(name, assInfo);
                        Assembly ass = Assembly.LoadFrom(assInfo.FullName);
                        foreach (Type exportedType in ass.GetExportedTypes())
                        {
                            if (!exportedType.IsGenericType)
                            {
                                AddedScripts.Add(gameObject.AddComponent(exportedType));
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            if (GUILayout.Button("Save"))
            {
                
            }
            if (GUILayout.Button("Load"))
            {
                
            }
            if (GUILayout.Button("Open/Close Editor"))
            {
                isEdit = !isEdit;
            }
            //GUILayout.EndArea();

            GUILayout.EndHorizontal();


            if (isEdit && !addingRefs)
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                sauce = GUILayout.TextArea(sauce);
                GUILayout.EndScrollView();
            }
            if (!isEdit && addingRefs)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.Label("Enter the name of your script: ");
                name = GUILayout.TextField(name);
                GUILayout.EndHorizontal();
                refPos = GUILayout.BeginScrollView(refPos);
                tempRefs = GUILayout.TextArea(tempRefs);
                GUILayout.EndScrollView();
            }
            GUI.DragWindow();
        }

        public void Update()
        {
            key = Keybindings.Get("Open scripting mod");
            if (Input.GetKey(key.Modifier) && Input.GetKeyDown(key.Trigger))
                isOpen = !isOpen;
        }

        public void OnUnload()
        {
            foreach (Component addedScript in AddedScripts)
            {
                Destroy(addedScript);
            }
            if (Directory.Exists(Application.dataPath + "/Mods/Scripts/TempScripts"))
            {
                Directory.Delete(Application.dataPath + "/Mods/Scripts/TempScripts");
            }
        }
    }
}
