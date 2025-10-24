using System;
using System.Collections.Generic;

namespace AntennaHelperNext
{
    public class AHUtil
    {
        public static double TruePower (double power) {
            // return the "true power" of the antenna, stock power * range modifier
            return power * HighLogic.CurrentGame.Parameters.CustomParams<CommNet.CommNetParams> ().rangeModifier;
        }
        
        public static double GetMaxRange (double activeAntPower, double targetAntPower)
        {
            return Math.Sqrt (activeAntPower * targetAntPower);
        }
        public static double GetNormalizedRange (double distance, double maxRange)
        {
            return 1-(distance/maxRange);
        }
        public static double GetSignalStrength (double normalizedRange)
        {
            // return signal Strength in %
            return ((3-2*normalizedRange) * (normalizedRange*normalizedRange))*100;
        }  
        
        public static double GetAWCE (List<ModuleDataTransmitter> antennas)
        {
            // Get the Average Weighted Combinability Exponent for this set of antennas
            // From the wiki : SUM (( Antenna 'n' Power * Antenna 'n' Exponent ) : ( Antenna 'n+1' Power * Antenna 'n+1' Exponent )) / SUM ( Antenna 'n' Power ) : ( Antenna 'n+1' Power )
            // (( 100e9 * 0.75 ) + ( 500e3 * 1.00 )) / ( 100e9 + 500e3 ) = 0.75000125
            // x / y = z

            double x = 0;
            double y = 0;
            double z;

            if (antennas.Count == 1) {
                return antennas[0].antennaCombinableExponent;
            }

            foreach (ModuleDataTransmitter ant in antennas)
            {
                var truePower = TruePower(ant.antennaPower);
                x += truePower * ant.antennaCombinableExponent;
                y += truePower;
            }
            z = x / y;
            return z;
        }
        
      
        
        
    }
}