using System.Collections.Generic;
using MaterialNumbers.Core;
using Verse;

namespace MaterialNumbers.Presets
{
    internal static class BuiltinPresetFactory
    {
        public const string OverviewId = "builtin-overview";
        public const string ConstructionId = "builtin-construction";
        public const string ApparelId = "builtin-apparel";
        public const string ToolsId = "builtin-tools";

        public static IReadOnlyList<MaterialViewPreset> CreateAll()
        {
            return new[]
            {
                Create(
                    OverviewId,
                    "MaterialNumbers.Preset.Overview".Translate(),
                    MaterialColumnIds.Amount,
                    MaterialColumnIds.StatBase("MarketValue"),
                    MaterialColumnIds.StatBase("Mass"),
                    MaterialColumnIds.StuffFactor("MaxHitPoints"),
                    MaterialColumnIds.StuffOffset("Beauty"),
                    MaterialColumnIds.StuffFactor("Flammability"),
                    MaterialColumnIds.StackLimit),
                Create(
                    ConstructionId,
                    "MaterialNumbers.Preset.Construction".Translate(),
                    MaterialColumnIds.Amount,
                    MaterialColumnIds.StuffFactor("MaxHitPoints"),
                    MaterialColumnIds.StuffFactor("WorkToBuild"),
                    MaterialColumnIds.StuffOffset("Beauty"),
                    MaterialColumnIds.StuffFactor("Flammability"),
                    MaterialColumnIds.StuffFactor("DoorOpenSpeed")),
                Create(
                    ApparelId,
                    "MaterialNumbers.Preset.Apparel".Translate(),
                    MaterialColumnIds.StatBase("Mass"),
                    MaterialColumnIds.StuffFactor("ArmorRating_Sharp"),
                    MaterialColumnIds.StuffFactor("ArmorRating_Blunt"),
                    MaterialColumnIds.StuffFactor("ArmorRating_Heat"),
                    MaterialColumnIds.StuffFactor("Insulation_Cold"),
                    MaterialColumnIds.StuffFactor("Insulation_Heat")),
                Create(
                    ToolsId,
                    "MaterialNumbers.Preset.Tools".Translate(),
                    MaterialColumnIds.Amount,
                    Tool("MiningSpeed"),
                    Tool("MiningYieldDigging"),
                    Tool("TreeFellingSpeed"),
                    Tool("PlantHarvestingSpeed"),
                    Tool("PlantWorkSpeed"),
                    Tool("ConstructionSpeed"))
            };
        }

        private static MaterialViewPreset Create(string id, string name, params string[] columns)
        {
            var states = new List<PresetColumnState>();
            foreach (string column in columns)
            {
                states.Add(new PresetColumnState(column, 105f));
            }

            return new MaterialViewPreset
            {
                Id = id,
                Name = name,
                IsBuiltIn = true,
                Columns = states,
                SortColumnId = null,
                SortAscending = false
            };
        }

        private static string Tool(string statDefName)
        {
            return MaterialColumnIds.Extension(
                "SurvivalToolsLite.StuffPropsTool",
                "toolStatFactors",
                statDefName);
        }
    }
}
