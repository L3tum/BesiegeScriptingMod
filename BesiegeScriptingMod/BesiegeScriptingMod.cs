using System;
using spaar.ModLoader;
using UnityEngine;

namespace BesiegeScriptingMod
{
    public class BesiegeScriptingMod : Mod
    {
        GameObject myGO;
        public override void OnLoad()
        {
            myGO = new GameObject("MortimersScriptingMod");
            myGO.AddComponent<ScriptHandler>();
            myGO.AddComponent<DontDestroyOnLoady>();
        }

        public override void OnUnload()
        {
            myGO.GetComponent<ScriptHandler>().OnUnload();
            MonoBehaviour.Destroy(myGO);
        }

        public override string Name => "BesiegeScriptingMod";
        public override string DisplayName => "Besiege Scripting Mod";
        public override string Author => "JadedMortimer aka. Letum";
        public override Version Version => new Version(0, 1, 1);
        public override bool CanBeUnloaded => true;
    }
}
