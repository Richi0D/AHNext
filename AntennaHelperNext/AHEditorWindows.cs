using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;

namespace AntennaHelperNext
{
	
	public static class AHEditorWindows
	{
		
		// Close button for all windows
		private static void DrawCloseButton(string windowName)
		{
			var winInfo = AntennaHelperEditor.EditorWindows[windowName];
			var rect = new Rect(winInfo.Position.width - 22, 2, 20, 18);

			if (GUI.Button(rect, "X"))
			{
				if (windowName == "EditorTarget")
				{
					// be sure other windows are closed again
					AntennaHelperEditor.CloseWindow("EditorTargetShipFlight");
					AntennaHelperEditor.CloseWindow("EditorTargetShipEditor");
					AntennaHelperEditor.CloseWindow("EditorTargetPart");
				}
				AntennaHelperEditor.CloseWindow(windowName);
			}
		}
		
		// Get Text for direct or relay antenna
		private static string GetAntennaTypeText(bool isDirect)
		{
			if (isDirect)
			{
				return Localizer.Format("#autoLOC_AH_0002");
			}
			else
			{
				return Localizer.Format("#autoLOC_AH_0003");
			}
		}
		
		// simplify Antenna Values and ranges
		public static string ToKMG(double value, bool useMetricSuffix = false, int decimalPlaces = 0)
		{
			string[] suffixes = useMetricSuffix ? new string[] { "km", "Mm", "Gm" } : new string[] { "k", "M", "G" };

			double absValue = Math.Abs(value);

			if (absValue >= 1_000_000_000f)
				return (value / 1_000_000_000f).ToString($"F{decimalPlaces}") + suffixes[2]; // G / Gm
			else if (absValue >= 1_000_000f)
				return (value / 1_000_000f).ToString($"F{decimalPlaces}") + suffixes[1];     // M / Mm
			else if (absValue >= 1_000f)
				return (value / 1_000f).ToString($"F{decimalPlaces}") + suffixes[0];         // k / km
			else
				return value.ToString($"F{decimalPlaces}");                                   // no suffix
		}
		
		
		public static void MainWindow (int id)
		{
			float widthFirstCol = AntennaHelperEditor.EditorWindows["EditorMain"].Position.width * .26f;
			float widthSecondCol = AntennaHelperEditor.EditorWindows["EditorMain"].Position.width * .37f;
			// Signal bar positions, 4 colors and 5 labels, list contains pos x and width for each label
			float margins = 10f;
			float barlabelmulitplier = (AntennaHelperEditor.EditorWindows["EditorMain"].Position.width-2*margins)/8;
			List<float> pos100 = new List<float>
			{
				margins,
				barlabelmulitplier,
			};
			List<float> pos75 = new List<float>
			{
				margins + barlabelmulitplier,
				barlabelmulitplier*2,
			};			
			List<float> pos50 = new List<float>
			{
				margins + barlabelmulitplier*3,
				barlabelmulitplier*2,
			};	
			List<float> pos25 = new List<float>
			{
				margins + barlabelmulitplier*5,
				barlabelmulitplier*2,
			};				
			List<float> pos0 = new List<float>
			{
				margins + barlabelmulitplier*7,
				barlabelmulitplier,
			};				
			
			// Close Button
			DrawCloseButton("EditorMain");
			
			// Start UI
			GUILayout.BeginVertical ();
			AHUIStyling.DrawSeparator();
			GUILayout.Space (5f);
			
			// Target Selection
			//GUILayout.Label(/*Target*/Localizer.Format ("#autoLOC_AH_0100"), AHUIStyling.HeaderLabel);
			if (GUILayout.Button (/*Pick A Target*/Localizer.Format ("#autoLOC_AH_0007"), AHUIStyling.ButtonDefault)) {
				if (AntennaHelperEditor.EditorWindows["EditorTarget"].IsVisible) {
					AntennaHelperEditor.CloseWindow("EditorTarget");
				} else {
					// be sure other windows are closed again
					AntennaHelperEditor.CloseWindow("EditorTargetShipFlight");
					AntennaHelperEditor.CloseWindow("EditorTargetShipEditor");
					AntennaHelperEditor.CloseWindow("EditorTargetPart");
					AntennaHelperEditor.ShowWindow("EditorTarget");
				}
			}
			GUILayout.BeginHorizontal ();
			GUILayout.Label( /*Current target*/Localizer.Format("#autoLOC_AH_0006") + " : ",
				AHUIStyling.DefaultLabel, GUILayout.Width(widthFirstCol));
			GUILayout.Label(AntennaHelperEditor.targetName, AHUIStyling.DefaultLabel);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal ();
			GUILayout.Label( /*Target Power*/Localizer.Format("#autoLOC_AH_0101") + " : ",
				AHUIStyling.DefaultLabel,GUILayout.Width(widthFirstCol));
			GUILayout.Label(  ToKMG(AntennaHelperEditor.targetPower), AHUIStyling.DefaultLabel);
			GUILayout.EndHorizontal();
			AHUIStyling.DrawSeparator();
			
			// Current Vessel
			//GUILayout.Label(/*Current Vessel*/Localizer.Format ("#autoLOC_AH_0102"), AHUIStyling.HeaderLabel);
			GUILayout.BeginHorizontal ();
			GUILayout.Label(/*Type*/Localizer.Format("#autoLOC_AH_0004") + " : ",
				AHUIStyling.DefaultLabel, GUILayout.Width(widthFirstCol));
			GUILayout.Label(/*Vessel*/Localizer.Format("#autoLOC_AH_0039"),
				AHUIStyling.BoldLabel, GUILayout.Width(widthSecondCol));
			GUILayout.Label(/*Relay*/Localizer.Format("#autoLOC_AH_0003"),
				AHUIStyling.BoldLabel);	
			GUILayout.EndHorizontal();
			// Number display :
			GUILayout.BeginHorizontal();
			GUILayout.Label(/*Status*/Localizer.Format("#autoLOC_AH_0008") + " : ",
				AHUIStyling.DefaultLabel,  GUILayout.Width(widthFirstCol));
			GUILayout.Label(/*Vessel*/Localizer.Format("#autoLOC_AH_0042", new string[] {
				(AntennaHelperEditor.EditorShipAntennas.DirectCombAntennas.Count + AntennaHelperEditor.EditorShipAntennas.RelayCombAntennas.Count).ToString (),
				AntennaHelperEditor.EditorShipAntennas.Antennas.Count.ToString ()
			}), AHUIStyling.DefaultLabel, GUILayout.Width(widthSecondCol));
			GUILayout.Label(/*Relay*/Localizer.Format("#autoLOC_AH_0042", new string[] {
				AntennaHelperEditor.EditorShipAntennas.RelayCombAntennas.Count.ToString (),
				AntennaHelperEditor.EditorShipAntennas.RelayAntennas.Count.ToString ()
			}), AHUIStyling.DefaultLabel);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label(/*Power*/Localizer.Format("#autoLOC_AH_0009") + " : ",
				AHUIStyling.DefaultLabel, GUILayout.Width(widthFirstCol));
			GUILayout.Label(/*Vessel*/ToKMG(AntennaHelperEditor.EditorShipAntennas.VesselPower,decimalPlaces:2),
				AHUIStyling.DefaultLabel, GUILayout.Width(widthSecondCol));
			GUILayout.Label(/*Relay*/ToKMG(AntennaHelperEditor.EditorShipAntennas.RelayPower,decimalPlaces:2),
				AHUIStyling.DefaultLabel);	
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label(/*Max Range*/Localizer.Format("#autoLOC_AH_0010") + " : ",
				AHUIStyling.DefaultLabel, GUILayout.Width(widthFirstCol));
			GUILayout.Label(/*Vessel*/ToKMG(AntennaHelperEditor.EditorShipAntennas.VesselRangesMax[0],
				true, 2), AHUIStyling.DefaultLabel, GUILayout.Width(widthSecondCol));
			GUILayout.Label(/*Relay*/ToKMG(AntennaHelperEditor.EditorShipAntennas.RelayRangesMax[0],
				true, 2), AHUIStyling.DefaultLabel);	
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label(/*Range 100%*/Localizer.Format("#autoLOC_AH_0011") + " : ",
				AHUIStyling.DefaultLabel, GUILayout.Width(widthFirstCol));
			GUILayout.Label(/*Vessel*/ToKMG(AntennaHelperEditor.EditorShipAntennas.VesselRangesMax[100],
				true, 2), AHUIStyling.DefaultLabel, GUILayout.Width(widthSecondCol));
			GUILayout.Label(/*Relay*/ToKMG(AntennaHelperEditor.EditorShipAntennas.RelayRangesMax[100],
				true, 2), AHUIStyling.DefaultLabel);	
			GUILayout.EndHorizontal();			
			AHUIStyling.DrawSeparator();
			
			// Signal Color Bar
			//GUILayout.Space (16f);
			GUILayout.Label(/*Vessel Ranges*/Localizer.Format("#autoLOC_AH_0103"), AHUIStyling.HeaderLabel);
			GUILayout.BeginHorizontal ();
			GUILayout.Label(/*Placeholder*/"", AHUIStyling.DefaultLabel, GUILayout.Width(pos100[1]));
			GUILayout.Label(/*75*/ToKMG(AntennaHelperEditor.EditorShipAntennas.VesselRangesMax[75],
				true, 2), AHUIStyling.CenterLabel, GUILayout.Width(pos75[1]));
			GUILayout.Label(/*50*/ToKMG(AntennaHelperEditor.EditorShipAntennas.VesselRangesMax[50],
				true, 2), AHUIStyling.CenterLabel, GUILayout.Width(pos50[1]));
			GUILayout.Label(/*25*/ToKMG(AntennaHelperEditor.EditorShipAntennas.VesselRangesMax[25],
				true, 2), AHUIStyling.CenterLabel, GUILayout.Width(pos25[1]));
			// GUILayout.Label(/*0*/ToKMG(AntennaHelperEditor.EditorShipAntennas.VesselRangesMax[0],
			// 	true, 2), AHUIStyling.CenterLabel, GUILayout.Width(pos100[1]));			
			GUILayout.EndHorizontal ();
			GUILayout.Label (StartVariables.signalPerDistanceTex, GUILayout.ExpandWidth(true));
			// position the text labels on the signal bar
			Rect baseRect = GUILayoutUtility.GetLastRect();
			Rect rect100   = new Rect(pos100[0] + 2, baseRect.y, pos100[1], baseRect.height);
			Rect rect75   = new Rect(pos75[0], baseRect.y, pos75[1], baseRect.height);
			Rect rect50 = new Rect(pos50[0], baseRect.y, pos50[1], baseRect.height);
			Rect rect25   = new Rect(pos25[0], baseRect.y, pos25[1], baseRect.height);
			Rect rect0   = new Rect(pos0[0], baseRect.y, pos0[1], baseRect.height);
			GUI.Label(rect100, "100%", AHUIStyling.EditorBarLabelLeft);
			GUI.Label(rect75, "75%", AHUIStyling.EditorBarLabelCenter);
			GUI.Label(rect50, "50%", AHUIStyling.EditorBarLabelCenter);
			GUI.Label(rect25, "25%", AHUIStyling.EditorBarLabelCenter);
			GUI.Label(rect0, "0%", AHUIStyling.EditorBarLabelRight);
			GUILayout.BeginHorizontal ();
			GUILayout.Label(/*Placeholder*/"", AHUIStyling.DefaultLabel, GUILayout.Width(pos100[1]));
			GUILayout.Label(/*75*/ToKMG(AntennaHelperEditor.EditorShipAntennas.RelayRangesMax[75],
				true, 2), AHUIStyling.CenterLabel, GUILayout.Width(pos75[1]));
			GUILayout.Label(/*50*/ToKMG(AntennaHelperEditor.EditorShipAntennas.RelayRangesMax[50],
				true, 2), AHUIStyling.CenterLabel, GUILayout.Width(pos50[1]));
			GUILayout.Label(/*25*/ToKMG(AntennaHelperEditor.EditorShipAntennas.RelayRangesMax[25],
				true, 2), AHUIStyling.CenterLabel, GUILayout.Width(pos25[1]));
			// GUILayout.Label(/*0*/ToKMG(AntennaHelperEditor.EditorShipAntennas.RelayRangesMax[0],
			// 	true, 2), AHUIStyling.CenterLabel, GUILayout.Width(pos0[1]));			
			GUILayout.EndHorizontal ();	
			GUILayout.Label(/*Relay Ranges*/Localizer.Format("#autoLOC_AH_0104"), AHUIStyling.HeaderLabel);
			AHUIStyling.DrawSeparator();
			
			// Planet view button
			if (GUILayout.Button (/*Signal Strength / Distance*/Localizer.Format ("#autoLOC_AH_0060") 
				+ " / " + Localizer.Format ("#autoLOC_AH_0059"), AHUIStyling.ButtonDefault))
				{
					if (AntennaHelperEditor.EditorWindows["EditorPlanet"].IsVisible) {
						AntennaHelperEditor.CloseWindow("EditorPlanet");
					} else {
						AntennaHelperEditor.ShowWindow("EditorPlanet");
					}
				}

			// not implemented yet
			/*if (GUILayout.Button (/*Add Ship to the Target List#1#Localizer.Format ("#autoLOC_AH_0013"))) {
				AHEditor.AddShipToShipList ();
			}*/
			
			// Label for debugging
			GUILayout.Label ("Flight ship count");
			GUILayout.Label (AHShipList.FlightProtoShipList.Count.ToString());
			GUILayout.Label ("VAB ship count");
			GUILayout.Label (AHShipList.EditorShipListVAB.Count.ToString());
			GUILayout.Label ("SHP ship count");
			GUILayout.Label (AHShipList.EditorShipListSPH.Count.ToString());
			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}

