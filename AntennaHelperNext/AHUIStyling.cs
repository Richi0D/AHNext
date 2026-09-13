using UnityEngine;

namespace AntennaHelperNext
{
    public static class AHUIStyling
    {
        public static readonly GUIStyle DefaultLabel;
        public static readonly GUIStyle BoldLabel;
        public static readonly GUIStyle CenterLabel;
        public static readonly GUIStyle HeaderLabel;
        public static readonly GUIStyle SecondaryDefaultLabel;
        public static readonly GUIStyle SecondaryBoldLabel;
        public static readonly GUIStyle SecondaryCenterLabel;        
        public static readonly GUIStyle EditorBarLabelLeft;
        public static readonly GUIStyle EditorBarLabelCenter;
        public static readonly GUIStyle EditorBarLabelRight;
        public static readonly GUIStyle SeparatorLine;
        public static readonly GUIStyle ButtonDefault;
        public static readonly GUIStyle ButtonBold;
        public static readonly GUIStyle ButtonRed;
        public static readonly GUIStyle ButtonGreen;
        public static readonly GUIStyle ButtonSelected;

        
        private static readonly RectOffset LabelMargin = new RectOffset(4, 4, 1, 1);
        private static readonly RectOffset LabelPadding = new RectOffset(0, 0, 1, 1);
        static AHUIStyling()
        {
            Color baseColor = GUI.skin.label.normal.textColor;
            Color secondaryColor = baseColor * 0.8f;
            secondaryColor.a = baseColor.a;
            
            // Default label
            DefaultLabel = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                margin = LabelMargin,
                padding = LabelPadding,
            };
                
            // Bold label
            BoldLabel = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                fontStyle = FontStyle.Bold,
                margin = LabelMargin,
                padding = new RectOffset(0, 0, 2, 2)
            };

            // Centered label
            CenterLabel = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true,
                margin = LabelMargin,
                padding = LabelPadding
            };
            
            // Secondary Default label
            SecondaryDefaultLabel = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                margin = LabelMargin,
                padding = LabelPadding,
                normal = {textColor = secondaryColor}
            };
                
            // Secondary Bold label
            SecondaryBoldLabel = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                fontStyle = FontStyle.Bold,
                margin = LabelMargin,
                padding = LabelPadding,
                normal = {textColor = secondaryColor}
            };

            // Secondary Centered label
            SecondaryCenterLabel = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true,
                margin = LabelMargin,
                padding = LabelPadding,
                normal = {textColor = secondaryColor}
            };            

            // Header label (bold)
            HeaderLabel = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true,
                margin = LabelMargin,
                padding = LabelPadding
                //fontSize = GUI.skin.label.fontSize + 2 //Does not work
            };
            
            // Editor Bar Text
            EditorBarLabelLeft = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                alignment = TextAnchor.MiddleLeft,
            };
            EditorBarLabelLeft.normal.textColor = Color.black;
            EditorBarLabelCenter = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                alignment = TextAnchor.MiddleCenter,
            };
            EditorBarLabelCenter.normal.textColor = Color.black;
            EditorBarLabelRight = new GUIStyle(GUI.skin.GetStyle("Label"))
            {
                alignment = TextAnchor.MiddleRight,
            };
            EditorBarLabelRight.normal.textColor = Color.black;
            
            // Default Button
            ButtonDefault = new GUIStyle (GUI.skin.GetStyle("Button"));
            
            // Bold Button
            ButtonBold = new GUIStyle (GUI.skin.GetStyle("Button"));
            ButtonBold.fontStyle = FontStyle.Bold;
            
            // Selected button
            ButtonSelected = new GUIStyle(GUI.skin.GetStyle("Button"));
            ButtonSelected.normal.textColor = AHColors.Selector;
            
            // Red button
            ButtonRed = new GUIStyle(GUI.skin.GetStyle("Button"));
            ButtonRed.normal.textColor = Color.red;

            // Green button
            ButtonGreen = new GUIStyle(GUI.skin.GetStyle("Button"));
            ButtonGreen.normal.textColor = Color.green;
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