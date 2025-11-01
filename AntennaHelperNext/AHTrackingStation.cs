using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;
using KSP.Localization;
using ToolbarControl_NS;
using ClickThroughFix;

namespace AntennaHelperNext
{
	[KSPAddon(KSPAddon.Startup.TrackingStation, false)]
	public class AHTrackingStation : MonoBehaviour
	{

		// Editor variables for GUI
		public static float trackingStationLevel;
		// Target variables
		public static double DSNPower = 0;
		public static AHDisplayType displayType = AHDisplayType.ACTIVE;
		public static AHTargetType selectedShipType = AHTargetType.FLIGHT;
		// Vessel variables
		public static Vessel activeVessel;
		public static AHShipAntennas ActiveShipAntennas = new AHShipAntennas();

		public void Start()
		{
			if (!HighLogic.CurrentGame.Parameters.CustomParams<AntennaHelperGameSettings>().enableInTrackingStation)
			{
				Destroy(this);
				return;
			}
			
			// init editor variables for GUI
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
			
			// fetch active Vessel and Antennas
			selectedShipType = AHTargetType.FLIGHT;
			GetActiveVessel();
			ActiveShipAntennas.UpdateRanges(DSNPower);			
			
			GameEvents.onPlanetariumTargetChanged.Add(NewTarget);
			GameEvents.OnMapFocusChange.Add(NewTarget);
			// GameEvents.CommNet.OnCommStatusChange.Add(CommNetUpdate);
			
			GameEvents.onGameSceneSwitchRequested.Add (QuitEditor);
		}

		public void OnDestroy()
		{
			// Toolbar
			GameEvents.onGUIApplicationLauncherReady.Remove (AddToolbarButton);
			GameEvents.onGUIApplicationLauncherDestroyed.Remove (RemoveToolbarButton);
			RemoveToolbarButton();
			
			GameEvents.onPlanetariumTargetChanged.Remove(NewTarget);
			GameEvents.OnMapFocusChange.Remove(NewTarget);
			// GameEvents.CommNet.OnCommStatusChange.Remove(CommNetUpdate);
			
			GameEvents.onGameSceneSwitchRequested.Remove (QuitEditor);
			// save positions and at last destroy the instance
			AntennaHelperSettings.Save();
			Destroy(this);
		}
		
		public void Update ()
		{
		}

		public static void GetActiveVessel()
		{
			var target = PlanetariumCamera.fetch?.target;
			if (target != null && target.type == MapObject.ObjectType.Vessel)
			{
				activeVessel = target.vessel;
				Debug.Log("Active part count: " + activeVessel.protoVessel.protoPartSnapshots.Count);
				ActiveShipAntennas.FetchAntennas(activeVessel.protoVessel.protoPartSnapshots, false);
				Debug.Log("Active vessel: " + activeVessel.vesselName);
				Debug.Log("Active power: " + ActiveShipAntennas.RelayPower);
			}
			else
			{
				activeVessel = null;
				ActiveShipAntennas = new AHShipAntennas();
			}
		}
		
		private void NewTarget (MapObject targetMapObject = null)
		{
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
				        AntennaHelperSettings.WindowPositions["tracking_station_main_window_position"].x-250, 
				        AntennaHelperSettings.WindowPositions["tracking_station_main_window_position"].y)
			        , new Vector2 (250, 200)),
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
				        AntennaHelperSettings.WindowPositions["tracking_station_main_window_position"].x-250, 
				        AntennaHelperSettings.WindowPositions["tracking_station_main_window_position"].y)
			        , new Vector2 (250, 200)),
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