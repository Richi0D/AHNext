using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;
using ToolbarControl_NS;
using CommNet;

namespace AntennaHelperNext
{
	[KSPAddon(KSPAddon.Startup.TrackingStation, false)]
	public class AHTrackingStation : MonoBehaviour
	{

		// Trackingstation variables for GUI
		public static float trackingStationLevel;
		public static double DSNPower = 0;
		// Vessel variables
		public static Vessel activeVessel;
		public static AHShipAntennas ActiveShipAntennas = new AHShipAntennas();
		
		//debugging
		public static double debugSignalStrength = 0;
		public static string debugPath = "";
		public static CommPath debugCommPath;
		
		public void Start()
		{
			if (!HighLogic.CurrentGame.Parameters.CustomParams<AntennaHelperGameSettings>().enableInTrackingStation)
			{
				Destroy(this);
				return;
			}
			
			// init trackingstation variables for GUI
			trackingStationLevel = ScenarioUpgradeableFacilities.GetFacilityLevel (SpaceCenterFacility.TrackingStation);
			DSNPower = GameVariables.Instance.GetDSNRange (trackingStationLevel);
			AHMapCircle.displayType = AHDisplayType.ACTIVE;

			// Toolbar
			GameEvents.onGUIApplicationLauncherReady.Add(AddToolbarButton);
			GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveToolbarButton);			
			
			// get all flying and editor vessels
			AHShipList.UpdateShipLists();
			// get all planets
			AHPlanetList.LoadPlanetList();   			
			
			// fetch active Vessel and Antennas
			AHMapCircle.selectedShipType = AHTargetType.FLIGHT;
			GetActiveVessel();
			ActiveShipAntennas.UpdateRanges(DSNPower);			
			
			GameEvents.onPlanetariumTargetChanged.Add(NewTarget);
			GameEvents.OnMapFocusChange.Add(NewTarget);
			// GameEvents.CommNet.OnCommStatusChange.Add(CommNetUpdate);
			GameEvents.onGameSceneSwitchRequested.Add (QuitEditor);
			
			// Cloud points
			AHMapCircle.LoadMat();
			DefinedParticleMeshes.Init();
			
			// Hook into rendering
			Camera.onPostRender += OnPostRenderCam;
        }
		
		
		private void OnPostRenderCam(Camera cam)
		{
			if (!MapView.MapIsEnabled || HighLogic.LoadedScene == GameScenes.SPACECENTER)
				return;
			
			// Only draw in planetarium (Tracking Station) camera
			if (cam != PlanetariumCamera.Camera) return;
			
			
			ParticleMesh mesh = DefinedParticleMeshes.MediumCloud;
			
			// Scale to ScaledSpace (Tracking Station uses scaled coords)
			CelestialBody body = FlightGlobals.GetHomeBody();
			Vector3d scaledPos = ScaledSpace.LocalToScaledSpace(body.position);
			double radius = body.Radius * 1.2;
			Matrix4x4 m = Matrix4x4.TRS(
				scaledPos,
				Quaternion.identity,
				Vector3.one * ScaledSpace.InverseScaleFactor * (float)radius
			);
			// set color
			pointMat.SetColor("POINT_COLOR", new Color(0.0f, 0.9f, 0.0f, 0.8f));
			pointMat.SetFloat("POINT_SIZE", 10.0f);				
			// enable material
			pointMat.SetPass(0);
			// render cloud
			mesh.Render(m);
			
			ParticleMesh meshTwo = DefinedParticleMeshes.MediumCloud;
			
			// Scale to ScaledSpace (Tracking Station uses scaled coords)
			double radiusTwo = body.Radius * 2.2;
			Matrix4x4 mTwo = Matrix4x4.TRS(
				scaledPos,
				Quaternion.identity,
				Vector3.one * ScaledSpace.InverseScaleFactor * (float)radiusTwo
			);
			// set color
			pointMat.SetColor("POINT_COLOR", new Color(0.9f, 0.9f, 0.0f, 0.8f));
			pointMat.SetFloat("POINT_SIZE", 10.0f);				
			// enable material
			pointMat.SetPass(0);
			// render cloud
			meshTwo.Render(mTwo);			
			
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
			GameEvents.OnMapFocusChange.Remove(NewTarget);
			// GameEvents.CommNet.OnCommStatusChange.Remove(CommNetUpdate);
			
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
				activeVessel = target.vessel;
				ActiveShipAntennas = new AHShipAntennas(); // create new instance, otherwise we overwrite another one.
				ActiveShipAntennas.FetchAntennas(activeVessel.protoVessel.protoPartSnapshots, false);
				AHMapCircle.selectedShipType = AHTargetType.FLIGHT;
				activeCommPathVessels = AHCommNet.GetCommPathVessels(activeVessel);
				
				// Guid vid = activeVessel.protoVessel.vesselID;
				// Debug.Log("Active Vessel: " + activeVessel.protoVessel.vesselID);
				// foreach (ProtoVessel vessel in AHShipList.FlightProtoShipList.Keys)
				// {
				// 	if (vessel.vesselID == vid)
				// 	{
				// 		Debug.Log("Found Vessel in Flight ProtoShipList");
				// 	}
				// 	else
				// 	{
				// 		Debug.Log("[AH] Vessel not found in Flight ProtoShipList");
				// 	}
				// }
			}
			else
			{
				activeVessel = null;
				ActiveShipAntennas = new AHShipAntennas();
				AHMapCircle.selectedShipType = AHTargetType.DSN; // we just set it to DSN, because we don't have a vessel selected.
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