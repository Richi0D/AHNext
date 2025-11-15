using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ClickThroughFix;
using EdyCommonTools;

namespace AntennaHelperNext
{
    public class WindowInfo
    {
        public int ID;
        public Rect Position;
        public bool IsVisible;
        public GUI.WindowFunction DrawFunction;  // delegate to GUI function
        public string Title;
        public string ParentWindow;
        public string SaveKey;
        public float MinHeight;
        public bool LockDragToParent;
        public bool LockLower;

        public WindowInfo(
            int id,
            Rect position,
            GUI.WindowFunction drawFunc,
            string title,
            string parentWindow = null,
            string saveKey = null,
            float minHeight = 40,
            bool lockDragToParent = false,
            bool lockLower = true
            )
        {
            ID = id;
            Position = position;
            DrawFunction = drawFunc;
            Title = title;
            SaveKey = saveKey;
            ParentWindow = parentWindow;
            MinHeight = minHeight;
            IsVisible = false;
            LockDragToParent = lockDragToParent;
            LockLower = lockLower;
        }
        
        public static void ShowWindow(string name, Dictionary<string, WindowInfo> winDictonary) =>
            winDictonary[name].IsVisible = true;
        
        public static void CloseWindow(string name, Dictionary<string, WindowInfo> winDictonary)
        {
            if (winDictonary.TryGetValue(name, out var win))
            {
                if (!string.IsNullOrEmpty(win.SaveKey))
                    AntennaHelperSettings.SavePosition(win.SaveKey, win.Position.position);
                win.IsVisible = false;
		        
                // close children windows
                var childWindows = winDictonary
                    .Where(pair => pair.Value.ParentWindow == name)
                    .Select(pair => pair.Key)
                    .ToList();
                foreach (var childWin in childWindows)
                {
                    CloseWindow(childWin, winDictonary);
                }
            }
        }        
        
        public static Vector2 ExtendWindowPos (Rect originalWindow, Rect childWindow, bool lower=true)
        {
            if (lower)
            {
                float yPos;
                if (originalWindow.position.y + originalWindow.height * 2 > Screen.height) {
                    yPos = originalWindow.position.y - originalWindow.height;
                } else {
                    yPos = originalWindow.position.y + originalWindow.height;
                }
                return new Vector2 (originalWindow.position.x, yPos);
            }
            else
            {
                float xPos;
                if (originalWindow.position.x - childWindow.width * 2 < 0) {
                    xPos = originalWindow.position.x + childWindow.width;
                } else {
                    xPos = originalWindow.position.x - childWindow.width;
                }
                return new Vector2 (xPos, originalWindow.position.y);                
            }
        }

        public static void onGuiWindow(Dictionary<string, WindowInfo> winDictonary)
        {
            // set Skin
            if (!HighLogic.CurrentGame.Parameters.CustomParams<AntennaHelperGameSettings>().altSkin)
                GUI.skin = HighLogic.Skin;

            // if visible show windows
            foreach (var kv in winDictonary)
            {
                var win = kv.Value;
				
                // skip windows that are not visible
                if (!win.IsVisible) 
                    continue;
				
                // adjust position relative to child window
                if (win.ParentWindow != null  && 
                    winDictonary.TryGetValue(win.ParentWindow, out var parentWin) && 
                    win.LockDragToParent)
                {
                    win.Position.position = ExtendWindowPos(parentWin.Position, win.Position, win.LockLower);
                }

                if (kv.Key == "FlightMain")
                {
                    // reset size for flight main window, map window is always bigger so we need to return to original height
                    win.Position.height = 150;
                }
				
                win.Position = ClickThruBlocker.GUILayoutWindow(
                    win.ID,          // You can assign a unique int per window in EditorWindowInfo
                    win.Position,
                    win.DrawFunction,
                    win.Title,
                    GUILayout.MinHeight (win.MinHeight)
                );				
            }	
        }
        
    }
}