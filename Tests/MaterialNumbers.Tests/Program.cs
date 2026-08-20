using System;
using System.Collections.Generic;
using System.Linq;
using MaterialNumbers.Core;

internal static class Program
{
    private sealed class FakeStatModifier
    {
    }

    private sealed class FakeStatDef
    {
    }

    private static int failures;

    private static int Main()
    {
        Run("stable column IDs", StableColumnIds);
        Run("unknown preset columns survive reconciliation", UnknownColumnsSurviveReconciliation);
        Run("new selected columns follow catalog order", NewColumnsFollowCatalogOrder);
        Run("missing values always sort last", MissingValuesAlwaysSortLast);
        Run("numeric and text values sort deterministically", ValuesSortDeterministically);
        Run("safe extension containers are classified", SafeContainersAreClassified);
        Run("material categories collapse into broad groups", MaterialCategoriesCollapseIntoBroadGroups);
        Run("material factor values use multiplier presentation", MaterialFactorsUseMultiplierPresentation);
        Run("material stat source markers distinguish columns", MaterialStatSourceMarkersDistinguishColumns);

        Console.WriteLine(failures == 0
            ? "All Material Numbers logic tests passed."
            : failures + " Material Numbers logic test(s) failed.");
        return failures == 0 ? 0 : 1;
    }

    private static void StableColumnIds()
    {
        Equal("stat-base:MarketValue", MaterialColumnIds.StatBase("MarketValue"));
        Equal("stuff-factor:MiningSpeed", MaterialColumnIds.StuffFactor("MiningSpeed"));
        Equal("stuff-offset:Beauty", MaterialColumnIds.StuffOffset("Beauty"));
        Equal(
            "extension:SurvivalToolsLite.StuffPropsTool:toolStatFactors:MiningSpeed",
            MaterialColumnIds.Extension("SurvivalToolsLite.StuffPropsTool", "toolStatFactors", "MiningSpeed"));
    }

    private static void UnknownColumnsSurviveReconciliation()
    {
        string[] existing = { "builtin:amount", "extension:missing:type:stat", "stuff-factor:Beauty" };
        var known = new HashSet<string>(new[] { "builtin:amount", "stuff-factor:Beauty", "stat-base:Mass" });
        var selected = new HashSet<string>(new[] { "stuff-factor:Beauty" });
        IReadOnlyList<string> result = ColumnSelectionReconciler.Reconcile(
            existing,
            known,
            selected,
            new[] { "builtin:amount", "stat-base:Mass", "stuff-factor:Beauty" });

        SequenceEqual(new[] { "extension:missing:type:stat", "stuff-factor:Beauty" }, result);
    }

    private static void NewColumnsFollowCatalogOrder()
    {
        var known = new HashSet<string>(new[] { "a", "b", "c" });
        var selected = new HashSet<string>(new[] { "a", "c" });
        IReadOnlyList<string> result = ColumnSelectionReconciler.Reconcile(
            new[] { "a" },
            known,
            selected,
            new[] { "a", "b", "c" });

        SequenceEqual(new[] { "a", "c" }, result);
    }

    private static void MissingValuesAlwaysSortLast()
    {
        MaterialCellValue value = new MaterialCellValue(1d, "100%", true);
        True(MaterialCellValueComparer.Compare(value, MaterialCellValue.Missing, true) < 0);
        True(MaterialCellValueComparer.Compare(value, MaterialCellValue.Missing, false) < 0);
    }

    private static void ValuesSortDeterministically()
    {
        MaterialCellValue low = new MaterialCellValue(0.8d, "80%", true);
        MaterialCellValue high = new MaterialCellValue(1.2d, "120%", true);
        True(MaterialCellValueComparer.Compare(low, high, true) < 0);
        True(MaterialCellValueComparer.Compare(low, high, false) > 0);

        MaterialCellValue alpha = new MaterialCellValue(0d, "Alpha", true);
        MaterialCellValue beta = new MaterialCellValue(0d, "Beta", true);
        True(MaterialCellValueComparer.Compare(alpha, beta, true) < 0);
    }

