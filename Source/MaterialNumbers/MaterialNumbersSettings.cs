using System;
using System.Collections.Generic;
using System.Linq;
using MaterialNumbers.Core;
using MaterialNumbers.Presets;
using Verse;

namespace MaterialNumbers
{
    public enum MaterialAvailabilityMode
    {
        AllLoaded,
        CurrentMap,
        CurrentStorage
    }

    public enum MaterialGroupFilter
    {
        Common,
        All,
        Metal,
        Stone,
        Wood,
        TextileLeather,
        PlasticGlass,
        Other
    }

    public sealed class MaterialNumbersSettings : ModSettings
    {
        public string CurrentPresetId = BuiltinPresetFactory.OverviewId;

        public string DefaultPresetId = BuiltinPresetFactory.OverviewId;

        public List<MaterialViewPreset> UserPresets = new List<MaterialViewPreset>();

        public MaterialAvailabilityMode AvailabilityMode = MaterialAvailabilityMode.AllLoaded;

        public MaterialGroupFilter GroupFilter = MaterialGroupFilter.Common;

        private List<string> legacySelectedCategoryDefNames;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref CurrentPresetId, "currentPresetId", BuiltinPresetFactory.OverviewId);
            Scribe_Values.Look(ref DefaultPresetId, "defaultPresetId", BuiltinPresetFactory.OverviewId);
            Scribe_Collections.Look(ref UserPresets, "userPresets", LookMode.Deep);
            Scribe_Values.Look(ref AvailabilityMode, "availabilityMode", MaterialAvailabilityMode.AllLoaded);
            Scribe_Values.Look(ref GroupFilter, "groupFilter", MaterialGroupFilter.Common);
            Scribe_Collections.Look(ref legacySelectedCategoryDefNames, "selectedCategoryDefNames", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Normalize();
            }
        }

        public void Normalize()
        {
            MigrateLegacyCategoryFilter();
            UserPresets = UserPresets ?? new List<MaterialViewPreset>();
            UserPresets = UserPresets
                .Where(preset => preset != null)
                .GroupBy(preset => preset.Id, StringComparer.Ordinal)
                .Select(group => group.First())
                .ToList();
            if (!Enum.IsDefined(typeof(MaterialGroupFilter), GroupFilter))
            {
                GroupFilter = MaterialGroupFilter.Common;
            }
        }

        private void MigrateLegacyCategoryFilter()
        {
            if (legacySelectedCategoryDefNames == null)
            {
                return;
            }

            if (legacySelectedCategoryDefNames.Count == 0)
            {
                GroupFilter = MaterialGroupFilter.Common;
                legacySelectedCategoryDefNames = null;
                return;
            }

            var groups = new HashSet<MaterialGroup>();
            foreach (string categoryDefName in legacySelectedCategoryDefNames)
            {
                groups.Add(MaterialGroupClassifier.Classify(new[] { categoryDefName }));
            }

            GroupFilter = groups.Count == 1
                ? ToFilter(groups.First())
                : MaterialGroupFilter.All;
            legacySelectedCategoryDefNames = null;
        }

        private static MaterialGroupFilter ToFilter(MaterialGroup group)
        {
            switch (group)
            {
                case MaterialGroup.Metal:
                    return MaterialGroupFilter.Metal;
                case MaterialGroup.Stone:
                    return MaterialGroupFilter.Stone;
                case MaterialGroup.Wood:
                    return MaterialGroupFilter.Wood;
                case MaterialGroup.TextileLeather:
                    return MaterialGroupFilter.TextileLeather;
                case MaterialGroup.PlasticGlass:
                    return MaterialGroupFilter.PlasticGlass;
                default:
                    return MaterialGroupFilter.Other;
            }
        }
    }
}
