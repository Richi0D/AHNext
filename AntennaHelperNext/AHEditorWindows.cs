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
				WindowInfo.CloseWindow(windowName, AntennaHelperEditor.EditorWindows);
			}
		}
		
		public static string GetStrongestAntennaString(
			ModuleDataTransmitter antennaOne,
			ModuleDataTransmitter antennaTwo)
		{
			ModuleDataTransmitter strongest = null;
			string strongestAntenna = "-";
			if (!(antennaOne is null || antennaTwo is (null)))
			{
				if (antennaOne.antennaPower >= antennaTwo.antennaPower)
				{
					strongest = antennaOne;
				}
				else
				{
					strongest = antennaTwo;
				}
			}
			else if (!(antennaOne is null))
			{
				strongest = antennaOne;
			}
			else if (!(antennaTwo is null))
			{
				strongest = antennaTwo;
			}

			if (!(strongest is null))
			{
				if (strongest.antennaCombinable)
				{
					strongestAntenna = "(C) ";
				}
				else
				{
					strongestAntenna = "(NC) ";
				}
				strongestAntenna += strongest.part.partInfo.title;
			}

			return strongestAntenna;
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
			
			// Close Button, Use toolbarcontroller to close window
			//DrawCloseButton("EditorMain");
			
			// Start UI
			GUILayout.BeginVertical ();
			AHUIStyling.DrawSeparator();
			GUILayout.Space (5f);
			
			// Target Selection
			//GUILayout.Label(/*Target*/Localizer.Format ("#autoLOC_AH_0100"), AHUIStyling.HeaderLabel);
			if (GUILayout.Button (/*Pick A Target*/Localizer.Format ("#autoLOC_AH_0007"), AHUIStyling.ButtonDefault)) {
				if (AntennaHelperEditor.EditorWindows["EditorTarget"].IsVisible) {
					WindowInfo.CloseWindow("EditorTarget", AntennaHelperEditor.EditorWindows);
				} else {
					// be sure other windows are closed again
					WindowInfo.CloseWindow("EditorTargetShipFlight", AntennaHelperEditor.EditorWindows);
					WindowInfo.CloseWindow("EditorTargetShipEditorVAB", AntennaHelperEditor.EditorWindows);
					WindowInfo.CloseWindow("EditorTargetShipEditorSPH", AntennaHelperEditor.EditorWindows);
					WindowInfo.CloseWindow("EditorTargetPart", AntennaHelperEditor.EditorWindows);
					WindowInfo.ShowWindow("EditorTarget", AntennaHelperEditor.EditorWindows);
				}
			}
			GUILayout.BeginHorizontal ();
			GUILayout.Label( /*Current target*/Localizer.Format("#autoLOC_AH_0006") + " : ",
				AHUIStyling.DefaultLabel, GUILayout.Width(widthFirstCol));
			GUILayout.Label(AntennaHelperEditor.selectedTarget.targetName, AHUIStyling.DefaultLabel);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal ();
			GUILayout.Label( /*Target Power*/Localizer.Format("#autoLOC_AH_0101") + " : ",
				AHUIStyling.DefaultLabel,GUILayout.Width(widthFirstCol));
			GUILayout.Label(  AHUtil.ToKMG(AntennaHelperEditor.selectedTarget.targetPower, decimalPlaces: 2), AHUIStyling.DefaultLabel);
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
				(AntennaHelperEditor.EditorShipAntennas.VesselCombAntennas.Count).ToString (),
				AntennaHelperEditor.EditorShipAntennas.VesselAntennas.Count.ToString ()
			}), AHUIStyling.DefaultLabel, GUILayout.Width(widthSecondCol));
			GUILayout.Label(/*Relay*/Localizer.Format("#autoLOC_AH_0042", new string[] {
				AntennaHelperEditor.EditorShipAntennas.RelayCombAntennas.Count.ToString (),
				AntennaHelperEditor.EditorShipAntennas.RelayAntennas.Count.ToString ()
			}), AHUIStyling.DefaultLabel);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label(/*Strongest*/Localizer.Format("#autoLOC_AH_0107") + " : ",
				AHUIStyling.DefaultLabel,  GUILayout.Width(widthFirstCol));
			// get strongest antennas
			string strongestAntenna = GetStrongestAntennaString(AntennaHelperEditor.EditorShipAntennas.StrongestVesselAntenna, null);
			string strongestAntennaRelay = GetStrongestAntennaString(AntennaHelperEditor.EditorShipAntennas.StrongestRelayAntenna, 
				AntennaHelperEditor.EditorShipAntennas.StrongestRelayAntennaNonCombinable);
			GUILayout.Label(/*Vessel*/strongestAntenna, AHUIStyling.DefaultLabel, GUILayout.Width(widthSecondCol));
			GUILayout.Label(/*Relay*/strongestAntennaRelay, AHUIStyling.DefaultLabel);
			GUILayout.EndHorizontal();			
			GUILayout.BeginHorizontal();
			GUILayout.Label(/*Power*/Localizer.Format("#autoLOC_AH_0009") + " : ",
				AHUIStyling.DefaultLabel, GUILayout.Width(widthFirstCol));
			GUILayout.Label(/*Vessel*/AHUtil.ToKMG(AntennaHelperEditor.EditorShipAntennas.VesselPower,decimalPlaces:2),
				AHUIStyling.DefaultLabel, GUILayout.Width(widthSecondCol));
			GUILayout.Label(/*Relay*/AHUtil.ToKMG(AntennaHelperEditor.EditorShipAntennas.RelayPower,decimalPlaces:2),
				AHUIStyling.DefaultLabel);	
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label(/*Max Range*/Localizer.Format("#autoLOC_AH_0010") + " : ",
				AHUIStyling.DefaultLabel, GUILayout.Width(widthFirstCol));
			GUILayout.Label(/*Vessel*/AHUtil.ToKMG(AntennaHelperEditor.EditorShipAntennas.VesselRangesMax[0],
				true, 2), AHUIStyling.DefaultLabel, GUILayout.Width(widthSecondCol));
			GUILayout.Label(/*Relay*/AHUtil.ToKMG(AntennaHelperEditor.EditorShipAntennas.RelayRangesMax[0],
				true, 2), AHUIStyling.DefaultLabel);	
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label(/*Range 100%*/Localizer.Format("#autoLOC_AH_0011") + " : ",
				AHUIStyling.DefaultLabel, GUILayout.Width(widthFirstCol));
			GUILayout.Label(/*Vessel*/AHUtil.ToKMG(AntennaHelperEditor.EditorShipAntennas.VesselRangesMax[100],
				true, 2), AHUIStyling.DefaultLabel, GUILayout.Width(widthSecondCol));
			GUILayout.Label(/*Relay*/AHUtil.ToKMG(AntennaHelperEditor.EditorShipAntennas.RelayRangesMax[100],
				true, 2), AHUIStyling.DefaultLabel);	
			GUILayout.EndHorizontal();			
			AHUIStyling.DrawSeparator();
			
			// Signal Color Bar
			//GUILayout.Space (16f);
			GUILayout.Label(/*Vessel Ranges*/Localizer.Format("#autoLOC_AH_0103"), AHUIStyling.HeaderLabel);
			GUILayout.BeginHorizontal ();
			GUILayout.Label(/*Placeholder*/"", AHUIStyling.DefaultLabel, GUILayout.Width(pos100[1]));
			GUILayout.Label(/*75*/AHUtil.ToKMG(AntennaHelperEditor.EditorShipAntennas.VesselRangesMax[75],
				true, 2), AHUIStyling.CenterLabel, GUILayout.Width(pos75[1]));
			GUILayout.Label(/*50*/AHUtil.ToKMG(AntennaHelperEditor.EditorShipAntennas.VesselRangesMax[50],
				true, 2), AHUIStyling.CenterLabel, GUILayout.Width(pos50[1]));
			GUILayout.Label(/*25*/AHUtil.ToKMG(AntennaHelperEditor.EditorShipAntennas.VesselRangesMax[25],
				true, 2), AHUIStyling.CenterLabel, GUILayout.Width(pos25[1]));
			// GUILayout.Label(/*0*/AHUtil.ToKMG(AntennaHelperEditor.EditorShipAntennas.VesselRangesMax[0],
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
			GUILayout.Label(/*75*/AHUtil.ToKMG(AntennaHelperEditor.EditorShipAntennas.RelayRangesMax[75],
				true, 2), AHUIStyling.CenterLabel, GUILayout.Width(pos75[1]));
			GUILayout.Label(/*50*/AHUtil.ToKMG(AntennaHelperEditor.EditorShipAntennas.RelayRangesMax[50],
				true, 2), AHUIStyling.CenterLabel, GUILayout.Width(pos50[1]));
			GUILayout.Label(/*25*/AHUtil.ToKMG(AntennaHelperEditor.EditorShipAntennas.RelayRangesMax[25],
				true, 2), AHUIStyling.CenterLabel, GUILayout.Width(pos25[1]));
			// GUILayout.Label(/*0*/AHUtil.ToKMG(AntennaHelperEditor.EditorShipAntennas.RelayRangesMax[0],
			// 	true, 2), AHUIStyling.CenterLabel, GUILayout.Width(pos0[1]));			
			GUILayout.EndHorizontal ();	
			GUILayout.Label(/*Relay Ranges*/Localizer.Format("#autoLOC_AH_0104"), AHUIStyling.HeaderLabel);
			AHUIStyling.DrawSeparator();
			
			// Planet view button
			if (GUILayout.Button (/*Signal Strength / Distance*/Localizer.Format ("#autoLOC_AH_0060") 
				+ " / " + Localizer.Format ("#autoLOC_AH_0059"), AHUIStyling.ButtonDefault))
				{
					if (AntennaHelperEditor.EditorWindows["EditorPlanet"].IsVisible) {
						WindowInfo.CloseWindow("EditorPlanet", AntennaHelperEditor.EditorWindows);
					} else {
						WindowInfo.ShowWindow("EditorPlanet", AntennaHelperEditor.EditorWindows);
					}
				}
			
			// Label for debugging
			// GUILayout.Label ("Flight ship count");
			// GUILayout.Label (AHShipList.FlightProtoShipList.Count.ToString());
			// GUILayout.Label ("VAB ship count");
			// GUILayout.Label (AHShipList.EditorShipListVAB.Count.ToString());
			// GUILayout.Label ("SPH ship count");
			// GUILayout.Label (AHShipList.EditorShipListSPH.Count.ToString());
			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}

		public static void TargetWindow (int id)
		{
			// Close Button
			DrawCloseButton("EditorTarget");
			
			// DSN Selector
			GUILayout.BeginVertical ();
			GUIStyle ButtonStyle = AHUIStyling.ButtonDefault;
			for (int i = 0 ; i < 3 ; i++) {
				String dsnStr = /*DSN Level*/ Localizer.Format ("#autoLOC_AH_0015") + " " + (i + 1) + "  ( " + AHUtil.ToKMG(GameVariables.Instance.GetDSNRange (i / 2f)) + " )";
				
				// mark current DSN level
				if (i / 2f == AntennaHelperEditor.trackingStationLevel) {
					dsnStr = "**" + dsnStr + "**";
				}
				// mark current target
				if (AntennaHelperEditor.selectedTarget.targetName == Localizer.Format("#autoLOC_AH_0015") + " " +
				    (int)((i / 2f) * 2 + 1))
				{
					ButtonStyle = AHUIStyling.ButtonSelected;
				}
				else
				{
					ButtonStyle = AHUIStyling.ButtonDefault;
				}
				
				if (GUILayout.Button(dsnStr, ButtonStyle))
				{
					//AntennaHelperEditor.CloseWindow("EditorTarget");
					double targetPower = GameVariables.Instance.GetDSNRange (i / 2f);
					string targetName = Localizer.Format("#autoLOC_AH_0015") + " " +
					                                 (int)((i / 2f) * 2 + 1);
					AntennaHelperEditor.selectedTarget = (targetName,Guid.Empty, targetPower, AHTargetType.DSN);
					AntennaHelperEditor.EditorShipAntennas.UpdateRanges(AntennaHelperEditor.selectedTarget.targetPower);
					AntennaHelperEditor.UpdateCustomRange(AntennaHelperEditor.EditorCustomRange.customDistance);
				}
			}

			// Ship Selector (In-Flight Ships)
			GUILayout.BeginHorizontal ();
			if (AHTargetType.FLIGHT == AntennaHelperEditor.selectedTarget.targetType)
			{
				ButtonStyle = AHUIStyling.ButtonSelected;
			}
			else
			{
				ButtonStyle = AHUIStyling.ButtonDefault;
			}
			if (GUILayout.Button (/*In-Flight Ships*/Localizer.Format ("#autoLOC_AH_0016"), ButtonStyle)) {
				//AntennaHelperEditor.CloseWindow("EditorTarget");
				WindowInfo.CloseWindow("EditorTargetShipEditorVAB", AntennaHelperEditor.EditorWindows);
				WindowInfo.CloseWindow("EditorTargetShipEditorSPH", AntennaHelperEditor.EditorWindows);
				WindowInfo.CloseWindow("EditorTargetPart", AntennaHelperEditor.EditorWindows);
				WindowInfo.ShowWindow("EditorTargetShipFlight", AntennaHelperEditor.EditorWindows);
			}
			// Ship Selector (Editor Ships VAB)
			if (AHTargetType.EDITORVAB == AntennaHelperEditor.selectedTarget.targetType)
			{
				ButtonStyle = AHUIStyling.ButtonSelected;
			}
			else
			{
				ButtonStyle = AHUIStyling.ButtonDefault;
			}
			if (GUILayout.Button (/*Editor Ships*/Localizer.Format ("#autoLOC_AH_0019"), ButtonStyle)) {
				//AntennaHelperEditor.CloseWindow("EditorTarget");
				WindowInfo.CloseWindow("EditorTargetShipFlight", AntennaHelperEditor.EditorWindows);
				WindowInfo.CloseWindow("EditorTargetPart", AntennaHelperEditor.EditorWindows);
				WindowInfo.CloseWindow("EditorTargetShipEditorSPH", AntennaHelperEditor.EditorWindows);
				WindowInfo.ShowWindow("EditorTargetShipEditorVAB", AntennaHelperEditor.EditorWindows);
			}
			// Ship Selector (Editor Ships SPH)
			if (AHTargetType.EDITORSPH == AntennaHelperEditor.selectedTarget.targetType)
			{
				ButtonStyle = AHUIStyling.ButtonSelected;
			}
			else
			{
				ButtonStyle = AHUIStyling.ButtonDefault;
			}
			if (GUILayout.Button (/*Editor Ships*/Localizer.Format ("#autoLOC_AH_0020"), ButtonStyle)) {
				//AntennaHelperEditor.CloseWindow("EditorTarget");
				WindowInfo.CloseWindow("EditorTargetShipFlight", AntennaHelperEditor.EditorWindows);
				WindowInfo.CloseWindow("EditorTargetPart", AntennaHelperEditor.EditorWindows);
				WindowInfo.CloseWindow("EditorTargetShipEditorVAB", AntennaHelperEditor.EditorWindows);
				WindowInfo.ShowWindow("EditorTargetShipEditorSPH", AntennaHelperEditor.EditorWindows);
			}			
			// Parts Selector
			if (AHTargetType.PART == AntennaHelperEditor.selectedTarget.targetType)
			{
				ButtonStyle = AHUIStyling.ButtonSelected;
			}
			else
			{
				ButtonStyle = AHUIStyling.ButtonDefault;
			}
			if (GUILayout.Button (/*Antenna Parts*/Localizer.Format ("#autoLOC_AH_0018"), ButtonStyle)) {
				//AntennaHelperEditor.CloseWindow("EditorTarget");
				WindowInfo.CloseWindow("EditorTargetShipFlight", AntennaHelperEditor.EditorWindows);
				WindowInfo.CloseWindow("EditorTargetShipEditorVAB", AntennaHelperEditor.EditorWindows);
				WindowInfo.CloseWindow("EditorTargetShipEditorSPH", AntennaHelperEditor.EditorWindows);
				WindowInfo.ShowWindow("EditorTargetPart", AntennaHelperEditor.EditorWindows);
				updateTargetPowerPart();
			}
			GUILayout.EndHorizontal ();
			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}

		private static Vector2 scrollVectorEditorVAB;
		public static void TargetWindowShipEditorVAB (int id)
		{

			// Close Button
			DrawCloseButton("EditorTargetShipEditorVAB");

			GUILayout.BeginVertical ();
			scrollVectorEditorVAB = GUILayout.BeginScrollView (scrollVectorEditorVAB);
			foreach (var item in AHShipList.EditorShipListVAB) {
				
				string vesselName = item.Key.name;
				Guid vid = item.Key.vID;
				AHShipAntennas shipantennas = item.Value;
				string vesselPower = AHUtil.ToKMG(shipantennas.RelayPower, false, 2);
				string strButton = vesselName + " (" + vesselPower + ")";

				GUIStyle buttonStyle;
				if (AntennaHelperEditor.selectedTarget.targetID == vid)
				{
					buttonStyle = AHUIStyling.ButtonSelected;
				}
				else
				{
					buttonStyle = AHUIStyling.ButtonDefault;
				}
				
				if (GUILayout.Button(strButton, buttonStyle)) {
					AntennaHelperEditor.selectedTarget =
						(vesselName, vid, shipantennas.RelayPower, AHTargetType.EDITORVAB);
					AntennaHelperEditor.EditorShipAntennas.UpdateRanges(AntennaHelperEditor.selectedTarget.targetPower);
					AntennaHelperEditor.UpdateCustomRange(AntennaHelperEditor.EditorCustomRange.customDistance);
				}
			}
			GUILayout.EndScrollView ();
			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}

		private static Vector2 scrollVectorEditorSPH;
		public static void TargetWindowShipEditorSPH (int id)
		{

			// Close Button
			DrawCloseButton("EditorTargetShipEditorSPH");

			GUILayout.BeginVertical ();
			scrollVectorEditorSPH = GUILayout.BeginScrollView (scrollVectorEditorSPH);
			foreach (var item in AHShipList.EditorShipListSPH) {
				
				string vesselName = item.Key.name;
				Guid vid = item.Key.vID;
				AHShipAntennas shipantennas = item.Value;
				string vesselPower = AHUtil.ToKMG(shipantennas.RelayPower, false, 2);
				string strButton = vesselName + " (" + vesselPower + ")";

				GUIStyle buttonStyle;
				if (AntennaHelperEditor.selectedTarget.targetID == vid)
				{
					buttonStyle = AHUIStyling.ButtonSelected;
				}
				else
				{
					buttonStyle = AHUIStyling.ButtonDefault;
				}
				
				if (GUILayout.Button(strButton, buttonStyle)) {
					AntennaHelperEditor.selectedTarget =
						(vesselName, vid, shipantennas.RelayPower, AHTargetType.EDITORSPH);
					AntennaHelperEditor.EditorShipAntennas.UpdateRanges(AntennaHelperEditor.selectedTarget.targetPower);
					AntennaHelperEditor.UpdateCustomRange(AntennaHelperEditor.EditorCustomRange.customDistance);
				}
			}
			GUILayout.EndScrollView ();
			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}		
		
		private static Vector2 scrollVectorFlight;
		public static void TargetWindowShipFlight (int id)
		{
			
			// Close Button
			DrawCloseButton("EditorTargetShipFlight");
			
			GUILayout.BeginVertical ();
			scrollVectorFlight = GUILayout.BeginScrollView (scrollVectorFlight);
			foreach (var item in AHShipList.EditorFlightShips) {
				ProtoVessel vessel = item.Key;
				AHShipAntennas shipantennas = item.Value;
				
				string vesselName = vessel.GetDisplayName();
				Guid vid = vessel.vesselID;
				string vesselPower = AHUtil.ToKMG(shipantennas.RelayPower, false, 2);
				string strButton = vesselName + " (" + vesselPower + ")";

				GUIStyle buttonStyle;
				if (AntennaHelperEditor.selectedTarget.targetID == vid)
				{
					buttonStyle = AHUIStyling.ButtonSelected;
				}
				else
				{
					buttonStyle = AHUIStyling.ButtonDefault;
				}
				
				if (GUILayout.Button(strButton, buttonStyle)) {
					AntennaHelperEditor.selectedTarget =
						(vesselName, vid, shipantennas.RelayPower, AHTargetType.FLIGHT);
					AntennaHelperEditor.EditorShipAntennas.UpdateRanges(AntennaHelperEditor.selectedTarget.targetPower);
					AntennaHelperEditor.UpdateCustomRange(AntennaHelperEditor.EditorCustomRange.customDistance);
				}
			}
			GUILayout.EndScrollView ();
			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}

		private static void updateTargetPowerPart()
		{
			AntennaHelperEditor.EditorAntennasPicker.UpdateAntennas();
			AntennaHelperEditor.selectedTarget.targetType = AHTargetType.PART;
			AntennaHelperEditor.selectedTarget.targetName = AntennaHelperEditor.EditorAntennasPicker.VesselAntennas.Count + 
			                                 " " + Localizer.Format ("#autoLOC_AH_0018");
			AntennaHelperEditor.selectedTarget.targetID = Guid.Empty;
			AntennaHelperEditor.selectedTarget.targetPower = AntennaHelperEditor.EditorAntennasPicker.RelayPower;
			AntennaHelperEditor.EditorShipAntennas.UpdateRanges(AntennaHelperEditor.selectedTarget.targetPower);
			AntennaHelperEditor.UpdateCustomRange(AntennaHelperEditor.EditorCustomRange.customDistance);
		}
		private static Vector2 scrollVectorPart;
		public static void TargetWindowPart (int id)
		{

			float widthFirstCol = AntennaHelperEditor.EditorWindows["EditorTargetPart"].Position.width * .10f;
			float widthSecondCol = AntennaHelperEditor.EditorWindows["EditorTargetPart"].Position.width * .08f;
			float widthThirdCol = AntennaHelperEditor.EditorWindows["EditorTargetPart"].Position.width * .08f;
			// Close Button
			DrawCloseButton("EditorTargetPart");
			
			GUILayout.BeginVertical ();
			scrollVectorPart = GUILayout.BeginScrollView (scrollVectorPart);
			foreach (var antenna in AHShipList.AntennaPartList) {

				string antennaName = antenna.Key;
				ModuleDataTransmitter antennaModule = antenna.Value;	
				string antennaCount = AntennaHelperEditor.EditorAntennasPicker.countantennas(antennaModule).ToString();
				string antennaPower = AHUtil.ToKMG(antennaModule.GetTruePower(), false, 2);
				string strButton = antennaName + " (" + antennaPower + ")";				
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label (antennaCount +"x", AHUIStyling.ButtonDefault, GUILayout.Width(widthFirstCol));
				if (GUILayout.Button ("+", AHUIStyling.ButtonBold, GUILayout.Width(widthSecondCol))) {
					AntennaHelperEditor.EditorAntennasPicker.AddAntenna(antennaModule);
					updateTargetPowerPart();
				}
				if (GUILayout.Button ("-", AHUIStyling.ButtonBold, GUILayout.Width(widthThirdCol))) {
					AntennaHelperEditor.EditorAntennasPicker.RemoveAntenna(antennaModule);
					updateTargetPowerPart();
				}

				GUILayout.Label (strButton, AHUIStyling.ButtonDefault);
				GUILayout.EndHorizontal ();
			}
			GUILayout.EndScrollView ();
			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}

		public static void PlanetWindow (int id)
		{
			
			float widthTitleFirstCol = AntennaHelperEditor.EditorWindows["EditorPlanet"].Position.width * .24f;
			float widthTitleSecondCol = AntennaHelperEditor.EditorWindows["EditorPlanet"].Position.width * .38f;
			float widthMinCol = AntennaHelperEditor.EditorWindows["EditorPlanet"].Position.width * .19f;	
			float widthMaxCol = AntennaHelperEditor.EditorWindows["EditorPlanet"].Position.width * .19f;			
			
			// Close Button
			DrawCloseButton("EditorPlanet");

			GUILayout.BeginVertical ();
			AHUIStyling.DrawSeparator();
			GUILayout.Space (5f);
			GUILayout.BeginHorizontal ();
			GUILayout.Label(/*Distance*/Localizer.Format("#autoLOC_AH_0108"),
				AHUIStyling.DefaultLabel, GUILayout.Width(widthTitleFirstCol));
			GUILayout.Label(/*Min*/Localizer.Format("#autoLOC_AH_0025"),
				AHUIStyling.DefaultLabel, GUILayout.Width(widthMinCol));
			GUILayout.Label(/*Max*/Localizer.Format("#autoLOC_AH_0026"),
				AHUIStyling.DefaultLabel, GUILayout.Width(widthMaxCol));	
			GUILayout.Label(/*Min*/Localizer.Format("#autoLOC_AH_0025"),
				AHUIStyling.DefaultLabel, GUILayout.Width(widthMinCol));
			GUILayout.Label(/*Max*/Localizer.Format("#autoLOC_AH_0026"),
				AHUIStyling.DefaultLabel);
			GUILayout.EndHorizontal();
			AHUIStyling.DrawSeparator();
			// Titles
			GUILayout.BeginHorizontal ();
			GUILayout.Label(/*Planet*/Localizer.Format("#autoLOC_AH_0024"),
				AHUIStyling.BoldLabel, GUILayout.Width(widthTitleFirstCol));
			GUILayout.Label(/*Vessel*/Localizer.Format("#autoLOC_AH_0039") + " " + Localizer.Format("#autoLOC_AH_0060"),
				AHUIStyling.BoldLabel, GUILayout.Width(widthTitleSecondCol));
			GUILayout.Label(/*Relay*/Localizer.Format("#autoLOC_AH_0003") + " " + Localizer.Format("#autoLOC_AH_0060"),
				AHUIStyling.BoldLabel);	
			GUILayout.EndHorizontal();
			//AHUIStyling.DrawSeparator();
			
			// planet list
			foreach (var planet in AHPlanetList.PlanetList)
			{
				string planetName = planet.Key.bodyName;
				GUILayout.BeginHorizontal ();
				GUILayout.Label(/*Planet*/planetName,
					AHUIStyling.DefaultLabel, GUILayout.Width(widthTitleFirstCol));

				string minVesselSignal = "N/A";
				string maxVesselSignal = "N/A";
				string minRelaySignal = "N/A";
				string maxRelaySignal = "N/A";
				if (AntennaHelperEditor.EditorShipAntennas.PlanetSignalStrengths.ContainsKey(planetName))
				{
					minVesselSignal = AntennaHelperEditor.EditorShipAntennas.PlanetSignalStrengths[planetName]
						.minVesselSignal.ToString("0.00%");
					maxVesselSignal = AntennaHelperEditor.EditorShipAntennas.PlanetSignalStrengths[planetName]
						.maxVesselSignal.ToString("0.00%");
					minRelaySignal = AntennaHelperEditor.EditorShipAntennas.PlanetSignalStrengths[planetName]
						.minRelaySignal.ToString("0.00%");
					maxRelaySignal = AntennaHelperEditor.EditorShipAntennas.PlanetSignalStrengths[planetName]
						.maxRelaySignal.ToString("0.00%");
				}
				GUILayout.Label(/*Min*/minVesselSignal,
					AHUIStyling.DefaultLabel, GUILayout.Width(widthMinCol));
				GUILayout.Label(/*Max*/maxVesselSignal,
					AHUIStyling.DefaultLabel, GUILayout.Width(widthMaxCol));	
				GUILayout.Label(/*Min*/minRelaySignal,
					AHUIStyling.DefaultLabel, GUILayout.Width(widthMinCol));
				GUILayout.Label(/*Max*/maxRelaySignal,
					AHUIStyling.DefaultLabel);
				GUILayout.EndHorizontal();
			}
			
			AHUIStyling.DrawSeparator();
			// Custom distance
			GUILayout.Label (/*Check the Signal Strength at a given distance */Localizer.Format ("#autoLOC_AH_0029") + " :");
			if (GUILayout.Button (/*Math*/ Localizer.Format ("#autoLOC_AH_0030")))
			{
				AntennaHelperEditor.UpdateCustomRange(AntennaHelperEditor.EditorCustomRange.customDistance);
			}
			GUILayout.BeginHorizontal ();
			string input = GUILayout.TextField(AntennaHelperEditor.EditorCustomRange.customDistance.ToString("N0"),
				GUILayout.Width(widthTitleFirstCol));
			if (double.TryParse(input, out double value))
			{
				AntennaHelperEditor.EditorCustomRange.customDistance = value;
			}		
			GUILayout.Label(AntennaHelperEditor.EditorCustomRange.customVesselSignal.ToString ("0.00%"),
				AHUIStyling.DefaultLabel, GUILayout.Width(widthTitleSecondCol));
			GUILayout.Label(AntennaHelperEditor.EditorCustomRange.customRelaySignal.ToString ("0.00%"),
				AHUIStyling.DefaultLabel);
			GUILayout.EndHorizontal();

			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}
	}
}