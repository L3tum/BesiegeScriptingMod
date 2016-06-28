using System;
using System.Globalization;
using spaar.ModLoader.UI;
using UnityEngine;

namespace BesiegeScriptingMod
{
    class SettingsGUI : MonoBehaviour
    {
        private int id = spaar.ModLoader.Util.GetWindowID();
        private Rect rect = new Rect(200.0f, 200.0f, 400.0f, 400.0f);
        private bool active;
        private int width = (int)Settings.WinSize.width;
        private int height = (int)Settings.WinSize.height;
        public void OnGUI()
        {
            if (active)
            {
                GUI.skin = ModGUI.Skin;
                rect = GUI.Window(id, rect, Func, "Settings");
            }
        }

        private void Func(int id)
        {
            GUIStyle toggleStyle = new GUIStyle(GUI.skin.button) { onNormal = Elements.Buttons.Red.hover };
            Settings.useAPI = GUILayout.Toggle(Settings.useAPI,
                new GUIContent("Use API[" + Settings.useAPI + "]", "Enables the API for Scripts. This creates more load, but can also only temporarily be activated by a Script if it has to use it."), toggleStyle);
            GUILayout.Label("Width of the main window:");
            width = int.Parse(GUILayout.TextField(width.ToString(CultureInfo.InvariantCulture)));
            GUILayout.Label("Height of the main window:");
            height = int.Parse(GUILayout.TextField(height.ToString(CultureInfo.InvariantCulture)));
            if ((int)Settings.WinSize.height != height)
            {
                Settings.WinSize.height = height;
            }
            if ((int) Settings.WinSize.width != width)
            {
                Settings.WinSize.width = width;
            }
            GUI.DragWindow();
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.P))
            {
                active = !active;
            }
        }
    }
}
