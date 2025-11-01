using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace AntennaHelperNext
{
    public class AHShipAntennas
    {
        public List<ModuleDataTransmitter> Antennas = new List<ModuleDataTransmitter>();
        public List<ModuleDataTransmitter> DirectAntennas = new List<ModuleDataTransmitter>();
        public List<ModuleDataTransmitter> DirectCombAntennas = new List<ModuleDataTransmitter>();
        public List<ModuleDataTransmitter> RelayAntennas = new List<ModuleDataTransmitter>();
        public List<ModuleDataTransmitter> RelayCombAntennas = new List<ModuleDataTransmitter>();
        public List<Part> AntennasNotExtended = new List<Part>();
        public ModuleDataTransmitter StrongestAntenna = null;
        public ModuleDataTransmitter StrongestRelayAntenna = null;
        public ModuleDataTransmitter StrongestDirectAntenna = null;
        public ModuleDataTransmitter StrongestRelayAntennaNonCombinable = null;
        public ModuleDataTransmitter StrongestDirectAntennaNonCombinable = null;        
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
        public Dictionary<string, (double minVesselSignal, double maxVesselSignal, double minRelaySignal, double maxRelaySignal)> PlanetSignalStrengths =
            new Dictionary<string, (double minVesselSignal, double maxVesselSignal, double minRelaySignal, double maxRelaySignal)>();

        public AHShipAntennas()
        {
            foreach (var planet in AHPlanetList.PlanetList)
            {
                string planetName = planet.Key.bodyName;
                PlanetSignalStrengths.Add(planetName, (0, 0, 0, 0));
            }
        }
        
        public void FetchAntennas(List<Part> parts, bool includeNotExtended = false)
        {
            Antennas.Clear();
            AntennasNotExtended.Clear();
            foreach (Part part in parts)
            {
                // skip not extended antennas
                if (!includeNotExtended)
                {
                    if (part.HasModuleImplementing<ModuleDeployableAntenna>()) {
                        ModuleDeployableAntenna antDep = part.FindModuleImplementing<ModuleDeployableAntenna> ();
                        if ((antDep.deployState != ModuleDeployablePart.DeployState.EXTENDED) 
                            && (antDep.deployState != ModuleDeployablePart.DeployState.EXTENDING)) {
                            AntennasNotExtended.Add(part);
                            continue;
                        }
                    }
                }
                AddAntenna(part);
            }
            UpdateAntennas();
        }
        
        public void FetchAntennas(List<ProtoPartSnapshot> protParts, bool includeNotExtended = false)
        {
            Antennas.Clear();
            AntennasNotExtended.Clear();
            foreach (ProtoPartSnapshot protPart in protParts)
            {
                bool skipPart = false;
                Part part = protPart.partPrefab;
                
                // skip not extended antennas
                if (!includeNotExtended)
                {
                    // find deploy state of part
                    foreach (ProtoPartModuleSnapshot protoModule in protPart.modules)
                    {
                        if (protoModule.moduleName == "ModuleDeployableAntenna")
                        {
                            ConfigNode moduleValues = protoModule.moduleValues;
                            if (moduleValues.HasValue("deployState"))
                            {
                                // RETRACTED, EXTENDED, RETRACTING, EXTENDING or BROKEN
                                string deployState = moduleValues.GetValue("deployState");
                                if (deployState != "EXTENDED" && deployState != "EXTENDING")
                                {
                                    AntennasNotExtended.Add(part);
                                    skipPart = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (skipPart) continue; // skip not extended antennas
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

        public int countantennas(ModuleDataTransmitter antenna)
        {
            if (antenna is null) return 0;
            return Antennas.Count(a => a == antenna);
        }
        
        public void UpdateAntennas()
        {
            // reset everything
            DirectAntennas.Clear();
            DirectCombAntennas.Clear();
            RelayAntennas.Clear();
            RelayCombAntennas.Clear();
            StrongestAntenna = null;
            StrongestRelayAntenna = null;
            StrongestDirectAntenna = null;
            StrongestRelayAntennaNonCombinable = null;
            StrongestDirectAntennaNonCombinable = null;   
            VesselPower = 0;
            RelayPower = 0;
            DirectPower = 0;
            double SumAntennaPower = 0;
            double SumRelayAntennaPower = 0;
            double SumDirectAntennaPower = 0;            
            
            foreach (ModuleDataTransmitter antenna in Antennas)
            {
                SumAntennaPower += antenna.antennaPower;
                if (StrongestAntenna is null || antenna.antennaPower > StrongestAntenna.antennaPower)
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
                    if (StrongestDirectAntenna is null || antenna.antennaPower > StrongestDirectAntenna.antennaPower)
                    {
                        StrongestDirectAntenna = antenna;
                    }
                    if (!antenna.antennaCombinable && (StrongestDirectAntennaNonCombinable is null || 
                        !antenna.antennaCombinable && antenna.antennaPower > StrongestDirectAntennaNonCombinable.antennaPower))
                    {
                        StrongestDirectAntennaNonCombinable = antenna;
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
                    if (StrongestRelayAntenna is null || antenna.antennaPower > StrongestRelayAntenna.antennaPower)
                    {
                        StrongestRelayAntenna = antenna;
                    }   
                    if (!antenna.antennaCombinable && (StrongestRelayAntennaNonCombinable is null || 
                        antenna.antennaPower > StrongestRelayAntennaNonCombinable.antennaPower))
                    {
                        StrongestRelayAntennaNonCombinable = antenna;
                    }                      
                }
                else if (antenna.antennaType == AntennaType.INTERNAL)
                {
                    // nothing to do here
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
            
            // update ranges for planets
            foreach (var planet in AHPlanetList.PlanetList)
            {
                string planetName = planet.Key.bodyName;
                double minDistance = planet.Value.minDistance;
                double maxDistance = planet.Value.maxDistance;

                double minVesselSignal = AHUtil.GetSignalStrength(AHUtil.GetNormalizedRange(minDistance, maxVesselRange));
                double maxVesselSignal = AHUtil.GetSignalStrength(AHUtil.GetNormalizedRange(maxDistance, maxVesselRange));
                double minRelaySignal = AHUtil.GetSignalStrength(AHUtil.GetNormalizedRange(minDistance, maxRelayRange));
                double maxRelaySignal = AHUtil.GetSignalStrength(AHUtil.GetNormalizedRange(maxDistance, maxRelayRange));

                PlanetSignalStrengths[planetName] = (minVesselSignal, maxVesselSignal, minRelaySignal, maxRelaySignal);
            }
        }
    }
}