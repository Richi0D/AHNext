using System;
using UnityEngine;

namespace AntennaHelperNext
{
    public class WindowInfo
    {
        public int ID;
        public Rect Position;
        public bool IsVisible;
        public GUI.WindowFunction DrawFunction;  // delegate to GUI function
        public string Title;
        public string ChildWindow;
        public string SaveKey;
        public float MinHeight;

        public WindowInfo(
            int id,
            Rect position,
            GUI.WindowFunction drawFunc,
            string title,
            string childWindow = null,
            string saveKey = null,
            float minHeight = 0
            )
        {
            ID = id;
            Position = position;
            DrawFunction = drawFunc;
            Title = title;
            SaveKey = saveKey;
            ChildWindow = childWindow;
            MinHeight = minHeight;
            IsVisible = false;
        }
    }
    
    
    
    
    
}