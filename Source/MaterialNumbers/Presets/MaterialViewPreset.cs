using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MaterialNumbers.Presets
{
    public sealed class MaterialViewPreset : IExposable
    {
        public string Id;

        public string Name;

        public bool IsBuiltIn;

        public List<PresetColumnState> Columns = new List<PresetColumnState>();

        public string SortColumnId;

        public bool SortAscending;

        public void ExposeData()
        {
            Scribe_Values.Look(ref Id, "id");
            Scribe_Values.Look(ref Name, "name");
            Scribe_Values.Look(ref IsBuiltIn, "isBuiltIn", false);
            Scribe_Collections.Look(ref Columns, "columns", LookMode.Deep);
            Scribe_Values.Look(ref SortColumnId, "sortColumnId");
            Scribe_Values.Look(ref SortAscending, "sortAscending", false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Columns = Columns ?? new List<PresetColumnState>();
                Columns = Columns
                    .Where(column => column != null && !string.IsNullOrWhiteSpace(column.ColumnId))
                    .GroupBy(column => column.ColumnId, StringComparer.Ordinal)
                    .Select(group => group.First())
                    .ToList();
                if (string.IsNullOrWhiteSpace(Id))
                {
                    Id = Guid.NewGuid().ToString("N");
                }
            }
        }

        public MaterialViewPreset Clone(string newId = null, string newName = null, bool? builtIn = null)
        {
            return new MaterialViewPreset
            {
                Id = newId ?? Id,
                Name = newName ?? Name,
                IsBuiltIn = builtIn ?? IsBuiltIn,
                Columns = Columns.Select(column => column.Clone()).ToList(),
                SortColumnId = SortColumnId,
                SortAscending = SortAscending
            };
        }
    }
}
