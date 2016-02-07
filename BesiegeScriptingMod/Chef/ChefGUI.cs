using System;
using UnityEngine;

namespace BesiegeScriptingMod.Chef
{
    class ChefGUI : MonoBehaviour
    {
        public String input = "";
        public String label = "";

        public void OnGUI()
        {
            GUILayout.Window(spaar.ModLoader.Util.GetWindowID(), new Rect(0, 0, 100, 100), Func, "Chef Input Prompt");
        }

        private void Func(int id)
        {
            GUILayout.BeginArea(new Rect(0, 0, 100, 100));
            GUILayout.Label(label);
            input = GUILayout.TextField(input);
            GUILayout.EndArea();
        }
    }
}
