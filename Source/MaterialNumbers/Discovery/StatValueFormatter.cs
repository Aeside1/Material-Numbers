using System;
using RimWorld;
using Verse;

namespace MaterialNumbers.Discovery
{
    internal static class StatValueFormatter
    {
        public static string Format(StatDef stat, float value)
        {
            if (stat == null)
            {
                return value.ToString("0.##");
            }

            try
            {
                return stat.ValueToString(value, ToStringNumberSense.Absolute, false);
            }
            catch (Exception)
            {
                return value.ToString("0.##");
            }
        }

        public static string FormatPlain(float value)
        {
            return value.ToString("0.##");
        }
    }
}
