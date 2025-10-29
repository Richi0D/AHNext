using System;
using System.Collections.Generic;
using UnityEngine;

namespace AntennaHelperNext
{
    public class ShipAntennas
    {
        public List<ModuleDataTransmitter> Antennas = new List<ModuleDataTransmitter>();
        public List<ModuleDataTransmitter> DirectAntennas = new List<ModuleDataTransmitter>();
        public List<ModuleDataTransmitter> DirectCombAntennas = new List<ModuleDataTransmitter>();
        public List<ModuleDataTransmitter> RelayAntennas = new List<ModuleDataTransmitter>();
        public List<ModuleDataTransmitter> RelayCombAntennas = new List<ModuleDataTransmitter>();
        public ModuleDataTransmitter StrongestAntenna = null;
        public ModuleDataTransmitter StrongestRelayAntenna = null;
        public ModuleDataTransmitter StrongestDirectAntenna = null;
        public double RelayPower = 0;
        public double DirectPower = 0;
        public double VesselPower = 0;
        public Dictionary<double, double> RelayRangesMax = new Dictionary<double, double>
        {
            {0, 0 }, // is 0.5%
            {25, 0 },
            {50, 0 },
            {75, 0 },
            {100, 0 } // is 99.5%%
        };
        public Dictionary<double, double> VesselRangesMax = new Dictionary<double, double>
        {
            {0, 0 }, // is 0.5%
            {25, 0 },
            {50, 0 },
            {75, 0 },
            {100, 0 } // is 99.5%%
        };
        
    public void FetchAntennas(List<Part> parts)
        {
            Antennas.Clear();
            foreach (Part part in parts)
            {
                AddAntenna(part);
            }
            UpdateAntennas();
        }
        
        public void ClearAntennas()
        {
            Antennas.Clear();
        }
        
        public void AddAntenna(ModuleDataTransmitter antenna)
        {
            Antennas.Add(antenna);
        }
        public void AddAntenna(Part part)
        {
            foreach (ModuleDataTransmitter antenna in part.FindModulesImplementing<ModuleDataTransmitter>())
            {
                Antennas.Add(antenna);
            }
        }
        public void RemoveAntenna(ModuleDataTransmitter antenna)
        {
            if (Antennas.Contains(antenna))
            {
                Antennas.Remove(antenna);
            }
        }
        public void RemoveAntenna(Part part)
        {
            foreach (ModuleDataTransmitter antenna in part.FindModulesImplementing<ModuleDataTransmitter>())
            {
                if (Antennas.Contains(antenna))
                {
                    Antennas.Remove(antenna);
                }
            }
        }
        
        public void UpdateAntennas()
        {
            // reset everything
            DirectAntennas = new List<ModuleDataTransmitter>();
            DirectCombAntennas = new List<ModuleDataTransmitter>();
            RelayAntennas = new List<ModuleDataTransmitter>();
            RelayCombAntennas = new List<ModuleDataTransmitter>();
            StrongestAntenna = null;
            StrongestRelayAntenna = null;
            StrongestDirectAntenna = null;
            VesselPower = 0;
            RelayPower = 0;
            DirectPower = 0;
            double SumAntennaPower = 0;
            double SumRelayAntennaPower = 0;
            double SumDirectAntennaPower = 0;            
            
            foreach (ModuleDataTransmitter antenna in Antennas)
            {
                SumAntennaPower += antenna.antennaPower;
                if (StrongestAntenna == null || antenna.antennaPower > StrongestAntenna.antennaPower)
                {
                    StrongestAntenna = antenna;
                }                
                if (antenna.antennaType == AntennaType.DIRECT)
                {
                    SumDirectAntennaPower += antenna.antennaPower;
                    DirectAntennas.Add(antenna);
                    if (antenna.antennaCombinable)
                    {
                        DirectCombAntennas.Add(antenna);
                    }
                    if (StrongestDirectAntenna == null || antenna.antennaPower > StrongestDirectAntenna.antennaPower)
                    {
                        StrongestDirectAntenna = antenna;
                    }                    
                }
                else if (antenna.antennaType == AntennaType.RELAY)
                {
                    SumRelayAntennaPower += antenna.antennaPower;
                    RelayAntennas.Add(antenna);
                    if (antenna.antennaCombinable)
                    {
                        RelayCombAntennas.Add(antenna);
                    }
                    if (StrongestRelayAntenna == null || antenna.antennaPower > StrongestRelayAntenna.antennaPower)
                    {
                        StrongestRelayAntenna = antenna;
                    }                    
                }
                else
                {
                    Debug.Log(antenna.part.partInfo.title + " has an unknown antenna type: " + antenna.antennaType);
                }
            }
            // with all the data we can calculate the different antenna powers
            if (StrongestAntenna != null)
            {
                VesselPower = AHUtil.CalcVesselPower(StrongestAntenna.antennaPower, SumAntennaPower, AHUtil.GetAWCE(Antennas));
            }
            else
            {
                VesselPower = 0;
            }
            if (StrongestRelayAntenna != null)
            {
                RelayPower = AHUtil.CalcVesselPower(StrongestRelayAntenna.antennaPower, SumRelayAntennaPower, AHUtil.GetAWCE(RelayAntennas));
            }
            else
            {
                RelayPower = 0;
            }
            if (StrongestDirectAntenna != null)
            {
                DirectPower = AHUtil.CalcVesselPower(StrongestDirectAntenna.antennaPower, SumDirectAntennaPower, AHUtil.GetAWCE(DirectAntennas));
            }
            else
            {
                DirectPower = 0;
            }
        }
        public void UpdateRanges(double targetPower)
        {
            double maxRelayRange = AHUtil.GetMaxRange(RelayPower, targetPower);
            RelayRangesMax = AHUtil.GetDistancesBySignalFixed(maxRelayRange);
            double maxVesselRange = AHUtil.GetMaxRange(VesselPower, targetPower);
            VesselRangesMax = AHUtil.GetDistancesBySignalFixed(maxVesselRange);
        }
        
    }
}