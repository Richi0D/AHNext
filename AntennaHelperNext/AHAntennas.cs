using System;
using System.Linq;
using System.Collections.Generic;
using CommNet;
using UnityEngine;

namespace AntennaHelperNext
{
    public class AHShipAntennas
    {
        public List<ModuleDataTransmitter> VesselAntennas = new List<ModuleDataTransmitter>();
        public List<ModuleDataTransmitter> VesselCombAntennas = new List<ModuleDataTransmitter>();
        public List<ModuleDataTransmitter> RelayAntennas = new List<ModuleDataTransmitter>();
        public List<ModuleDataTransmitter> RelayCombAntennas = new List<ModuleDataTransmitter>();
        public List<Part> AntennasNotExtended = new List<Part>();
        public ModuleDataTransmitter StrongestVesselAntenna = null;
        public ModuleDataTransmitter StrongestRelayAntenna = null;
        public ModuleDataTransmitter StrongestVesselAntennaNonCombinable = null;    
        public ModuleDataTransmitter StrongestRelayAntennaNonCombinable = null;
        public double VesselPower = 0;
        public double RelayPower = 0;
        public Dictionary<double, double> VesselRangesMax;
        public Dictionary<double, double> RelayRangesMax;
        public Dictionary<string, (double minVesselSignal, double maxVesselSignal, double minRelaySignal, double
            maxRelaySignal)> PlanetSignalStrengths;

        public AHShipAntennas()
        {
            // init planet signal strengths
            PlanetSignalStrengths =
                new Dictionary<string, (double minVesselSignal, double maxVesselSignal, double minRelaySignal, double maxRelaySignal)>();
            foreach (var planet in AHPlanetList.PlanetList)
            {
                string planetName = planet.Key.bodyName;
                PlanetSignalStrengths.Add(planetName, (0, 0, 0, 0));
            }
            // init vessel ranges
            RelayRangesMax = new Dictionary<double, double>();
            VesselRangesMax = new Dictionary<double, double>();
            foreach (var signal in AHUtil.SignalMultipliers)
            {
                double interval = signal.Key;
                RelayRangesMax.Add(interval, 0);
                VesselRangesMax.Add(interval, 0);
            }
        }
        
        public void FetchAntennas(List<Part> parts, bool includeNotExtended = false)
        {
            VesselAntennas.Clear();
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
            VesselAntennas.Clear();
            AntennasNotExtended.Clear();
            foreach (ProtoPartSnapshot protPart in protParts)
            {
                Part part = protPart.partPrefab;
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
                // // skip not extended antennas
                // bool skipPart = false;
                // if (!includeNotExtended)
                // {
                //     // find deploy state of part
                //     foreach (ProtoPartModuleSnapshot protoModule in protPart.modules)
                //     {
                //         if (protoModule.moduleName == "ModuleDeployableAntenna")
                //         {
                //             ConfigNode moduleValues = protoModule.moduleValues;
                //             if (moduleValues.HasValue("deployState"))
                //             {
                //                 // RETRACTED, EXTENDED, RETRACTING, EXTENDING or BROKEN
                //                 string deployState = moduleValues.GetValue("deployState");
                //                 if (deployState == "RETRACTED" && deployState == "RETRACTING")
                //                 {
                //                     AntennasNotExtended.Add(part);
                //                     skipPart = true;
                //                     break;
                //                 }
                //             }
                //         }
                //     }
                // }
                // if (skipPart) continue; // skip not extended antennas
                // AddAntenna(part);
            }
            UpdateAntennas();
        }        
        
        public void ClearAntennas()
        {
            VesselAntennas.Clear();
        }
        
        public void AddAntenna(ModuleDataTransmitter antenna)
        {
            VesselAntennas.Add(antenna);
        }       
        
        public void AddAntenna(Part part)
        {
            foreach (ModuleDataTransmitter antenna in part.FindModulesImplementing<ModuleDataTransmitter>())
            {
                VesselAntennas.Add(antenna);
            }
        }
        public void RemoveAntenna(ModuleDataTransmitter antenna)
        {
            if (VesselAntennas.Contains(antenna))
            {
                VesselAntennas.Remove(antenna);
            }
        }
        public void RemoveAntenna(Part part)
        {
            foreach (ModuleDataTransmitter antenna in part.FindModulesImplementing<ModuleDataTransmitter>())
            {
                if (VesselAntennas.Contains(antenna))
                {
                    VesselAntennas.Remove(antenna);
                }
            }
        }

        public int countantennas(ModuleDataTransmitter antenna)
        {
            if (antenna is null) return 0;
            return VesselAntennas.Count(a => a == antenna);
        }
        
