using UnityEngine;

namespace AntennaHelperNext
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class AHUIStyling
    {
        public static readonly GUIStyle DefaultLabel;
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
            
            // Default label
            DefaultLabel = new GUIStyle(GUI.skin.GetStyle("Label"));
                
            // Bold label
            BoldLabel = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                fontStyle = FontStyle.Bold
            };

            // Centered label
            CenterLabel = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                alignment = TextAnchor.MiddleCenter
            };

            // Header label (bold)
            HeaderLabel = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true,
                //fontSize = GUI.skin.label.fontSize + 2 //Does not work
            };
            
            // Default Button
            ButtonDefault = new GUIStyle (GUI.skin.GetStyle("Button"));
            
            // Bold Button
            ButtonBold = new GUIStyle (GUI.skin.GetStyle("Button"));
            ButtonBold.fontStyle = FontStyle.Bold;

            // Red button
            ButtonRed = new GUIStyle(GUI.skin.GetStyle("Button"));
            ButtonRed.normal.textColor = Color.red;
            ButtonRed.hover.textColor = Color.red;

            // Green button
            ButtonGreen = new GUIStyle(GUI.skin.GetStyle("Button"));
            ButtonGreen.normal.textColor = Color.green;
            ButtonGreen.hover.textColor = Color.green;
        }
        
        public static void DrawSeparator(float height = 2f)
        {
            // Make a style that uses the texture as background
            GUIStyle lineStyle = new GUIStyle();
            lineStyle.normal.background = StartVariables.separatorTex;
            lineStyle.margin = new RectOffset(4, 4, 4, 4);
            // Draw a space (stretch horizontally, fixed height)
            GUILayout.Box(GUIContent.none, lineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(height));
        }
    }
}