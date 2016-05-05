﻿using System;
using System.Collections.Generic;
using spaar.ModLoader;
using UnityEngine;

namespace BesiegeScriptingMod
{
    public class BesiegeScriptingMod
    {
        GameObject myGO;
        public void OnLoad(Key activator, Key selector)
        {
            myGO = new GameObject("MortimersScriptingMod");
            ScriptHandler sh = (ScriptHandler)myGO.AddComponent<ScriptHandler>();
            sh.SetKeys(activator, selector);
            myGO.AddComponent<DontDestroyOnLoady>();
        }

        public void OnUnload()
        {
            myGO.GetComponent<ScriptHandler>().OnUnload();
            MonoBehaviour.Destroy(myGO);
        }
    }
}
