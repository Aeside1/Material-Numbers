using System;
using System.Collections.Generic;

namespace MaterialNumbers.Core
{
    public static class MaterialNumbersRegistry
    {
        private static readonly List<IMaterialColumnProvider> Providers = new List<IMaterialColumnProvider>();

        public static event Action CatalogInvalidated;

        public static void Register(IMaterialColumnProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (Providers.Exists(item => string.Equals(item.ProviderId, provider.ProviderId, StringComparison.Ordinal)))
            {
                throw new InvalidOperationException("A Material Numbers provider with this ID is already registered: " + provider.ProviderId);
            }

            Providers.Add(provider);
            CatalogInvalidated?.Invoke();
        }

        internal static IReadOnlyList<IMaterialColumnProvider> RegisteredProviders => Providers;
    }
}
