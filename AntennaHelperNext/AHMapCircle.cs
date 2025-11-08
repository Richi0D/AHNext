using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;


namespace AntennaHelperNext
{
    public static class AHMapCircle
    {
        // Material
        public static Material pointMat;
        
        // Vessel variables
        public static (string name, Guid id, Vessel vessel) activeVessel; // we need an extended type for editor vessels. There vessel can be null
        public static AHShipAntennas ActiveShipAntennas = new AHShipAntennas();
        public static List<ProtoVessel> activeCommPathVessels = new List<ProtoVessel>(); // save here the vessels from the commpath
        public static double DSNPower = 0;
        //public static bool connectedToHome = false;
        public static bool directconnectedToHome = false;
        
        // Selector variables
        public static AHDisplayType displayType = AHDisplayType.ACTIVE;
        public static AHTargetType selectedShipType = AHTargetType.FLIGHT;
        public static double selectedSignalStrength = 0;
        public static AHAntennaType selectedAntennaType = AHAntennaType.ALL;
        public static bool inMapView = false;
        
        // ParticleMeshes and Position Matrices for each entity
        public static (ParticleMesh mesh, Matrix4x4 matrix) DirectConDSNBubble;
        public static (ProtoVessel vessel, ParticleMesh mesh, Matrix4x4 matrix) FirstHopBubble; // this can be either the DSN or a relay vessel
        public static (ParticleMesh mesh, Matrix4x4 matrix) ActiveVesselBubble; // only needed when the active vessel is an editor vessel
        public static List<(ProtoVessel vessel, ParticleMesh mesh, Matrix4x4 matrix)> RelayBubbles = new List<(ProtoVessel vessel, ParticleMesh mesh, Matrix4x4 matrix)>();
        //public static List<(ProtoVessel vessel, ParticleMesh mesh, Matrix4x4 matrix)> ActiveRelayBubbles = new List<(ProtoVessel vessel, ParticleMesh mesh, Matrix4x4 matrix)>();

        
        // call this before whenever the scene is loaded
        public static void Init()
        {
            float trackingStationLevel = ScenarioUpgradeableFacilities.GetFacilityLevel (SpaceCenterFacility.TrackingStation);
            DSNPower = GameVariables.Instance.GetDSNRange (trackingStationLevel);
            LoadMat();
            
            // init the single bubbles
            DirectConDSNBubble = (DefinedParticleMeshes.MediumCloud, Matrix4x4.identity);
            FirstHopBubble = (null, DefinedParticleMeshes.MediumCloud, Matrix4x4.identity);
            ActiveVesselBubble = (DefinedParticleMeshes.MediumCloud, Matrix4x4.identity);
            InitRelayBubbles();
        }
        
        public static void LoadMat()
        {
            if (pointMat == null)
            {
                pointMat = Lib.GetShader("PointParticle");
            }
        }
        
        
        // Relay list can change if any vessel is destroyed or added
        public static void InitRelayBubbles()
        {
            List<(ProtoVessel vessel, ParticleMesh mesh, Matrix4x4 matrix)> bubbles = new List<(ProtoVessel vessel, ParticleMesh mesh, Matrix4x4 matrix)>();
            // Flying ship list should be up to date
            foreach (var vessel in AHShipList.FlightProtoShipList)
            {
                ProtoVessel protoVessel = vessel.Key;
                AHShipAntennas shipAntennas = vessel.Value;
                if (shipAntennas.RelayPower > 0)
                {
                    bubbles.Add((protoVessel, DefinedParticleMeshes.MediumCloud, Matrix4x4.identity));
                }
            }
            RelayBubbles = bubbles;
        }
        
        
        // Get current commnet path and set bubbles
        public static void GetCommNetPathBubbles()
        {
            
            
            //List<(ProtoVessel vessel, ParticleMesh mesh, Matrix4x4 matrix)> bubbles = new List<(ProtoVessel vessel, ParticleMesh mesh, Matrix4x4 matrix)>();
            if (activeVessel.vessel != null)
            {
                activeCommPathVessels = AHCommNet.GetCommPathVessels(activeVessel.vessel);
                if (directconnectedToHome)
                {
                    FirstHopBubble = (null, DefinedParticleMeshes.MediumCloud, Matrix4x4.identity);
                }
                else
                {
                    // get first ship on path
                    ProtoVessel commnetVessel = activeCommPathVessels[0];
                    FirstHopBubble = (commnetVessel, DefinedParticleMeshes.MediumCloud, Matrix4x4.identity);
                }
                
                // foreach (var commnetVessel in  activeCommPathVessels)
                // {
                //     bubbles.Add((commnetVessel, DefinedParticleMeshes.MediumCloud, Matrix4x4.identity));
                // }
                
                // // if list is empty and we are connected to home, we have a direct connection to home
                // if (activeCommPathVessels.Count == 0 && connectedToHome)
                // {
                //     
                // }
            }
            //ActiveRelayBubbles = bubbles;
        }


