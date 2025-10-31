using System;
using System.Collections.Generic;
using UnityEngine;

namespace AntennaHelperNext
{
    public static class AHPlanetList
    {
        public static CelestialBody homeBody = FlightGlobals.GetHomeBody();
        public static Dictionary<string,(double, double)> PlanetList = new Dictionary<string,(double, double)>();
        
        
        public static void LoadPlanetList()
        {
            PlanetList.Clear();
            foreach (CelestialBody moon in homeBody.orbitingBodies) {
                if (moon.DiscoveryInfo.HaveKnowledgeAbout())
                {
                    PlanetList.Add(moon.bodyName, (moon.orbit.PeR, moon.orbit.ApR));
                }
                
            }
            foreach (CelestialBody planet in FlightGlobals.Bodies[0].orbitingBodies) {
                if (planet != homeBody) {
                    PlanetList.Add (GetDistancePlanet (homePlanet, planet));
                }
            }
        }
    }
}