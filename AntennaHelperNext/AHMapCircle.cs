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
        public static List<Vessel> activeCommPathVessels = new List<Vessel>(); // save here the vessels from the commpath
        public static double DSNPower = 0;
        public static bool connectedToHome = false;
        public static bool directconnectedToHome = false;
        
        // Selector variables
        public static AHDisplayType displayType = AHDisplayType.ACTIVE;
        public static AHTargetType selectedShipType = AHTargetType.FLIGHT;
        public static double selectedSignalStrength = 0;
        public static AHAntennaType selectedAntennaType = AHAntennaType.ALL;
        public static bool inMapView = false;
        
        // ParticleMeshes and Position Matrices for each entity
        public static (ParticleMesh mesh, Matrix4x4 matrix) DirectConDSNBubble;
        public static (Vessel vessel, ParticleMesh mesh, Matrix4x4 matrix) FirstHopBubble; // this can be either the DSN or a relay vessel
        public static List<(Vessel vessel, ParticleMesh mesh, Matrix4x4 matrix)> RelayBubbles = new List<(Vessel vessel, ParticleMesh mesh, Matrix4x4 matrix)>();
        
        // call this before whenever the scene is loaded
        public static void Init()
        {
            float trackingStationLevel = ScenarioUpgradeableFacilities.GetFacilityLevel (SpaceCenterFacility.TrackingStation);
            DSNPower = GameVariables.Instance.GetDSNRange (trackingStationLevel);
            LoadMat();
            
            // init the single bubbles
            DirectConDSNBubble = (DefinedParticleMeshes.MediumCloud, Matrix4x4.identity);
            FirstHopBubble = (null, DefinedParticleMeshes.MediumCloud, Matrix4x4.identity);
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
            List<(Vessel vessel, ParticleMesh mesh, Matrix4x4 matrix)> bubbles = new List<(Vessel vessel, ParticleMesh mesh, Matrix4x4 matrix)>();
            // Flying ship list should be up to date
            foreach (var vessel in AHShipList.FlightShipList)
            {
                Vessel protoVessel = vessel.Key;
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
            if (activeVessel.vessel != null)
            {
                activeCommPathVessels = AHCommNet.GetCommPathVessels(activeVessel.vessel);
                // Debug.Log("Connected direct to home:" + directconnectedToHome);
                // Debug.Log("Connected to anything:" + activeVessel.vessel.connection.IsConnected);
                if (directconnectedToHome)
                {
                    FirstHopBubble = (null, DefinedParticleMeshes.MediumCloud, Matrix4x4.identity);
                }
                else
                {
                    // get first ship on path, no vessels on path if not connected!
                    if (activeVessel.vessel.connection.IsConnected)
                    {
                        Vessel commnetVessel = activeCommPathVessels[0];
                        FirstHopBubble = (commnetVessel, DefinedParticleMeshes.MediumCloud, Matrix4x4.identity);
                    }
                }
            }
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
                    if (activeVessel.vessel.connection.IsConnected)
                    {
                        AHShipAntennas firstHopShipAntennas = AHShipList.FlightShipList[FirstHopBubble.vessel];
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
                    }
                }
            }
            
            
            // Relay Bubbles
            for (int i = 0; i < RelayBubbles.Count; i++)
            {
                var bubble = RelayBubbles[i];    
                AHShipAntennas shipAntennas = AHShipList.FlightShipList[bubble.vessel];
                if (selectedAntennaType == AHAntennaType.ALL)
                {
                    shipAntennas.UpdateRanges(ActiveShipAntennas.VesselPower);
                }
                else
                {
                    shipAntennas.UpdateRanges(ActiveShipAntennas.RelayPower);
                }
                radiusBubble = shipAntennas.RelayRangesMax[selectedSignalStrength];
                Matrix4x4 newMatrix = UpdateMatrix(bubble.matrix, newRadius: radiusBubble);
                RelayBubbles[i] = (bubble.vessel, bubble.mesh, newMatrix);
            }
        }
        
        
        public static void Render()
        {
            // only render when we have a valid target
            if (selectedShipType != AHTargetType.FLIGHT && 
                selectedShipType != AHTargetType.EDITORVAB && 
                selectedShipType != AHTargetType.EDITORSPH) return;
            
            // render only if we have relay power if relay is selected
            if (selectedAntennaType == AHAntennaType.RELAY && ActiveShipAntennas.RelayPower <= 0) return;
            
            
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
            if (displayType == AHDisplayType.DSN ||
                displayType == AHDisplayType.DSNRELAY)
            {
                // always scale in render time to get correct camera scale!
                DirectConDSNBubble.mesh.Render(ToScaledSpace(DirectConDSNBubble.matrix));
            }
            
            // render active connection and only on Flight ships.
            if (displayType == AHDisplayType.ACTIVE &&
                selectedShipType == AHTargetType.FLIGHT &&
                activeVessel.vessel != null && 
                activeVessel.vessel.connection.IsConnected)
            {
                FirstHopBubble.mesh.Render(ToScaledSpace(FirstHopBubble.matrix));
            }
            
            // render relay bubbles
            if (displayType == AHDisplayType.RELAY ||
                displayType == AHDisplayType.DSNRELAY)
            {
                foreach (var bubble in RelayBubbles)
                {
                    bubble.mesh.Render(ToScaledSpace(bubble.matrix));
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
                    Vessel v = FirstHopBubble.vessel;
                    FirstHopBubble.matrix = UpdateMatrix(FirstHopBubble.matrix, newWorldPos: v.GetWorldPos3D());                    
                }
            }
            
            // Relay Bubbles
            for (int i = 0; i < RelayBubbles.Count; i++)
            {
                var bubble = RelayBubbles[i];    
                Vessel v = bubble.vessel;
                Matrix4x4 newMatrix = UpdateMatrix(bubble.matrix, newWorldPos: v.GetWorldPos3D());
                RelayBubbles[i] = (bubble.vessel, bubble.mesh, newMatrix);
            }
        }       
    }        
}