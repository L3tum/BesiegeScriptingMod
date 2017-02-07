#region usings

using spaar.ModLoader;
using UnityEngine;

#endregion

namespace BesiegeScriptingMod
{
    public class BesiegeScriptingMod
    {
        private GameObject myGO;

        public void OnLoad(Key activator, Key selector, Key settings, Key bi)
        {
            myGO = new GameObject("MortimersScriptingMod");
            ScriptHandler sh = myGO.AddComponent<ScriptHandler>();
            sh.SetKeys(activator, selector, settings, bi);
            myGO.AddComponent<DontDestroyOnLoady>();
        }

        public void OnUnload()
        {
            myGO.GetComponent<ScriptHandler>().OnUnload();
            Object.Destroy(myGO);
        }

        public void ScaleX(string arg)
        {
            float scale = float.Parse(arg);
            Settings.WinSize.width = Settings.WinSize.width*scale;
        }

        public void ScaleY(string arg)
        {
            float scale = float.Parse(arg);
            Settings.WinSize.height = Settings.WinSize.height*scale;
        }
    }
}