using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AntennaHelperNext
{
    public static class AHShipList
    {
        public static string VABSavePath = ShipConstruction.GetCurrentGameShipsPathFor(EditorFacility.VAB);
        public static string SPHSavePath = ShipConstruction.GetCurrentGameShipsPathFor(EditorFacility.SPH);
        public static List<string> EditorShipListVAB;
        public static List<string> EditorShipListSPH;
        public static Dictionary<Vessel, AHShipAntennas> FlightShipList;
        public static Dictionary<ProtoVessel, AHShipAntennas> FlightProtoShipList;

        static AHShipList()
        {
            EditorShipListVAB = GetAllSavedShips(VABSavePath);
            EditorShipListSPH = GetAllSavedShips(SPHSavePath);
            FlightShipList = GetAllFlyingVessels();
            FlightProtoShipList = GetAllFlyingProtoVessels();
        }

        public static List<string> GetAllSavedShips (string folderPath)
        {
            var ShipFiles = new List<string>();
            
            if (!Directory.Exists(folderPath))
            {
                Debug.LogWarning($"[AntennaHelper] Folder not found: {folderPath}");
                return ShipFiles;
            }

            try
            {
                // Get all .craft files in folder (non-recursive)
                string[] metaFiles = Directory.GetFiles(folderPath, "*.craft", SearchOption.TopDirectoryOnly);
                foreach (var file in metaFiles)
                {
                    try
                    {
                        // Read text and look for line
                        string[] lines = File.ReadAllLines(file);
                        foreach (string line in lines)
                        {
                            if (line.Contains("partModules = ModuleDataTransmitter"))
                            {
                                string fileName = Path.GetFileNameWithoutExtension(file);
                                ShipFiles.Add(fileName);
                                break; // no need to check further lines
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[AntennaHelper] Error reading {file}: {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[AntennaHelper] Error scanning folder {folderPath}: {e.Message}");
            }
            return ShipFiles;
        }

        private static AHShipAntennas GetAntennasFromCraftFile(ConfigNode craftFile)
        {
            
        }
        
        public static Dictionary<Vessel, AHShipAntennas> GetAllFlyingVessels()
        {
            Dictionary<Vessel, AHShipAntennas> vesselDict = new Dictionary<Vessel, AHShipAntennas>();
            List<Vessel> vesselList = new List<Vessel>();
            vesselList = FlightGlobals.Vessels.FindAll(
                v => (v.vesselType != VesselType.EVA) &&
                     (v.vesselType != VesselType.Flag) &&
                     (v.vesselType != VesselType.SpaceObject) &&
                     (v.vesselType != VesselType.Unknown) &&
                     (v.vesselType != VesselType.Debris));
            
            // fetch antennas for each vessel
            foreach (Vessel vessel in vesselList)
            {
                AHShipAntennas shipAntennas = new AHShipAntennas();
                shipAntennas.FetchAntennas(vessel.parts);
                if (shipAntennas.RelayPower > 0)
                {
                    vesselDict.Add(vessel, shipAntennas);
                }
            }
            FlightShipList = vesselDict;
            return vesselDict;
        }
        
        public static Dictionary<ProtoVessel, AHShipAntennas> GetAllFlyingProtoVessels()
        {
            Dictionary<ProtoVessel, AHShipAntennas> vesselDict = new Dictionary<ProtoVessel, AHShipAntennas>();
            List<ProtoVessel> vesselList = new List<ProtoVessel>();
            vesselList = HighLogic.CurrentGame.flightState.protoVessels.FindAll(
                v => (v.vesselType != VesselType.EVA) &&
                     (v.vesselType != VesselType.Flag) &&
                     (v.vesselType != VesselType.SpaceObject) &&
                     (v.vesselType != VesselType.Unknown) &&
                     (v.vesselType != VesselType.Debris));
            
            // fetch antennas for each vessel
            foreach (ProtoVessel vessel in vesselList)
            {
                AHShipAntennas shipAntennas = new AHShipAntennas();
                shipAntennas.FetchAntennas(vessel.protoPartSnapshots);
                if (shipAntennas.RelayPower > 0)
                {
                    vesselDict.Add(vessel, shipAntennas);
                }
            }
            FlightProtoShipList = vesselDict;
            return vesselDict;
        }        
    }
}