		public static void TargetWindow (int id)
		{
			// Close Button
			DrawCloseButton("EditorTarget");
			
			// DSN Selector
			GUILayout.BeginVertical ();
			GUIStyle DSNButtonStyle = AHUIStyling.ButtonDefault;
			for (int i = 0 ; i < 3 ; i++) {
				String dsnStr = /*DSN Level*/ Localizer.Format ("#autoLOC_AH_0015") + " " + (i + 1) + "  ( " + ToKMG(GameVariables.Instance.GetDSNRange (i / 2f)) + " )";
				
				// mark current DSN level
				if (i / 2f == AntennaHelperEditor.trackingStationLevel) {
					dsnStr = "**" + dsnStr + "**";
				}
				// mark current target
				if (AntennaHelperEditor.targetName == Localizer.Format("#autoLOC_AH_0015") + " " +
				    (int)((i / 2f) * 2 + 1))
				{
					DSNButtonStyle = AHUIStyling.ButtonRed;
				}
				else
				{
					DSNButtonStyle = AHUIStyling.ButtonDefault;
				}
				
				if (GUILayout.Button(dsnStr, DSNButtonStyle))
				{
					//AntennaHelperEditor.CloseWindow("EditorTarget");
					AntennaHelperEditor.targetPower = GameVariables.Instance.GetDSNRange (i / 2f);
					AntennaHelperEditor.targetName = Localizer.Format("#autoLOC_AH_0015") + " " +
					                                 (int)((i / 2f) * 2 + 1);
					AntennaHelperEditor.targetType = AHTargetType.DSN;
					AntennaHelperEditor.EditorShipAntennas.UpdateRanges(AntennaHelperEditor.targetPower);
				}
			}

			// Ship Selector (In-Flight Ships)
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (/*In-Flight Ships*/Localizer.Format ("#autoLOC_AH_0016"), AHUIStyling.ButtonDefault)) {
				//AntennaHelperEditor.CloseWindow("EditorTarget");
				AntennaHelperEditor.CloseWindow("EditorTargetShipEditor");
				AntennaHelperEditor.CloseWindow("EditorTargetPart");
				AntennaHelperEditor.ShowWindow("EditorTargetShipFlight");
			}
			// Ship Selector (Editor Ships)
			if (GUILayout.Button (/*Editor Ships*/Localizer.Format ("#autoLOC_AH_0017"), AHUIStyling.ButtonDefault)) {
				//AntennaHelperEditor.CloseWindow("EditorTarget");
				AntennaHelperEditor.CloseWindow("EditorTargetShipFlight");
				AntennaHelperEditor.CloseWindow("EditorTargetPart");
				AntennaHelperEditor.ShowWindow("EditorTargetShipEditor");
			}
			// Parts Selector
			if (GUILayout.Button (/*Antenna Parts*/Localizer.Format ("#autoLOC_AH_0018"), AHUIStyling.ButtonDefault)) {
				//AntennaHelperEditor.CloseWindow("EditorTarget");
				AntennaHelperEditor.CloseWindow("EditorTargetShipFlight");
				AntennaHelperEditor.CloseWindow("EditorTargetShipEditor");
				AntennaHelperEditor.ShowWindow("EditorTargetPart");
			}
			GUILayout.EndHorizontal ();
			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}

