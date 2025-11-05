using System;
using System.Collections.Generic;
using UnityEngine;

namespace AntennaHelperNext
{
    public static class AHPlanetList
    {
        public static Dictionary<CelestialBody,(double minDistance, double maxDistance)> PlanetList = new Dictionary<CelestialBody,(double, double)>();
        
        private static (double minDistance, double maxDistance) GetDistancePlanet (CelestialBody home, CelestialBody target)
        {
            if (target.orbit?.referenceBody == home)
            {
                return (target.orbit.PeR, target.orbit.ApR);
            }
            
            double max = home.orbit.ApR + target.orbit.ApR;
            double min;
            if (home.orbit.PeR > target.orbit.PeR) {
                min = home.orbit.PeR - target.orbit.PeR;
            } else {
                min = target.orbit.PeR - home.orbit.PeR;
            }
            return (min, max);
        }
        
        public static void LoadPlanetList()
        {
            PlanetList.Clear();
            CelestialBody homeBody = FlightGlobals.GetHomeBody();
            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                // Skip the Sun and the home planet itself
                if (body == homeBody || body == Planetarium.fetch.Sun)
                    continue;

                // Only include discovered bodies
                if (body.DiscoveryInfo != null && 
                    body.DiscoveryInfo.HaveKnowledgeAbout(DiscoveryLevels.Presence))
                {
                    // and only include main planets that orbit sun or home planet
                    if (body.orbit?.referenceBody == homeBody || body.orbit?.referenceBody == Planetarium.fetch.Sun)
                    {
                        // calculate max and min distance to home planet
                        PlanetList[body] = GetDistancePlanet(homeBody, body);                        
                    }
                }
            }            
        }
    }
}