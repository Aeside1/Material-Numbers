using System;
using System.Collections.Generic;
using System.Linq;
using MaterialNumbers.Core;
using MaterialNumbers.Presets;
using RimWorld;
using UnityEngine;
using Verse;

namespace MaterialNumbers.UI
{
    public sealed class MainTabWindow_MaterialNumbers : MainTabWindow
    {
        private const float ToolbarHeight = 78f;
        private const float HeaderHeight = 34f;
        private const float RowHeight = 34f;
        private const float NameColumnWidth = 230f;
        private const float ResizeHandleWidth = 7f;

        private readonly MapMaterialCounts mapCounts = new MapMaterialCounts();
        private MaterialColumnCatalog catalog;
        private MaterialDataset dataset;
        private MaterialViewPreset workingPreset;
        private Vector2 tableScroll;
        private string search = string.Empty;
        private bool catalogDirty = true;
        private bool presetDirty;
        private int lastCountRefreshFrame = -1000;
        private int resizingColumn = -1;
        private int draggingColumn = -1;
        private float dragStartX;
        private float resizeStartX;
        private float resizeStartWidth;
        private bool headerDragged;

        public MainTabWindow_MaterialNumbers()
        {
            MaterialNumbersRegistry.CatalogInvalidated += () => catalogDirty = true;
        }

        public override Vector2 RequestedTabSize => new Vector2(Verse.UI.screenWidth * 0.95f, Verse.UI.screenHeight * 0.75f);

        public override void PostOpen()
        {
            base.PostOpen();
            EnsureCatalog();
            LoadPreset(MaterialNumbersMod.Settings.CurrentPresetId);
            RefreshMapCounts(true);
        }

        public override void DoWindowContents(Rect inRect)
        {
            EnsureCatalog();
            RefreshMapCounts(false);
            DrawToolbar(new Rect(0f, 0f, inRect.width, ToolbarHeight));
            DrawTable(new Rect(0f, ToolbarHeight + 4f, inRect.width, inRect.height - ToolbarHeight - 4f));
        }

        private void EnsureCatalog()
        {
            if (!catalogDirty && catalog != null)
            {
                return;
            }

            catalog = MaterialColumnCatalog.Build();
            dataset = MaterialDataset.Build(catalog);
            catalogDirty = false;
            if (workingPreset != null)
            {
                tableScroll = Vector2.zero;
            }
        }

        private void LoadPreset(string id)
        {
            MaterialViewPreset source = MaterialNumbersMod.FindPreset(id);
            workingPreset = source.Clone();
            MaterialNumbersMod.Settings.CurrentPresetId = source.Id;
            MaterialNumbersMod.SaveSettings();
            presetDirty = false;
            tableScroll = Vector2.zero;
        }

        private void DrawToolbar(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            const float padding = 8f;
            const float firstRowHeight = 30f;
            float y = rect.y + 6f;
            float x = rect.x + padding;

            string presetLabel = workingPreset?.Name ?? "MaterialNumbers.Preset.Overview".Translate();
            if (presetDirty)
            {
                presetLabel += " *";
            }

            if (Widgets.ButtonText(new Rect(x, y, 190f, firstRowHeight), presetLabel))
            {
                OpenPresetMenu();
            }

            x += 198f;
            if (Widgets.ButtonText(new Rect(x, y, 118f, firstRowHeight), "MaterialNumbers.Action.Columns".Translate()))
            {
                OpenColumnPicker();
            }

            x += 126f;
            if (Widgets.ButtonText(new Rect(x, y, 88f, firstRowHeight), "MaterialNumbers.Action.Save".Translate()))
            {
                SaveCurrentPreset();
            }

            x += 96f;
            if (Widgets.ButtonText(new Rect(x, y, 38f, firstRowHeight), "..."))
            {
                OpenPresetActions();
            }

            x += 46f;
            float searchWidth = Math.Max(160f, rect.xMax - padding - x);
            search = Widgets.TextField(new Rect(x, y, searchWidth, firstRowHeight), search);
            TooltipHandler.TipRegion(new Rect(x, y, searchWidth, firstRowHeight), "MaterialNumbers.Filter.SearchTip".Translate());

            y += 36f;
            x = rect.x + padding;
            if (Widgets.ButtonText(new Rect(x, y, 190f, 30f), GroupButtonLabel()))
            {
                OpenGroupMenu();
            }

            x += 204f;
            DrawAvailabilityButtons(new Rect(x, y, Math.Min(510f, rect.xMax - x - padding), 30f));
        }

