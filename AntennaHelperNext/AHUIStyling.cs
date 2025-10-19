using UnityEngine;

namespace AntennaHelperNext
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class AHUIStyling : MonoBehaviour
    {
        public static readonly GUIStyle BoldLabel;
        public static readonly GUIStyle CenterLabel;
        public static readonly GUIStyle HeaderLabel;
        public static readonly GUIStyle SeparatorLine;
        public static readonly GUIStyle ButtonDefault;
        public static readonly GUIStyle ButtonBold;
        public static readonly GUIStyle ButtonRed;
        public static readonly GUIStyle ButtonGreen;

        static AHUIStyling()
        {
            // Bold label
            BoldLabel = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold
            };

            // Centered label
            CenterLabel = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };

            // Header label (bold + slightly larger)
            HeaderLabel = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = GUI.skin.label.fontSize + 2,
                alignment = TextAnchor.MiddleCenter
            };

            // Horizontal separator line
            SeparatorLine = new GUIStyle(GUI.skin.box)
            {
                fixedHeight = 1,
                stretchWidth = true,
                margin = new RectOffset(4, 4, 4, 4)
            };
            
            // Default Button
            ButtonDefault = new GUIStyle (GUI.skin.button);
            
            // Bold Button
            ButtonBold = new GUIStyle (GUI.skin.button);
            ButtonBold.fontStyle = FontStyle.Bold;

            // Red button
            ButtonRed = new GUIStyle(GUI.skin.button);
            ButtonRed.normal.textColor = Color.red;
            ButtonRed.hover.textColor = Color.red;

            // Green button
            ButtonGreen = new GUIStyle(GUI.skin.button);
            ButtonGreen.normal.textColor = Color.green;
            ButtonGreen.hover.textColor = Color.green;
        }

        public static void DrawSeparator()
        {
            GUILayout.Box(GUIContent.none, SeparatorLine);
        }
    }
}