        public void UpdateAntennas()
        {
            // reset everything
            RelayAntennas.Clear();
            VesselCombAntennas.Clear();
            RelayCombAntennas.Clear();
            StrongestVesselAntenna = null;
            StrongestRelayAntenna = null;
            StrongestVesselAntennaNonCombinable = null;   
            StrongestRelayAntennaNonCombinable = null;
            VesselPower = 0;
            RelayPower = 0;
            double SumAntennaPower = 0;
            double SumRelayAntennaPower = 0;
            
            // find the strongest antenna and the sum of all antennas
            foreach (ModuleDataTransmitter antenna in VesselAntennas)
            {
                if (StrongestVesselAntenna is null || antenna.antennaPower > StrongestVesselAntenna.antennaPower)
                {
                    StrongestVesselAntenna = antenna;
                }
                if (antenna.antennaCombinable)
                {
                    VesselCombAntennas.Add(antenna);
                    SumAntennaPower += antenna.antennaPower; // only add combinable antennas to the sum
                }      
                if (!antenna.antennaCombinable && (StrongestVesselAntennaNonCombinable is null || 
                                                   antenna.antennaPower > StrongestVesselAntennaNonCombinable.antennaPower))
                {
                    StrongestVesselAntennaNonCombinable = antenna;
                }              
                
                if (antenna.antennaType == AntennaType.RELAY)
                {
                    RelayAntennas.Add(antenna);
                    if (StrongestRelayAntenna is null || antenna.antennaPower > StrongestRelayAntenna.antennaPower)
                    {
                        StrongestRelayAntenna = antenna;
                    }                      
                    if (antenna.antennaCombinable)
                    {
                        RelayCombAntennas.Add(antenna);
                        SumRelayAntennaPower += antenna.antennaPower; // only add combinable antennas to the sum
                    }
                    if (!antenna.antennaCombinable && (StrongestRelayAntennaNonCombinable is null || 
                        antenna.antennaPower > StrongestRelayAntennaNonCombinable.antennaPower))
                    {
                        StrongestRelayAntennaNonCombinable = antenna;
                    }                      
                }
                // else if (antenna.antennaType == AntennaType.INTERNAL)
                // {
                //     // nothing to do here
                // }
                // else
                // {
                //     Debug.Log(antenna.part.partInfo.title + " has an unknown antenna type: " + antenna.antennaType);
                // }
            }
            
            // with all the data we can calculate the total antenna powers
            if (VesselAntennas.Count == 0)
            {
                VesselPower = 0; // no antennas on Vessel
            }
            else if (VesselAntennas.Count == 1)
            {
                VesselPower = VesselAntennas[0].antennaPower; // only a single antenna on Vessel
            }
            else
            {
                // mutliple antennas on Vessel calculate the total power
                if (VesselCombAntennas.Count > 0)
                {
                    // only calculate if we have combinable antennas
                    VesselPower = AHUtil.CalcVesselPower(StrongestVesselAntenna.antennaPower, SumAntennaPower, AHUtil.GetAWCE(VesselCombAntennas));
                }
                else
                {
                    // fallback to the strongest vessel antenna
                    VesselPower = StrongestVesselAntenna.antennaPower;
                }
                
                if (StrongestVesselAntennaNonCombinable != null && StrongestVesselAntennaNonCombinable.antennaPower > VesselPower)
                {
                    // there exists a stronger non-combinable antenna on Vessel
                    VesselPower = StrongestVesselAntennaNonCombinable.antennaPower;
                }
            }
            // now we need the same for the relay antennas
            if (RelayAntennas.Count == 0)
            {
                RelayPower = 0; // no antennas on Vessel
            }
            else if (RelayAntennas.Count == 1)
            {
                RelayPower = RelayAntennas[0].antennaPower; // only a single antenna on Vessel
            }
            else
            {
                // mutliple antennas on Vessel calculate the total power
                if (RelayCombAntennas.Count > 0)
                {
                    // only calculate if we have combinable antennas
                    RelayPower = AHUtil.CalcVesselPower(StrongestRelayAntenna.antennaPower, SumRelayAntennaPower, AHUtil.GetAWCE(RelayCombAntennas));
                }
                else
                {
                    // fallback to the strongest relay antenna
                    RelayPower = StrongestRelayAntenna.antennaPower;
                }                
                if (StrongestRelayAntennaNonCombinable != null && StrongestRelayAntennaNonCombinable.antennaPower > RelayPower)
                {
                    // there exists a stronger non-combinable antenna on Vessel
                    RelayPower = StrongestRelayAntennaNonCombinable.antennaPower;
                }
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