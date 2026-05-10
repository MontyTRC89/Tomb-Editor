# LuaLS / Scripting Studio Feature Plan

Date: 2026-05-10.

## Goal

Add these Lua editor features in TombIDE Scripting Studio:

- VSCode-like back / forward navigation across document, caret, and selection state.
- A dedicated diagnostics tool window for the active Lua document, with severity toggle filters.
- LuaLS Find References.
- LuaLS Rename Symbol.
- LuaLS document reformatting, exposed through the existing document cleanup command.

This plan is written as an implementation handoff for the next agent. It is intentionally concrete and biased toward the smallest set of architectural changes that fit the current codebase.

## Verified Starting Point

- [LuaEditor](TombLib/TombLib.Scripting.Lua/LuaEditor.cs) already owns Lua-specific editor UX and currently wires completion, hover, signature help, and definition navigation.
- [LuaEditor.Navigation](TombLib/TombLib.Scripting.Lua/LuaEditor.Navigation.cs) already routes `F12` and `Ctrl+Click` into the definition-navigation controller, so definition jumps already have a single owner on the editor side.
- [LuaStudio.Intellisense](TombIDE/TombIDE.ScriptingStudio/LuaStudio.Intellisense.cs) is the current cross-document bridge. It opens Lua documents in the provider, applies diagnostics and semantic tokens, and performs the actual open-file jump for definitions.
- [ILuaIntellisenseProvider](TombLib/TombLib.Scripting.Lua/Services/ILuaIntellisenseProvider.cs) currently exposes only completion, hover, definition, signature help, diagnostics, semantic tokens, and document lifecycle. References, rename, and formatting are not in the contract yet.
- [StudioBase](TombIDE/TombIDE.ScriptingStudio/Bases/StudioBase.cs) already owns command routing, dockable tool-window visibility, and the shared document command pipeline.
- [UICommand](TombIDE/TombIDE.ScriptingStudio/UI/UICommand.cs), [UIKeys](TombIDE/TombIDE.ScriptingStudio/UI/UIKeys.cs), and the Lua menu/context XML files are the existing command-registration surface.
- [SearchResults](TombIDE/TombIDE.ScriptingStudio/ToolWindows/SearchResults.cs) is a docked results window already, but it is tightly coupled to `FindReplaceEventArgs` and regex-match navigation.
- [TextEditorBase](TombLib/TombLib.Scripting/Bases/TextEditorBase.cs) already raises `StatusChanged` on caret and selection changes, and stores diagnostics locally through `SetDiagnostics(...)`.
- [TextEditorDiagnostic](TombLib/TombLib.Scripting/Objects/TextEditorDiagnostic.cs) currently carries severity plus start/end offsets only. That is enough to build a current-document diagnostics list, but line/column text will need to be derived in the host UI.
- [TombIDE.ScriptingStudio.csproj](TombIDE/TombIDE.ScriptingStudio/TombIDE.ScriptingStudio.csproj) already has `UseWPF=true`, references `DarkUI.WPF`, and uses `ElementHost` elsewhere, so new WPF panes can be hosted inside existing `DarkToolWindow` shells.
- The visible `Reindent` command is currently routed through [StudioBase.HandleDocumentCommands](TombIDE/TombIDE.ScriptingStudio/Bases/StudioBase.cs) to [TextEditorBase.TidyCode](TombLib/TombLib.Scripting/Bases/TextEditorBase.cs), which only trims ending whitespace. Lua reformatting must override this path instead of reusing the current base behavior.

## Recommended Decisions

- Keep cross-document navigation history in the Scripting Studio host, not in `LuaEditor`. The editor knows about caret and selection, but only the studio knows about tabs, file opening, tool-window jumps, and document lifetime.
- Add new LuaLS protocol features inside `TombIDE.ScriptingStudio/Services/LuaIntellisense` using the existing feature-sliced layout. Do not collapse them into one generic parser file.
- Build new front-end panes as `DarkToolWindow` plus `ElementHost` plus a DarkUI.WPF `UserControl`. That gives a WPF front-end while preserving the existing WinForms dock host.
- Do not force `SearchResults` to become a generic results shell in the first pass. Extract only the shared jump helper if needed. A dedicated references window is lower risk than untangling the existing find/replace tree.
- Reuse the existing `InputBoxWindow` shape for rename unless the first iteration truly needs rename preview. A dedicated rename-preview dialog can wait.
- Keep the `UICommand.Reindent` identifier for now and rename only the visible localization strings to `Reformat`. That avoids broad command-churn across shared menus and settings.
- Treat diagnostics filter buttons as `Errors`, `Warnings`, and `Messages`, where `Information` and `Hint` roll up into `Messages`. Internally, keep the original severity values intact.
- Keep format-on-save out of the first Lua slice. The explicit document command should land first so edit-application, caret preservation, and latency can be validated before wiring it into save flows.