        private void DrawAvailabilityButtons(Rect rect)
        {
            float width = rect.width / 3f;
            DrawAvailabilityButton(new Rect(rect.x, rect.y, width, rect.height), MaterialAvailabilityMode.AllLoaded, "MaterialNumbers.Filter.AllLoaded".Translate());
            DrawAvailabilityButton(new Rect(rect.x + width, rect.y, width, rect.height), MaterialAvailabilityMode.CurrentMap, "MaterialNumbers.Filter.CurrentMap".Translate());
            DrawAvailabilityButton(new Rect(rect.x + width * 2f, rect.y, width, rect.height), MaterialAvailabilityMode.CurrentStorage, "MaterialNumbers.Filter.CurrentStorage".Translate());
        }

        private void DrawAvailabilityButton(Rect rect, MaterialAvailabilityMode mode, string label)
        {
            bool selected = MaterialNumbersMod.Settings.AvailabilityMode == mode;
            if (selected)
            {
                Widgets.DrawHighlightSelected(rect);
            }

            if (Widgets.ButtonText(rect, label, true, false, true))
            {
                MaterialNumbersMod.Settings.AvailabilityMode = mode;
                MaterialNumbersMod.SaveSettings();
            }
        }

        private void DrawTable(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            List<PresetColumnState> visibleStates = VisibleColumnStates();
            List<MaterialRow> rows = FilterAndSortRows();

            Rect nameHeaderRect = new Rect(rect.x, rect.y, NameColumnWidth, HeaderHeight);
            Rect dataHeaderRect = new Rect(nameHeaderRect.xMax, rect.y, rect.width - NameColumnWidth, HeaderHeight);
            Rect nameRowsRect = new Rect(rect.x, nameHeaderRect.yMax, NameColumnWidth, rect.height - HeaderHeight);
            Rect dataRowsRect = new Rect(dataHeaderRect.x, dataHeaderRect.yMax, dataHeaderRect.width, rect.height - HeaderHeight);

            Widgets.DrawBoxSolid(nameHeaderRect, new Color(0.17f, 0.18f, 0.2f));
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(nameHeaderRect.ContractedBy(8f), "MaterialNumbers.Column.Material".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            DrawColumnHeaders(dataHeaderRect, visibleStates);

            float contentWidth = Math.Max(dataRowsRect.width - 16f, visibleStates.Sum(state => state.Width));
            float contentHeight = Math.Max(dataRowsRect.height - 16f, rows.Count * RowHeight);
            Rect viewRect = new Rect(0f, 0f, contentWidth, contentHeight);

            Widgets.BeginScrollView(dataRowsRect, ref tableScroll, viewRect);
            DrawDataRows(rows, visibleStates, contentWidth);
            Widgets.EndScrollView();
            DrawNameRows(nameRowsRect, rows);

            if (visibleStates.Count == 0)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(dataRowsRect, "MaterialNumbers.Message.NoColumns".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }

        private void DrawColumnHeaders(Rect rect, List<PresetColumnState> states)
        {
            GUI.BeginGroup(rect);
            float x = -tableScroll.x;
            Event current = Event.current;
            for (int index = 0; index < states.Count; index++)
            {
                PresetColumnState state = states[index];
                if (!catalog.TryGet(state.ColumnId, out MaterialColumnDefinition column))
                {
                    continue;
                }

                Rect headerRect = new Rect(x, 0f, state.Width, HeaderHeight);
                Widgets.DrawBoxSolid(headerRect, new Color(0.17f, 0.18f, 0.2f));
                Widgets.DrawHighlightIfMouseover(headerRect);
                string sortMarker = workingPreset.SortColumnId == state.ColumnId
                    ? workingPreset.SortAscending ? " ^" : " v"
                    : string.Empty;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(headerRect.ContractedBy(5f), column.Label + sortMarker);
                Text.Anchor = TextAnchor.UpperLeft;
                TooltipHandler.TipRegion(headerRect, BuildColumnTip(column));

                Rect resizeRect = new Rect(headerRect.xMax - ResizeHandleWidth, 0f, ResizeHandleWidth, HeaderHeight);
                HandleHeaderInput(current, headerRect, resizeRect, index, states);
                x += state.Width;
            }

            GUI.EndGroup();
        }

        private void HandleHeaderInput(Event current, Rect headerRect, Rect resizeRect, int index, List<PresetColumnState> states)
        {
            if (current.type == EventType.MouseDown && current.button == 0 && resizeRect.Contains(current.mousePosition))
            {
                resizingColumn = index;
                resizeStartX = current.mousePosition.x;
                resizeStartWidth = states[index].Width;
                current.Use();
                return;
            }

            if (current.type == EventType.MouseDown && current.button == 0 && headerRect.Contains(current.mousePosition))
            {
                draggingColumn = index;
                dragStartX = current.mousePosition.x;
                headerDragged = false;
                current.Use();
                return;
            }

            if (current.type == EventType.MouseDown && current.button == 1 && headerRect.Contains(current.mousePosition))
            {
                RemoveColumn(states[index]);
                current.Use();
                return;
            }

            if (current.type == EventType.MouseDrag && resizingColumn == index)
            {
                states[index].Width = Math.Max(60f, resizeStartWidth + current.mousePosition.x - resizeStartX);
                presetDirty = true;
                current.Use();
                return;
            }

            if (current.type == EventType.MouseDrag && draggingColumn == index)
            {
                headerDragged = Math.Abs(current.mousePosition.x - dragStartX) >= 5f;
                current.Use();
                return;
            }

            if (current.rawType == EventType.MouseUp && resizingColumn == index)
            {
                resizingColumn = -1;
                presetDirty = true;
                current.Use();
                return;
            }

            if (current.rawType == EventType.MouseUp && draggingColumn == index)
            {
                if (headerDragged)
                {
                    int target = FindColumnAt(states, current.mousePosition.x + tableScroll.x);
                    ReorderVisibleColumns(states, index, target);
                }
                else
                {
                    ToggleSort(states[index].ColumnId);
                }

                draggingColumn = -1;
                headerDragged = false;
                current.Use();
            }
        }

        private static int FindColumnAt(List<PresetColumnState> states, float contentX)
        {
            float x = 0f;
            for (int index = 0; index < states.Count; index++)
            {
                x += states[index].Width;
                if (contentX < x)
                {
                    return index;
                }
            }

            return Math.Max(0, states.Count - 1);
        }

        private void ReorderVisibleColumns(List<PresetColumnState> states, int from, int to)
        {
            if (from == to || from < 0 || to < 0 || from >= states.Count || to >= states.Count)
            {
                return;
            }

            PresetColumnState moved = states[from];
            states.RemoveAt(from);
            states.Insert(to, moved);

            int knownIndex = 0;
            for (int index = 0; index < workingPreset.Columns.Count; index++)
            {
                if (catalog.TryGet(workingPreset.Columns[index].ColumnId, out _))
                {
                    workingPreset.Columns[index] = states[knownIndex++];
                }
            }

            presetDirty = true;
        }

        private void DrawNameRows(Rect rect, List<MaterialRow> rows)
        {
            GUI.BeginGroup(rect);
            for (int index = 0; index < rows.Count; index++)
            {
                float y = index * RowHeight - tableScroll.y;
                if (y + RowHeight < 0f || y > rect.height)
                {
                    continue;
                }

                Rect rowRect = new Rect(0f, y, rect.width, RowHeight);
                DrawRowBackground(rowRect, index);
                Rect iconRect = new Rect(5f, y + 4f, 26f, 26f);
                Widgets.ThingIcon(iconRect, rows[index].Material);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(37f, y, rect.width - 42f, RowHeight), rows[index].Material.LabelCap);
                Text.Anchor = TextAnchor.UpperLeft;
                TooltipHandler.TipRegion(rowRect, BuildMaterialTip(rows[index].Material));
            }

            GUI.EndGroup();
        }

        private void DrawDataRows(List<MaterialRow> rows, List<PresetColumnState> states, float contentWidth)
        {
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                float y = rowIndex * RowHeight;
                if (y + RowHeight < tableScroll.y || y > tableScroll.y + Verse.UI.screenHeight)
                {
                    continue;
                }

                DrawRowBackground(new Rect(0f, y, contentWidth, RowHeight), rowIndex);
                float x = 0f;
                foreach (PresetColumnState state in states)
                {
                    if (!catalog.TryGet(state.ColumnId, out MaterialColumnDefinition column))
                    {
                        continue;
                    }

                    Rect cellRect = new Rect(x, y, state.Width, RowHeight);
                    MaterialCellValue value = GetCell(rows[rowIndex], state.ColumnId);
                    Color previousColor = GUI.color;
                    if (value.HasValue && !value.HasExplicitValue)
                    {
                        GUI.color = Color.gray;
                    }

                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(cellRect.ContractedBy(4f), value.HasValue ? value.DisplayValue : "-");
                    Text.Anchor = TextAnchor.UpperLeft;
                    GUI.color = previousColor;
                    TooltipHandler.TipRegion(cellRect, BuildCellTip(column, value));
                    x += state.Width;
                }
            }
        }

