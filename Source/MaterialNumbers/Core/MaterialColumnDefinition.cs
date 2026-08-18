using System;
using Verse;

namespace MaterialNumbers.Core
{
    public sealed class MaterialColumnDefinition
    {
        private readonly Func<ThingDef, MaterialCellValue> valueReader;

        public MaterialColumnDefinition(
            string id,
            string label,
            string description,
            string group,
            string source,
            float defaultWidth,
            Func<ThingDef, MaterialCellValue> valueReader)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Column ID must not be empty.", nameof(id));
            }

            Id = id;
            Label = string.IsNullOrWhiteSpace(label) ? id : label;
            Description = description ?? string.Empty;
            Group = group ?? string.Empty;
            Source = source ?? string.Empty;
            DefaultWidth = Math.Max(60f, defaultWidth);
            this.valueReader = valueReader ?? throw new ArgumentNullException(nameof(valueReader));
        }

        public string Id { get; }

        public string Label { get; }

        public string Description { get; }

        public string Group { get; }

        public string Source { get; }

        public float DefaultWidth { get; }

        public MaterialCellValue Read(ThingDef material)
        {
            return valueReader(material);
        }
    }
}
