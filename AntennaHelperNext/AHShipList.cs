using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AntennaHelperNext
{
    public class AHShipList
    {
        public static string VABSavePath = ShipConstruction.GetCurrentGameShipsPathFor(EditorFacility.VAB);
        public static string SPHSavePath = ShipConstruction.GetCurrentGameShipsPathFor(EditorFacility.SPH);
        public static List<string> EditorShipListVAB;
        public static List<string> EditorShipListSPH;
        public static List<Vessel> FlightShipList;

        static AHShipList()
        {
            EditorShipListVAB = FindShipMetaFiles(VABSavePath);
            EditorShipListSPH = FindShipMetaFiles(SPHSavePath);
            FlightShipList = GetAllFlyingVessels();
        }

        public static List<string> FindShipMetaFiles (string folderPath)
        {
            var ShipFiles = new List<string>();
            
            if (!Directory.Exists(folderPath))
            {
                Debug.LogWarning($"[AntennaHelper] Folder not found: {folderPath}");
                return ShipFiles;
            }

            try
            {
                // Get all .loadmeta files in folder (non-recursive)
                string[] metaFiles = Directory.GetFiles(folderPath, "*.loadmeta", SearchOption.TopDirectoryOnly);
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
        
        public static List<Vessel> GetAllFlyingVessels()
        {
            return FlightGlobals.Vessels.FindAll(
                v => (v.vesselType != VesselType.EVA) &&
                     (v.vesselType != VesselType.Flag) &&
                     (v.vesselType != VesselType.SpaceObject) &&
                     (v.vesselType != VesselType.Unknown) &&
                     (v.vesselType != VesselType.Debris));
        }
    }
}