        private static void DrawRowBackground(Rect rect, int index)
        {
            if ((index & 1) == 1)
            {
                Widgets.DrawBoxSolid(rect, new Color(1f, 1f, 1f, 0.035f));
            }

            Widgets.DrawHighlightIfMouseover(rect);
        }

        private List<MaterialRow> FilterAndSortRows()
        {
            string term = search.Trim();
            List<MaterialRow> rows = dataset.Rows.Where(row =>
                    MatchesSearch(row.Material, term) &&
                    MatchesGroup(row.Material, MaterialNumbersMod.Settings.GroupFilter) &&
                    MatchesAvailability(row.Material))
                .ToList();
            rows.Sort(CompareRows);
            return rows;
        }

        private static bool MatchesSearch(ThingDef material, string term)
        {
            if (term.Length == 0)
            {
                return true;
            }

            return Contains(material.LabelCap.ToString(), term) ||
                   Contains(material.defName, term) ||
                   Contains(material.modContentPack?.Name, term);
        }

        private static bool MatchesGroup(ThingDef material, MaterialGroupFilter filter)
        {
            if (filter == MaterialGroupFilter.All)
            {
                return true;
            }

            MaterialGroup group = MaterialGroupClassifier.Classify(
                material.stuffProps?.categories?.Where(category => category != null).Select(category => category.defName),
                material.thingCategories?.Where(category => category != null).Select(category => category.defName),
                material.defName);
            switch (filter)
            {
                case MaterialGroupFilter.Metal:
                    return group == MaterialGroup.Metal;
                case MaterialGroupFilter.Stone:
                    return group == MaterialGroup.Stone;
                case MaterialGroupFilter.Wood:
                    return group == MaterialGroup.Wood;
                case MaterialGroupFilter.TextileLeather:
                    return group == MaterialGroup.TextileLeather;
                case MaterialGroupFilter.PlasticGlass:
                    return group == MaterialGroup.PlasticGlass;
                case MaterialGroupFilter.Other:
                    return group == MaterialGroup.Other;
                default:
                    return group != MaterialGroup.Other;
            }
        }

