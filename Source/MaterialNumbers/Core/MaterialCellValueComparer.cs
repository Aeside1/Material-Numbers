using System;

namespace MaterialNumbers.Core
{
    internal static class MaterialCellValueComparer
    {
        public static int Compare(MaterialCellValue left, MaterialCellValue right, bool ascending)
        {
            if (left.HasValue != right.HasValue)
            {
                return left.HasValue ? -1 : 1;
            }

            if (!left.HasValue)
            {
                return 0;
            }

            int comparison = left.SortValue.CompareTo(right.SortValue);
            if (comparison == 0)
            {
                comparison = string.Compare(left.DisplayValue, right.DisplayValue, StringComparison.CurrentCultureIgnoreCase);
            }

            return ascending ? comparison : -comparison;
        }
    }
}
