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
		public void Start()
		{
			if ((!HighLogic.CurrentGame.Parameters.CustomParams<AntennaHelperGameSettings> ().enableInFlight) 
			    && !HighLogic.CurrentGame.Parameters.CustomParams<AntennaHelperGameSettings> ().enableInMapView) {
				Destroy (this);
				return;
			}
			
			// Toolbar
			GameEvents.onGUIApplicationLauncherReady.Add(AddToolbarButton);
			GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveToolbarButton);			
			
			// get all flying
			AHShipList.UpdateShipLists(doSavedShips: false);
			// get all planets
			AHPlanetList.LoadPlanetList();
			
			// Cloud points
			DefinedParticleMeshes.Init(); // init mesh before circles!
			AHMapCircle.Init();
			
			// fetch active Vessel and Antennas
			AHMapCircle.inMapView = false;
			GetActiveVessel();	
			
			GameEvents.onVesselWasModified.Add (VesselModified);
			GameEvents.onVesselChange.Add (VesselSwitch);
			GameEvents.onVesselDestroy.Add (VesselDestroy);
			GameEvents.CommNet.OnCommStatusChange.Add(CommNetUpdate);

			GameEvents.OnMapEntered.Add(EnteringMap);
			GameEvents.OnMapExited.Add(ExitingMap);
			
			GameEvents.onGameSceneSwitchRequested.Add(QuitEditor);
			
			// Hook into rendering
			Camera.onPostRender += OnPostRenderCam;

		}


		public void OnDestroy()
		{
			// Remove Hook into rendering
			Camera.onPostRender -= OnPostRenderCam;
			
			// Toolbar
			GameEvents.onGUIApplicationLauncherReady.Remove (AddToolbarButton);
			GameEvents.onGUIApplicationLauncherDestroyed.Remove (RemoveToolbarButton);
			RemoveToolbarButton();
			
			GameEvents.onVesselWasModified.Remove (VesselModified); // fires also on docking and undocking
			GameEvents.onVesselChange.Remove (VesselSwitch);
			GameEvents.onVesselDestroy.Remove (VesselDestroy);
			GameEvents.CommNet.OnCommStatusChange.Remove(CommNetUpdate);

			GameEvents.OnMapEntered.Remove(EnteringMap);
			GameEvents.OnMapExited.Remove(ExitingMap);
			
			GameEvents.onGameSceneSwitchRequested.Remove(QuitEditor);
			// save positions and at last destroy the instance
			AntennaHelperSettings.Save();
			Destroy(this);
		}
		
		
		private void OnPostRenderCam(Camera cam)
		{
			// Draw when Button is active
			if (FlightWindows.ContainsKey("FlightMain"))
			{
				if (!FlightWindows["FlightMain"].IsVisible) return;
			}
			
			// Draw only in Map View
			if (!MapView.MapIsEnabled || HighLogic.LoadedScene == GameScenes.SPACECENTER)
				return;
			
			// Only draw in planetarium (Tracking Station) camera
			if (cam != PlanetariumCamera.Camera) return;
			
			// Draw circles
			AHMapCircle.Render();
		}
		
		
		// physics update
		public void FixedUpdate()
		{
			if (FlightWindows["FlightMain"].IsVisible)
			{
				AntennaStateWatcher();
				
				// Kerbalism antenna rate calculation laggs a bit behind, so we frequently check it here
				if (KerbalismApi.usingKerbalism)
				{
					Vessel v = FlightGlobals.ActiveVessel;
					if (v != null)
					{
						AHMapCircle.ActiveShipAntennas.GetKerbalismRate(v);
					}
				}				
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
			Vessel v = FlightGlobals.ActiveVessel;
			if (v != null)
			{
				AHMapCircle.activeVessel = (v.vesselName, v.id, v);
				AHMapCircle.ActiveShipAntennas = new AHShipAntennas(); // create new instance, otherwise we overwrite another one.
				AHMapCircle.ActiveShipAntennas.FetchAntennas(AHMapCircle.activeVessel.vessel.parts, false);
				AHMapCircle.selectedShipType = AHTargetType.FLIGHT;
				AHMapCircle.OnVesselChange();
			}
			else
			{
				AHMapCircle.activeVessel = (null, Guid.Empty, null);
				AHMapCircle.ActiveShipAntennas = new AHShipAntennas();
				AHMapCircle.selectedShipType = AHTargetType.DSN; // we just set it to DSN, because we don't have a vessel selected.
				AHMapCircle.OnVesselChange();
			}
		}
		
		private void VesselModified (Vessel v = null)
		{
			// when undocking and docking the vessel list might change with new or removed relays. So update all
			AHShipList.UpdateShipLists(doSavedShips: false);
			AHMapCircle.InitRelayBubbles(); // update the bubbles for new relays
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
			
			if (v == AHMapCircle.activeVessel.vessel) {
				Debug.Log ("[AH] the active vessel is destroyed");
				Destroy (this);
				return;
			}
			// any other vessel is destroyed, update the list of vessels
			AHShipList.UpdateShipLists(doSavedShips: false);
		}	
		
		private void CommNetUpdate (Vessel v, bool b)
		{
			// i guess we only need an update when the active vessel changes commnet
			if (AHMapCircle.activeVessel.vessel != null &&
			    v != null &&
			    AHMapCircle.activeVessel.vessel == v)
			{
				AHMapCircle.OnVesselChange();
			}
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
			if (AHMapCircle.activeVessel.vessel == null) return;
			foreach (var antenna in AHMapCircle.activeVessel.vessel.FindPartModulesImplementing<ModuleDeployableAntenna>())
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
					AHMapCircle.ActiveShipAntennas.FetchAntennas(AHMapCircle.activeVessel.vessel.parts, false);
					// also update ranges
					AHMapCircle.UpdateBubbleRanges();
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
		        AHTrackingStationWindows.MainWindow,
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