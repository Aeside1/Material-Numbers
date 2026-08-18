using System.Collections.Generic;
using MaterialNumbers.Core;
using Verse;

namespace MaterialNumbers.Discovery
{
    internal sealed class BuiltinColumnProvider : IMaterialColumnProvider
    {
        public string ProviderId => "materialnumbers.builtin";

        public IEnumerable<MaterialColumnDefinition> CreateColumns(MaterialDiscoveryContext context)
        {
            yield return new MaterialColumnDefinition(
                MaterialColumnIds.Amount,
                "MaterialNumbers.Column.Amount".Translate(),
                "MaterialNumbers.Column.Amount.Description".Translate(),
                "MaterialNumbers.Group.Basic".Translate(),
                "Material Numbers",
                82f,
                material => MaterialCellValue.Missing);

            yield return new MaterialColumnDefinition(
                MaterialColumnIds.StackLimit,
                "MaterialNumbers.Column.StackLimit".Translate(),
                "MaterialNumbers.Column.StackLimit.Description".Translate(),
                "MaterialNumbers.Group.Basic".Translate(),
                "RimWorld",
                82f,
                material => new MaterialCellValue(material.stackLimit, material.stackLimit.ToString(), true));

            yield return new MaterialColumnDefinition(
                MaterialColumnIds.SourceMod,
                "MaterialNumbers.Column.SourceMod".Translate(),
                "MaterialNumbers.Column.SourceMod.Description".Translate(),
                "MaterialNumbers.Group.Basic".Translate(),
                "RimWorld",
                150f,
                material => new MaterialCellValue(
                    0d,
                    material.modContentPack?.Name ?? "RimWorld",
                    true));
        }
    }
}
