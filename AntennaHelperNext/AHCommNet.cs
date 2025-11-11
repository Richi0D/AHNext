using System.Collections.Generic;
using CommNet;
using UnityEngine;

namespace AntennaHelperNext
{
    public static class AHCommNet
    {
        
        public static List<ProtoVessel> GetCommPathVessels(Vessel vessel, bool isEditorVessel=false)
        {
            List<ProtoVessel> vesselsOnPath = new List<ProtoVessel>();
            if (vessel == null || isEditorVessel)
            {
                // no path for editor vessel or no vessel selected
                return vesselsOnPath;
            }
            AHMapCircle.connectedToHome = false; // reset
            AHMapCircle.directconnectedToHome = false; // reset
			
            // a CommPath has multiple links each link is a CommLink, A is source, B is destination
            CommPath commpath = vessel.connection.ControlPath;
            foreach (CommLink link in commpath)
            {
                // Debug.Log ("[AH] Vessel a on commpath : " + link.a.displayName);
                // Debug.Log ("[AH] Vessel b on commpath : " + link.b.displayName);
                // get Vessel
                Vessel v = link.a.transform.GetComponent<Vessel>();
                if (v != null && v.id != vessel.id)
                {
                    // find matching vessel from Flightlist
                    foreach (var kvp in AHShipList.FlightProtoShipList)
                    {
                    	if (kvp.Key.vesselID == v.protoVessel.vesselID)
                    	{
                    		vesselsOnPath.Add(kvp.Key);
                    	}
                    }
                }
                
                if (v !=null && v.id == vessel.id)
                {
                    // check and save if we are directly connected to home
                    AHMapCircle.directconnectedToHome = link.b.isHome;
                }
                
                if (link.b.isHome)
                {
                    // any vessel on the path is connected to home
                    AHMapCircle.connectedToHome = true;
                }
            }
            
            // the length should be always smaller than 1 from the CommPath (exclude current vessel)
            if (commpath.Count > 1 && commpath.Count - 1 != vesselsOnPath.Count)
            {
                Debug.Log("[AH] It seems not all CommPath vessels are found." + " CommPath Count: " + commpath.Count + " Found Count: " + vesselsOnPath.Count + "");
            }
            
            return vesselsOnPath;
        }
    }
}