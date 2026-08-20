using System.Globalization;

namespace MaterialNumbers.Core
{
    internal static class MaterialStatPresentation
    {
        public static string AddMarker(string label, string marker)
        {
            string safeLabel = label ?? string.Empty;
            return string.IsNullOrWhiteSpace(marker)
                ? safeLabel
                : safeLabel + " " + marker.Trim();
        }

        public static string FormatFactor(float value)
        {
            return (value * 100f).ToString("0.##", CultureInfo.InvariantCulture) + "%";
        }
    }
}
