using System;
using System.Collections.Generic;
using System.Linq;
using MaterialNumbers.Core;
using UnityEngine;
using Verse;

namespace MaterialNumbers.UI
{
    internal sealed class Dialog_ColumnPicker : Window
    {
        private readonly IReadOnlyList<MaterialColumnDefinition> columns;
        private readonly HashSet<string> selected;
        private readonly Action<HashSet<string>> accepted;
        private Vector2 scrollPosition;
        private string search = string.Empty;

        public Dialog_ColumnPicker(
            IReadOnlyList<MaterialColumnDefinition> columns,
            IEnumerable<string> selectedIds,
            Action<HashSet<string>> accepted)
        {
            this.columns = columns;
            selected = new HashSet<string>(selectedIds, StringComparer.Ordinal);
            this.accepted = accepted;
            doCloseX = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(680f, 760f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 34f), "MaterialNumbers.Columns.Title".Translate());
            Text.Font = GameFont.Small;

            search = Widgets.TextField(new Rect(0f, 42f, inRect.width, 32f), search);
            TooltipHandler.TipRegion(new Rect(0f, 42f, inRect.width, 32f), "MaterialNumbers.Columns.SearchTip".Translate());

            Rect listRect = new Rect(0f, 84f, inRect.width, inRect.height - 140f);
            List<IGrouping<string, MaterialColumnDefinition>> groups = FilteredColumns()
                .GroupBy(column => column.Group)
                .OrderBy(group => group.Key)
                .ToList();
            float contentHeight = groups.Sum(group => 34f + group.Count() * 34f) + 8f;
            Rect viewRect = new Rect(0f, 0f, listRect.width - 18f, Math.Max(contentHeight, listRect.height));

            Widgets.BeginScrollView(listRect, ref scrollPosition, viewRect);
            float y = 0f;
            foreach (IGrouping<string, MaterialColumnDefinition> group in groups)
            {
                Text.Font = GameFont.Medium;
                Widgets.Label(new Rect(4f, y, viewRect.width - 8f, 30f), group.Key);
                Text.Font = GameFont.Small;
                y += 34f;

                foreach (MaterialColumnDefinition column in group.OrderBy(item => item.Label))
                {
                    Rect rowRect = new Rect(4f, y, viewRect.width - 8f, 30f);
                    bool isSelected = selected.Contains(column.Id);
                    Widgets.CheckboxLabeled(rowRect, column.Label, ref isSelected);
                    if (isSelected)
                    {
                        selected.Add(column.Id);
                    }
                    else
                    {
                        selected.Remove(column.Id);
                    }

                    TooltipHandler.TipRegion(rowRect, BuildTip(column));
                    y += 34f;
                }
            }

            Widgets.EndScrollView();

            Widgets.Label(
                new Rect(0f, inRect.height - 48f, inRect.width - 290f, 36f),
                "MaterialNumbers.Columns.Selected".Translate(selected.Count, columns.Count));
            if (Widgets.ButtonText(new Rect(inRect.width - 280f, inRect.height - 48f, 130f, 36f), "CancelButton".Translate()))
            {
                Close();
            }

            if (Widgets.ButtonText(new Rect(inRect.width - 140f, inRect.height - 48f, 140f, 36f), "MaterialNumbers.Action.Apply".Translate()))
            {
                accepted?.Invoke(new HashSet<string>(selected, StringComparer.Ordinal));
                Close();
            }
        }

        private IEnumerable<MaterialColumnDefinition> FilteredColumns()
        {
            string term = search.Trim();
            if (term.Length == 0)
            {
                return columns;
            }

            return columns.Where(column =>
                Contains(column.Label, term) ||
                Contains(column.Id, term) ||
                Contains(column.Source, term) ||
                Contains(column.Group, term));
        }

        private static bool Contains(string text, string term)
        {
            return text?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string BuildTip(MaterialColumnDefinition column)
        {
            return column.Description + "\n\n" +
                   "MaterialNumbers.Tooltip.Source".Translate(column.Source) + "\n" +
                   column.Id;
        }
    }
}
