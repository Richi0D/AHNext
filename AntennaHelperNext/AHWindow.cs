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
        public bool LockBottom;

        public WindowInfo(
            int id,
            Rect position,
            GUI.WindowFunction drawFunc,
            string title,
            string parentWindow = null,
            string saveKey = null,
            float minHeight = 40,
            bool lockDragToParent = false,
            bool lockLower = true,
            bool lockBottom = false
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
            LockBottom = lockBottom;
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

        // stick window to the lower left of the parent, bottom edges aligned.
        // the height fills the space below windows sticking to the upper left of the same parent
        private static void AlignBottomLeft(WindowInfo win, WindowInfo parentWin, Dictionary<string, WindowInfo> winDictonary)
        {
            float top = parentWin.Position.y;
            foreach (var sibling in winDictonary.Values)
            {
                if (sibling != win && sibling.IsVisible &&
                    sibling.ParentWindow == win.ParentWindow &&
                    sibling.LockDragToParent && !sibling.LockLower && !sibling.LockBottom)
                {
                    top = Mathf.Max(top, sibling.Position.yMax);
                }
            }
            win.Position.height = Mathf.Max(win.MinHeight, parentWin.Position.yMax - top);
            float xPos = ExtendWindowPos(parentWin.Position, win.Position, false).x;
            win.Position.position = new Vector2(xPos, parentWin.Position.yMax - win.Position.height);
        }

        private static float lastMinWindowHeightFlight = 150;
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
                    if (win.LockBottom)
                    {
                        AlignBottomLeft(win, parentWin, winDictonary);
                    }
                    else
                    {
                        win.Position.position = ExtendWindowPos(parentWin.Position, win.Position, win.LockLower);
                    }
                }

                if (kv.Key == "FlightMain" && !AHMapCircle.inMapView
                    && win.Position.height > lastMinWindowHeightFlight)
                {
                    // it is a bit of a dirty fix
                    // reset size for flight main window, map window is always bigger so we need to return to original height
                    win.Position.height = 200;
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