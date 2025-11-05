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
        
        // Selector variables
        public static AHDisplayType displayType = AHDisplayType.ACTIVE;
        public static AHTargetType selectedShipType = AHTargetType.FLIGHT;
        public static double selectedSignalStrength = 0;
        // ParticleMeshes
        public static ParticleMesh DSNBubble;
        public static ParticleMesh ActiveVesselBubble;
        public static Dictionary<string, ParticleMesh> RelayBubbles = new Dictionary<string, ParticleMesh>();
        
        public static List<ProtoVessel> activeCommPathVessels; // save here the vessels from the commpath
        
        public static void LoadMat()
        {
            if (pointMat == null)
            {
                pointMat = Lib.GetShader("PointParticle");
            }
        }
        
        
        public static void Render()
        {
            // only render when we have a valid target
            if (selectedShipType != AHTargetType.FLIGHT && 
                selectedShipType != AHTargetType.EDITORVAB && 
                selectedShipType != AHTargetType.EDITORSPH) return;

            if (pointMat == null)
            {
                LoadMat();
            }
            
            // set color
            pointMat.SetColor("POINT_COLOR", new Color(0.0f, 0.9f, 0.0f, 0.8f));
            pointMat.SetFloat("POINT_SIZE", 10.0f);				
            // enable material
            pointMat.SetPass(0);
            
            
            // render DSN if selected
            if (DSNBubble != null)
            {
                CelestialBody homeBody = FlightGlobals.GetHomeBody();
                Vector3d scaledPos = ScaledSpace.LocalToScaledSpace(homeBody.position);
                double radiusDSN = homeBody.Radius * 1.2;
                Matrix4x4 mDSN = Matrix4x4.TRS(
                    scaledPos,
                    Quaternion.identity,
                    Vector3.one * ScaledSpace.InverseScaleFactor * (float)radius
                );
                // render cloud
                DSNBubble.Render(mDSN);
            }
            
 

            
            
        }
    }        
}