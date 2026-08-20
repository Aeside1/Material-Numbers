using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MaterialNumbers.Core;
using RimWorld;
using Verse;

namespace MaterialNumbers.Discovery
{
    internal sealed class ExtensionStatColumnProvider : IMaterialColumnProvider
    {
        private static readonly BindingFlags MemberFlags = BindingFlags.Instance | BindingFlags.Public;
        private readonly HashSet<string> loggedFailures = new HashSet<string>();

        public string ProviderId => "materialnumbers.extension-stats";

        public IEnumerable<MaterialColumnDefinition> CreateColumns(MaterialDiscoveryContext context)
        {
            List<ExtensionMemberAccessor> accessors = DiscoverAccessors(context).ToList();
            foreach (ExtensionMemberAccessor accessor in accessors)
            {
                foreach (StatDef stat in DiscoverStats(context.Materials, accessor))
                {
                    ExtensionMemberAccessor capturedAccessor = accessor;
                    StatDef capturedStat = stat;
                    yield return new MaterialColumnDefinition(
                        MaterialColumnIds.Extension(accessor.ExtensionType.FullName, accessor.Member.Name, stat.defName),
                        MaterialStatPresentation.AddMarker(stat.LabelCap.ToString(), MarkerFor(accessor.Semantic)),
                        stat.description,
                        accessor.Group,
                        accessor.Source,
                        110f,
                        material => ReadValue(material, capturedAccessor, capturedStat));
                }
            }
        }

        private IEnumerable<ExtensionMemberAccessor> DiscoverAccessors(MaterialDiscoveryContext context)
        {
            IEnumerable<Type> extensionTypes = context.Materials
                .Where(material => material.modExtensions != null)
                .SelectMany(material => material.modExtensions)
                .Where(extension => extension != null)
                .Select(extension => extension.GetType())
                .Distinct();

            foreach (Type extensionType in extensionTypes)
            {
                foreach (MemberInfo member in GetMembers(extensionType))
                {
                    Type valueType = GetValueType(member);
                    if (!ExtensionContainerClassifier.IsEnumerableContainer(valueType))
                    {
                        continue;
                    }

                    ExtensionSemantic semantic = InferSemantic(member.Name);
                    bool typedStatContainer = ExtensionContainerClassifier.IsSupportedTypedContainer(
                        valueType,
                        typeof(StatModifier),
                        typeof(StatDef));
                    if (!typedStatContainer && semantic == ExtensionSemantic.None)
                    {
                        continue;
                    }

                    ExtensionMemberAccessor accessor = new ExtensionMemberAccessor(
                        extensionType,
                        member,
                        semantic,
                        IsToolAccessor(extensionType, member)
                            ? "MaterialNumbers.Group.ToolStats".Translate()
                            : "MaterialNumbers.Group.ModExtensions".Translate(),
                        extensionType.Assembly.GetName().Name);

                    if (context.Materials.Any(material => HasReadableStatValues(material, accessor)))
                    {
                        yield return accessor;
                    }
                }
            }
        }

        private static IEnumerable<MemberInfo> GetMembers(Type type)
        {
            foreach (FieldInfo field in type.GetFields(MemberFlags))
            {
                if (!field.IsStatic)
                {
                    yield return field;
                }
            }

            foreach (PropertyInfo property in type.GetProperties(MemberFlags))
            {
                if (property.GetIndexParameters().Length == 0 && property.CanRead && property.GetMethod != null && !property.GetMethod.IsStatic)
                {
                    yield return property;
                }
            }
        }

        private static Type GetValueType(MemberInfo member)
        {
            if (member is FieldInfo field)
            {
                return field.FieldType;
            }

            return (member as PropertyInfo)?.PropertyType;
        }

        private static ExtensionSemantic InferSemantic(string memberName)
        {
            if (memberName.EndsWith("StatFactors", StringComparison.OrdinalIgnoreCase))
            {
                return ExtensionSemantic.Factor;
            }

            if (memberName.EndsWith("StatOffsets", StringComparison.OrdinalIgnoreCase))
            {
                return ExtensionSemantic.Offset;
            }

            return ExtensionSemantic.None;
        }

        private static bool IsToolAccessor(Type extensionType, MemberInfo member)
        {
            return string.Equals(extensionType.FullName, "SurvivalToolsLite.StuffPropsTool", StringComparison.Ordinal) ||
                   string.Equals(member.Name, "toolStatFactors", StringComparison.OrdinalIgnoreCase);
        }

        private bool HasReadableStatValues(ThingDef material, ExtensionMemberAccessor accessor)
        {
            return EnumerateValues(material, accessor).Any();
        }

        private IEnumerable<StatDef> DiscoverStats(IReadOnlyList<ThingDef> materials, ExtensionMemberAccessor accessor)
        {
            return materials
                .SelectMany(material => EnumerateValues(material, accessor))
                .Where(pair => pair.Stat != null)
                .Select(pair => pair.Stat)
                .GroupBy(stat => stat.defName)
                .Select(group => group.First())
                .OrderBy(stat => stat.LabelCap.ToString());
        }

