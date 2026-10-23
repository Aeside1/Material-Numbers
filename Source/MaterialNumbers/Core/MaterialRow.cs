using System;
using System.Collections.Generic;
using Verse;

namespace MaterialNumbers.Core
{
    public sealed class MaterialRow
    {
        private readonly Dictionary<string, MaterialCellValue> cells;
        private readonly IReadOnlyDictionary<string, MaterialColumnDefinition> columns;

        public MaterialRow(ThingDef material, IReadOnlyDictionary<string, MaterialCellValue> cells)
        {
            Material = material;
            this.cells = cells == null
                ? new Dictionary<string, MaterialCellValue>(StringComparer.Ordinal)
                : new Dictionary<string, MaterialCellValue>(cells, StringComparer.Ordinal);
        }

        internal MaterialRow(ThingDef material, IReadOnlyList<MaterialColumnDefinition> columns)
        {
            Material = material;
            this.cells = new Dictionary<string, MaterialCellValue>(StringComparer.Ordinal);
            var columnMap = new Dictionary<string, MaterialColumnDefinition>(StringComparer.Ordinal);
            foreach (MaterialColumnDefinition column in columns)
            {
                if (column != null)
                {
                    columnMap[column.Id] = column;
                }
            }

            this.columns = columnMap;
        }

        public ThingDef Material { get; }

        public MaterialCellValue GetCell(string columnId)
        {
            if (cells.TryGetValue(columnId, out MaterialCellValue cached))
            {
                return cached;
            }

            if (columns == null || !columns.TryGetValue(columnId, out MaterialColumnDefinition column))
            {
                return MaterialCellValue.Missing;
            }

            MaterialCellValue value;
            try
            {
                value = column.Read(Material);
            }
            catch (System.Exception exception)
            {
                Log.Warning("[Material Numbers] Failed to read " + columnId + " for " + Material.defName + ": " + exception.Message);
                value = MaterialCellValue.Missing;
            }

            cells[columnId] = value;
            return value;
        }
    }
}
