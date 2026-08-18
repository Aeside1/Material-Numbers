using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MaterialNumbers.Discovery;
using Verse;

namespace MaterialNumbers.Core
{
    public sealed class MaterialColumnCatalog
    {
        private readonly Dictionary<string, MaterialColumnDefinition> byId;

        private MaterialColumnCatalog(
            IReadOnlyList<ThingDef> materials,
            IReadOnlyList<MaterialColumnDefinition> columns)
        {
            Materials = materials;
            Columns = columns;
            byId = columns.ToDictionary(column => column.Id, StringComparer.Ordinal);
        }

        public IReadOnlyList<ThingDef> Materials { get; }

        public IReadOnlyList<MaterialColumnDefinition> Columns { get; }

        public bool TryGet(string id, out MaterialColumnDefinition column)
        {
            return byId.TryGetValue(id, out column);
        }

        public static MaterialColumnCatalog Build()
        {
            List<ThingDef> materials = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(definition => definition.IsStuff)
                .OrderBy(definition => definition.LabelCap.ToString())
                .ToList();

            var context = new MaterialDiscoveryContext(
                new ReadOnlyCollection<ThingDef>(materials),
                warning => Log.Warning("[Material Numbers] " + warning));

            var providers = new List<IMaterialColumnProvider>
            {
                new BuiltinColumnProvider(),
                new StandardStatColumnProvider(),
                new ExtensionStatColumnProvider()
            };
            providers.AddRange(MaterialNumbersRegistry.RegisteredProviders);

            var columns = new List<MaterialColumnDefinition>();
            var knownIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (IMaterialColumnProvider provider in providers)
            {
                try
                {
                    IEnumerable<MaterialColumnDefinition> providedColumns = provider.CreateColumns(context) ?? Enumerable.Empty<MaterialColumnDefinition>();
                    foreach (MaterialColumnDefinition column in providedColumns)
                    {
                        if (column == null)
                        {
                            continue;
                        }

                        if (knownIds.Add(column.Id))
                        {
                            columns.Add(column);
                        }
                        else
                        {
                            Log.Warning("[Material Numbers] Ignored duplicate column ID from provider " + provider.ProviderId + ": " + column.Id);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.Error("[Material Numbers] Column provider failed: " + provider.ProviderId + "\n" + exception);
                }
            }

            return new MaterialColumnCatalog(
                new ReadOnlyCollection<ThingDef>(materials),
                new ReadOnlyCollection<MaterialColumnDefinition>(columns));
        }
    }
}
