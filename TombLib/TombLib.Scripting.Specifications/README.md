# TombLib.Scripting.Specifications

Purpose: scripting language definitions, normalized models, and legacy-input adapters.

Use this project for:
- structured loaders and normalized models for ClassicScript, GameFlow, and TRX inputs
- adapters that read legacy formats such as `.rdda`, `.resx`, XML, JSON, and schema files
- resource-path helpers and specification services consumed by language projects
- shared packaging of non-UI specification assets consumed by multiple language slices, the studio host, and tests

Do not place here:
- AvalonEdit, WPF, WinForms, or DarkUI code
- editor rendering or other presentation behavior

Folder direction:
- treat legacy assets as inputs, not as the internal working model
- keep assets under the language-owned top-level slices (`ClassicScript`, `GameFlow`, `TRX`) instead of under a cross-language `Resources` bucket
- keep feature folders aligned to language/specification slices rather than catch-all resource buckets