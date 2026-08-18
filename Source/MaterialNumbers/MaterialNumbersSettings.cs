using System;
using System.Collections.Generic;
using System.Linq;
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

    public sealed class MaterialNumbersSettings : ModSettings
    {
        public string CurrentPresetId = BuiltinPresetFactory.OverviewId;

        public string DefaultPresetId = BuiltinPresetFactory.OverviewId;

        public List<MaterialViewPreset> UserPresets = new List<MaterialViewPreset>();

        public MaterialAvailabilityMode AvailabilityMode = MaterialAvailabilityMode.AllLoaded;

        public List<string> SelectedCategoryDefNames = new List<string>();

        public override void ExposeData()
        {
            Scribe_Values.Look(ref CurrentPresetId, "currentPresetId", BuiltinPresetFactory.OverviewId);
            Scribe_Values.Look(ref DefaultPresetId, "defaultPresetId", BuiltinPresetFactory.OverviewId);
            Scribe_Collections.Look(ref UserPresets, "userPresets", LookMode.Deep);
            Scribe_Values.Look(ref AvailabilityMode, "availabilityMode", MaterialAvailabilityMode.AllLoaded);
            Scribe_Collections.Look(ref SelectedCategoryDefNames, "selectedCategoryDefNames", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Normalize();
            }
        }

        public void Normalize()
        {
            UserPresets = UserPresets ?? new List<MaterialViewPreset>();
            UserPresets = UserPresets
                .Where(preset => preset != null)
                .GroupBy(preset => preset.Id, StringComparer.Ordinal)
                .Select(group => group.First())
                .ToList();
            SelectedCategoryDefNames = SelectedCategoryDefNames?
                .Where(defName => !string.IsNullOrWhiteSpace(defName))
                .Distinct(StringComparer.Ordinal)
                .ToList() ?? new List<string>();
        }
    }
}
