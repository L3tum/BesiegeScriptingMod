using System;
using NLua;
using UnityEngine;

namespace BesiegeScriptingMod
{
    public class LuaBehaviour : MonoBehaviour
    {
        String source;
        Lua env;

        public void SourceCodening(String sauce)
        {
            this.source = @sauce;
        }

        public void Awakening()
        {
            env = new Lua();
            env.LoadCLRPackage();
            env["this"] = this; // Give the script access to the gameobject.
            env["transform"] = transform;
            env["gameObject"] = gameObject;
            env["enabled"] = enabled; 

            try
            {
                env.DoString(source);
            }
            catch (NLua.Exceptions.LuaException e)
            {
                Debug.LogError(FormatException(e), context: gameObject);
            }
            Call("Awake");
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