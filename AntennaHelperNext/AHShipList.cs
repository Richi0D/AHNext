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

        public static Dictionary<(string name, Guid vID), AHShipAntennas> EditorShipListVAB =
            new Dictionary<(string name, Guid vID), AHShipAntennas>();

        public static Dictionary<(string name, Guid vID), AHShipAntennas> EditorShipListSPH =
            new Dictionary<(string name, Guid vID), AHShipAntennas>();

        public static Dictionary<Vessel, AHShipAntennas> FlightShipList = new Dictionary<Vessel, AHShipAntennas>();

        public static Dictionary<ProtoVessel, AHShipAntennas> FlightProtoShipList =
            new Dictionary<ProtoVessel, AHShipAntennas>();

        public static Dictionary<string, ModuleDataTransmitter> AntennaPartList =
            new Dictionary<string, ModuleDataTransmitter>();

        // static AHShipList()
        // {
        //     EditorShipListVAB = GetAllSavedShips(VABSavePath);
        //     EditorShipListSPH = GetAllSavedShips(SPHSavePath);
        //     FlightShipList = GetAllFlyingVessels();
        //     FlightProtoShipList = GetAllFlyingProtoVessels();
        // }

        public static Dictionary<(string name, Guid vID), AHShipAntennas> GetAllSavedShips(string folderPath, bool onlyRelayShips = true)
        {
            Dictionary<(string name, Guid vID), AHShipAntennas> shipFiles =
                new Dictionary<(string name, Guid vID), AHShipAntennas>();
            if (!Directory.Exists(folderPath))
            {
                Debug.LogWarning($"[AntennaHelper] Folder not found: {folderPath}");
                return shipFiles;
            }

            try
            {
                // Get all .craft files in folder (non-recursive)
                string[] metaFiles = Directory.GetFiles(folderPath, "*.craft", SearchOption.TopDirectoryOnly);
                foreach (var file in metaFiles)
                {
                    try
                    {
                        ConfigNode craftFile = ConfigNode.Load(file);
                        //string shipname = craftFile.GetValue("ship");
                        string shipname = Path.GetFileNameWithoutExtension(file);
                        Guid shipID = Guid.NewGuid(); // we need a new GUID for each ship

                        AHShipAntennas shipAntennas = GetAntennasFromCraftFile(craftFile);
                        // always do relays
                        if (shipAntennas.RelayPower > 0)
                        {
                            shipFiles.Add((shipname, shipID), shipAntennas);
                        }
                        else
                        {
                            // only do non-relay ships if there are antennas
                            if (!onlyRelayShips && shipAntennas.VesselAntennas.Count > 0)
                            {
                                shipFiles.Add((shipname, shipID), shipAntennas);
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

            return shipFiles;
        }

        private static AHShipAntennas GetAntennasFromCraftFile(ConfigNode craftFile)
        {
            AHShipAntennas shipAntennas = new AHShipAntennas();
            ConfigNode[] partNodes = craftFile.GetNodes("PART");
            foreach (ConfigNode partNode in partNodes)
            {
                string partName = partNode.GetValue("part").Split('_')[0];
                AvailablePart ap = PartLoader.getPartInfoByName(partName);
                Part prefab = ap.partPrefab;
                shipAntennas.AddAntenna(prefab);
            }

            shipAntennas.UpdateAntennas();
            return shipAntennas;
        }

        public static Dictionary<Vessel, AHShipAntennas> GetAllFlyingVessels()
        {
            Dictionary<Vessel, AHShipAntennas> vesselDict = new Dictionary<Vessel, AHShipAntennas>();
            List<Vessel> vesselList = new List<Vessel>();
            vesselList = FlightGlobals.Vessels.FindAll(v => (v.vesselType != VesselType.EVA) &&
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

            return vesselDict;
        }

        public static Dictionary<ProtoVessel, AHShipAntennas> GetAllFlyingProtoVessels()
        {
            Dictionary<ProtoVessel, AHShipAntennas> vesselDict = new Dictionary<ProtoVessel, AHShipAntennas>();
            List<ProtoVessel> vesselList = new List<ProtoVessel>();
            vesselList = HighLogic.CurrentGame.flightState.protoVessels.FindAll(v => (v.vesselType != VesselType.EVA) &&
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
            return vesselDict;
        }

        public static void UpdateShipLists(bool doSavedShips = true, bool editorOnlyRelayShips = true)
        {
            if (doSavedShips)
            {
                EditorShipListVAB.Clear();
                EditorShipListSPH.Clear();
                EditorShipListVAB = GetAllSavedShips(VABSavePath, editorOnlyRelayShips);
                EditorShipListSPH = GetAllSavedShips(SPHSavePath, editorOnlyRelayShips);
            }
            
            FlightShipList.Clear();
            FlightProtoShipList.Clear();
            // FlightShipList = GetAllFlyingVessels(); // this does not get part infos from unloaded vessels, we use the protovessels
            FlightProtoShipList = GetAllFlyingProtoVessels();
        }

        public static void GetAntennaPartList()
        {
            AntennaPartList.Clear();
            foreach (AvailablePart aPart in PartLoader.LoadedPartsList)
            {
                Part prefab = aPart.partPrefab;
                if (prefab != null && !aPart.name.StartsWith("kerbalEVA"))
                {
                    try
                    {
                        foreach (ModuleDataTransmitter antenna in
                                 prefab.FindModulesImplementing<ModuleDataTransmitter>())
                        {
                            if (antenna.antennaType == AntennaType.RELAY)
                            {
                                AntennaPartList.Add(prefab.partInfo.title, antenna);
                            }
                        }
                    }
                    catch
                    {
                        Debug.LogWarning("[AH] Cannot load Antennas for part, skipping: " + aPart.name);
                    }
                }
            }
        }
    }
}