using System;
using System.Collections.Generic;
using System.Reflection;
using spaar.ModLoader;
using UnityEngine;
using Commands = spaar.Commands;
using Console = spaar.ModLoader.Internal.Tools.Console;

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
            Commands.RegisterCommand("ScaleX", ScaleX, "Scales width of the Scripting Mod");
            Commands.RegisterCommand("ScaleY", ScaleY, "Scales heigth of the Scripting Mod");
            foreach (string s in _list)
            {
                Assembly.LoadFrom(Resource + s);
            }
            Assembly mod = Assembly.LoadFrom(Resource + "BesiegeScriptingMod.dll");
            bsmType = mod.GetType("BesiegeScriptingMod.BesiegeScriptingMod");
            bsm = Activator.CreateInstance(bsmType);
            bsmType.GetMethod("OnLoad").Invoke(bsm, new object[]{Key, selecting});
        }

        private string ScaleY(string[] args, IDictionary<string, string> namedArgs)
        {
            bsmType.GetMethod("ScaleY").Invoke(bsm, new[] {args[0]});
            return "Scaled by " + args[0];
        }

        private string ScaleX(string[] args, IDictionary<string, string> namedArgs)
        {
            bsmType.GetMethod("ScaleX").Invoke(bsm, new[] {args[0]});
            return "Scaled by " + args[0];
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