        private bool MatchesAvailability(ThingDef material)
        {
            switch (MaterialNumbersMod.Settings.AvailabilityMode)
            {
                case MaterialAvailabilityMode.CurrentMap:
                    return mapCounts.GetAll(material) > 0;
                case MaterialAvailabilityMode.CurrentStorage:
                    return mapCounts.GetStored(material) > 0;
                default:
                    return true;
            }
        }

        private int CompareRows(MaterialRow left, MaterialRow right)
        {
            string sortColumnId = workingPreset.SortColumnId;
            if (string.IsNullOrEmpty(sortColumnId))
            {
                return CompareMaterialNames(left.Material, right.Material);
            }

            MaterialCellValue leftValue = GetCell(left, sortColumnId);
            MaterialCellValue rightValue = GetCell(right, sortColumnId);
            if (leftValue.HasValue != rightValue.HasValue)
            {
                return leftValue.HasValue ? -1 : 1;
            }

            int comparison = MaterialCellValueComparer.Compare(leftValue, rightValue, workingPreset.SortAscending);

            return comparison != 0 ? comparison : CompareMaterialNames(left.Material, right.Material);
        }

        private static int CompareMaterialNames(ThingDef left, ThingDef right)
        {
            return string.Compare(left.LabelCap.ToString(), right.LabelCap.ToString(), StringComparison.CurrentCultureIgnoreCase);
        }

