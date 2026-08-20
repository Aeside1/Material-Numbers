# Material Numbers

[English](README.md) | [简体中文](README.zh-CN.md)

Material Numbers is a RimWorld 1.6 material comparison tab with configurable columns and global named presets.

## Features

- Discovers every loaded `ThingDef` where `IsStuff` is true.
- Builds columns from material base stats, factors, and offsets.
- Marks base, factor, and offset columns with `=`, `×`, and `+`, respectively, and formats every factor as a percentage.
- Safely discovers direct `IEnumerable<StatModifier>` and `IDictionary<StatDef, float>` containers on mod extensions.
- Recognizes `SurvivalToolsLite.StuffPropsTool.toolStatFactors` without a hard dependency and groups those columns as tool stats.
- Supports searchable columns, horizontal and vertical scrolling, sorting, column removal, drag reordering, and width adjustment.
- Filters by compact material groups, current map availability, or valid storage. The default Common materials view excludes unclassified stuff-like definitions; All definitions remains available for troubleshooting unusual mods.
- Provides six built-in layouts for overview, construction, armor/apparel, gathering/production, comfort/rest, and trade/stock, plus global user presets.

Missing factor and offset values use their neutral values (`100%` and `0`) and are shown in gray. Other missing values remain blank (`-`). Column IDs from temporarily absent mods remain in saved presets and become visible again when their source mod returns.

## Build

```powershell
& "G:/Github/Game/Rimworld/_tooling/scripts/Build-RimWorldMod.ps1" `
  -Project "G:/Github/Game/Rimworld/Material-Numbers/Source/MaterialNumbers/MaterialNumbers.csproj" `
  -Configuration Release
```

The build writes `MaterialNumbers.dll` to `1.6/Assemblies`. The project does not require Harmony and does not read private game assemblies during the build.

## Extension API

Other mods can implement `IMaterialColumnProvider` and call `MaterialNumbersRegistry.Register(provider)`. Providers describe columns and value readers; Material Numbers owns discovery caching, layout state, and rendering.

## Scope

- RimWorld 1.6 only.
- Presets are global across saves.
- No preset import/export in the first release.
- No hard dependency on Stuff List, Numbers, Survival Tools Lite, HSK, or Harmony.
