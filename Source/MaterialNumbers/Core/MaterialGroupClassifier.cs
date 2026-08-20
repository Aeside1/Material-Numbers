using System;
using System.Collections.Generic;

namespace MaterialNumbers.Core
{
    public enum MaterialGroup
    {
        Metal,
        Stone,
        Wood,
        TextileLeather,
        PlasticGlass,
        Other
    }

    public static class MaterialGroupClassifier
    {
        public static MaterialGroup Classify(IEnumerable<string> categoryDefNames)
        {
            return Classify(categoryDefNames, null, null);
        }

        public static MaterialGroup Classify(
            IEnumerable<string> categoryDefNames,
            IEnumerable<string> thingCategoryDefNames,
            string defName)
        {
            bool hasMetal = false;
            bool hasStone = false;
            bool hasWood = false;
            bool hasTextileLeather = false;
            bool hasHighPerformanceFiber = false;
            bool hasPlasticGlass = false;

            if (categoryDefNames != null)
            {
                foreach (string categoryDefName in categoryDefNames)
                {
                    if (string.IsNullOrWhiteSpace(categoryDefName))
                    {
                        continue;
                    }

                    string category = categoryDefName.Trim();
                    hasMetal |= IsMetal(category);
                    hasStone |= IsStone(category);
                    hasWood |= IsWood(category);
                    hasTextileLeather |= IsTextileLeather(category);
                    hasHighPerformanceFiber |= string.Equals(category, "HF", StringComparison.OrdinalIgnoreCase);
                    hasPlasticGlass |= IsPlasticGlass(category);
                }
            }

            // Composite HSK categories can belong to more than one family. Prefer
            // the most recognisable user-facing material family over secondary tags.
            if (hasPlasticGlass)
            {
                return MaterialGroup.PlasticGlass;
            }

            if (hasWood)
            {
                return MaterialGroup.Wood;
            }

            if (hasStone)
            {
                return MaterialGroup.Stone;
            }

            if (hasTextileLeather)
            {
                return MaterialGroup.TextileLeather;
            }

            if (hasMetal)
            {
                return MaterialGroup.Metal;
            }

            if (hasHighPerformanceFiber)
            {
                return MaterialGroup.TextileLeather;
            }

            if (HasMetalThingCategory(thingCategoryDefNames) || IsMetalLikeDefName(defName))
            {
                return MaterialGroup.Metal;
            }

            if (IsStoneLikeDefName(defName))
            {
                return MaterialGroup.Stone;
            }

            if (IsWoodLikeDefName(defName))
            {
                return MaterialGroup.Wood;
            }

            return IsSyntheticLikeDefName(defName) ? MaterialGroup.PlasticGlass : MaterialGroup.Other;
        }

        private static bool IsMetal(string category)
        {
            return category.IndexOf("metallic", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   category.IndexOf("metal", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   string.Equals(category, "Precious", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(category, "Bioferrite", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsStone(string category)
        {
            return category.IndexOf("ston", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   category.IndexOf("brick", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   category.IndexOf("ceramic", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   string.Equals(category, "Aggregate", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsWood(string category)
        {
            return category.IndexOf("wood", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   category.IndexOf("lumber", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   category.IndexOf("bamboo", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   category.IndexOf("kindling", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsTextileLeather(string category)
        {
            return category.IndexOf("fabric", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   category.IndexOf("leather", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   string.Equals(category, "SoftArmor", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(category, "Straw", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPlasticGlass(string category)
        {
            return category.IndexOf("plastic", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   category.IndexOf("glass", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool HasMetalThingCategory(IEnumerable<string> thingCategoryDefNames)
        {
            if (thingCategoryDefNames == null)
            {
                return false;
            }

            foreach (string categoryDefName in thingCategoryDefNames)
            {
                if (string.Equals(categoryDefName, "HCM", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(categoryDefName, "HVY", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(categoryDefName, "PRS", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(categoryDefName, "SLD", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(categoryDefName, "RAR", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(categoryDefName, "RAD", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsMetalLikeDefName(string defName)
        {
            return ContainsAny(defName,
                "metal", "iron", "steel", "copper", "bronze", "brass", "aluminium", "aluminum",
                "nickel", "titanium", "tin", "uranium", "gold", "silver", "lead", "zinc", "cobalt",
                "chromium", "tungsten", "wolfram", "ilmenite", "anglesite", "sphalerite", "magnetite",
                "bioferrite", "depleteduranium", "titanomagnetite");
        }

        private static bool IsStoneLikeDefName(string defName)
        {
            return ContainsAny(defName,
                "stone", "granite", "limestone", "marble", "sandstone", "slate", "basalt", "mudstone",
                "pegmatite", "dunite", "alabaster", "obsidian", "concrete", "ceramic", "brick", "clay",
                "vacstone", "jade");
        }

        private static bool IsWoodLikeDefName(string defName)
        {
            return ContainsAny(defName, "wood", "plank", "log", "bamboo", "lumber", "kindling", "timber");
        }

        private static bool IsSyntheticLikeDefName(string defName)
        {
            return ContainsAny(defName, "plastic", "poly", "nylon", "fiber", "fibers", "fiberglass", "plexiglass", "glass", "pvc");
        }

        private static bool ContainsAny(string value, params string[] terms)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            foreach (string term in terms)
            {
                if (value.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
