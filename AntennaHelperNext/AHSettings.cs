using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using ToolbarControl_NS;
using KSP.Localization;

namespace AntennaHelperNext
{
	
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class RegisterToolbar : MonoBehaviour
	{
		void Start()
		{
			ToolbarControl.RegisterMod(AntennaHelperEditor.MODID, Localizer.Format(AntennaHelperEditor.MODNAME));
		}
	}
	
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class StartVariables : MonoBehaviour
	{
		public static string ApplicationRootPath;
		public static Texture signalPerDistanceTex;
		public static float uiScale;
		public static Texture2D separatorTex;
		public static Color EditorbarColor;

		void Start()
		{
			ApplicationRootPath = KSPUtil.ApplicationRootPath;
			// Load textures
			signalPerDistanceTex = (Texture)GameDatabase.Instance.GetTexture ("AntennaHelperNext/Textures/signal_per_distance", false);
			EditorbarColor = new Color(122f/255, 161f/255, 186f/255);
			// scaling parameter
			uiScale = GameSettings.UI_SCALE;
			// create texture for gui seperator
			InitSeparatorTex();
			
		}
		
		public static void InitSeparatorTex()
		{
			// Create a simple colored texture (1x1 pixel)
			Color separatorColor = new Color(48f/255, 63f/255, 73f/255);
			separatorTex = new Texture2D(1,1);
			separatorTex.SetPixel(0,0, separatorColor);
			separatorTex.Apply();
		}
	}
	
	// Add Mod settings to Game Parameters
	public class AntennaHelperGameSettings : GameParameters.CustomParameterNode
	{
		public override string Title {
			get {
				return /*"Antenna Helper"*/Localizer.Format ("#autoLOC_AH_0001");
			}
		}
		public override string Section {
			get {
				return /*"Antenna Helper"*/Localizer.Format ("#autoLOC_AH_0001");
			}
		}
		public override string DisplaySection {
			get {
				return /*"Antenna Helper"*/Localizer.Format ("#autoLOC_AH_0001");
			}
		}
		public override int SectionOrder {
			get {
				return 1;
			}
		}
		public override GameParameters.GameMode GameMode {
			get {
				return GameParameters.GameMode.ANY;
			}
		}
		public override bool HasPresets {
			get {
				return false;
			}
		}

		[GameParameters.CustomParameterUI (/*"Enable in the Editor"*/"#autoLOC_AH_0065", toolTip = "#autoLOC_AH_0072")]
		public bool enableInEditor = true;

		[GameParameters.CustomParameterUI (/*"Enable in the Tracking Station"*/"#autoLOC_AH_0066", toolTip = "#autoLOC_AH_0072")]
		public bool enableInTrackingStation = true;

		[GameParameters.CustomParameterUI (/*"Enable in Flight"*/"#autoLOC_AH_0067", toolTip = "#autoLOC_AH_0072")]
		public bool enableInFlight = true;

		[GameParameters.CustomParameterUI (/*"Enable in the MapView"*/"#autoLOC_AH_0068", toolTip = "#autoLOC_AH_0072")]
		public bool enableInMapView = true;
		
		[GameParameters.CustomParameterUI("Alternate skin", toolTip = "Use the alternate skin")]
		public bool altSkin = false;

		public override bool Enabled (MemberInfo member, GameParameters parameters)
		{
			return true;
		}
	}
	
	// Handle internal settings here. like save to .cfg
	public static class AntennaHelperSettings
	{
		private static ConfigNode settingsNode;
		private static ConfigNode nodePosWindows;

		// Path to settings.cfg
		public static string location_settings = "GameData/AntennaHelperNext/Settings.cfg";
		
		// default window positions
		private static readonly Dictionary<string, Vector2> defaultPositions = new Dictionary<string, Vector2>()
		{
			// Editor
			{ "editor_main_window_position", new Vector2(Screen.width / 2f, Screen.height / 2f) },
			{ "editor_target_window_position", new Vector2(Screen.width / 2f - 400f, Screen.height / 2f) },
			{ "editor_signal_strenght_per_planet_window_position", new Vector2(Screen.width / 2f + 400f, Screen.height / 2f) },

			// Flight
			{ "flight_main_window_position", new Vector2(Screen.width / 2f, Screen.height / 2f) },
			{ "flight_map_view_window_position", new Vector2(Screen.width / 2f + 300f, Screen.height / 2f) },

			// Tracking Station
			{ "tracking_station_main_window_position", new Vector2(Screen.width - 150f, Screen.height - 285f) },
			{ "tracking_station_ship_window_position", new Vector2(Screen.width - 500f, Screen.height - 285f) }
		};
		public static readonly Dictionary<string, Vector2> WindowPositions = new Dictionary<string, Vector2>(defaultPositions);
		
		static AntennaHelperSettings ()
		{
			Load ();
			Save ();
		}

		public static void Load()
		{
			string path = Path.Combine(StartVariables.ApplicationRootPath, location_settings);
			settingsNode = ConfigNode.Load(path);
			if (settingsNode == null)
				settingsNode = new ConfigNode();

			if (!settingsNode.HasNode("Windows_Position"))
				settingsNode.AddNode("Windows_Position");

			nodePosWindows = settingsNode.GetNode("Windows_Position");

			foreach (KeyValuePair<string, Vector2> kvp in defaultPositions)
			{
				if (nodePosWindows.HasValue(kvp.Key))
				{
					WindowPositions[kvp.Key] = ConfigNode.ParseVector2(nodePosWindows.GetValue(kvp.Key));
				}
				else
				{
					nodePosWindows.AddValue(kvp.Key, kvp.Value);
				}
			}
		}

		public static void SavePosition (string windowName, Vector2 position)
		{
			WindowPositions[windowName] = position;
			nodePosWindows.SetValue(windowName, position, true);
		}

		public static void Save()
		{
			string path = StartVariables.ApplicationRootPath + location_settings;
			settingsNode.Save(path);
		}
	}
}

