using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MaterialNumbers.UI
{
    internal sealed class Dialog_ThingCategoryPicker : Window
    {
        private readonly IReadOnlyList<ThingCategoryDef> categories;
        private readonly HashSet<string> selected;
        private readonly Action<HashSet<string>> accepted;
        private readonly List<ThingCategoryDef> path = new List<ThingCategoryDef>();
        private Vector2 scrollPosition;
        private string search = string.Empty;

        public Dialog_ThingCategoryPicker(
            IReadOnlyList<ThingCategoryDef> categories,
            IEnumerable<string> selectedNames,
            Action<HashSet<string>> accepted)
        {
            this.categories = categories ?? Array.Empty<ThingCategoryDef>();
            selected = new HashSet<string>(selectedNames ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
            this.accepted = accepted;
            doCloseX = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(720f, 760f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 34f), "MaterialNumbers.Filter.DefinitionTypesTitle".Translate());
            Text.Font = GameFont.Small;

            search = Widgets.TextField(new Rect(0f, 42f, inRect.width, 32f), search);
            TooltipHandler.TipRegion(new Rect(0f, 42f, inRect.width, 32f), "MaterialNumbers.Filter.DefinitionTypesSearchTip".Translate());

            Rect breadcrumbRect = new Rect(0f, 84f, inRect.width, 30f);
            DrawBreadcrumb(breadcrumbRect);

            Rect listRect = new Rect(0f, 122f, inRect.width, inRect.height - 178f);
            List<ThingCategoryDef> visible = GetVisibleCategories();
            Rect viewRect = new Rect(0f, 0f, listRect.width - 18f, Math.Max(visible.Count * 34f + 8f, listRect.height));
            Widgets.BeginScrollView(listRect, ref scrollPosition, viewRect);
            float y = 0f;
            foreach (ThingCategoryDef category in visible)
            {
                Rect rowRect = new Rect(4f, y, viewRect.width - 8f, 30f);
                bool isSelected = selected.Contains(category.defName);
                Widgets.CheckboxLabeled(rowRect, CategoryLabel(category), ref isSelected);
                if (isSelected)
                {
                    selected.Add(category.defName);
                }
                else
                {
                    selected.Remove(category.defName);
                }

                if (string.IsNullOrEmpty(search) && HasChildren(category))
                {
                    Rect enterRect = new Rect(rowRect.xMax - 30f, rowRect.y, 26f, rowRect.height);
                    if (Widgets.ButtonText(enterRect, ">"))
                    {
                        path.Add(category);
                        scrollPosition = Vector2.zero;
                    }
                }

                y += 34f;
            }

            Widgets.EndScrollView();

            Widgets.Label(
                new Rect(0f, inRect.height - 44f, inRect.width - 290f, 32f),
                "MaterialNumbers.Filter.DefinitionTypesSelected".Translate(selected.Count));
            if (Widgets.ButtonText(new Rect(inRect.width - 280f, inRect.height - 44f, 130f, 32f), "CancelButton".Translate()))
            {
                Close();
            }

            if (Widgets.ButtonText(new Rect(inRect.width - 140f, inRect.height - 44f, 140f, 32f), "MaterialNumbers.Action.Apply".Translate()))
            {
                accepted?.Invoke(new HashSet<string>(selected, StringComparer.Ordinal));
                Close();
            }
        }

        private void DrawBreadcrumb(Rect rect)
        {
            string label = path.Count == 0
                ? "MaterialNumbers.Filter.DefinitionTypesRoot".Translate()
                : string.Join(" / ", path.Select(category => category.LabelCap.ToString()));
            if (path.Count > 0 && Widgets.ButtonText(new Rect(rect.x, rect.y, 90f, rect.height), "BackButton".Translate()))
            {
                path.RemoveAt(path.Count - 1);
                scrollPosition = Vector2.zero;
                return;
            }

            Widgets.Label(new Rect(rect.x + (path.Count > 0 ? 98f : 0f), rect.y, rect.width - (path.Count > 0 ? 98f : 0f), rect.height), label);
        }

        private List<ThingCategoryDef> GetVisibleCategories()
        {
            string term = search.Trim();
            if (term.Length > 0)
            {
                return categories
                    .Where(category => Contains(category.LabelCap.ToString(), term) || Contains(category.defName, term))
                    .OrderBy(category => category.LabelCap.ToString())
                    .ToList();
            }

            ThingCategoryDef parent = path.Count == 0 ? null : path[path.Count - 1];
            return categories
                .Where(category => category.parent == parent)
                .OrderBy(category => category.LabelCap.ToString())
                .ToList();
        }

        private bool HasChildren(ThingCategoryDef category)
        {
            return categories.Any(candidate => candidate.parent == category);
        }

        private string CategoryLabel(ThingCategoryDef category)
        {
            int descendants = categories.Count(candidate => IsDescendantOrSelf(candidate, category));
            return category.LabelCap + " (" + descendants + ")";
        }

        private static bool IsDescendantOrSelf(ThingCategoryDef candidate, ThingCategoryDef ancestor)
        {
            ThingCategoryDef current = candidate;
            while (current != null)
            {
                if (current == ancestor)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static bool Contains(string value, string term)
        {
            return value?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
