using System;
using System.Collections.Generic;

namespace AntennaHelperNext
{
    public enum AHTargetType
    {
        DSN,
        FLIGHT,
        EDITORVAB,
        EDITORSPH,
        PART
    }
    
    public enum AHDisplayType
    {
        ACTIVE,
        DSN,
        RELAY,
        DSNRELAY,
    }    
    
    // extensions for ModuleDataTransmitter. We need to apply modifier to every call of antennaPower
    public static class AntennaExtensions
    {
        public static double GetTruePower(this ModuleDataTransmitter antenna)
        {
            return AHUtil.TruePower(antenna.antennaPower);
        }
    }
    
    public static class AHUtil
    {
        public static double TruePower (double power) {
            // return the "true power" of the antenna, stock power * range modifier
            return power * HighLogic.CurrentGame.Parameters.CustomParams<CommNet.CommNetParams>().rangeModifier;
        }
        
        public static double GetMaxRange (double activeAntPower, double targetAntPower)
        {
            return Math.Sqrt (activeAntPower * targetAntPower);
        }
        public static double GetNormalizedRange (double distance, double maxRange)
        {
            if (distance > maxRange)
            {
                return 0;
            }
            return 1-(distance/maxRange);
        }
        public static double GetSignalStrength (double normalizedRange)
        {
            // return signal Strength
            return ((3-2*normalizedRange) * (normalizedRange*normalizedRange));
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
                var truePower = ant.GetTruePower();
                x += truePower * ant.antennaCombinableExponent;
                y += truePower;
            }
            z = x / y;
            return z;
        }
        
        public static double CalcVesselPower(double strongestPower, double sumPower, double avgCombExp)
        {
            return strongestPower * Math.Pow(sumPower/strongestPower, avgCombExp);
        }

        
        public static readonly Dictionary<double, double> SignalMultipliers = new Dictionary<double, double>
        {
            {0, 0.958599849283928 }, // is 0.5%
            {25, 0.6736481776670189 },
            {50, 0.5 },
            {75, 0.3263518223329811 },
            {100, 0.04140015071607195 } // is 99.5%
        };
        public static Dictionary<double, double> GetDistancesBySignalFixed(double maxRange)
        {
            var distanceBySignal = new Dictionary<double, double>();
            foreach (var kvp in SignalMultipliers)
            {
                double interval = kvp.Key;
                double multiplier = kvp.Value;
                if (Math.Abs(interval - 100) < 1e-6)
                {
                    distanceBySignal[interval] = maxRange;
                }
                else
                {
                    distanceBySignal[interval]  = maxRange * multiplier;
                }
            }
            return distanceBySignal;
        }
        
        /// <summary>
        /// Solve (1 + 2d/m) * (1 - d/m)^2 = y for d, where 0 <= d <= m.
        /// Returns distance for given signal strength y and max range m.
        /// </summary>
        public static double GetDistanceBySignal(double m, double y, double tol = 1e-6, int maxIter = 100)
        {
            if (y < 0.0 || y > 1.0)
                throw new ArgumentOutOfRangeException(nameof(y), "y must be between 0 and 1");

            // Trivial cases
            if (Math.Abs(y - 1.0) < tol)
                return 0.0;
            if (Math.Abs(y) < tol)
                return m;

            // f(x) for x = d/m
            double F(double x) => 2 * Math.Pow(x, 3) - 3 * Math.Pow(x, 2) + (1 - y);

            double a = 0.0;
            double b = 1.0;
            double fa = F(a);
            double fb = F(b); // Should be 0

            if (Math.Abs(fa) < tol)
                return 0.0;

            // Bisection loop
            for (int i = 0; i < maxIter; i++)
            {
                double c = 0.5 * (a + b);
                double fc = F(c);

                if (Math.Abs(fc) < tol || (b - a) / 2 < tol)
                    return m * c;

                if (fa * fc <= 0)
                {
                    b = c;
                    fb = fc;
                }
                else
                {
                    a = c;
                    fa = fc;
                }
            }

            // Fallback return if not converged
            return m * 0.5 * (a + b);
        }       
        
        // simplify Antenna Values and ranges
        public static string ToKMG(double value, bool useMetricSuffix = false, int decimalPlaces = 0)
        {
            string[] suffixes = useMetricSuffix ? new string[] { "km", "Mm", "Gm" } : new string[] { "k", "M", "G" };

            double absValue = Math.Abs(value);

            if (absValue >= 1_000_000_000f)
                return (value / 1_000_000_000f).ToString($"F{decimalPlaces}") + suffixes[2]; // G / Gm
            else if (absValue >= 1_000_000f)
                return (value / 1_000_000f).ToString($"F{decimalPlaces}") + suffixes[1];     // M / Mm
            else if (absValue >= 1_000f)
                return (value / 1_000f).ToString($"F{decimalPlaces}") + suffixes[0];         // k / km
            else
                return value.ToString($"F{decimalPlaces}");                                   // no suffix
        }        
    }
}