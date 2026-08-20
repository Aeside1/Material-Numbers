using System.Collections.Generic;
using System.Linq;
using MaterialNumbers.Core;
using RimWorld;
using Verse;

namespace MaterialNumbers.Discovery
{
    internal sealed class StandardStatColumnProvider : IMaterialColumnProvider
    {
        public string ProviderId => "materialnumbers.standard-stats";

        public IEnumerable<MaterialColumnDefinition> CreateColumns(MaterialDiscoveryContext context)
        {
            foreach (StatDef stat in CollectStats(context.Materials, material => material.statBases))
            {
                StatDef capturedStat = stat;
                yield return CreateColumn(
                    MaterialColumnIds.StatBase(stat.defName),
                    stat,
                    "MaterialNumbers.Group.BaseStats".Translate(),
                    "MaterialNumbers.ColumnMarker.Base".Translate(),
                    material => ReadBase(material, capturedStat));
            }

            foreach (StatDef stat in CollectStats(context.Materials, material => material.stuffProps?.statFactors))
            {
                StatDef capturedStat = stat;
                yield return CreateColumn(
                    MaterialColumnIds.StuffFactor(stat.defName),
                    stat,
                    "MaterialNumbers.Group.StuffFactors".Translate(),
                    "MaterialNumbers.ColumnMarker.Factor".Translate(),
                    material => ReadFactor(material, capturedStat));
            }

            foreach (StatDef stat in CollectStats(context.Materials, material => material.stuffProps?.statOffsets))
            {
                StatDef capturedStat = stat;
                yield return CreateColumn(
                    MaterialColumnIds.StuffOffset(stat.defName),
                    stat,
                    "MaterialNumbers.Group.StuffOffsets".Translate(),
                    "MaterialNumbers.ColumnMarker.Offset".Translate(),
                    material => ReadOffset(material, capturedStat));
            }
        }

        private static IEnumerable<StatDef> CollectStats(
            IReadOnlyList<ThingDef> materials,
            System.Func<ThingDef, IEnumerable<StatModifier>> selector)
        {
            return materials
                .SelectMany(material => selector(material) ?? Enumerable.Empty<StatModifier>())
                .Where(modifier => modifier?.stat != null)
                .Select(modifier => modifier.stat)
                .GroupBy(stat => stat.defName)
                .Select(group => group.First())
                .OrderBy(stat => stat.LabelCap.ToString());
        }

        private static MaterialColumnDefinition CreateColumn(
            string id,
            StatDef stat,
            string group,
            string marker,
            System.Func<ThingDef, MaterialCellValue> reader)
        {
            return new MaterialColumnDefinition(
                id,
                MaterialStatPresentation.AddMarker(stat.LabelCap.ToString(), marker),
                stat.description,
                group,
                stat.modContentPack?.Name ?? "RimWorld",
                105f,
                reader);
        }

        private static MaterialCellValue ReadBase(ThingDef material, StatDef stat)
        {
            return TryGet(material.statBases, stat, out float value)
                ? Explicit(stat, value)
                : MaterialCellValue.Missing;
        }

        private static MaterialCellValue ReadFactor(ThingDef material, StatDef stat)
        {
            return TryGet(material.stuffProps?.statFactors, stat, out float value)
                ? ExplicitFactor(value)
                : NeutralFactor(1f);
        }

        private static MaterialCellValue ReadOffset(ThingDef material, StatDef stat)
        {
            return TryGet(material.stuffProps?.statOffsets, stat, out float value)
                ? Explicit(stat, value)
                : Neutral(stat, 0f);
        }

        private static bool TryGet(IEnumerable<StatModifier> modifiers, StatDef stat, out float value)
        {
            if (modifiers != null)
            {
                foreach (StatModifier modifier in modifiers)
                {
                    if (modifier?.stat == stat)
                    {
                        value = modifier.value;
                        return true;
                    }
                }
            }

            value = 0f;
            return false;
        }

        private static MaterialCellValue Explicit(StatDef stat, float value)
        {
            return new MaterialCellValue(value, StatValueFormatter.Format(stat, value), true);
        }

        private static MaterialCellValue Neutral(StatDef stat, float value)
        {
            return new MaterialCellValue(
                value,
                StatValueFormatter.Format(stat, value),
                false,
                "MaterialNumbers.Value.Neutral".Translate());
        }

        private static MaterialCellValue ExplicitFactor(float value)
        {
            return new MaterialCellValue(value, MaterialStatPresentation.FormatFactor(value), true);
        }

        private static MaterialCellValue NeutralFactor(float value)
        {
            return new MaterialCellValue(
                value,
                MaterialStatPresentation.FormatFactor(value),
                false,
                "MaterialNumbers.Value.Neutral".Translate());
        }
    }
}
