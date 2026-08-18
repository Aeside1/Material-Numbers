using System.Collections.Generic;

namespace MaterialNumbers.Core
{
    public interface IMaterialColumnProvider
    {
        string ProviderId { get; }

        IEnumerable<MaterialColumnDefinition> CreateColumns(MaterialDiscoveryContext context);
    }
}