		/*private static bool vab = true;
		private static bool relay = false;
		private static Vector2 scrollVectorEditor;
		private static List<Dictionary<string, string>> displayList;*/
		public static void TargetWindowShipEditor (int id)
		{
			/*GUIStyle guiStyleLabel;
			GUIStyle guiStyleLabelNorm = new GUIStyle (GUI.skin.GetStyle ("Label"));
			GUIStyle guiStyleLabelBold = new GUIStyle (GUI.skin.GetStyle ("Label"));
			guiStyleLabelBold.fontStyle = FontStyle.Bold;

			GUIStyle guiStyleButton;
			GUIStyle guiStyleButtonNorm = new GUIStyle (GUI.skin.GetStyle ("Button"));
			GUIStyle guiStyleButtonBold = new GUIStyle (GUI.skin.GetStyle ("Button"));
			guiStyleButtonBold.fontStyle = FontStyle.Bold;

			GUIStyle guiStyleButtonRed = new GUIStyle (GUI.skin.GetStyle ("Button"));
			guiStyleButtonRed.fontStyle = FontStyle.Bold;
			guiStyleButtonRed.normal.textColor = Color.red;
			guiStyleButtonRed.hover.textColor = Color.red;*/

			// Close Button
			DrawCloseButton("EditorTargetShipEditor");

			/*GUILayout.BeginVertical ();

			GUILayout.BeginHorizontal ();
			if (vab) {
				guiStyleButton = guiStyleButtonBold;
			} else {
				guiStyleButton = guiStyleButtonNorm;
			}
			if (GUILayout.Button (/*VAB#1#Localizer.Format ("#autoLOC_AH_0019"), guiStyleButton)) {
				vab = true;
			}

			if (vab) {
				guiStyleButton = guiStyleButtonNorm;
			} else {
				guiStyleButton = guiStyleButtonBold;
			}
			if (GUILayout.Button (/*SPH#1#Localizer.Format ("#autoLOC_AH_0020"), guiStyleButton)) {
				vab = false;
			}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Space (35f);
			if (relay) {
				guiStyleButton = guiStyleButtonNorm;
			} else {
				guiStyleButton = guiStyleButtonBold;
			}
			if (GUILayout.Button (/*All#1#Localizer.Format ("#autoLOC_AH_0021"), guiStyleButton)) {
				relay = false;
			}

			if (relay) {
				guiStyleButton = guiStyleButtonBold;
			} else {
				guiStyleButton = guiStyleButtonNorm;
			}
			if (GUILayout.Button (/*Relay#1#Localizer.Format ("#autoLOC_AH_0003"), guiStyleButton)) {
				relay = true;
			}
			GUILayout.Space (35f);
			GUILayout.EndHorizontal ();

			scrollVectorEditor = GUILayout.BeginScrollView (scrollVectorEditor);
			if (vab) {
				if (relay) {
					displayList = AHEditor.guiExternListShipEditorVabRelay;
				} else {
					displayList = AHEditor.guiExternListShipEditorVabAll;
				}
			} else {
				if (relay) {
					displayList = AHEditor.guiExternListShipEditorSphRelay;
				} else {
					displayList = AHEditor.guiExternListShipEditorSphAll;
				}
			}

			foreach (Dictionary <string, string> vesselInfo in displayList) {
				if ((vab && (vesselInfo ["type"] != "VAB")) || (!vab && (vesselInfo ["type"] != "SPH"))) {
					continue;
				}

				GUILayout.BeginHorizontal ();
				if (GUILayout.Button (Localizer.Format ("#autoLOC_AH_0022"), GUILayout.Width (60f))) {
					AHEditor.SetTarget (vesselInfo ["pid"]);
				}

				if (AHEditor.targetPid == vesselInfo ["pid"]) {
					guiStyleLabel = guiStyleLabelBold;
				} else {
					guiStyleLabel = guiStyleLabelNorm;
				}
				GUILayout.Label (
					"("
					+ AHUtil.TruePower (Double.Parse (vesselInfo ["powerRelay"])).ToString ("N0")
					+ ")  "
					+ vesselInfo ["name"], guiStyleLabel);
				if (GUILayout.Button ("X", guiStyleButtonRed, GUILayout.Width (22f))) {
					AHEditor.RemoveShipFromShipList (vesselInfo ["pid"]);
				}
				GUILayout.EndHorizontal ();
			}
			GUILayout.EndScrollView ();

			GUILayout.EndVertical ();*/
		}

