using System;
using System.Collections.Generic;
using Verse;

namespace MaterialNumbers.Core
{
    public sealed class MaterialDiscoveryContext
    {
        internal MaterialDiscoveryContext(IReadOnlyList<ThingDef> materials, Action<string> logWarning)
        {
            Materials = materials;
            LogWarning = logWarning;
        }

        public IReadOnlyList<ThingDef> Materials { get; }

        public Action<string> LogWarning { get; }
    }
}
