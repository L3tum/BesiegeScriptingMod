#region usings

using System;
using BesiegeScriptingMod.LibrariesForScripts;
using NLua;
using NLua.Exceptions;
using spaar.ModLoader;
using UnityEngine;

#endregion

namespace BesiegeScriptingMod.Lua
{
    public class LuaBehaviour : MonoBehaviour
    {
        private NLua.Lua env;
        private string source;

        public void SourceCodening(string sauce)
        {
            source = @sauce;
        }

        public bool Awakening()
        {
            Game.OnSimulationToggle += GameOnOnSimulationToggle;
            Game.OnLevelWon += GameOnOnLevelWon;
            env = new NLua.Lua();
            env.LoadCLRPackage();
            env["this"] = this; // Give the script access to the gameobject.
            env["transform"] = transform;
            env["gameObject"] = gameObject;
            env["enabled"] = enabled;
            env["useAPI"] = new Action(UseAPI);
            env["disableAPI"] = new Action(DisableAPI);

            if (Settings.useAPI)
            {
                Besiege.SetUp();
                env["besiege"] = Besiege._besiege;
            }
            try
            {
                env.DoString(source);
            }
            catch (LuaException e)
            {
                Debug.LogError(FormatException(e), gameObject);
                return false;
            }
            Call("Awake");
            return true;
        }

        private void GameOnOnLevelWon()
        {
            Call("OnLevelWon");
        }

        private void GameOnOnSimulationToggle(bool simulating)
        {
            Call("OnSimulationToggle", simulating);
        }

        private void Awake()
        {
            Call("Awake");
        }

        private void Start()
        {
            Call("Start");
        }

        private void Update()
        {
            Call("Update");
        }

        private void OnGUI()
        {
            Call("OnGUI");
        }

        private void FixedUpdate()
        {
            Call("FixedUpdate");
        }

        private void LateUpdate()
        {
            Call("LateUpdate");
        }

        private void OnLevelWasLoaded(int level)
        {
            Call("OnLevelWasLoaded", level);
        }

        public void OnDestroy()
        {
            Call("OnDestroy");
        }

        private void UseAPI()
        {
            Settings.useAPI = true;
            Besiege.SetUp();
            env["besiege"] = Besiege._besiege;
        }

        private void DisableAPI()
        {
            Settings.useAPI = false;
            env["besiege"] = null;
        }

        public object[] Call(string function, params object[] args)
        {
            object[] result = new object[0];
            if (env == null) return result;
            LuaFunction lf = env.GetFunction(function);
            if (lf == null) return result;
            try
            {
                if (args != null)
                {
                    result = lf.Call(args);
                }
                else
                {
                    result = lf.Call();
                }
            }
            catch (LuaException e)
            {
                Debug.LogError(FormatException(e), gameObject);
                throw e;
            }
            return result;
        }

        public object[] Call(string function)
        {
            return Call(function, null);
        }

        public static string FormatException(LuaException e)
        {
            string source = string.IsNullOrEmpty(e.Source) ? "<no source>" : e.Source.Substring(0, e.Source.Length - 2);
            return string.Format("{0}\nLua (at {2})", e.Message, string.Empty, source);
        }
    }
}