		private static Vector2 scrollVectorFlight;
		public static void TargetWindowShipFlight (int id)
		{

			// Close Button
			DrawCloseButton("EditorTargetShipFlight");
			
			GUILayout.BeginVertical ();
			scrollVectorFlight = GUILayout.BeginScrollView (scrollVectorFlight);
			foreach (var item in AHShipList.FlightProtoShipList) {
				ProtoVessel vessel = item.Key;
				AHShipAntennas shipantennas = item.Value;
				
				string vesselName = vessel.GetDisplayName();
				string vesselPower = ToKMG(shipantennas.RelayPower, false, 2);
				string strButton = vesselName + " (" + vesselPower + ")";

				GUIStyle buttonStyle;
				if (AntennaHelperEditor.targetName == vesselName)
				{
					buttonStyle = AHUIStyling.ButtonRed;
				}
				else
				{
					buttonStyle = AHUIStyling.ButtonDefault;
				}
				
				if (GUILayout.Button(strButton, buttonStyle)) {
					AntennaHelperEditor.targetName = vesselName;
					AntennaHelperEditor.targetPower = shipantennas.RelayPower;
					AntennaHelperEditor.targetType = AHTargetType.FLIGHT;
					AntennaHelperEditor.EditorShipAntennas.UpdateRanges(AntennaHelperEditor.targetPower);
				}
			}
			GUILayout.EndScrollView ();
			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}

