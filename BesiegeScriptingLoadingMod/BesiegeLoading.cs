using System;
using System.Collections.Generic;
using System.Reflection;
using spaar.ModLoader;
using UnityEngine;

namespace BesiegeScriptingLoadingMod
{
    public class BesiegeLoading : spaar.ModLoader.Mod
    {
        private static readonly System.String Resource = Application.dataPath + "/Mods/Scripts/Resource/";
        private object bsm;
        private Type bsmType;
        private Key Key = new Key(KeyCode.LeftControl, KeyCode.L);
        private Key selecting = new Key(KeyCode.LeftControl, KeyCode.Mouse2);

        private readonly List<String> _list = new List<string>()
        {
            "ICSharpCode.NRefactory.dll",
            "IronPython.dll",
            "IronPython.Modules.dll",
            "IronPython.SQLite.dll",
            "Microsoft.Dynamic.dll",
            "Microsoft.Scripting.AspNet.dll",
            "Microsoft.Scripting.Core.dll",
            "Microsoft.Scripting.dll",
            "Microsoft.Scripting.Metadata.dll",
            "Mono.Cecil.dll"
        };

        public override void OnLoad()
        {
            Key = Keybindings.AddKeybinding("Open scripting mod", Key);
            selecting = Keybindings.AddKeybinding("Selected gameobject", selecting);
            foreach (string s in _list)
            {
                Assembly.LoadFrom(Resource + s);
            }
            Assembly mod = Assembly.LoadFrom(Resource + "BesiegeScriptingMod.dll");
            bsmType = mod.GetType("BesiegeScriptingMod.BesiegeScriptingMod");
            bsm = Activator.CreateInstance(bsmType);
            bsmType.GetMethod("OnLoad").Invoke(bsm, new object[]{Key, selecting});
        }

        public override void OnUnload()
        {
            bsmType.GetMethod("OnUnload").Invoke(bsm, null);
        }

        public override string Name => "BesiegeScriptingMod";
        public override string DisplayName => "Besiege Scripting Mod";
        public override string Author => "JadedMortimer aka. Letum";
        public override Version Version => new Version(0, 1, 1);
        public override bool CanBeUnloaded => true;
    }
}
