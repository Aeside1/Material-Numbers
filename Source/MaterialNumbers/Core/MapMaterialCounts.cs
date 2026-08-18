using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MaterialNumbers.Core
{
    public sealed class MapMaterialCounts
    {
        private readonly Dictionary<ThingDef, int> allCounts = new Dictionary<ThingDef, int>();
        private readonly Dictionary<ThingDef, int> storageCounts = new Dictionary<ThingDef, int>();

        public int GetAll(ThingDef material)
        {
            return allCounts.TryGetValue(material, out int count) ? count : 0;
        }

        public int GetStored(ThingDef material)
        {
            return storageCounts.TryGetValue(material, out int count) ? count : 0;
        }

        public void Refresh(Map map)
        {
            allCounts.Clear();
            storageCounts.Clear();
            if (map == null)
            {
                return;
            }

            foreach (Thing thing in map.listerThings.AllThings)
            {
                if (thing?.def == null || !thing.def.IsStuff)
                {
                    continue;
                }

                Add(allCounts, thing.def, thing.stackCount);
                if (StoreUtility.IsInValidStorage(thing))
                {
                    Add(storageCounts, thing.def, thing.stackCount);
                }
            }
        }

        private static void Add(Dictionary<ThingDef, int> counts, ThingDef material, int amount)
        {
            counts.TryGetValue(material, out int current);
            counts[material] = current + amount;
        }
    }
}
