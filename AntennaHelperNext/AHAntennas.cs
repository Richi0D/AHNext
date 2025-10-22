using System.Collections.Generic;
using UnityEngine;

namespace AntennaHelperNext
{
    public class ShipAntennas
    {
        public List<ModuleDataTransmitter> antennas;
        
        public ShipAntennas()
        {
            antennas = new List<ModuleDataTransmitter>();
        }

        public void FetchAntennas(List<Part> parts)
        {
            antennas.Clear();
            foreach (Part part in parts)
            {
                foreach (ModuleDataTransmitter antenna in part.FindModulesImplementing<ModuleDataTransmitter>())
                {
                    antennas.Add(antenna);
                }
            }
        }
        
        public void ClearAntennas()
        {
            antennas.Clear();
        }
        
        public void AddAntenna(ModuleDataTransmitter antenna)
        {
            antennas.Add(antenna);
        }
    }
}