        private MaterialCellValue ReadValue(ThingDef material, ExtensionMemberAccessor accessor, StatDef stat)
        {
            foreach (StatValuePair pair in EnumerateValues(material, accessor))
            {
                if (pair.Stat == stat)
                {
                    return new MaterialCellValue(
                        pair.Value,
                        FormatValue(accessor.Semantic, stat, pair.Value),
                        true);
                }
            }

            if (accessor.Semantic == ExtensionSemantic.Factor)
            {
                return new MaterialCellValue(
                    1f,
                    MaterialStatPresentation.FormatFactor(1f),
                    false,
                    "MaterialNumbers.Value.Neutral".Translate());
            }

            if (accessor.Semantic == ExtensionSemantic.Offset)
            {
                return new MaterialCellValue(
                    0f,
                    StatValueFormatter.Format(stat, 0f),
                    false,
                    "MaterialNumbers.Value.Neutral".Translate());
            }

            return MaterialCellValue.Missing;
        }

        private static string MarkerFor(ExtensionSemantic semantic)
        {
            switch (semantic)
            {
                case ExtensionSemantic.Factor:
                    return "MaterialNumbers.ColumnMarker.Factor".Translate();
                case ExtensionSemantic.Offset:
                    return "MaterialNumbers.ColumnMarker.Offset".Translate();
                default:
                    return null;
            }
        }

        private static string FormatValue(ExtensionSemantic semantic, StatDef stat, float value)
        {
            return semantic == ExtensionSemantic.Factor
                ? MaterialStatPresentation.FormatFactor(value)
                : StatValueFormatter.Format(stat, value);
        }

        private IEnumerable<StatValuePair> EnumerateValues(ThingDef material, ExtensionMemberAccessor accessor)
        {
            DefModExtension extension = material.modExtensions?.FirstOrDefault(item => item != null && item.GetType() == accessor.ExtensionType);
            if (extension == null)
            {
                yield break;
            }

            object container;
            try
            {
                container = accessor.GetValue(extension);
            }
            catch (Exception exception)
            {
                LogFailure(accessor, exception);
                yield break;
            }

            if (!(container is IEnumerable enumerable))
            {
                yield break;
            }

            IEnumerator enumerator;
            try
            {
                enumerator = enumerable.GetEnumerator();
            }
            catch (Exception exception)
            {
                LogFailure(accessor, exception);
                yield break;
            }

            while (true)
            {
                object item;
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }

                    item = enumerator.Current;
                }
                catch (Exception exception)
                {
                    LogFailure(accessor, exception);
                    yield break;
                }

                if (item is StatModifier modifier && modifier.stat != null)
                {
                    yield return new StatValuePair(modifier.stat, modifier.value);
                    continue;
                }

                if (TryReadDictionaryEntry(item, out StatDef stat, out float value))
                {
                    yield return new StatValuePair(stat, value);
                }
            }
        }

        private static bool TryReadDictionaryEntry(object item, out StatDef stat, out float value)
        {
            if (item is DictionaryEntry entry && entry.Key is StatDef dictionaryStat && entry.Value is float dictionaryValue)
            {
                stat = dictionaryStat;
                value = dictionaryValue;
                return true;
            }

            if (item != null)
            {
                Type itemType = item.GetType();
                PropertyInfo keyProperty = itemType.GetProperty("Key", MemberFlags);
                PropertyInfo valueProperty = itemType.GetProperty("Value", MemberFlags);
                if (keyProperty?.GetValue(item) is StatDef genericStat && valueProperty?.GetValue(item) is float genericValue)
                {
                    stat = genericStat;
                    value = genericValue;
                    return true;
                }
            }

            stat = null;
            value = 0f;
            return false;
        }

        private void LogFailure(ExtensionMemberAccessor accessor, Exception exception)
        {
            string key = accessor.ExtensionType.FullName + "." + accessor.Member.Name;
            if (loggedFailures.Add(key))
            {
                Log.Warning("[Material Numbers] Could not read extension stat container " + key + ": " + exception.Message);
            }
        }

        private enum ExtensionSemantic
        {
            None,
            Factor,
            Offset
        }

        private sealed class ExtensionMemberAccessor
        {
            public ExtensionMemberAccessor(Type extensionType, MemberInfo member, ExtensionSemantic semantic, string group, string source)
            {
                ExtensionType = extensionType;
                Member = member;
                Semantic = semantic;
                Group = group;
                Source = source;
            }

            public Type ExtensionType { get; }

            public MemberInfo Member { get; }

            public ExtensionSemantic Semantic { get; }

            public string Group { get; }

            public string Source { get; }

            public object GetValue(object instance)
            {
                if (Member is FieldInfo field)
                {
                    return field.GetValue(instance);
                }

                return ((PropertyInfo)Member).GetValue(instance);
            }
        }

        private readonly struct StatValuePair
        {
            public StatValuePair(StatDef stat, float value)
            {
                Stat = stat;
                Value = value;
            }

            public StatDef Stat { get; }

            public float Value { get; }
        }
    }
}