## Phase 1. Shared Location And Navigation Infrastructure

### Objective

Create a reusable location model and navigation-history service that every Lua jump can use.

### Why First

Definition jumps already exist, and diagnostics and references both need the same open-file-and-position behavior. Landing this first avoids three separate navigation implementations.

### Work Items

- Add new commands and shortcuts:
  - `NavigateBack` mapped to `Alt+Left`.
  - `NavigateForward` mapped to `Alt+Right`.
  - `FindReferences` mapped to `Shift+F12`.
  - `RenameSymbol` mapped to `F2` when a text editor has focus.
- Update:
  - [UICommand](TombIDE/TombIDE.ScriptingStudio/UI/UICommand.cs)
  - [UIKeys](TombIDE/TombIDE.ScriptingStudio/UI/UIKeys.cs)
  - [Lua document menu](TombIDE/TombIDE.ScriptingStudio/UI/DocumentModePresets/MenuStrips/Lua.xml)
  - [Lua context menu](TombIDE/TombIDE.ScriptingStudio/UI/DocumentModePresets/ContextMenus/Lua.xml)
  - [Lua studio menu](TombIDE/TombIDE.ScriptingStudio/UI/StudioModePresets/MenuStrips/Lua.xml)
  - [EN localization](TombIDE/TombIDE.Shared/Resources/Localization/EN/TombIDE.xml)
  - [PL localization](TombIDE/TombIDE.Shared/Resources/Localization/PL/TombIDE.xml)
- Introduce a small studio-owned location model, for example:
  - `EditorNavigationLocation` with file path, caret offset, selection start, selection length, and optional preferred line for scroll restoration.
- Introduce a studio-owned history service, for example:
  - `EditorNavigationHistoryService` with back stack, forward stack, and a suppression scope for programmatic jumps.
- Subscribe to `IEditorControl.StatusChanged` in the studio host for Lua editors and record debounced user movement when it represents a meaningful location change.
- Add a single host-side navigation method in `LuaStudio`, for example `NavigateToLocation(EditorNavigationLocation location, NavigationOrigin origin)`, and route all Lua jumps through it.
- Change [LuaStudio.NavigateToDefinition](TombIDE/TombIDE.ScriptingStudio/LuaStudio.Intellisense.cs) so definition navigation pushes the current location before opening the target location.
- Extract the target-file jump logic from [SearchResults](TombIDE/TombIDE.ScriptingStudio/ToolWindows/SearchResults.cs) into a shared helper or callback so later tool windows can reuse it.

### Done When

- `Alt+Left` and `Alt+Right` work after `F12`, `Ctrl+Click`, and existing Search Results jumps.
- Programmatic jumps do not create duplicate history entries.
- Same-file caret and selection movement can be recorded without polluting the history on every cursor tick.

### Validation

- Focused build: `dotnet build .\TombIDE\TombIDE.ScriptingStudio\TombIDE.ScriptingStudio.csproj -c Debug`
- Manual check:
  - Jump to a definition in another file.
  - Jump back.
  - Jump forward.
  - Select text, move caret materially, and verify history behaves predictably.

## Phase 2. Active-Document Diagnostics Tool Window

### Objective

Add a dedicated diagnostics pane for the current Lua document, with three severity toggle buttons and jump-to-diagnostic behavior.

### Recommended UI Shape

- New `DarkToolWindow` hosted in the dock panel.
- Inside it, host a DarkUI.WPF `UserControl` through `ElementHost`.
- Use DarkUI.WPF `ToggleButton` controls for `Errors`, `Warnings`, and `Messages`.
- Use a WPF `DataGrid` or `ListBox` for rows. Prefer `DataGrid` because the row shape is tabular: severity, line, column, message.

### Work Items

- Add a new public tool-window field on `LuaStudio`, for example `LuaDiagnostics`, so it participates in the normal dock-layout serialization and View menu handling.
- Create new files under `TombIDE.ScriptingStudio`, for example:
  - `ToolWindows/LuaDiagnostics.cs`
  - `Views/LuaDiagnosticsView.xaml`
  - `Views/LuaDiagnosticsView.xaml.cs`
  - `ViewModels/LuaDiagnosticsViewModel.cs`
  - `Objects/LuaDiagnosticListItem.cs`