        private MaterialCellValue GetCell(MaterialRow row, string columnId)
        {
            if (columnId == MaterialColumnIds.Amount)
            {
                int amount = mapCounts.GetAll(row.Material);
                return new MaterialCellValue(amount, amount.ToString(), true);
            }

            return row.GetCell(columnId);
        }

        private List<PresetColumnState> VisibleColumnStates()
        {
            if (workingPreset == null)
            {
                return new List<PresetColumnState>();
            }

            return workingPreset.Columns
                .Where(state => state != null && catalog.TryGet(state.ColumnId, out _))
                .ToList();
        }

        private void ToggleSort(string columnId)
        {
            if (workingPreset.SortColumnId == columnId)
            {
                workingPreset.SortAscending = !workingPreset.SortAscending;
            }
            else
            {
                workingPreset.SortColumnId = columnId;
                workingPreset.SortAscending = false;
            }

            presetDirty = true;
        }

        private void RemoveColumn(PresetColumnState state)
        {
            workingPreset.Columns.Remove(state);
            if (workingPreset.SortColumnId == state.ColumnId)
            {
                workingPreset.SortColumnId = null;
            }

            presetDirty = true;
        }

        private void OpenColumnPicker()
        {
            Find.WindowStack.Add(new Dialog_ColumnPicker(
                catalog.Columns,
                VisibleColumnStates().Select(state => state.ColumnId),
                ApplyColumnSelection));
        }

        private void ApplyColumnSelection(HashSet<string> selectedIds)
        {
            var knownIds = new HashSet<string>(catalog.Columns.Select(column => column.Id), StringComparer.Ordinal);
            IReadOnlyList<string> reconciledIds = ColumnSelectionReconciler.Reconcile(
                workingPreset.Columns.Select(state => state.ColumnId),
                knownIds,
                selectedIds,
                catalog.Columns.Select(column => column.Id));
            var existingStates = workingPreset.Columns.ToDictionary(state => state.ColumnId, StringComparer.Ordinal);
            workingPreset.Columns = reconciledIds.Select(id =>
            {
                if (existingStates.TryGetValue(id, out PresetColumnState state))
                {
                    return state;
                }

                catalog.TryGet(id, out MaterialColumnDefinition column);
                return new PresetColumnState(id, column?.DefaultWidth ?? 100f);
            }).ToList();

            if (!string.IsNullOrEmpty(workingPreset.SortColumnId) && !selectedIds.Contains(workingPreset.SortColumnId))
            {
                workingPreset.SortColumnId = null;
            }

            presetDirty = true;
        }

        private void OpenPresetMenu()
        {
            var options = new List<FloatMenuOption>();
            foreach (MaterialViewPreset preset in MaterialNumbersMod.GetAllPresets())
            {
                MaterialViewPreset captured = preset;
                string marker = preset.Id == MaterialNumbersMod.Settings.CurrentPresetId ? "[x] " : string.Empty;
                options.Add(new FloatMenuOption(marker + preset.Name, () => SelectPreset(captured.Id)));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void SelectPreset(string id)
        {
            if (id == MaterialNumbersMod.Settings.CurrentPresetId)
            {
                return;
            }

            if (presetDirty)
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "MaterialNumbers.Confirm.DiscardChanges".Translate(),
                    () => LoadPreset(id)));
                return;
            }

