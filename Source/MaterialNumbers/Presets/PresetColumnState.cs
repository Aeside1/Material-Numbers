using System;
using Verse;

namespace MaterialNumbers.Presets
{
    public sealed class PresetColumnState : IExposable
    {
        public PresetColumnState()
        {
        }

        public PresetColumnState(string columnId, float width)
        {
            ColumnId = columnId;
            Width = width;
        }

        public string ColumnId;

        public float Width = 100f;

        public void ExposeData()
        {
            Scribe_Values.Look(ref ColumnId, "columnId");
            Scribe_Values.Look(ref Width, "width", 100f);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Width = Math.Max(60f, Width);
            }
        }

        public PresetColumnState Clone()
        {
            return new PresetColumnState(ColumnId, Width);
        }
    }
}
