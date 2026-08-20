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
        public const string ComfortId = "builtin-comfort";
        public const string EconomyId = "builtin-economy";

        public static IReadOnlyList<MaterialViewPreset> CreateAll()
        {
            return new[]
            {
                Create(
                    OverviewId,
                    "MaterialNumbers.Preset.Overview".Translate(),
                    MaterialColumnIds.Amount,
                    Base("MarketValue"),
                    Factor("MarketValue"),
                    Base("Mass"),
                    Factor("Mass"),
                    Factor("MaxHitPoints"),
                    Factor("Beauty"),
                    Offset("Beauty"),
                    Factor("Flammability"),
                    MaterialColumnIds.StackLimit),
                CreateSorted(
                    ConstructionId,
                    "MaterialNumbers.Preset.Construction".Translate(),
                    Factor("WorkToBuild"),
                    true,
                    MaterialColumnIds.Amount,
                    Factor("MaxHitPoints"),
                    Factor("WorkToBuild"),
                    Factor("WorkToMake"),
                    Factor("Beauty"),
                    Offset("Beauty"),
                    Factor("Flammability"),
                    Factor("DoorOpenSpeed"),
                    Factor("Mass")),
                CreateSorted(
                    ApparelId,
                    "MaterialNumbers.Preset.Apparel".Translate(),
                    Factor("ArmorRating_Sharp"),
                    MaterialColumnIds.Amount,
                    Base("Mass"),
                    Factor("MaxHitPoints"),
                    Factor("ArmorRating_Sharp"),
                    Factor("ArmorRating_Blunt"),
                    Factor("ArmorRating_Heat"),
                    Factor("Insulation_Cold"),
                    Factor("Insulation_Heat"),
                    Factor("WornBulk"),
                    Factor("MoveSpeed"),
                    Factor("Beauty")),
                CreateSorted(
                    ToolsId,
                    "MaterialNumbers.Preset.Tools".Translate(),
                    Tool("MiningSpeed"),
                    MaterialColumnIds.Amount,
                    Tool("MiningSpeed"),
                    Tool("MiningYieldDigging"),
                    Tool("TreeFellingSpeed"),
                    Tool("PlantHarvestingSpeed"),
                    Tool("PlantWorkSpeed"),
                    Tool("ConstructionSpeed"),
                    Tool("SmithingSpeed"),
                    Tool("CookSpeed"),
                    Tool("ButcheryFleshSpeed")),
                CreateSorted(
                    ComfortId,
                    "MaterialNumbers.Preset.Comfort".Translate(),
                    Factor("BedRestEffectiveness"),
                    MaterialColumnIds.Amount,
                    Factor("BedRestEffectiveness"),
                    Factor("Comfort"),
                    Factor("Cleanliness"),
                    Factor("Beauty"),
                    Offset("Beauty"),
                    Factor("MaxHitPoints"),
                    Factor("Mass"),
                    Factor("Flammability")),
                CreateSorted(
                    EconomyId,
                    "MaterialNumbers.Preset.Economy".Translate(),
                    Factor("MarketValue"),
                    MaterialColumnIds.Amount,
                    Base("MarketValue"),
                    Factor("MarketValue"),
                    Base("Mass"),
                    Factor("Mass"),
                    MaterialColumnIds.StackLimit,
                    Factor("MaxHitPoints"),
                    Factor("WorkToMake"),
                    Factor("Beauty"),
                    Offset("Beauty"))
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

        private static MaterialViewPreset CreateSorted(
            string id,
            string name,
            string sortColumnId,
            params string[] columns)
        {
            return CreateSorted(id, name, sortColumnId, false, columns);
        }

        private static MaterialViewPreset CreateSorted(
            string id,
            string name,
            string sortColumnId,
            bool sortAscending,
            params string[] columns)
        {
            MaterialViewPreset preset = Create(id, name, columns);
            preset.SortColumnId = sortColumnId;
            preset.SortAscending = sortAscending;
            return preset;
        }

        private static string Base(string statDefName)
        {
            return MaterialColumnIds.StatBase(statDefName);
        }

        private static string Factor(string statDefName)
        {
            return MaterialColumnIds.StuffFactor(statDefName);
        }

        private static string Offset(string statDefName)
        {
            return MaterialColumnIds.StuffOffset(statDefName);
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
