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
			
            // a CommPath has multiple links each link is a CommLink, A is source, B is destination
            CommPath commpath = vessel.connection.ControlPath;
            // Debug.Log ("[AH] commpath : " + commpath);
            // debugSignalStrength = vessel.connection.SignalStrength;
			
            foreach (CommLink link in commpath)
            {
                // get Vessel
                Vessel v = link.a.transform.GetComponent<Vessel>();
                if (v != null && v.id != vessel.id)
                {
                    vesselsOnPath.Add(v.protoVessel);
                    // // find matching vessel from Flightlist
                    // foreach (ProtoVessel protovessel in AHShipList.FlightProtoShipList.Keys)
                    // {
                    // 	if (protovessel.vesselID == v.protoVessel.vesselID)
                    // 	{
                    // 		vesselsOnPath.Add(protovessel);
                    // 	}
                    // }					
                }
            }
            return vesselsOnPath;
        }
    }
}