        // this should be called when the vessel changes or any update on the commnet path
        public static void OnVesselChange()
        {
            GetCommNetPathBubbles();
            UpdateBubbleRanges();
        }

        
        // here we update the ranges of the bubbles
        public static void UpdateBubbleRanges()
        {
            double radiusBubble = 0;
            CelestialBody homeBody = FlightGlobals.GetHomeBody();
            
            // DSN Bubble for active connection
            ActiveShipAntennas.UpdateRanges(DSNPower);
            if (selectedAntennaType == AHAntennaType.ALL)
            {
                radiusBubble = homeBody.Radius + ActiveShipAntennas.VesselRangesMax[selectedSignalStrength];
            }
            else
            {
                radiusBubble = homeBody.Radius + ActiveShipAntennas.RelayRangesMax[selectedSignalStrength];
            }     
            DirectConDSNBubble.matrix = UpdateMatrix(DirectConDSNBubble.matrix, newRadius: radiusBubble);
            
            
            // CommPath Bubbles
            if (activeVessel.vessel != null)
            {
                if (directconnectedToHome)
                {
                    // is direct connected to home. Use radius from above
                    FirstHopBubble.matrix = UpdateMatrix(FirstHopBubble.matrix, newRadius: radiusBubble);
                }
                else
                {
                    AHShipAntennas firstHopShipAntennas = AHShipList.FlightProtoShipList[FirstHopBubble.vessel];
                    // this is first hop. so we can use all antennas or only relay antennas from active ship
                    if (selectedAntennaType == AHAntennaType.ALL)
                    {
                        firstHopShipAntennas.UpdateRanges(ActiveShipAntennas.VesselPower);
                    }
                    else
                    {
                        firstHopShipAntennas.UpdateRanges(ActiveShipAntennas.RelayPower);
                    }                    
                    radiusBubble = firstHopShipAntennas.RelayRangesMax[selectedSignalStrength];
                    FirstHopBubble.matrix = UpdateMatrix(FirstHopBubble.matrix, newRadius: radiusBubble);
                    
                    // ProtoVessel previousVessel = null;
                    // for (int i = 0; i < ActiveRelayBubbles.Count; i++) // just lets hope this is in correct order
                    // {
                    //     bool isFirst = (i == 0);
                    //     bool isLast = (i == ActiveRelayBubbles.Count - 1);
                    //     
                    //     var bubble= ActiveRelayBubbles[i];
                    //     ProtoVessel currentVessel = ActiveRelayBubbles[i].vessel;
                    //     Matrix4x4 currentMatrix = ActiveRelayBubbles[i].matrix;
                    //     AHShipAntennas currentShipAntennas = AHShipList.FlightProtoShipList[currentVessel];
                    //     
                    //     if (isFirst)
                    //     {
                    //         // this is first hop. so we can use all antennas or only relay antennas from active ship
                    //         if (selectedAntennaType == AHAntennaType.ALL)
                    //         {
                    //             currentShipAntennas.UpdateRanges(ActiveShipAntennas.VesselPower);
                    //         }
                    //         else
                    //         {
                    //             currentShipAntennas.UpdateRanges(ActiveShipAntennas.RelayPower);
                    //         }
                    //         radiusBubble = currentShipAntennas.RelayRangesMax[selectedSignalStrength];
                    //         bubble.matrix = UpdateMatrix(currentMatrix, newRadius: radiusBubble);
                    //         ActiveRelayBubbles[i] = bubble;
                    //     }
                    //     if (isLast)  // first and last can be same vessel
                    //     {
                    //         // when it is last, this is usually connected to home. Update range for DSN
                    //         currentShipAntennas.UpdateRanges(DSNPower);
                    //         radiusBubble = homeBody.Radius + currentShipAntennas.RelayRangesMax[selectedSignalStrength];
                    //         RelayConDSNBubble.matrix = UpdateMatrix(RelayConDSNBubble.matrix, newRadius: radiusBubble);
                    //     }
                    //     if (!isFirst && !isLast)
                    //     {
                    //         // in between relays
                    //         if (previousVessel != null)
                    //         {
                    //             AHShipAntennas previousShipAntennas = AHShipList.FlightProtoShipList[previousVessel];
                    //             currentShipAntennas.UpdateRanges(previousShipAntennas.RelayPower);
                    //             radiusBubble = currentShipAntennas.RelayRangesMax[selectedSignalStrength];
                    //             bubble.matrix = UpdateMatrix(currentMatrix, newRadius: radiusBubble);
                    //             ActiveRelayBubbles[i] = bubble;                                
                    //         }
                    //     }
                    //     previousVessel = currentVessel;
                    // }
                }
            }
        }
        


