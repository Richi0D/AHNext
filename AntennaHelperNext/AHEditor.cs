using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.Localization;
using ToolbarControl_NS;

namespace AntennaHelperNext
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class AntennaHelperEditor : MonoBehaviour
    {
	    
	    private static AntennaHelperEditor instance;
	    
        // Start is called before the first frame update
        public void Start()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<AntennaHelperGameSettings>().enableInEditor)
            {
                Destroy(this);
                return;
            }
            
            GameEvents.onGUIApplicationLauncherReady.Add (AddToolbarButton);
            GameEvents.onGUIApplicationLauncherDestroyed.Add (RemoveToolbarButton);

            // GameEvents.onEditorLoad.Add (VesselLoad);
            // GameEvents.onEditorPartEvent.Add (PartEvent);
            // GameEvents.onEditorPodPicked.Add (PodPicked);
            // GameEvents.onEditorPodDeleted.Add (PodDeleted);

            // GameEvents.onGameSceneSwitchRequested.Add (QuitEditor);
        }
        
        // onDestroy is called when the script instance is being destroyed
        public void OnDestroy ()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove (AddToolbarButton);
            GameEvents.onGUIApplicationLauncherDestroyed.Remove (RemoveToolbarButton);
            RemoveToolbarButton();

            // GameEvents.onEditorLoad.Remove (VesselLoad);
            // GameEvents.onEditorPartEvent.Remove (PartEvent);
            // GameEvents.onEditorPodPicked.Remove (PodPicked);
            // GameEvents.onEditorPodDeleted.Remove (PodDeleted);

            // GameEvents.onGameSceneSwitchRequested.Remove (QuitEditor);
        }
        
        // VesselLoad is called when a ship is loaded
        public void VesselLoad (ShipConstruct ship, KSP.UI.Screens.CraftBrowserDialog.LoadType screenType)
        {
            //CreateAntennaList ();
            //DoTheMath ();
        }        
        
        // Update is called once per frame
        public void Update ()
        {
            // Control locks to avoid controls in the editor when Mouse is over one of the windows
            bool mouseOverWindow = EditorWindows.Values.Any(w => w.IsVisible && w.Position.Contains(Mouse.screenPos));

            const string lockID = "AntennaHelper_inputLock";
            const ControlTypes lockType = ControlTypes.UI | ControlTypes.EDITOR_PAD_PICK_PLACE | ControlTypes.CAMERACONTROLS;

            if (mouseOverWindow)
            {
	            InputLockManager.SetControlLock(lockType, lockID);
            }
            else
            {
	            InputLockManager.RemoveControlLock(lockID);
            }
        }
        
        
        #region GUI
        // window positions
        private static readonly Vector2 defaultTargetSize = new Vector2 (400, 80);
        public static readonly Dictionary<string, WindowInfo> EditorWindows = new Dictionary<string, WindowInfo>()
        {
	        { "EditorMain", new WindowInfo(
		        835298,
		        new Rect(AntennaHelperSettings.WindowPositions["editor_main_window_position"], new Vector2(400, 200)),
		        AHEditorWindows.MainWindow,
		        Localizer.Format ("#autoLOC_AH_0001"),
		        saveKey:"editor_main_window_position")
	        },
	        { "EditorTarget", new WindowInfo(
			        419256,
			        new Rect(AntennaHelperSettings.WindowPositions["editor_target_window_position"], defaultTargetSize),
			        AHEditorWindows.TargetWindow,
			        Localizer.Format ("#autoLOC_AH_0007"),
			        saveKey:"editor_target_window_position"
			        )
	        },
	        { "EditorPlanet", new WindowInfo(
			        332980,
			        new Rect(AntennaHelperSettings.WindowPositions["editor_signal_strenght_per_planet_window_position"], new Vector2(450, 240)),
			        AHEditorWindows.PlanetWindow,
			        Localizer.Format ("#autoLOC_AH_0060") + " / " + Localizer.Format ("#autoLOC_AH_0059"),
			        saveKey:"editor_signal_strenght_per_planet_window_position"
			        )
	        },
	        { "EditorTargetShipEditor", new WindowInfo(
			        415014,
			        new Rect (new Vector2 (
					        AntennaHelperSettings.WindowPositions["editor_target_window_position"].x, 
					        AntennaHelperSettings.WindowPositions["editor_target_window_position"].y + defaultTargetSize.y)
				        , new Vector2 (400, 150)),
			        AHEditorWindows.TargetWindowShipEditor,
			        Localizer.Format ("#autoLOC_AH_0017"),
			        childWindow: "EditorTarget",
			        minHeight: defaultTargetSize.y
		        )
	        },
	        { "EditorTargetShipFlight", new WindowInfo(
			        892715,
			        new Rect(AntennaHelperSettings.WindowPositions["editor_target_window_position"], defaultTargetSize),
			        AHEditorWindows.TargetWindowShipFlight,
			        Localizer.Format ("#autoLOC_AH_0016"),
			        childWindow: "EditorTarget",
			        minHeight: defaultTargetSize.y
			        )
	        },
	        { "EditorTargetPart", new WindowInfo(
			        595592,
			        new Rect(AntennaHelperSettings.WindowPositions["editor_target_window_position"], defaultTargetSize),
			        AHEditorWindows.TargetWindowPart,
			        Localizer.Format ("#autoLOC_AH_0031"),
			        childWindow: "EditorTarget",
			        minHeight: defaultTargetSize.y
			        )
	        }
        };
        
        public static void ShowWindow(string name) =>
	        EditorWindows[name].IsVisible = true;
        
        public static void CloseWindow(string name)
        {
	        if (EditorWindows.TryGetValue(name, out var win))
	        {
		        if (!string.IsNullOrEmpty(win.SaveKey))
			        AntennaHelperSettings.SavePosition(win.SaveKey, win.Position.position);
		        win.IsVisible = false;

		        if (name == "EditorTarget")
		        {
			        // close children windows
			        CloseWindow("EditorTargetShipEditor");
			        CloseWindow("EditorTargetShipFlight");
			        CloseWindow("EditorTargetPart");
		        }
	        }
        }

		private Vector2 ExtendWindowPos (Rect originalWindow)
		{
			float yPos;
			if (originalWindow.position.y + originalWindow.height * 2 > Screen.height) {
				yPos = originalWindow.position.y - originalWindow.height;
			} else {
				yPos = originalWindow.position.y + originalWindow.height;
			}
			return new Vector2 (originalWindow.position.x, yPos);
		}

		public void OnGUI ()
		{
			// set Skin
			if (!HighLogic.CurrentGame.Parameters.CustomParams<AntennaHelperGameSettings>().altSkin)
				GUI.skin = HighLogic.Skin;

			// if visible show windows
			foreach (var kv in EditorWindows)
			{
				var win = kv.Value;
				
				// skip windows that are not visible
				if (!win.IsVisible) 
					continue;
				
				// adjust position relative to child window
				if (win.ChildWindow != null && EditorWindows.TryGetValue(win.ChildWindow, out var childWin))
				{
					win.Position.position = ExtendWindowPos(childWin.Position);
				}
				

				GUILayout.BeginArea(win.Position);
				win.Position = GUILayout.Window(
					win.ID,          // You can assign a unique int per window in EditorWindowInfo
					win.Position,
					win.DrawFunction,
					win.Title,
					GUILayout.MinHeight (win.MinHeight)
				);
				GUILayout.EndArea();
			}			
		}
		#endregion
        
		
        #region ToolbarButton
        private ToolbarControl toolbarControl;
        internal const string MODID = "AntennaHelper_NS";
        internal const string MODNAME = "#autoLOC_AH_0001";
        private void AddToolbarButton ()
        {
            toolbarControl = gameObject.AddComponent<ToolbarControl> ();

            toolbarControl.AddToAllToolbars (
                ToolbarButtonOnTrue,
                ToolbarButtonOnFalse,
                KSP.UI.Screens.ApplicationLauncher.AppScenes.VAB | KSP.UI.Screens.ApplicationLauncher.AppScenes.SPH,
                MODID,
                "823779",
                "AntennaHelperNext/Textures/icon_dish_on",
                "AntennaHelperNext/Textures/icon_off",
                "AntennaHelperNext/Textures/icon_dish_on_small",
                "AntennaHelperNext/Textures/icon_dish_off_small",
                Localizer.Format (MODNAME));

        }

        private void RemoveToolbarButton ()
        {
	        foreach (var win in EditorWindows.Keys)
		        CloseWindow(win);

            if (toolbarControl != null) {
                toolbarControl.OnDestroy ();
                Destroy (toolbarControl);
            }
        }

        private void ToolbarButtonOnTrue ()
        {
	        ShowWindow("EditorMain");
        }

        private void ToolbarButtonOnFalse ()
        {
	        foreach (var win in EditorWindows.Keys)
		        CloseWindow(win);
        }
        #endregion
    }
}