    private static void SafeContainersAreClassified()
    {
        True(ExtensionContainerClassifier.IsSupportedTypedContainer(
            typeof(List<FakeStatModifier>),
            typeof(FakeStatModifier),
            typeof(FakeStatDef)));
        True(ExtensionContainerClassifier.IsSupportedTypedContainer(
            typeof(Dictionary<FakeStatDef, float>),
            typeof(FakeStatModifier),
            typeof(FakeStatDef)));
        False(ExtensionContainerClassifier.IsSupportedTypedContainer(
            typeof(List<float>),
            typeof(FakeStatModifier),
            typeof(FakeStatDef)));
        False(ExtensionContainerClassifier.IsEnumerableContainer(typeof(float)));
        False(ExtensionContainerClassifier.IsEnumerableContainer(typeof(string)));
    }

    private static void MaterialCategoriesCollapseIntoBroadGroups()
    {
        Equal(MaterialGroup.Metal, MaterialGroupClassifier.Classify(new[] { "StrongMetallic", "RuggedMetallic" }));
        Equal(MaterialGroup.Stone, MaterialGroupClassifier.Classify(new[] { "Stony", "Metallic" }));
        Equal(MaterialGroup.Wood, MaterialGroupClassifier.Classify(new[] { "HardwoodLumber" }));
        Equal(MaterialGroup.TextileLeather, MaterialGroupClassifier.Classify(new[] { "Fabric", "HF" }));
        Equal(MaterialGroup.TextileLeather, MaterialGroupClassifier.Classify(new[] { "HF" }));
        Equal(MaterialGroup.Metal, MaterialGroupClassifier.Classify(new[] { "Metallic", "HF" }));
        Equal(MaterialGroup.PlasticGlass, MaterialGroupClassifier.Classify(new[] { "Plastic", "StrongMetallic" }));
        Equal(MaterialGroup.Metal, MaterialGroupClassifier.Classify(new[] { "Matty" }, new[] { "ResourcesRaw" }, "Bioferrite"));
        Equal(MaterialGroup.Metal, MaterialGroupClassifier.Classify(Array.Empty<string>(), new[] { "HCM" }, "Copper"));
        Equal(MaterialGroup.PlasticGlass, MaterialGroupClassifier.Classify(Array.Empty<string>(), new[] { "Chemical" }, "Nylon"));
        Equal(MaterialGroup.Other, MaterialGroupClassifier.Classify(new[] { "Matty" }, new[] { "WeaponParts" }, "Weapon_Parts"));
        Equal(MaterialGroup.Other, MaterialGroupClassifier.Classify(new[] { "Matty" }));
        Equal(MaterialGroup.Other, MaterialGroupClassifier.Classify(new[] { "Foods", "Stuff" }));
        Equal(MaterialGroup.Other, MaterialGroupClassifier.Classify(Array.Empty<string>()));
    }

    private static void MaterialFactorsUseMultiplierPresentation()
    {
        Equal("100%", MaterialStatPresentation.FormatFactor(1f));
        Equal("150%", MaterialStatPresentation.FormatFactor(1.5f));
        Equal("400%", MaterialStatPresentation.FormatFactor(4f));
        Equal("85.5%", MaterialStatPresentation.FormatFactor(0.855f));
    }

    private static void MaterialStatSourceMarkersDistinguishColumns()
    {
        Equal("Rest effectiveness =", MaterialStatPresentation.AddMarker("Rest effectiveness", "="));
        Equal("Rest effectiveness x", MaterialStatPresentation.AddMarker("Rest effectiveness", "x"));
        Equal("Beauty +", MaterialStatPresentation.AddMarker("Beauty", "+"));
    }

    private static void Run(string name, Action test)
    {
        try
        {
            test();
            Console.WriteLine("PASS " + name);
        }
        catch (Exception exception)
        {
            failures++;
            Console.WriteLine("FAIL " + name + ": " + exception.Message);
        }
    }

    private static void Equal(string expected, string actual)
    {
        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Expected '" + expected + "' but got '" + actual + "'.");
        }
    }

    private static void Equal(MaterialGroup expected, MaterialGroup actual)
    {
        if (expected != actual)
        {
            throw new InvalidOperationException("Expected '" + expected + "' but got '" + actual + "'.");
        }
    }

    private static void SequenceEqual(IEnumerable<string> expected, IEnumerable<string> actual)
    {
        if (!expected.SequenceEqual(actual))
        {
            throw new InvalidOperationException("Expected [" + string.Join(", ", expected) + "] but got [" + string.Join(", ", actual) + "].");
        }
    }

    private static void True(bool value)
    {
        if (!value)
        {
            throw new InvalidOperationException("Expected true.");
        }
    }

    private static void False(bool value)
    {
        if (value)
        {
            throw new InvalidOperationException("Expected false.");
        }
    }
}