		//private static Vector2 scrollVectorPart;
		public static void TargetWindowPart (int id)
		{
			/*GUIStyle guiStyleLabel;
			GUIStyle guiStyleLabelNorm = new GUIStyle (GUI.skin.GetStyle ("Label"));
			GUIStyle guiStyleLabelBold = new GUIStyle (GUI.skin.GetStyle ("Label"));
			guiStyleLabelBold.fontStyle = FontStyle.Bold;

			GUIStyle guiStyleButtonBold = new GUIStyle (GUI.skin.GetStyle ("Button"));
			guiStyleButtonBold.fontStyle = FontStyle.Bold;*/

			// Close Button
			DrawCloseButton("EditorTargetPart");


			/*GUILayout.BeginVertical ();
			scrollVectorPart = GUILayout.BeginScrollView (scrollVectorPart);

			foreach (ModuleDataTransmitter antenna in AHShipList.listAntennaPart) {
				
				if (antenna.antennaType != AntennaType.RELAY) {
					continue;
				}

				if (AHEditor.listAntennaPart [antenna] > 0) {
					guiStyleLabel = guiStyleLabelBold;
				} else {
					guiStyleLabel = guiStyleLabelNorm;
				}

				GUILayout.BeginHorizontal ();

				GUILayout.Label (AHEditor.listAntennaPart [antenna].ToString (), guiStyleLabel, GUILayout.Width (15f));

				if (GUILayout.Button ("+", guiStyleButtonBold, GUILayout.Width (20f))) {
					AHEditor.listAntennaPart [antenna]++;
					AHEditor.UpdateTargetPartPower ();
				}
				if (GUILayout.Button ("-", guiStyleButtonBold, GUILayout.Width (20f))) {
					AHEditor.listAntennaPart [antenna]--;
					AHEditor.UpdateTargetPartPower ();
				}

				GUILayout.Label (
					"(" + AHUtil.TruePower (antenna.antennaPower).ToString ("N0") + ")  " 
					+ antenna.part.partInfo.title, guiStyleLabel);

				GUILayout.EndHorizontal ();
			}
			GUILayout.EndScrollView ();

			GUILayout.Space (10f);

			GUILayout.BeginHorizontal ();
			GUILayout.Label (/*Power#1#Localizer.Format ("#autoLOC_AH_0009") + " : " + AHEditor.targetPartPower.ToString ("N0"));
			if (GUILayout.Button (/*Set As Target#1#Localizer.Format ("#autoLOC_AH_0023"))) {
				AHEditor.SetTargetAsPart ();
			}
			GUILayout.EndHorizontal ();

			GUILayout.EndVertical ();*/
		}