            LoadPreset(id);
        }

        private void SaveCurrentPreset()
        {
            if (workingPreset.IsBuiltIn)
            {
                OpenSaveAsDialog();
                return;
            }

            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "MaterialNumbers.Confirm.Overwrite".Translate(workingPreset.Name),
                OverwriteCurrentPreset));
        }

        private void OverwriteCurrentPreset()
        {
            int index = MaterialNumbersMod.Settings.UserPresets.FindIndex(preset => preset.Id == workingPreset.Id);
            if (index < 0)
            {
                OpenSaveAsDialog();
                return;
            }

            MaterialNumbersMod.Settings.UserPresets[index] = workingPreset.Clone(workingPreset.Id, workingPreset.Name, false);
            presetDirty = false;
            MaterialNumbersMod.SaveSettings();
        }

        private void OpenPresetActions()
        {
            var options = new List<FloatMenuOption>
            {
                new FloatMenuOption("MaterialNumbers.Action.SaveAs".Translate(), OpenSaveAsDialog),
                new FloatMenuOption("MaterialNumbers.Action.SetDefault".Translate(), SetCurrentAsDefault),
                new FloatMenuOption("MaterialNumbers.Action.Restore".Translate(), RestoreCurrentPreset)
            };

            if (!workingPreset.IsBuiltIn)
            {
                options.Add(new FloatMenuOption("MaterialNumbers.Action.Rename".Translate(), OpenRenameDialog));
                options.Add(new FloatMenuOption("MaterialNumbers.Action.Delete".Translate(), ConfirmDeleteCurrentPreset));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void OpenSaveAsDialog()
        {
            Find.WindowStack.Add(new Dialog_PresetName(
                "MaterialNumbers.Dialog.SaveAs".Translate(),
                workingPreset.Name,
                SaveAs));
        }

        private void SaveAs(string name)
        {
            string uniqueName = MakeUniquePresetName(name, null);
            MaterialViewPreset saved = workingPreset.Clone(Guid.NewGuid().ToString("N"), uniqueName, false);
            MaterialNumbersMod.Settings.UserPresets.Add(saved);
            MaterialNumbersMod.Settings.CurrentPresetId = saved.Id;
            workingPreset = saved.Clone();
            presetDirty = false;
            MaterialNumbersMod.SaveSettings();
        }

        private void OpenRenameDialog()
        {
            Find.WindowStack.Add(new Dialog_PresetName(
                "MaterialNumbers.Dialog.Rename".Translate(),
                workingPreset.Name,
                RenameCurrentPreset));
        }

        private void RenameCurrentPreset(string name)
        {
            MaterialViewPreset saved = MaterialNumbersMod.Settings.UserPresets.FirstOrDefault(preset => preset.Id == workingPreset.Id);
            if (saved == null)
            {
                return;
            }

            string uniqueName = MakeUniquePresetName(name, saved.Id);
            saved.Name = uniqueName;
            workingPreset.Name = uniqueName;
            MaterialNumbersMod.SaveSettings();
        }

        private static string MakeUniquePresetName(string requested, string currentId)
        {
            string candidate = requested.Trim();
            var names = new HashSet<string>(
                MaterialNumbersMod.GetAllPresets()
                    .Where(preset => preset.Id != currentId)
                    .Select(preset => preset.Name),
                StringComparer.CurrentCultureIgnoreCase);
            if (!names.Contains(candidate))
            {
                return candidate;
            }

            int suffix = 2;
            while (names.Contains(candidate + " " + suffix))
            {
                suffix++;
            }

            return candidate + " " + suffix;
        }

        private void ConfirmDeleteCurrentPreset()
        {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                "MaterialNumbers.Confirm.Delete".Translate(workingPreset.Name),
                DeleteCurrentPreset));
        }

        private void DeleteCurrentPreset()
        {
            string deletedId = workingPreset.Id;
            MaterialNumbersMod.Settings.UserPresets.RemoveAll(preset => preset.Id == deletedId);
            if (MaterialNumbersMod.Settings.DefaultPresetId == deletedId)
            {
                MaterialNumbersMod.Settings.DefaultPresetId = BuiltinPresetFactory.OverviewId;
            }

            LoadPreset(MaterialNumbersMod.Settings.DefaultPresetId);
        }

        private void SetCurrentAsDefault()
        {
            MaterialNumbersMod.Settings.DefaultPresetId = workingPreset.Id;
            MaterialNumbersMod.SaveSettings();
            Messages.Message("MaterialNumbers.Message.DefaultSet".Translate(workingPreset.Name), MessageTypeDefOf.TaskCompletion, false);
        }

        private void RestoreCurrentPreset()
        {
            LoadPreset(workingPreset.Id);
        }

        private string GroupButtonLabel()
        {
            return GroupLabel(MaterialNumbersMod.Settings.GroupFilter);
        }

        private void OpenGroupMenu()
        {
            var options = new List<FloatMenuOption>();
            foreach (MaterialGroupFilter filter in Enum.GetValues(typeof(MaterialGroupFilter)))
            {
                MaterialGroupFilter captured = filter;
                bool selected = MaterialNumbersMod.Settings.GroupFilter == filter;
                options.Add(new FloatMenuOption((selected ? "[x] " : "[ ] ") + GroupLabel(filter), () => SelectGroupFilter(captured)));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static void SelectGroupFilter(MaterialGroupFilter filter)
        {
            MaterialNumbersMod.Settings.GroupFilter = filter;
            MaterialNumbersMod.SaveSettings();
        }

        private static string GroupLabel(MaterialGroupFilter filter)
        {
            switch (filter)
            {
                case MaterialGroupFilter.All:
                    return "MaterialNumbers.Filter.Group.All".Translate();
                case MaterialGroupFilter.Metal:
                    return "MaterialNumbers.Filter.Group.Metal".Translate();
                case MaterialGroupFilter.Stone:
                    return "MaterialNumbers.Filter.Group.Stone".Translate();
                case MaterialGroupFilter.Wood:
                    return "MaterialNumbers.Filter.Group.Wood".Translate();
                case MaterialGroupFilter.TextileLeather:
                    return "MaterialNumbers.Filter.Group.TextileLeather".Translate();
                case MaterialGroupFilter.PlasticGlass:
                    return "MaterialNumbers.Filter.Group.PlasticGlass".Translate();
                case MaterialGroupFilter.Other:
                    return "MaterialNumbers.Filter.Group.Other".Translate();
                default:
                    return "MaterialNumbers.Filter.Group.Common".Translate();
            }
        }

        private void RefreshMapCounts(bool force)
        {
            if (!force && Time.frameCount - lastCountRefreshFrame < 120)
            {
                return;
            }

            mapCounts.Refresh(Find.CurrentMap);
            lastCountRefreshFrame = Time.frameCount;
        }

        private static bool Contains(string text, string term)
        {
            return text?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string BuildColumnTip(MaterialColumnDefinition column)
        {
            return column.Description + "\n\n" +
                   "MaterialNumbers.Tooltip.Source".Translate(column.Source) + "\n" +
                   column.Id + "\n\n" +
                   "MaterialNumbers.Tooltip.HeaderActions".Translate();
        }

        private static string BuildCellTip(MaterialColumnDefinition column, MaterialCellValue value)
        {
            string status = !value.HasValue
                ? "MaterialNumbers.Value.Missing".Translate()
                : value.HasExplicitValue
                    ? "MaterialNumbers.Value.Explicit".Translate()
                    : value.Detail ?? "MaterialNumbers.Value.Neutral".Translate();
            return column.Description + "\n\n" +
                   "MaterialNumbers.Tooltip.Value".Translate(value.HasValue ? value.DisplayValue : "-") + "\n" +
                   "MaterialNumbers.Tooltip.Status".Translate(status) + "\n" +
                   "MaterialNumbers.Tooltip.Source".Translate(column.Source);
        }

        private static string BuildMaterialTip(ThingDef material)
        {
            return material.description + "\n\n" +
                   material.defName + "\n" +
                   "MaterialNumbers.Tooltip.Source".Translate(material.modContentPack?.Name ?? "RimWorld");
        }
    }
}
