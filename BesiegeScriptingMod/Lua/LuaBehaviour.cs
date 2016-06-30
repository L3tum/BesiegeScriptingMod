using System;
using BesiegeScriptingMod.LibrariesForScripts;
using NLua;
using UnityEngine;

namespace BesiegeScriptingMod.Lua
{
    public class LuaBehaviour : MonoBehaviour
    {
        String source;
        NLua.Lua env;

        public void SourceCodening(String sauce)
        {
            this.source = @sauce;
        }

        public bool Awakening()
        {
            spaar.ModLoader.Game.OnSimulationToggle += GameOnOnSimulationToggle;
            spaar.ModLoader.Game.OnLevelWon += GameOnOnLevelWon;
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
            catch (NLua.Exceptions.LuaException e)
            {
                Debug.LogError(FormatException(e), context: gameObject);
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

        void Awake()
        {
            Call("Awake");
        }

        void Start()
        {
            Call("Start");
        }

        void Update()
        {
            Call("Update");
        }

        void OnGUI()
        {
            Call("OnGUI");
        }

        void FixedUpdate()
        {
            Call("FixedUpdate");
        }

        void LateUpdate()
        {
            Call("LateUpdate");
        }

        void OnLevelWasLoaded(int level)
        {
            Call("OnLevelWasLoaded", level);
        }

        public void OnDestroy()
        {
            Call("OnDestroy");
        }

        void UseAPI()
        {
            Settings.useAPI = true;
            Besiege.SetUp();
            env["besiege"] = Besiege._besiege;
        }

        void DisableAPI()
        {
            Settings.useAPI = false;
            env["besiege"] = null;
        }

        public System.Object[] Call(string function, params System.Object[] args)
        {
            System.Object[] result = new System.Object[0];
            if (env == null) return result;
            LuaFunction lf = env.GetFunction(function);
            if (lf == null) return result;
            try
            {
                if (args != null)
                {
                    result = lf.Call(args);
                }
                else {
                    result = lf.Call();
                }
            }
            catch (NLua.Exceptions.LuaException e)
            {
                Debug.LogError(FormatException(e), gameObject);
                throw e;
            }
            return result;
        }

        public System.Object[] Call(string function)
        {
            return Call(function, null);
        }

        public static string FormatException(NLua.Exceptions.LuaException e)
        {
            string source = (string.IsNullOrEmpty(e.Source)) ? "<no source>" : e.Source.Substring(0, e.Source.Length - 2);
            return string.Format("{0}\nLua (at {2})", e.Message, string.Empty, source);
        }
    }
}