- Populate the view model from the active Lua editor only. The source of truth should remain `ILuaIntellisenseProvider.GetDiagnostics(filePath)` plus the active editor document for line/column/message projection.
- Project each `TextEditorDiagnostic` into a row model with:
  - severity
  - line
  - column
  - message
  - start offset
  - end offset
- Refresh the pane when:
  - the selected tab changes
  - diagnostics update for the active file
  - the current Lua editor changes locally
- On local edit, clear or mark pending diagnostics for the active file immediately instead of displaying known-stale results until the delayed provider refresh arrives.
- Double-click or press `Enter` on a row to navigate through the shared navigation service from Phase 1.
- Add a View menu entry and default bottom-dock placement near Search Results.

### Done When

- The pane always shows diagnostics for the active Lua document only.
- `Errors`, `Warnings`, and `Messages` toggles filter rows live.
- Jumping from the diagnostics list records history correctly.
- Switching to a non-Lua tab clears or disables the pane instead of leaving stale Lua results on screen.

### Validation

- Focused build: `dotnet build .\TombIDE\TombIDE.ScriptingStudio\TombIDE.ScriptingStudio.csproj -c Debug`
- Manual check:
  - Open a Lua file with multiple diagnostics.
  - Toggle each severity filter.
  - Edit the document and verify stale rows clear quickly.
  - Double-click a row and jump back with `Alt+Left`.

## Phase 3. LuaLS Find References

### Objective

Add LuaLS references requests and surface the results in a dedicated docked results pane.

### Recommended UI Shape

- Use a dedicated references tool window instead of reusing the current Search Results tool window.
- Keep the shell pattern consistent with diagnostics: `DarkToolWindow` plus `ElementHost` plus WPF `UserControl`.
- Group results by file and show line/column plus preview text.

### Work Items

- Extend [ILuaIntellisenseProvider](TombLib/TombLib.Scripting.Lua/Services/ILuaIntellisenseProvider.cs) with a new async references API.
- Add new DTOs under `TombLib.Scripting.Lua/Objects`, for example:
  - `LuaReferenceLocation`
  - `LuaDocumentRange`
- Add provider support inside `TombIDE.ScriptingStudio/Services/LuaIntellisense`:
  - new request method in the provider partials
  - new response parser under a new `References/` folder or under `Navigation/` if kept small
  - capability detection during initialization
- Add a studio command handler in `LuaStudio` for `Shift+F12` and context-menu invocation.
- Create a new results pane, for example:
  - `ToolWindows/LuaReferencesResults.cs`
  - `Views/LuaReferencesResultsView.xaml`
  - `ViewModels/LuaReferencesResultsViewModel.cs`
- Model the rows with enough information to jump directly:
  - file path
  - line
  - column
  - preview text
  - start/end offsets where available
- Prefer file grouping plus count headers so the result list stays readable in multi-file workspaces.
- Route all activation through the Phase 1 navigation service.
- If LuaLS does not advertise references support, disable the command cleanly instead of failing at runtime.

### Done When

- `Shift+F12` from a valid symbol opens a references pane and groups matches by file.
- Double-clicking a result jumps to the location and back/forward navigation works afterward.
- Re-running references replaces the previous Lua references result set cleanly.

### Validation

- Focused build: `dotnet build .\TombIDE\TombIDE.ScriptingStudio\TombIDE.ScriptingStudio.csproj -c Debug`
- Targeted test pass if provider parser changes add or update testable seams under `TombLib.Test`.
- Manual check:
  - local variable references in one file
  - shared symbol references across multiple files
  - no-result case

## Phase 4. LuaLS Rename Symbol

### Objective

Add symbol rename via LuaLS and apply the returned workspace edits safely across open and unopened Lua files.

### Recommended UI Shape

- Start with the existing [InputBoxWindow](TombLib/TombLib.Forms/Views/InputBoxWindow.xaml) visual pattern.
- Reuse `InputBoxWindowViewModel` if the first slice only needs title, label, and new name text.
- Only create a dedicated rename dialog if a preview list or extra validation text becomes necessary.

### Work Items

- Extend [ILuaIntellisenseProvider](TombLib/TombLib.Scripting.Lua/Services/ILuaIntellisenseProvider.cs) with a rename API.
- Add workspace-edit DTOs, for example:
  - `LuaWorkspaceEdit`
  - `LuaDocumentEdit`
  - `LuaTextEdit`