        // public static void UpdateBubbles()
        // {
        //     
        //     if (selectedShipType != AHTargetType.EDITORSPH && 
        //         selectedShipType != AHTargetType.EDITORVAB &&
        //         (selectedShipType == AHTargetType.DSN || activeVessel.vessel == null))
        //     {
        //         // nothing selected, so no bubbles to show
        //         DSNBubble = (null, Matrix4x4.identity);
        //         ActiveVesselBubble = (null, Matrix4x4.identity);
        //         RelayBubbles = new Dictionary<ProtoVessel, (ParticleMesh mesh, Matrix4x4 matrix)>();
        //         connectionRelayBubbles = new Dictionary<ProtoVessel, (ParticleMesh mesh, Matrix4x4 matrix)>();
        //         return;
        //     }
        //     
        //     double radiusBubble = 0;
        //     CelestialBody homeBody = FlightGlobals.GetHomeBody();
        //     
        //     // if (selectedShipType == AHTargetType.EDITORVAB || selectedShipType == AHTargetType.EDITORSPH)
        //     // {
        //     //     // editor ships
        //     // }
        //     
        //     // DSN Bubble
        //     DSNBubble.mesh = DefinedParticleMeshes.MediumCloud;
        //     ActiveShipAntennas.UpdateRanges(DSNPower);
        //     if (selectedAntennaType == AHAntennaType.ALL)
        //     {
        //         radiusBubble = homeBody.Radius + ActiveShipAntennas.VesselRangesMax[selectedSignalStrength];
        //     }
        //     else
        //     {
        //         radiusBubble = homeBody.Radius + ActiveShipAntennas.RelayRangesMax[selectedSignalStrength];
        //     }
        //     DSNBubble.matrix = Matrix4x4.TRS(
        //         homeBody.position,
        //         Quaternion.identity,
        //         Vector3.one * (float)radiusBubble
        //     );
        //     
        //     
        //     // Active connection
        //     connectionRelayBubbles = new Dictionary<ProtoVessel, (ParticleMesh mesh, Matrix4x4 matrix)>(); // when target get resetted
        //     if (activeVessel.vessel != null)
        //     {
        //         activeCommPathVessels = AHCommNet.GetCommPathVessels(activeVessel.vessel);
        //         foreach (ProtoVessel vessel in activeCommPathVessels)
        //         {
        //             AHShipAntennas shipantennas = new AHShipAntennas();
        //             shipantennas.FetchAntennas(vessel.protoPartSnapshots, false);
        //             if (selectedAntennaType == AHAntennaType.ALL)
        //             {
        //                 shipantennas.UpdateRanges(ActiveShipAntennas.VesselPower);
        //                 radiusBubble = shipantennas.VesselRangesMax[selectedSignalStrength];
        //             }
        //             else
        //             {
        //                 shipantennas.UpdateRanges(ActiveShipAntennas.RelayPower);
        //                 radiusBubble = shipantennas.RelayRangesMax[selectedSignalStrength];
        //             }
        //
        //             connectionRelayBubbles[vessel] = (DefinedParticleMeshes.MediumCloud,
        //                 DSNBubble.matrix = Matrix4x4.TRS(
        //                     vessel.position,
        //                     Quaternion.identity,
        //                     Vector3.one * (float)radiusBubble
        //                 ));
        //         }                
        //     }
        // }
        
