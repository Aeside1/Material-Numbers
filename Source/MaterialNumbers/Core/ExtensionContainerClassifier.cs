using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MaterialNumbers.Core
{
    internal static class ExtensionContainerClassifier
    {
        public static bool IsEnumerableContainer(Type valueType)
        {
            return valueType != null && valueType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(valueType);
        }

        public static bool IsSupportedTypedContainer(Type valueType, Type statModifierType, Type statDefType)
        {
            if (!IsEnumerableContainer(valueType))
            {
                return false;
            }

            IEnumerable<Type> candidates = valueType.GetInterfaces().Concat(new[] { valueType });
            return candidates.Any(item =>
                       item.IsGenericType &&
                       item.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                       item.GetGenericArguments()[0] == statModifierType) ||
                   candidates.Any(item =>
                       item.IsGenericType &&
                       item.GetGenericTypeDefinition() == typeof(IDictionary<,>) &&
                       item.GetGenericArguments()[0] == statDefType &&
                       item.GetGenericArguments()[1] == typeof(float));
        }
    }
}