- Add LuaLS rename request handling under `TombIDE.ScriptingStudio/Services/LuaIntellisense`:
  - request method
  - parser for `WorkspaceEdit`
  - capability detection
- Add a studio-owned edit applier, for example `LuaWorkspaceEditApplier`, that:
  - applies edits in descending offset order per file
  - uses open editors when available
  - opens files through `EditorTabControl` when needed
  - leaves affected files dirty instead of silently saving them
  - preserves the previously selected tab when practical
- Add a host-side command handler for `F2` when a Lua editor is focused.
- Seed the rename box from the current selection or the identifier at the caret.
- If the server returns no edits, show a non-blocking message and leave the editor untouched.
- If the server refuses the rename, surface the error cleanly through DarkUI message UI.
- After edits apply, refresh provider-tracked document state so diagnostics, semantic tokens, references, and future rename requests remain aligned.
- Keep file-explorer `F2` rename behavior intact when the editor does not have focus.

### Done When

- `F2` on a renameable Lua symbol opens a DarkUI.WPF-style prompt and applies changes across the workspace.
- Open tabs update in place and unopened files are updated safely.
- Back/forward history still works after the rename operation.
- Rename failures and unsupported cases are visible and non-destructive.

### Validation

- Focused build: `dotnet build .\TombIDE\TombIDE.ScriptingStudio\TombIDE.ScriptingStudio.csproj -c Debug`
- Manual check:
  - rename a local variable in one file
  - rename a shared symbol across files
  - cancel rename
  - invalid or unsupported rename

## Phase 5. LuaLS Reformat Command

### Objective

Replace the current Lua `Reindent` behavior with provider-backed Lua formatting while keeping the existing shared command slot.

### Work Items

- Extend [ILuaIntellisenseProvider](TombLib/TombLib.Scripting.Lua/Services/ILuaIntellisenseProvider.cs) with a document-formatting API.
- Add formatting request support under `TombIDE.ScriptingStudio/Services/LuaIntellisense`:
  - request method
  - parser for returned text edits
  - capability detection
- Reuse the workspace-edit application machinery from Phase 4 for single-document formatting edits.
- Override the Lua document-command path in `LuaStudio` so `UICommand.Reindent` does this instead:
  - when the current editor is `LuaEditor` and the provider supports formatting, request formatting edits and apply them
  - when the provider is unavailable, disable the command or show a warning instead of silently falling back to trim-only behavior
- Change visible menu text from `Reindent` to `Reformat` in localization files. Keep the existing command and configuration identifiers for now.
- Decide explicitly whether the existing visible `ReindentOnSave` option should also be renamed to `Reformat on Save`. Recommended first slice: rename the visible text only if the save path is still intentionally trim-only across non-Lua editors; otherwise defer the label change to avoid implying Lua format-on-save before it exists.
- Keep range formatting out of scope for the first pass.

### Done When

- The Lua document menu command reformats a Lua document through LuaLS.
- Caret and selection are preserved well enough for normal editing.
- The visible UI says `Reformat` instead of `Reindent`.
- Non-Lua editors keep their current behavior.

### Validation

- Focused build: `dotnet build .\TombIDE\TombIDE.ScriptingStudio\TombIDE.ScriptingStudio.csproj -c Debug`
- Manual check:
  - reformat a messy Lua file
  - verify edits are applied once
  - verify the editor state remains usable afterward

## Phase 6. Polish, Layout, And Regression Pass

### Work Items

- Add default layout entries for the new diagnostics and references panes if they should participate in restored layouts from first launch.
- Verify View menu checked states and serialization keys for every new tool window.
- Add or update documentation notes in the LuaLS service README if ownership shifts or new folders are introduced.
- Update tests in `TombLib.Test` for any provider/parser contracts that change.
- Re-run a full manual regression pass over:
  - completion
  - hover
  - signature help
  - definition navigation
  - diagnostics refresh
  - semantic token stability after rename and reformat

### Validation

- Focused build: `dotnet build .\TombIDE\TombIDE.ScriptingStudio\TombIDE.ScriptingStudio.csproj -c Debug`
- Broader test pass: `dotnet test .\TombLib\TombLib.Test\TombLib.Test.csproj`

## Suggested File Touch Map

### Command And Menu Surface