        public static void Render()
        {
            // only render when we have a valid target
            if (selectedShipType != AHTargetType.FLIGHT && 
                selectedShipType != AHTargetType.EDITORVAB && 
                selectedShipType != AHTargetType.EDITORSPH) return;

 
            
            // check mat type
            if (pointMat == null)
            {
                LoadMat();
            }
            
            // set color for selection
            switch (selectedSignalStrength)
            {
                case 0:
                    pointMat.SetColor("POINT_COLOR", AHColors.Bubble0);
                    break;
                case 25:
                    pointMat.SetColor("POINT_COLOR", AHColors.Bubble25);
                    break;
                case 50:
                    pointMat.SetColor("POINT_COLOR", AHColors.Bubble50);
                    break;
                case 75:
                    pointMat.SetColor("POINT_COLOR", AHColors.Bubble75);
                    break;
                case 100:
                    pointMat.SetColor("POINT_COLOR", AHColors.Bubble100);
                    break;
                default:
                    pointMat.SetColor("POINT_COLOR", AHColors.Bubble100);
                    break;
            }
            pointMat.SetFloat("POINT_SIZE", 10.0f);				
            // enable material
            pointMat.SetPass(0);
            
            // update bubble positions
            UpdatePosition();
            
            
            // render DSN if selected
            if (
                (// standard conditions
                    displayType == AHDisplayType.DSN ||
                    displayType == AHDisplayType.DSNRELAY
                    ) && 
                (// extra conditions to check
                    !(selectedAntennaType == AHAntennaType.RELAY && ActiveShipAntennas.RelayPower <= 0)
                    )
                )
            {
                // always scale in render time to get correct camera scale!
                DirectConDSNBubble.mesh.Render(ToScaledSpace(DirectConDSNBubble.matrix));
            }

            
            // // render nothing when set active and current vessel is not connected
            // if(selectedShipType == AHTargetType.FLIGHT && 
            //    displayType == AHDisplayType.ACTIVE && 
            //    !activeVessel.vessel.connection.IsConnected) return;
            
            // render active connection and only on Flight ships.
            if (displayType == AHDisplayType.ACTIVE &&
                selectedShipType == AHTargetType.FLIGHT)
            {
                // the firsthop we only should render if we have relay power if relay is selected
                if (!(selectedAntennaType == AHAntennaType.RELAY && ActiveShipAntennas.RelayPower <= 0))
                {
                    FirstHopBubble.mesh.Render(ToScaledSpace(FirstHopBubble.matrix));
                }
            }
        }
        
