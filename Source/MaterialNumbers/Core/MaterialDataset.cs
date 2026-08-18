using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Verse;

namespace MaterialNumbers.Core
{
    public sealed class MaterialDataset
    {
        private readonly Dictionary<ThingDef, MaterialRow> byMaterial;

        private MaterialDataset(IReadOnlyList<MaterialRow> rows)
        {
            Rows = rows;
            byMaterial = new Dictionary<ThingDef, MaterialRow>();
            foreach (MaterialRow row in rows)
            {
                byMaterial[row.Material] = row;
            }
        }

        public IReadOnlyList<MaterialRow> Rows { get; }

        public MaterialRow GetRow(ThingDef material)
        {
            return byMaterial.TryGetValue(material, out MaterialRow row) ? row : null;
        }

        public static MaterialDataset Build(MaterialColumnCatalog catalog)
        {
            var rows = new List<MaterialRow>(catalog.Materials.Count);
            foreach (ThingDef material in catalog.Materials)
            {
                var cells = new Dictionary<string, MaterialCellValue>(StringComparer.Ordinal);
                foreach (MaterialColumnDefinition column in catalog.Columns)
                {
                    if (column.Id == MaterialColumnIds.Amount)
                    {
                        continue;
                    }

                    try
                    {
                        cells[column.Id] = column.Read(material);
                    }
                    catch (Exception exception)
                    {
                        Log.Warning("[Material Numbers] Failed to read " + column.Id + " for " + material.defName + ": " + exception.Message);
                        cells[column.Id] = MaterialCellValue.Missing;
                    }
                }

                rows.Add(new MaterialRow(material, new ReadOnlyDictionary<string, MaterialCellValue>(cells)));
            }

            return new MaterialDataset(new ReadOnlyCollection<MaterialRow>(rows));
        }
    }
}
