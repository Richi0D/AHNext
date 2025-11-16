using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.Localization;
using ToolbarControl_NS;
using CommNet;

namespace AntennaHelperNext
{
	[KSPAddon(KSPAddon.Startup.TrackingStation, false)]
	public class AHTrackingStation : MonoBehaviour
	{
		
		public void Start()
		{
			if (!HighLogic.CurrentGame.Parameters.CustomParams<AntennaHelperGameSettings>().enableInTrackingStation)
			{
				Destroy(this);
				return;
			}
			
			// Toolbar
			GameEvents.onGUIApplicationLauncherReady.Add(AddToolbarButton);
			GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveToolbarButton);			
			
			// get all flying and editor vessels
			AHShipList.UpdateShipLists(editorOnlyRelayShips: false);
			// get all planets
			AHPlanetList.LoadPlanetList();   			
			
			// Cloud points
			DefinedParticleMeshes.Init(); // init mesh before circles!
			AHMapCircle.Init();
			
			// fetch active Vessel and Antennas
			AHMapCircle.inMapView = true;
			GetActiveVessel();
			
			GameEvents.onPlanetariumTargetChanged.Add(NewTarget);
			//GameEvents.OnMapFocusChange.Add(NewTarget); // this doubles the onPlanetariumTargetChanged
			GameEvents.onVesselDestroy.Add(VesselDestroy);
			GameEvents.CommNet.OnCommStatusChange.Add(CommNetUpdate);
			GameEvents.onGameSceneSwitchRequested.Add(QuitEditor);
			
			// Hook into rendering
			Camera.onPostRender += OnPostRenderCam;
        }
		
		
		private void OnPostRenderCam(Camera cam)
		{
			// Draw when Button is active
			if (TrackingStationWindows.ContainsKey("TrackingMain"))
			{
				if (!TrackingStationWindows["TrackingMain"].IsVisible) return;
			}
			// Draw only in Map View
			if (!MapView.MapIsEnabled || HighLogic.LoadedScene == GameScenes.SPACECENTER)
				return;
			
			// Only draw in planetarium (Tracking Station) camera
			if (cam != PlanetariumCamera.Camera) return;
			
			// Draw circles
			AHMapCircle.Render();
		}

		public void OnDestroy()
		{
			// Remove Hook into rendering
			Camera.onPostRender -= OnPostRenderCam;
			
			// Toolbar
			GameEvents.onGUIApplicationLauncherReady.Remove (AddToolbarButton);
			GameEvents.onGUIApplicationLauncherDestroyed.Remove (RemoveToolbarButton);
			RemoveToolbarButton();
			
			GameEvents.onPlanetariumTargetChanged.Remove(NewTarget);
			//GameEvents.OnMapFocusChange.Remove(NewTarget);
			GameEvents.onVesselDestroy.Remove(VesselDestroy);
			GameEvents.CommNet.OnCommStatusChange.Remove(CommNetUpdate);
			
			GameEvents.onGameSceneSwitchRequested.Remove (QuitEditor);
			// save positions and at last destroy the instance
			AntennaHelperSettings.Save();
			Destroy(this);
		}
		
		public void GetActiveVessel()
		{
			var target = PlanetariumCamera.fetch?.target;
			if (target != null && target.type == MapObject.ObjectType.Vessel)
			{
				AHMapCircle.activeVessel = (target.vessel.vesselName, target.vessel.id, target.vessel);
				AHMapCircle.ActiveShipAntennas = new AHShipAntennas(); // create new instance, otherwise we overwrite another one.
				AHMapCircle.ActiveShipAntennas.FetchAntennas(AHMapCircle.activeVessel.vessel.protoVessel.protoPartSnapshots, false);
				//StartCoroutine(AHUtil.UpdateKerbalismRateNextFrame(target.vessel)); // For Kerbalism get antenna rates
				AHMapCircle.ActiveShipAntennas.GetKerbalismRate(target.vessel);
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
		
		private void NewTarget (MapObject targetMapObject = null)
		{
			// only update when we have a vessel to avoid loose of bubbles
			if (targetMapObject != null && targetMapObject.type == MapObject.ObjectType.Vessel)
			{
				GetActiveVessel();
			}
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
		
		
		private void VesselDestroy (Vessel v = null)
		{
			if (v == null) {
				Debug.Log ("[AH] a null vessel is destroyed");
			}
			
			if (v == AHMapCircle.activeVessel.vessel) {
				Debug.Log ("[AH] the active vessel is destroyed");
			}
			// any other vessel is destroyed, update the list of vessels
			AHShipList.UpdateShipLists(editorOnlyRelayShips: false);
			GetActiveVessel();
		}			
		
		
		public void QuitEditor (GameEvents.FromToAction<GameScenes, GameScenes> eData)
		{
			AntennaHelperSettings.Save();
			foreach (var win in TrackingStationWindows.Keys)
				WindowInfo.CloseWindow(win, TrackingStationWindows);
		}		
		
        #region GUI
        // window positions
        public static readonly Dictionary<string, WindowInfo> TrackingStationWindows = new Dictionary<string, WindowInfo>()
        {
	        { "TrackingMain", new WindowInfo(
		        835289,
		        new Rect(AntennaHelperSettings.WindowPositions["tracking_station_main_window_position"], new Vector2(250, 150)),
		        AHTrackingStationWindows.MainWindow,
		        Localizer.Format ("#autoLOC_AH_0001"),
		        saveKey:"tracking_station_main_window_position")
	        },
	        { "TrackingTargetVAB", new WindowInfo(
		        415656,
		        new Rect (new Vector2 (
				        AntennaHelperSettings.WindowPositions["tracking_station_main_window_position"].x-260, 
				        AntennaHelperSettings.WindowPositions["tracking_station_main_window_position"].y)
			        , new Vector2 (260, 200)),
		        AHTrackingStationWindows.ShipListWindowVAB,
		        Localizer.Format ("#autoLOC_AH_0017") + " " + Localizer.Format ("#autoLOC_AH_0019"),
		        parentWindow: "TrackingMain",
		        lockDragToParent: true,
		        lockLower: false
		        )
	        },
	        { "TrackingTargetSPH", new WindowInfo(
		        568736,
		        new Rect (new Vector2 (
				        AntennaHelperSettings.WindowPositions["tracking_station_main_window_position"].x-260, 
				        AntennaHelperSettings.WindowPositions["tracking_station_main_window_position"].y)
			        , new Vector2 (260, 200)),
		        AHTrackingStationWindows.ShipListWindowSPH,
		        Localizer.Format ("#autoLOC_AH_0017") + " " + Localizer.Format ("#autoLOC_AH_0020"),
		        parentWindow: "TrackingMain",
		        lockDragToParent: true,
		        lockLower: false
				)
			}
        };
        
		public void OnGUI ()
		{
			WindowInfo.onGuiWindow(TrackingStationWindows);		
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
				KSP.UI.Screens.ApplicationLauncher.AppScenes.TRACKSTATION,
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
			foreach (var win in TrackingStationWindows.Keys)
				WindowInfo.CloseWindow(win, TrackingStationWindows);

			if (toolbarControl != null) {
				toolbarControl.OnDestroy ();
				Destroy (toolbarControl);
			}
		}

		private void ToolbarButtonOnTrue ()
		{
			WindowInfo.ShowWindow("TrackingMain", TrackingStationWindows);
		}

		private void ToolbarButtonOnFalse ()
		{
			foreach (var win in TrackingStationWindows.Keys)
				WindowInfo.CloseWindow(win, TrackingStationWindows);
		}
		#endregion		
		
	}
}