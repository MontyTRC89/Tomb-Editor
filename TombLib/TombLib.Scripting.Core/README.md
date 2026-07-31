# TombLib.Scripting.Core

Purpose: UI-free scripting contracts, DTOs, and reusable abstractions.

Use this project for:
- diagnostics, edits, ranges, and other transportable text models
- completion, hover, signature, definition, reference, and semantic token DTOs
- provider contracts that do not require AvalonEdit, WPF, WinForms, or DarkUI

Do not place here:
- editor controls, renderers, popups, or host workflow code
- language asset files or source-of-truth syntax/specification resources

Folder direction:
- keep folders responsibility-based
- prefer feature-neutral names over language-specific ones when the contract is shared
