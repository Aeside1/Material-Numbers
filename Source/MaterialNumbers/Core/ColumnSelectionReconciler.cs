using System;
using System.Collections.Generic;

namespace MaterialNumbers.Core
{
    internal static class ColumnSelectionReconciler
    {
        public static IReadOnlyList<string> Reconcile(
            IEnumerable<string> existingIds,
            ISet<string> knownIds,
            ISet<string> selectedKnownIds,
            IEnumerable<string> catalogOrder)
        {
            var result = new List<string>();
            var added = new HashSet<string>(StringComparer.Ordinal);
            foreach (string id in existingIds)
            {
                bool isKnown = knownIds.Contains(id);
                if ((!isKnown || selectedKnownIds.Contains(id)) && added.Add(id))
                {
                    result.Add(id);
                }
            }

            foreach (string id in catalogOrder)
            {
                if (selectedKnownIds.Contains(id) && added.Add(id))
                {
                    result.Add(id);
                }
            }

            return result;
        }
    }
}
