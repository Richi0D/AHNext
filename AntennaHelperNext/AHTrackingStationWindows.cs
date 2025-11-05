using System;
using System.Collections.Generic;
using CommNet;
using UnityEngine;
using KSP.Localization;

namespace AntennaHelperNext
{

    public static class AHTrackingStationWindows
    {

        // Close button for all windows
        private static void DrawCloseButton(string windowName)
        {
            var winInfo = AHTrackingStation.TrackingStationWindows[windowName];
            var rect = new Rect(winInfo.Position.width - 22, 2, 20, 18);

            if (GUI.Button(rect, "X"))
            {
                WindowInfo.CloseWindow(windowName, AHTrackingStation.TrackingStationWindows);
            }
        }

        public static void MainWindow(int id)
        {
            float widthFirstCol = AHTrackingStation.TrackingStationWindows["TrackingMain"].Position.width * .45f;
            GUIStyle ButtonStyle = AHUIStyling.ButtonDefault;

            // Close Button, Use toolbarcontroller to close window
            //DrawCloseButton("TrackingMain");

            // Start UI
            GUILayout.BeginVertical ();
            AHUIStyling.DrawSeparator();
            GUILayout.Space (5f);
            
            // Selected Vessel Info
            // Ship selector
            GUILayout.BeginHorizontal();
            if (AHTargetType.EDITORVAB == AHMapCircle.selectedShipType)
            {
                ButtonStyle = AHUIStyling.ButtonSelected;
            }
            else
            {
                ButtonStyle = AHUIStyling.ButtonDefault;
            }                  
            if (GUILayout.Button(Localizer.Format ("#autoLOC_AH_0017") + " " + Localizer.Format ("#autoLOC_AH_0019"),
                    ButtonStyle))
            {
                if (AHTrackingStation.TrackingStationWindows["TrackingTargetVAB"].IsVisible)
                {
                    WindowInfo.CloseWindow("TrackingTargetVAB", AHTrackingStation.TrackingStationWindows);
                }
                WindowInfo.CloseWindow("TrackingTargetSPH", AHTrackingStation.TrackingStationWindows);
                WindowInfo.ShowWindow("TrackingTargetVAB", AHTrackingStation.TrackingStationWindows);
                    
            }
            if (AHTargetType.EDITORSPH == AHMapCircle.selectedShipType)
            {
                ButtonStyle = AHUIStyling.ButtonSelected;
            }
            else
            {
                ButtonStyle = AHUIStyling.ButtonDefault;
            }                  
            if (GUILayout.Button(Localizer.Format ("#autoLOC_AH_0017") + " " + Localizer.Format ("#autoLOC_AH_0020"),
                    ButtonStyle))
            {
                if (AHTrackingStation.TrackingStationWindows["TrackingTargetSPH"].IsVisible)
                {
                    WindowInfo.CloseWindow("TrackingTargetSPH", AHTrackingStation.TrackingStationWindows);
                }
                WindowInfo.CloseWindow("TrackingTargetVAB", AHTrackingStation.TrackingStationWindows);
                WindowInfo.ShowWindow("TrackingTargetSPH", AHTrackingStation.TrackingStationWindows);
                    
            }
            GUILayout.EndHorizontal();            
            
            if (AHTrackingStation.activeVessel is null)
            {
                GUILayout.Label(/*nothing selected*/Localizer.Format("#autoLOC_AH_0075"),
                    AHUIStyling.BoldLabel);
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(/*Selected Vessel*/Localizer.Format("#autoLOC_AH_0077") + " : ",
                    AHUIStyling.DefaultLabel, GUILayout.Width(widthFirstCol));
                GUILayout.Label(/*Vessel Name*/AHTrackingStation.activeVessel.vesselName,
                    AHUIStyling.DefaultLabel);
                GUILayout.EndHorizontal();  
                
                GUILayout.BeginHorizontal();
                GUILayout.Label(/*Relay Power*/Localizer.Format("#autoLOC_AH_0057") + " : ",
                    AHUIStyling.DefaultLabel, GUILayout.Width(widthFirstCol));
                GUILayout.Label(/*Vessel*/AHUtil.ToKMG(AHTrackingStation.ActiveShipAntennas.RelayPower,decimalPlaces:2),
                    AHUIStyling.DefaultLabel);
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Label(/*Total Power*/Localizer.Format("#autoLOC_AH_0058") + " : ",
                    AHUIStyling.DefaultLabel, GUILayout.Width(widthFirstCol));
                GUILayout.Label(/*Vessel*/AHUtil.ToKMG(AHTrackingStation.ActiveShipAntennas.VesselPower,decimalPlaces:2),
                    AHUIStyling.DefaultLabel);
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Label(/*Antennas extended*/Localizer.Format("#autoLOC_AH_0109") + " : ",
                    AHUIStyling.DefaultLabel, GUILayout.Width(widthFirstCol));
                int antennacount = AHTrackingStation.ActiveShipAntennas.AntennasNotExtended.Count + AHTrackingStation.ActiveShipAntennas.VesselAntennas.Count;
                GUILayout.Label(/*count*/Localizer.Format("#autoLOC_AH_0110", new string[] {
                        (antennacount - AHTrackingStation.ActiveShipAntennas.AntennasNotExtended.Count).ToString(),
                        (antennacount).ToString()
                    }),
                    AHUIStyling.DefaultLabel);
                GUILayout.EndHorizontal();                
            }
            AHUIStyling.DrawSeparator();
            GUILayout.Label(/*Selected Signal Strength*/Localizer.Format("#autoLOC_AH_0112") ,
                AHUIStyling.DefaultLabel);
            GUILayout.BeginHorizontal();
            foreach (var item in AHUtil.SignalMultipliers) {
                double strength = item.Key;
                
                if (AHMapCircle.selectedSignalStrength == strength)
                {
                    ButtonStyle = AHUIStyling.ButtonSelected;
                }
                else
                {
                    ButtonStyle = AHUIStyling.ButtonDefault;
                }
				
                if (GUILayout.Button(strength.ToString("N0"), ButtonStyle))
                {
                    AHMapCircle.selectedSignalStrength = strength;
                }
            }            
            GUILayout.EndHorizontal();         
            AHUIStyling.DrawSeparator();
            
            // Button Active connection
            if (AHDisplayType.ACTIVE == AHMapCircle.displayType)
            {
                ButtonStyle = AHUIStyling.ButtonSelected;
            }
            else
            {
                ButtonStyle = AHUIStyling.ButtonDefault;
            }            
            if (GUILayout.Button(Localizer.Format("#autoLOC_AH_0045"), ButtonStyle))
            {
                AHMapCircle.displayType = AHDisplayType.ACTIVE;
                // TODO: Update GUI
            }         
            
            // Button DSN connection
            if (AHDisplayType.DSN == AHMapCircle.displayType)
            {
                ButtonStyle = AHUIStyling.ButtonSelected;
            }
            else
            {
                ButtonStyle = AHUIStyling.ButtonDefault;
            }            
            if (GUILayout.Button(Localizer.Format("#autoLOC_AH_0046"), ButtonStyle))
            {
                AHMapCircle.displayType = AHDisplayType.DSN;
                // TODO: Update GUI
            }       
            
            // Button RELAY connection
            if (AHDisplayType.RELAY == AHMapCircle.displayType)
            {
                ButtonStyle = AHUIStyling.ButtonSelected;
            }
            else
            {
                ButtonStyle = AHUIStyling.ButtonDefault;
            }            
            if (GUILayout.Button(Localizer.Format("#autoLOC_AH_0048"), ButtonStyle))
            {
                AHMapCircle.displayType = AHDisplayType.RELAY;
                // TODO: Update GUI
            } 
            
            // Button DSN+RELAY connection
            if (AHDisplayType.DSNRELAY == AHMapCircle.displayType)
            {
                ButtonStyle = AHUIStyling.ButtonSelected;
            }
            else
            {
                ButtonStyle = AHUIStyling.ButtonDefault;
            }            
            if (GUILayout.Button(Localizer.Format("#autoLOC_AH_0047"), ButtonStyle))
            {
                AHMapCircle.displayType = AHDisplayType.DSNRELAY;
                // TODO: Update GUI
            }
            
            // DEBUG
            // GUILayout.Label ("SignalStrenght");
            // GUILayout.Label (AHTrackingStation.debugSignalStrength.ToString());
            
            // find matching vessel from Flightlist
            // foreach (ProtoVessel protovessel in AHShipList.FlightProtoShipList.Keys)
            // {
            // 	if (protovessel.vesselID == AHTrackingStation.activeVessel?.protoVessel.vesselID)
            // 	{
            //         Vector3d pos = protovessel.position;
            //         GUILayout.Label ("Vesselpos proto");
            //         GUILayout.Label (pos.ToString());                    
            // 	}
            // }       
            //
            // GUILayout.Label ("Vesselpos active");
            // GUILayout.Label (AHTrackingStation.activeVessel?.GetWorldPos3D().ToString());               
            
            // GUILayout.Label ("CommPath");
            // foreach (CommLink link in AHTrackingStation.debugCommPath)
            // {
            //     Vessel v = link.a.transform.GetComponent<Vessel>();
            //     
            //     GUILayout.Label (link.a.displayName);
            //     GUILayout.Label (v.vesselName);
            // }
            
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private static Vector2 scrollVectorShipListVAB;
        public static void ShipListWindowVAB(int id)
        {
            // Close Button
            DrawCloseButton("TrackingTargetVAB");

            GUILayout.BeginVertical ();
            scrollVectorShipListVAB = GUILayout.BeginScrollView (scrollVectorShipListVAB);
            foreach (var item in AHShipList.EditorShipListVAB) {
                AHShipAntennas shipantennas = item.Value;
                string vesselName = item.Key.name;
                Guid vid = item.Key.vID;
                string vesselPower = AHUtil.ToKMG(shipantennas.RelayPower, false, 2);
                string strButton = vesselName + " (" + vesselPower + ")";

                GUIStyle buttonStyle;
                if (AHTrackingStation.activeVessel != null && AHTrackingStation.activeVessel.id == vid)
                {
                    buttonStyle = AHUIStyling.ButtonSelected;
                }
                else
                {
                    buttonStyle = AHUIStyling.ButtonDefault;
                }
				
                if (GUILayout.Button(strButton, buttonStyle)) {
                    // create a new dummy vessel
                    Vessel dummyVessel = new Vessel();
                    dummyVessel.vesselName = vesselName;
                    dummyVessel.id = vid;
                    //dummyVessel.protoVessel.vesselID = new Guid(vid);
                    dummyVessel.vesselType = VesselType.Unknown;
                    AHMapCircle.selectedShipType = AHTargetType.EDITORVAB;
                    AHTrackingStation.activeVessel = dummyVessel;
                    AHTrackingStation.ActiveShipAntennas = shipantennas;
                    
                    // update commpath
                    AHCommNet.GetCommPathVessels(dummyVessel, true);
                    //TODO: update GUI
                }
            }
            GUILayout.EndScrollView ();
            GUILayout.EndVertical ();
            GUI.DragWindow ();
        }
        
        private static Vector2 scrollVectorShipListSPH;
        public static void ShipListWindowSPH(int id)
        {
            // Close Button
            DrawCloseButton("TrackingTargetSPH");

            GUILayout.BeginVertical ();
            scrollVectorShipListSPH = GUILayout.BeginScrollView (scrollVectorShipListSPH);
            foreach (var item in AHShipList.EditorShipListSPH) {
                AHShipAntennas shipantennas = item.Value;
                string vesselName = item.Key.name;
                Guid vid = item.Key.vID;
                string vesselPower = AHUtil.ToKMG(shipantennas.RelayPower, false, 2);
                string strButton = vesselName + " (" + vesselPower + ")";

                GUIStyle buttonStyle;
                if (AHTrackingStation.activeVessel != null && AHTrackingStation.activeVessel.id== vid)
                {
                    buttonStyle = AHUIStyling.ButtonSelected;
                }
                else
                {
                    buttonStyle = AHUIStyling.ButtonDefault;
                }
				
                if (GUILayout.Button(strButton, buttonStyle)) {
                    // create a new dummy vessel
                    Vessel dummyVessel = new Vessel();
                    dummyVessel.vesselName = vesselName;
                    dummyVessel.id = vid;
                    dummyVessel.vesselType = VesselType.Unknown;
                    AHMapCircle.selectedShipType = AHTargetType.EDITORSPH;
                    AHTrackingStation.activeVessel = dummyVessel;
                    AHTrackingStation.ActiveShipAntennas = shipantennas;
                    
                    // update commpath
                    AHCommNet.GetCommPathVessels(dummyVessel, true);
                    //TODO: update GUI
                }
            }
            GUILayout.EndScrollView ();
            GUILayout.EndVertical ();
            GUI.DragWindow ();
        }            
        
        
    }
}