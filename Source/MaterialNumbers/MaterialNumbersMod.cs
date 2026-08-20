using System.Collections.Generic;
using MaterialNumbers.Presets;
using UnityEngine;
using Verse;

namespace MaterialNumbers
{
    public sealed class MaterialNumbersMod : Mod
    {
        public MaterialNumbersMod(ModContentPack content) : base(content)
        {
            Instance = this;
            Settings = GetSettings<MaterialNumbersSettings>();
            Settings.Normalize();
        }

        public static MaterialNumbersMod Instance { get; private set; }

        public static MaterialNumbersSettings Settings { get; private set; }

        public override string SettingsCategory()
        {
            return "MaterialNumbers.Title".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.Label("MaterialNumbers.Settings.Description".Translate());
            listing.Gap();
            if (listing.ButtonText("MaterialNumbers.Settings.ResetFilters".Translate()))
            {
                Settings.AvailabilityMode = MaterialAvailabilityMode.AllLoaded;
                Settings.GroupFilter = MaterialGroupFilter.Common;
                WriteSettings();
            }

            listing.End();
        }

        public static IReadOnlyList<MaterialViewPreset> GetAllPresets()
        {
            var presets = new List<MaterialViewPreset>();
            presets.AddRange(BuiltinPresetFactory.CreateAll());
            presets.AddRange(Settings.UserPresets);
            return presets;
        }

        public static MaterialViewPreset FindPreset(string id)
        {
            foreach (MaterialViewPreset preset in GetAllPresets())
            {
                if (preset.Id == id)
                {
                    return preset;
                }
            }

            return BuiltinPresetFactory.CreateAll()[0];
        }

        public static void SaveSettings()
        {
            Instance?.WriteSettings();
        }
    }
}