- [TombIDE/TombIDE.ScriptingStudio/UI/UICommand.cs](TombIDE/TombIDE.ScriptingStudio/UI/UICommand.cs)
- [TombIDE/TombIDE.ScriptingStudio/UI/UIKeys.cs](TombIDE/TombIDE.ScriptingStudio/UI/UIKeys.cs)
- [TombIDE/TombIDE.ScriptingStudio/UI/DocumentModePresets/MenuStrips/Lua.xml](TombIDE/TombIDE.ScriptingStudio/UI/DocumentModePresets/MenuStrips/Lua.xml)
- [TombIDE/TombIDE.ScriptingStudio/UI/DocumentModePresets/ContextMenus/Lua.xml](TombIDE/TombIDE.ScriptingStudio/UI/DocumentModePresets/ContextMenus/Lua.xml)
- [TombIDE/TombIDE.ScriptingStudio/UI/StudioModePresets/MenuStrips/Lua.xml](TombIDE/TombIDE.ScriptingStudio/UI/StudioModePresets/MenuStrips/Lua.xml)
- [TombIDE/TombIDE.Shared/Resources/Localization/EN/TombIDE.xml](TombIDE/TombIDE.Shared/Resources/Localization/EN/TombIDE.xml)
- [TombIDE/TombIDE.Shared/Resources/Localization/PL/TombIDE.xml](TombIDE/TombIDE.Shared/Resources/Localization/PL/TombIDE.xml)

### Host / Studio Side

- [TombIDE/TombIDE.ScriptingStudio/LuaStudio.cs](TombIDE/TombIDE.ScriptingStudio/LuaStudio.cs)
- [TombIDE/TombIDE.ScriptingStudio/LuaStudio.Intellisense.cs](TombIDE/TombIDE.ScriptingStudio/LuaStudio.Intellisense.cs)
- [TombIDE/TombIDE.ScriptingStudio/Bases/StudioBase.cs](TombIDE/TombIDE.ScriptingStudio/Bases/StudioBase.cs)
- [TombIDE/TombIDE.ScriptingStudio/Controls/EditorTabControl.cs](TombIDE/TombIDE.ScriptingStudio/Controls/EditorTabControl.cs)
- [TombIDE/TombIDE.ScriptingStudio/ToolWindows/SearchResults.cs](TombIDE/TombIDE.ScriptingStudio/ToolWindows/SearchResults.cs)

### New Host Files Likely Needed

- `TombIDE/TombIDE.ScriptingStudio/Objects/EditorNavigationLocation.cs`
- `TombIDE/TombIDE.ScriptingStudio/Services/EditorNavigationHistoryService.cs`
- `TombIDE/TombIDE.ScriptingStudio/Services/LuaWorkspaceEditApplier.cs`
- `TombIDE/TombIDE.ScriptingStudio/ToolWindows/LuaDiagnostics.cs`
- `TombIDE/TombIDE.ScriptingStudio/ToolWindows/LuaReferencesResults.cs`
- `TombIDE/TombIDE.ScriptingStudio/Views/LuaDiagnosticsView.xaml`
- `TombIDE/TombIDE.ScriptingStudio/Views/LuaReferencesResultsView.xaml`
- `TombIDE/TombIDE.ScriptingStudio/ViewModels/LuaDiagnosticsViewModel.cs`
- `TombIDE/TombIDE.ScriptingStudio/ViewModels/LuaReferencesResultsViewModel.cs`

### Provider / Contract Side

- [TombLib/TombLib.Scripting.Lua/Services/ILuaIntellisenseProvider.cs](TombLib/TombLib.Scripting.Lua/Services/ILuaIntellisenseProvider.cs)
- `TombLib/TombLib.Scripting.Lua/Objects/LuaReferenceLocation.cs`
- `TombLib/TombLib.Scripting.Lua/Objects/LuaWorkspaceEdit.cs`
- `TombLib/TombLib.Scripting.Lua/Objects/LuaTextEdit.cs`
- [TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/Provider/LuaLanguageServerIntellisenseProvider.Requests.cs](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/Provider/LuaLanguageServerIntellisenseProvider.Requests.cs)
- New service folders under [TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense)

## Not In Scope For The First Slice

- Code actions and quick fixes.
- Rename preview with per-file accept/reject UI.
- Range formatting.
- Workspace-wide diagnostics across every Lua file.
- Generalizing every existing results surface into a single mega-control.

## Agent Execution Notes

- Implement one phase at a time and validate it before widening scope.
- Do not start rename-edit application before the shared navigation and location model exist.
- Do not change non-Lua editor behavior unless a shared command surface absolutely requires it.
- Prefer small, reversible refactors in the studio host over broad shell rewrites.
- If a WPF result pane adds too much friction inside the dock host, keep the `DarkToolWindow` shell and debug the `ElementHost` bridge first instead of falling back immediately to a second WinForms tree/grid implementation.