		public static void PlanetWindow (int id)
		{
			// Close Button
			DrawCloseButton("EditorPlanet");

			/*GUILayout.BeginVertical ();
			GUILayout.BeginHorizontal ();
			GUILayout.BeginVertical ();
			// Planet name
			GUILayout.Label (/*Planet / Moon#1#Localizer.Format ("#autoLOC_AH_0024"));
			foreach (MyTuple planet in AHUtil.signalPlanetList) {
				GUILayout.Label (
					new GUIContent (
						planet.item1, 
						/*Min#1#Localizer.Format ("#autoLOC_AH_0025") + " = " + planet.item2.ToString ("N0") + "m | " 
					+ /*Max#1#Localizer.Format ("#autoLOC_AH_0026") + " = " + planet.item3.ToString ("N0") + "m"));
//				GUI.Label (new Rect (Mouse.screenPos.x, Mouse.screenPos.y, 50, 20), GUI.tooltip);
				GUILayout.BeginArea (new Rect 
					(Mouse.screenPos.x - AHEditor.rectPlanetWindow.position.x, 
						Mouse.screenPos.y - AHEditor.rectPlanetWindow.position.y - 15, 450, 30));
				GUILayout.Label (GUI.tooltip);
				GUILayout.EndArea ();
			}
			GUILayout.EndVertical ();
			GUILayout.BeginVertical ();
			// Min distance
			GUILayout.Label (/*Signal at Min Distance#1#Localizer.Format ("#autoLOC_AH_0027"));
			if (antennaTypeIsDirect) {
				foreach (double signal in AHEditor.signalMinDirect) {
					GUILayout.Label (signal.ToString ("0.00%"));
				}
			} else {
				foreach (double signal in AHEditor.signalMinRelay) {
					GUILayout.Label (signal.ToString ("0.00%"));
				}
			}

			GUILayout.EndVertical ();
			GUILayout.BeginVertical ();
			// Max distance
			GUILayout.Label (/*Signal at Max Distance#1#Localizer.Format ("#autoLOC_AH_0028"));
			if (antennaTypeIsDirect) {
				foreach (double signal in AHEditor.signalMaxDirect) {
					GUILayout.Label (signal.ToString ("0.00%"));
				}
			} else {
				foreach (double signal in AHEditor.signalMaxRelay) {
					GUILayout.Label (signal.ToString ("0.00%"));
				}
			}
			GUILayout.EndVertical ();
			GUILayout.EndHorizontal ();

			// Custom distance
			GUILayout.Label (/*Check the Signal Strength at a given distance#1#Localizer.Format ("#autoLOC_AH_0029") + " :");
			GUILayout.BeginHorizontal ();
			GUILayout.BeginVertical ();
			AHEditor.customDistance = GUILayout.TextField (AHEditor.customDistance);
			GUILayout.EndVertical ();
			GUILayout.BeginVertical ();
			if (antennaTypeIsDirect) {
				GUILayout.Label (AHEditor.signalCustomDistanceDirect.ToString ("0.00%"));
			} else {
				GUILayout.Label (AHEditor.signalCustomDistanceRelay.ToString ("0.00%"));
			}
			GUILayout.EndVertical ();
			GUILayout.BeginVertical ();
			if (GUILayout.Button (/*Math !#1#Localizer.Format ("#autoLOC_AH_0030"))) {
				AHEditor.CalcCustomDistance ();
			}
			GUILayout.EndVertical ();
			GUILayout.EndHorizontal ();
			GUILayout.EndVertical ();*/
			GUI.DragWindow ();
		}
	}
}