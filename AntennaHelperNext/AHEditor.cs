using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.Localization;
using ToolbarControl_NS;
using ClickThroughFix;

namespace AntennaHelperNext
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class AntennaHelperEditor : MonoBehaviour
    {
	    
	    // Editor variables for GUI
	    public static float trackingStationLevel;
	    // Target variables
	    public static double targetPower = 0;
	    public static string targetName = "";
	    public static AHTargetType targetType = AHTargetType.DSN;
	    // Vessel variables
	    public static AHShipAntennas EditorShipAntennas = new AHShipAntennas();
	    public static AHShipAntennas EditorAntennasPicker = new AHShipAntennas();
	    public static (double customDistance, double customVesselSignal, double customRelaySignal) EditorCustomRange = (0, 0, 0);
	    
        // Start is called before the first frame update
        public void Start()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<AntennaHelperGameSettings>().enableInEditor)
            {
                Destroy(this);
                return;
            }
            
            // init editor variables for GUI
            trackingStationLevel = ScenarioUpgradeableFacilities.GetFacilityLevel (SpaceCenterFacility.TrackingStation);
            targetPower = GameVariables.Instance.GetDSNRange (trackingStationLevel);
            targetName = Localizer.Format ("#autoLOC_AH_0015") + " " + (int)(trackingStationLevel * 2 + 1);
			targetType = AHTargetType.DSN;
            
            // Toolbar
            GameEvents.onGUIApplicationLauncherReady.Add (AddToolbarButton);
            GameEvents.onGUIApplicationLauncherDestroyed.Add (RemoveToolbarButton);
            
            // get all flying and editor vessels
            AHShipList.UpdateShipLists();
            AHShipList.GetAntennaPartList();
            // get all planets
            AHPlanetList.LoadPlanetList();            
            
            // fetch Antennas
            EditorShipAntennas.FetchAntennas(EditorLogic.fetch.ship.Parts, true);
            EditorShipAntennas.UpdateRanges(targetPower);
			
            // attach editor logic to each event
            GameEvents.onEditorLoad.Add (VesselLoad);
            GameEvents.onEditorPartEvent.Add (PartEvent);
            GameEvents.onEditorPodPicked.Add (PodPicked);
            GameEvents.onEditorPodDeleted.Add (PodDeleted);
            GameEvents.onEditorUndo.Add (EditorUndo);
            
            GameEvents.onGameSceneSwitchRequested.Add (QuitEditor);
        }
        
        // onDestroy is called when the instance is being destroyed
        public void OnDestroy ()
        {
	        // Toolbar
            GameEvents.onGUIApplicationLauncherReady.Remove (AddToolbarButton);
            GameEvents.onGUIApplicationLauncherDestroyed.Remove (RemoveToolbarButton);
            RemoveToolbarButton();

	        // remove editor logic from each event
            GameEvents.onEditorLoad.Remove (VesselLoad);
            GameEvents.onEditorPartEvent.Remove (PartEvent);
            GameEvents.onEditorPodPicked.Remove (PodPicked);
            GameEvents.onEditorPodDeleted.Remove (PodDeleted);
            GameEvents.onEditorUndo.Remove (EditorUndo);
            
            GameEvents.onGameSceneSwitchRequested.Remove (QuitEditor);
            // save positions and at last destroy the instance
			AntennaHelperSettings.Save();
            Destroy(this);
        }
        
        // Update is called once per frame
        public void Update ()
        {
        }
        
        public void QuitEditor (GameEvents.FromToAction<GameScenes, GameScenes> eData)
        {
	        AntennaHelperSettings.Save();
	        foreach (var win in EditorWindows.Keys)
		        WindowInfo.CloseWindow(win, EditorWindows);
        }
        
        public void VesselLoad (ShipConstruct ship, KSP.UI.Screens.CraftBrowserDialog.LoadType screenType)
        {
	        RefreshAntennas();
        }
        public void PodDeleted ()
        {
	        RefreshAntennas();
        }
        public void PodPicked (Part part = null)
        {
	        RefreshAntennas();
        }
        public void EditorUndo (ShipConstruct ship)
        {
	        RefreshAntennas();
        }
        
        private void RefreshAntennas()
        {
	        EditorShipAntennas.FetchAntennas(EditorLogic.fetch.ship.Parts, true);
	        EditorShipAntennas.UpdateRanges(targetPower);
	        UpdateCustomRange(EditorCustomRange.customDistance);
        }
        
        public void PartEvent (ConstructionEventType eventType, Part part)
        {
	        
	        if (part == null) return;
	        if (eventType != ConstructionEventType.PartAttached && eventType != ConstructionEventType.PartDetached)
		        return;
	        
	        // we only need to change the list if it actually has a ModuleDataTransmitter
	        var transmitters = part.FindModulesImplementing<ModuleDataTransmitter>();
	        if (transmitters == null || transmitters.Count == 0)
		        return;
	        
	        if (eventType == ConstructionEventType.PartAttached)
	        {
		        EditorShipAntennas.AddAntenna(part);
		        // Symmetry counterparts
		        foreach (Part symPart in part.symmetryCounterparts)
		        {
			        EditorShipAntennas.AddAntenna(symPart);
		        }
		        // Child part
		        foreach (Part childPart in part.children)
		        {
			        EditorShipAntennas.AddAntenna(childPart);
		        }
	        }

	        if (eventType == ConstructionEventType.PartDetached)
	        {
		        EditorShipAntennas.RemoveAntenna(part);
		        // Symmetry counterparts
		        foreach (ModuleDataTransmitter antennaSym in EditorShipAntennas.Antennas.ToList())
		        {
			        if (antennaSym.part.isSymmetryCounterPart(part))
			        {
				        EditorShipAntennas.RemoveAntenna(antennaSym);
			        }
		        }
		        // Child part
		        foreach (Part childPart in part.children)
		        {
			        EditorShipAntennas.RemoveAntenna(childPart);
		        }
	        }
	        EditorShipAntennas.UpdateAntennas();
	        EditorShipAntennas.UpdateRanges(targetPower);
	        UpdateCustomRange(EditorCustomRange.customDistance);
        }

        public static void UpdateCustomRange(double range)
        {
	        double maxVesselRange = AHUtil.GetMaxRange(EditorShipAntennas.VesselPower, targetPower);
	        double maxRelayRange = AHUtil.GetMaxRange(EditorShipAntennas.RelayPower, targetPower);
	        double VesselSignal = AHUtil.GetSignalStrength(AHUtil.GetNormalizedRange(range, maxVesselRange));
	        double RelaySignal = AHUtil.GetSignalStrength(AHUtil.GetNormalizedRange(range, maxRelayRange));
	        EditorCustomRange = (range, VesselSignal, RelaySignal);
        }
        
        
        #region GUI
        // window positions
        private static readonly Vector2 defaultTargetSize = new Vector2 (400, 80);
        private static readonly float targetWindowHeight = 200;
        public static readonly Dictionary<string, WindowInfo> EditorWindows = new Dictionary<string, WindowInfo>()
        {
	        { "EditorMain", new WindowInfo(
		        835298,
		        new Rect(AntennaHelperSettings.WindowPositions["editor_main_window_position"], new Vector2(450, 450)),
		        AHEditorWindows.MainWindow,
		        Localizer.Format ("#autoLOC_AH_0001"),
		        saveKey:"editor_main_window_position")
	        },
	        { "EditorTarget", new WindowInfo(
			        419256,
			        new Rect(AntennaHelperSettings.WindowPositions["editor_target_window_position"], defaultTargetSize),
			        AHEditorWindows.TargetWindow,
			        Localizer.Format ("#autoLOC_AH_0007"),
			        saveKey:"editor_target_window_position",
			        parentWindow: "EditorMain"
			        )
	        },
	        { "EditorPlanet", new WindowInfo(
			        332980,
			        new Rect(AntennaHelperSettings.WindowPositions["editor_signal_strenght_per_planet_window_position"], new Vector2(450, 240)),
			        AHEditorWindows.PlanetWindow,
			        Localizer.Format ("#autoLOC_AH_0060") + " / " + Localizer.Format ("#autoLOC_AH_0059"),
			        saveKey:"editor_signal_strenght_per_planet_window_position",
			        parentWindow: "EditorMain"
			        )
	        },
	        { "EditorTargetShipEditorVAB", new WindowInfo(
			        415014,
			        new Rect (new Vector2 (
					        AntennaHelperSettings.WindowPositions["editor_target_window_position"].x, 
					        AntennaHelperSettings.WindowPositions["editor_target_window_position"].y + defaultTargetSize.y)
				        , new Vector2 (defaultTargetSize.x, targetWindowHeight)),
			        AHEditorWindows.TargetWindowShipEditorVAB,
			        Localizer.Format ("#autoLOC_AH_0017"),
			        parentWindow: "EditorTarget",
			        minHeight: defaultTargetSize.y,
			        lockDragToParent: true
		        )
	        },
	        { "EditorTargetShipEditorSPH", new WindowInfo(
			        415015,
			        new Rect (new Vector2 (
					        AntennaHelperSettings.WindowPositions["editor_target_window_position"].x, 
					        AntennaHelperSettings.WindowPositions["editor_target_window_position"].y + defaultTargetSize.y)
				        , new Vector2 (defaultTargetSize.x, targetWindowHeight)),
			        AHEditorWindows.TargetWindowShipEditorSPH,
			        Localizer.Format ("#autoLOC_AH_0017"),
			        parentWindow: "EditorTarget",
			        minHeight: defaultTargetSize.y,
			        lockDragToParent: true
		        )
	        },
	        { "EditorTargetShipFlight", new WindowInfo(
			        892715,
			        new Rect (new Vector2 (
					        AntennaHelperSettings.WindowPositions["editor_target_window_position"].x, 
					        AntennaHelperSettings.WindowPositions["editor_target_window_position"].y + defaultTargetSize.y)
				        , new Vector2 (defaultTargetSize.x, targetWindowHeight)),
			        AHEditorWindows.TargetWindowShipFlight,
			        Localizer.Format ("#autoLOC_AH_0016"),
			        parentWindow: "EditorTarget",
			        minHeight: defaultTargetSize.y,
			        lockDragToParent: true
			        )
	        },
	        { "EditorTargetPart", new WindowInfo(
			        595592,
			        new Rect (new Vector2 (
					        AntennaHelperSettings.WindowPositions["editor_target_window_position"].x, 
					        AntennaHelperSettings.WindowPositions["editor_target_window_position"].y + defaultTargetSize.y)
				        , new Vector2 (defaultTargetSize.x, targetWindowHeight)),
			        AHEditorWindows.TargetWindowPart,
			        Localizer.Format ("#autoLOC_AH_0031"),
			        parentWindow: "EditorTarget",
			        minHeight: defaultTargetSize.y,
					lockDragToParent: true
			        )
	        }
        };
        
		public void OnGUI ()
		{
			WindowInfo.onGuiWindow(EditorWindows);
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
		        WindowInfo.CloseWindow(win, EditorWindows);

            if (toolbarControl != null) {
                toolbarControl.OnDestroy ();
                Destroy (toolbarControl);
            }
        }

        private void ToolbarButtonOnTrue()
        {
	        WindowInfo.ShowWindow("EditorMain", EditorWindows);
        }

        public void ToolbarButtonOnFalse()
        {
	        foreach (var win in EditorWindows.Keys)
		        WindowInfo.CloseWindow(win, EditorWindows);
        }
        #endregion
    }
}