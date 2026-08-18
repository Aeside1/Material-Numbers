using System;

namespace MaterialNumbers.Core
{
    public readonly struct MaterialCellValue
    {
        public MaterialCellValue(double sortValue, string displayValue, bool hasExplicitValue, string detail = null)
        {
            SortValue = sortValue;
            DisplayValue = displayValue ?? string.Empty;
            HasExplicitValue = hasExplicitValue;
            Detail = detail;
            HasValue = true;
        }

        private MaterialCellValue(bool hasValue)
        {
            SortValue = 0d;
            DisplayValue = string.Empty;
            HasExplicitValue = false;
            Detail = null;
            HasValue = hasValue;
        }

        public static MaterialCellValue Missing => new MaterialCellValue(false);

        public double SortValue { get; }

        public string DisplayValue { get; }

        public bool HasExplicitValue { get; }

        public bool HasValue { get; }

        public string Detail { get; }
    }
}
