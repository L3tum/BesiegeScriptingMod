#region usings

using UnityEngine;

#endregion

namespace BesiegeScriptingMod.Chef
{
    internal class ChefGUI : MonoBehaviour
    {
        public string input = "";
        public string label = "";

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