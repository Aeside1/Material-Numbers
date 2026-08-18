using System.Collections.Generic;
using Verse;

namespace MaterialNumbers.Core
{
    public sealed class MaterialRow
    {
        private readonly IReadOnlyDictionary<string, MaterialCellValue> cells;

        public MaterialRow(ThingDef material, IReadOnlyDictionary<string, MaterialCellValue> cells)
        {
            Material = material;
            this.cells = cells;
        }

        public ThingDef Material { get; }

        public MaterialCellValue GetCell(string columnId)
        {
            return cells.TryGetValue(columnId, out MaterialCellValue value)
                ? value
                : MaterialCellValue.Missing;
        }
    }
}