        public static Matrix4x4 ToScaledSpace(Matrix4x4 worldMatrix)
        {
            // Extract position and scale
            Vector3 position = worldMatrix.GetColumn(3);
            Vector3 scale = new Vector3(
                worldMatrix.GetColumn(0).magnitude,
                worldMatrix.GetColumn(1).magnitude,
                worldMatrix.GetColumn(2).magnitude
            );

            // Convert position and scale to ScaledSpace
            Vector3 scaledPos = ScaledSpace.LocalToScaledSpace(position);
            float scaledFactor = ScaledSpace.InverseScaleFactor;

            return Matrix4x4.TRS(
                scaledPos,
                Quaternion.identity,
                Vector3.one * scaledFactor * scale.x // assume uniform scale
            );
        }
        
        public static Matrix4x4 UpdateMatrix(Matrix4x4 current, Vector3? newWorldPos = null, double? newRadius = null)
        {
            // If nothing to update, just return the original
            if (newWorldPos == null && newRadius == null)
                return current;

            // --- Extract old values ---
            Vector3 oldWorldPos = current.GetColumn(3);
            Vector3 oldRadius = new Vector3(
                current.GetColumn(0).magnitude,
                current.GetColumn(1).magnitude,
                current.GetColumn(2).magnitude
            );

            // --- Determine what to use ---
            Vector3 worldPos = newWorldPos ?? oldWorldPos;
            float radius = (float)(newRadius ?? oldRadius.x);
            
            // --- Rebuild matrix ---
            return Matrix4x4.TRS(
                worldPos,
                Quaternion.identity,
                Vector3.one * radius
            );
        }        
        
        
        // The world is moving, we need to update positions
        public static void UpdatePosition()
        {
            
            // DSN Bubble
            CelestialBody homeBody = FlightGlobals.GetHomeBody();
            DirectConDSNBubble.matrix = UpdateMatrix(DirectConDSNBubble.matrix, newWorldPos: homeBody.position);
            
            
            // Active Connection
            if (directconnectedToHome)
            {
                FirstHopBubble.matrix = UpdateMatrix(FirstHopBubble.matrix, newWorldPos: homeBody.position);
            }
            else
            {
                // first hop is a relay
                if (FirstHopBubble.vessel != null)
                {
                    Vessel v = FirstHopBubble.vessel.vesselRef;
                    FirstHopBubble.matrix = UpdateMatrix(FirstHopBubble.matrix, newWorldPos: v.GetWorldPos3D());                    
                }
            }
            
            
            //Vector3 radiusBubble = Vector3.zero;
            // radiusBubble = new Vector3(
            //     DirectConDSNBubble.matrix.GetColumn(0).magnitude,
            //     DirectConDSNBubble.matrix.GetColumn(1).magnitude,
            //     DirectConDSNBubble.matrix.GetColumn(2).magnitude
            // );
            // DirectConDSNBubble.matrix = Matrix4x4.TRS(
            //     homeBody.position,
            //     Quaternion.identity,
            //     Vector3.one * radiusBubble.x
            // ); 
            
            // // Active connection
            // var updates = new List<(ProtoVessel key, (ParticleMesh mesh, Matrix4x4 matrix) val)>();
            // foreach (var kvp in connectionRelayBubbles)
            // {
            //     Matrix4x4 relayMatrix = kvp.Value.matrix;
            //     radiusBubble = new Vector3(
            //         relayMatrix.GetColumn(0).magnitude,
            //         relayMatrix.GetColumn(1).magnitude,
            //         relayMatrix.GetColumn(2).magnitude
            //     );
            //     // get new position
            //     Vessel v = kvp.Key.vesselRef;
            //     Matrix4x4 newMat = Matrix4x4.TRS(
            //         v.GetWorldPos3D(),
            //         Quaternion.identity,
            //         Vector3.one * radiusBubble.x
            //     );    
            //     updates.Add((kvp.Key, (kvp.Value.mesh, newMat)));
            // }
            // // Apply changes after iteration
            // foreach (var u in updates)
            // {
            //     connectionRelayBubbles[u.key] = u.val;
            // }
        }       
    }        
}