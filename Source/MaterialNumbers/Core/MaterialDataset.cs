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
                rows.Add(new MaterialRow(material, catalog.Columns));
            }

            return new MaterialDataset(new ReadOnlyCollection<MaterialRow>(rows));
        }
    }
}
