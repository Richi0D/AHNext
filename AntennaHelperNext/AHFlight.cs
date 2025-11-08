using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;
using ToolbarControl_NS;
using CommNet;

namespace AntennaHelperNext
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class AHFlight : MonoBehaviour
    {
        // Flight variables for GUI
		public static float trackingStationLevel;
		public static double DSNPower = 0;
		// Target variables
		public static AHDisplayType displayType = AHDisplayType.ACTIVE;
		// Vessel variables
		public static Vessel activeVessel;
		public static AHShipAntennas ActiveShipAntennas = new AHShipAntennas();
		public static List<ProtoVessel> activeCommPathVessels; // save here the vessels from the commpath
		
		//debugging
		public static double debugSignalStrength = 0;
		public static string debugPath = "";
		public static CommPath debugCommPath;

		public void Start()
		{
			if ((!HighLogic.CurrentGame.Parameters.CustomParams<AntennaHelperGameSettings> ().enableInFlight) 
			    && !HighLogic.CurrentGame.Parameters.CustomParams<AntennaHelperGameSettings> ().enableInMapView) {
				Destroy (this);
				return;
			}
			
			// init flight variables for GUI
			trackingStationLevel = ScenarioUpgradeableFacilities.GetFacilityLevel (SpaceCenterFacility.TrackingStation);
			DSNPower = GameVariables.Instance.GetDSNRange (trackingStationLevel);
			displayType = AHDisplayType.ACTIVE;

			// Toolbar
			GameEvents.onGUIApplicationLauncherReady.Add(AddToolbarButton);
			GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveToolbarButton);			
			
			// get all flying and editor vessels
			AHShipList.UpdateShipLists();
			// get all planets
			AHPlanetList.LoadPlanetList();
			AHMapCircle.inMapView = false;
			
			// fetch active Vessel and Antennas
			GetActiveVessel();
			ActiveShipAntennas.UpdateRanges(DSNPower);			
			
			GameEvents.onVesselWasModified.Add (VesselModified);
			GameEvents.onVesselChange.Add (VesselSwitch);
			GameEvents.onVesselDestroy.Add (VesselDestroy);

			GameEvents.OnMapEntered.Add(EnteringMap);
			GameEvents.OnMapExited.Add(ExitingMap);
			
			GameEvents.onGameSceneSwitchRequested.Add(QuitEditor);
		}


		public void OnDestroy()
		{
			// Toolbar
			GameEvents.onGUIApplicationLauncherReady.Remove (AddToolbarButton);
			GameEvents.onGUIApplicationLauncherDestroyed.Remove (RemoveToolbarButton);
			RemoveToolbarButton();
			
			GameEvents.onVesselWasModified.Remove (VesselModified);
			GameEvents.onVesselChange.Remove (VesselSwitch);
			GameEvents.onVesselDestroy.Remove (VesselDestroy);

			GameEvents.OnMapEntered.Remove(EnteringMap);
			GameEvents.OnMapExited.Remove(ExitingMap);
			
			GameEvents.onGameSceneSwitchRequested.Remove(QuitEditor);
			// save positions and at last destroy the instance
			AntennaHelperSettings.Save();
			Destroy(this);
		}
		
		public void Update ()
		{
			if (FlightWindows["FlightMain"].IsVisible)
			{
				AntennaStateWatcher();
			}
		}
		
		private void EnteringMap ()
		{
			AHMapCircle.inMapView = true;
		}

		private void ExitingMap ()
		{
			AHMapCircle.inMapView = false;
		}		
		
		public void GetActiveVessel()
		{
			activeVessel = FlightGlobals.ActiveVessel;
			ActiveShipAntennas = new AHShipAntennas(); // create new instance, otherwise we overwrite another one.
			ActiveShipAntennas.FetchAntennas(activeVessel.Parts, false);
			//activeCommPathVessels = AHCommNet.GetCommPathVessels(activeVessel);
		}
		
		private void VesselModified (Vessel v = null)
		{
			GetActiveVessel();
		}
		
		private void VesselSwitch (Vessel v)
		{
			GetActiveVessel();
		}		
		
		private void VesselDestroy (Vessel v = null)
		{
			if (v == null) {
				Debug.Log ("[AH] a null vessel is destroyed");
				GetActiveVessel();
				return;
			}
			
			if (v == activeVessel) {
				Debug.Log ("[AH] the active vessel is destroyed");
				Destroy (this);
				return;
			}
			// any other vessel is destroyed, update the list of vessels
			AHShipList.UpdateShipLists();
		}			
		
		public void QuitEditor (GameEvents.FromToAction<GameScenes, GameScenes> eData)
		{
			AntennaHelperSettings.Save();
			foreach (var win in FlightWindows.Keys)
				WindowInfo.CloseWindow(win, FlightWindows);
		}		
		
		
		// watch for antenna state changes and update the list of antennas
		private readonly Dictionary<ModuleDeployableAntenna, ModuleDeployablePart.DeployState> lastStates =
			new Dictionary<ModuleDeployableAntenna, ModuleDeployablePart.DeployState>();
		public void AntennaStateWatcher()
		{
			if (activeVessel == null) return;
			foreach (var antenna in activeVessel.FindPartModulesImplementing<ModuleDeployableAntenna>())
			{
				var currentState = antenna.deployState;
				if (!lastStates.TryGetValue(antenna, out var prevState))
				{
					lastStates[antenna] = currentState;
					continue;
				}

				if (prevState != currentState)
				{
					lastStates[antenna] = currentState;
					// since we filter extended antennas on part level, we need to update the whole antenna list
					ActiveShipAntennas.FetchAntennas(activeVessel.parts, false);
				}
			}
		}		
		
		
        #region GUI
        // window positions
        public static readonly Dictionary<string, WindowInfo> FlightWindows = new Dictionary<string, WindowInfo>()
        {
	        { "FlightMain", new WindowInfo(
		        835862,
		        new Rect(AntennaHelperSettings.WindowPositions["flight_main_window_position"], new Vector2(250, 150)),
		        AHFlightWindows.MainWindow,
		        Localizer.Format ("#autoLOC_AH_0001"),
		        saveKey:"flight_main_window_position")
	        }
        };
        
		public void OnGUI ()
		{
			WindowInfo.onGuiWindow(FlightWindows);		
		}
		#endregion	
		
		#region ToolbarButton
		private ToolbarControl toolbarControl;
		internal const string MODID = "AntennaHelper_NS";
		internal const string MODNAME = "#autoLOC_AH_0001";
		private void AddToolbarButton ()
		{
			KSP.UI.Screens.ApplicationLauncher.AppScenes scenes = 
				KSP.UI.Screens.ApplicationLauncher.AppScenes.FLIGHT 
				| KSP.UI.Screens.ApplicationLauncher.AppScenes.MAPVIEW;

			if (!HighLogic.CurrentGame.Parameters.CustomParams<AntennaHelperGameSettings> ().enableInFlight) {
				scenes = KSP.UI.Screens.ApplicationLauncher.AppScenes.MAPVIEW;
			}
			if (!HighLogic.CurrentGame.Parameters.CustomParams<AntennaHelperGameSettings> ().enableInMapView) {
				scenes = KSP.UI.Screens.ApplicationLauncher.AppScenes.FLIGHT;
			}			
			
			
			toolbarControl = gameObject.AddComponent<ToolbarControl> ();

			toolbarControl.AddToAllToolbars (
				ToolbarButtonOnTrue,
				ToolbarButtonOnFalse,
				scenes,
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
			foreach (var win in FlightWindows.Keys)
				WindowInfo.CloseWindow(win, FlightWindows);

			if (toolbarControl != null) {
				toolbarControl.OnDestroy ();
				Destroy (toolbarControl);
			}
		}

		private void ToolbarButtonOnTrue ()
		{
			WindowInfo.ShowWindow("FlightMain", FlightWindows);
		}

		private void ToolbarButtonOnFalse ()
		{
			foreach (var win in FlightWindows.Keys)
				WindowInfo.CloseWindow(win, FlightWindows);
		}
		#endregion		        
        
    }
}