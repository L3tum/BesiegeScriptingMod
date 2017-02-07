#region usings

using System.Globalization;
using spaar.ModLoader;
using spaar.ModLoader.UI;
using UnityEngine;

#endregion

namespace BesiegeScriptingMod
{
    internal class SettingsGUI : MonoBehaviour
    {
        private bool active;
        private int height = (int) Settings.WinSize.height;
        private readonly int id = spaar.ModLoader.Util.GetWindowID();
        private Rect rect = new Rect(200.0f, 200.0f, 400.0f, 400.0f);
        private Key settingsGUI;
        private int width = (int) Settings.WinSize.width;

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
            GUIStyle toggleStyle = new GUIStyle(GUI.skin.button) {onNormal = Elements.Buttons.Red.hover, fontSize = 24};
            Settings.useAPI = GUILayout.Toggle(Settings.useAPI,
                new GUIContent("Use API[" + Settings.useAPI + "]", "Enables the API for Scripts. This creates more load, but can also only temporarily be activated by a Script if it has to use it."),
                toggleStyle);
            GUILayout.Space(25.0f);
            GUILayout.Label("Width Of The Scripting Window", new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter, fontSize = 24, normal = Elements.Buttons.Disabled.normal});
            GUILayout.BeginHorizontal();
            GUILayout.Space(50.0f);
            width = int.Parse(GUILayout.TextField(width.ToString(CultureInfo.InvariantCulture), new GUIStyle(GUI.skin.textField) {alignment = TextAnchor.MiddleCenter, fontSize = 22}));
            GUILayout.EndHorizontal();

            GUILayout.Space(25.0f);

            GUILayout.Label("Height Of The Scripting Window", new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter, fontSize = 24, normal = Elements.Buttons.Disabled.normal});
            GUILayout.BeginHorizontal();
            GUILayout.Space(50.0f);
            height = int.Parse(GUILayout.TextField(height.ToString(CultureInfo.InvariantCulture), new GUIStyle(GUI.skin.textField) {alignment = TextAnchor.MiddleCenter, fontSize = 22}));
            GUILayout.EndHorizontal();

            if ((int) Settings.WinSize.height != height)
            {
                Settings.WinSize.height = height;
            }
            if ((int) Settings.WinSize.width != width)
            {
                Settings.WinSize.width = width;
            }
            gameObject.GetComponent<ScriptHandler>().LastTooltip = GUI.tooltip;
            GUI.DragWindow();
        }

        public void Update()
        {
            if (settingsGUI.Pressed())
            {
                active = !active;
            }
        }

        internal void SetKey(Key key1)
        {
            settingsGUI = key1;
        }
    }
}