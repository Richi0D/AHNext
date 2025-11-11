// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using KSP.Localization;
//
// namespace AntennaHelperNext
// {
//     public static class AHFlightWindows
//     {
//         
//         // Close button for all windows
//         private static void DrawCloseButton(string windowName)
//         {
//             var winInfo = AHFlight.FlightWindows[windowName];
//             var rect = new Rect(winInfo.Position.width - 22, 2, 20, 18);
//
//             if (GUI.Button(rect, "X"))
//             {
//                 WindowInfo.CloseWindow(windowName, AHFlight.FlightWindows);
//             }
//         }
//
//         public static void MainWindow(int id)
//         {
//             float widthFirstCol = AHFlight.FlightWindows["FlightMain"].Position.width * .45f;
//             GUIStyle ButtonStyle = AHUIStyling.ButtonDefault;
//
//             // Close Button, Use toolbarcontroller to close window
//             //DrawCloseButton("TrackingMain");
//
//             // Start UI
//             GUILayout.BeginVertical ();
//             AHUIStyling.DrawSeparator();
//             GUILayout.Space (5f);
//             
//             // Selected Vessel Info
//             if (AHFlight.activeVessel is null)
//             {
//                 GUILayout.Label(/*nothing selected*/Localizer.Format("#autoLOC_AH_0111"),
//                     AHUIStyling.BoldLabel);
//             }
//             else
//             {
//                 GUILayout.BeginHorizontal();
//                 GUILayout.Label(/*Selected Vessel*/Localizer.Format("#autoLOC_AH_0077") + " : ",
//                     AHUIStyling.DefaultLabel, GUILayout.Width(widthFirstCol));
//                 GUILayout.Label(/*Vessel Name*/AHFlight.activeVessel.vesselName,
//                     AHUIStyling.DefaultLabel);
//                 GUILayout.EndHorizontal();  
//                 
//                 GUILayout.BeginHorizontal();
//                 GUILayout.Label(/*Relay Power*/Localizer.Format("#autoLOC_AH_0057") + " : ",
//                     AHUIStyling.DefaultLabel, GUILayout.Width(widthFirstCol));
//                 GUILayout.Label(/*Vessel*/AHUtil.ToKMG(AHFlight.ActiveShipAntennas.RelayPower,decimalPlaces:2),
//                     AHUIStyling.DefaultLabel);
//                 GUILayout.EndHorizontal();
//                 
//                 GUILayout.BeginHorizontal();
//                 GUILayout.Label(/*Total Power*/Localizer.Format("#autoLOC_AH_0058") + " : ",
//                     AHUIStyling.DefaultLabel, GUILayout.Width(widthFirstCol));
//                 GUILayout.Label(/*Vessel*/AHUtil.ToKMG(AHFlight.ActiveShipAntennas.VesselPower,decimalPlaces:2),
//                     AHUIStyling.DefaultLabel);
//                 GUILayout.EndHorizontal();
//                 
//                 GUILayout.BeginHorizontal();
//                 GUILayout.Label(/*Antennas extended*/Localizer.Format("#autoLOC_AH_0109") + " : ",
//                     AHUIStyling.DefaultLabel, GUILayout.Width(widthFirstCol));
//                 int antennacount = AHFlight.ActiveShipAntennas.AntennasNotExtended.Count + AHFlight.ActiveShipAntennas.VesselAntennas.Count;
//                 GUILayout.Label(/*count*/Localizer.Format("#autoLOC_AH_0110", new string[] {
//                         (antennacount - AHFlight.ActiveShipAntennas.AntennasNotExtended.Count).ToString(),
//                         (antennacount).ToString()
//                     }),
//                     AHUIStyling.DefaultLabel);
//                 GUILayout.EndHorizontal();                
//             }
//             AHUIStyling.DrawSeparator();
//             if (AHMapCircle.inMapView)
//             {
//                 // Button Active connection
//                 if (AHDisplayType.ACTIVE == AHFlight.displayType)
//                 {
//                     ButtonStyle = AHUIStyling.ButtonSelected;
//                 }
//                 else
//                 {
//                     ButtonStyle = AHUIStyling.ButtonDefault;
//                 }            
//                 if (GUILayout.Button(Localizer.Format("#autoLOC_AH_0045"), ButtonStyle))
//                 {
//                     AHFlight.displayType = AHDisplayType.ACTIVE;
//                     // TODO: Update GUI
//                 }         
//                 
//                 // Button DSN connection
//                 if (AHDisplayType.DSN == AHFlight.displayType)
//                 {
//                     ButtonStyle = AHUIStyling.ButtonSelected;
//                 }
//                 else
//                 {
//                     ButtonStyle = AHUIStyling.ButtonDefault;
//                 }            
//                 if (GUILayout.Button(Localizer.Format("#autoLOC_AH_0046"), ButtonStyle))
//                 {
//                     AHFlight.displayType = AHDisplayType.DSN;
//                     // TODO: Update GUI
//                 }       
//                 
//                 // Button RELAY connection
//                 if (AHDisplayType.RELAY == AHFlight.displayType)
//                 {
//                     ButtonStyle = AHUIStyling.ButtonSelected;
//                 }
//                 else
//                 {
//                     ButtonStyle = AHUIStyling.ButtonDefault;
//                 }            
//                 if (GUILayout.Button(Localizer.Format("#autoLOC_AH_0048"), ButtonStyle))
//                 {
//                     AHFlight.displayType = AHDisplayType.RELAY;
//                     // TODO: Update GUI
//                 } 
//                 
//                 // Button DSN+RELAY connection
//                 if (AHDisplayType.DSNRELAY == AHFlight.displayType)
//                 {
//                     ButtonStyle = AHUIStyling.ButtonSelected;
//                 }
//                 else
//                 {
//                     ButtonStyle = AHUIStyling.ButtonDefault;
//                 }            
//                 if (GUILayout.Button(Localizer.Format("#autoLOC_AH_0047"), ButtonStyle))
//                 {
//                     AHFlight.displayType = AHDisplayType.DSNRELAY;
//                     // TODO: Update GUI
//                 }
//             }
//             
//             // DEBUG
//             GUILayout.Label ("SignalStrenghtKSP");
//             GUILayout.Label (AHFlight.activeVessel.connection.SignalStrength.ToString());
//             
//             GUILayout.Label ("SignalStrenghtCalctoDNS");
//             double VesselmaxRange = AHUtil.GetMaxRange(AHFlight.ActiveShipAntennas.VesselPower, AHFlight.DSNPower);
//             double RelaymaxRange = AHUtil.GetMaxRange(AHFlight.ActiveShipAntennas.RelayPower, AHFlight.DSNPower);
//             
//             CelestialBody home = FlightGlobals.GetHomeBody();
//             double distancetoHome = Vector3d.Distance (AHFlight.activeVessel.GetWorldPos3D(), home.position) - home.Radius;
//             GUILayout.Label (AHUtil.GetSignalStrength(AHUtil.GetNormalizedRange(distancetoHome, VesselmaxRange)).ToString());
//             
//             GUILayout.Label ("Distance to DSN");
//             GUILayout.Label (distancetoHome.ToString());
//             
//             GUILayout.Label ("MaxRangeVessel");
//             GUILayout.Label (VesselmaxRange.ToString());
//             
//             GUILayout.Label ("MaxRangeRelay");
//             GUILayout.Label (RelaymaxRange.ToString());
//             
//             GUILayout.Label ("Antennas not Extendet");
//             GUILayout.Label (AHFlight.ActiveShipAntennas.AntennasNotExtended.Count.ToString());
//             
//             GUILayout.Label ("Antennas count");
//             GUILayout.Label (AHFlight.ActiveShipAntennas.VesselAntennas.Count.ToString());
//             
//             GUILayout.Label ("on Flight vessels");
//             GUILayout.Label (AHShipList.FlightShipList.Count.ToString());
//             
//             GUILayout.EndVertical();
//             GUI.DragWindow();
//         }        
//     }
// }