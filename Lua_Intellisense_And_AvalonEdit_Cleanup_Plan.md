# Lua IntelliSense And AvalonEdit Cleanup Plan

## Scope

This plan covers the Lua editor and the Lua language-server integration code that currently spans these areas:

- `TombLib/TombLib.Scripting.Lua/LuaEditor*.cs`
- `TombLib/TombLib.Scripting.Lua/Utils/*`
- `TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/*`
- `TombLib/TombLib.Scripting/Bases/TextEditorBase.cs`

Nearby editors such as `GameFlowEditor` and `ClassicScriptEditor` are comparison points only. They are not part of the first cleanup passes unless a small shared helper clearly benefits more than one editor.

## Goals

- Reduce the number of responsibilities owned by `LuaEditor` and its partials.
- Move stateful feature logic behind focused collaborators so timers, cancellation, popups, and request tokens have a single owner per feature.
- Replace unclear multi-value returns with named result types where the tuple shape hides meaning.
- Reorganize the LuaLS implementation into slices that are easy to discover by feature and by responsibility.
- Keep behavior stable while refactoring by adding tests before moving the highest-risk code.
- Follow `AGENTS.md`: partials only when the responsibility still belongs to the same type, prefer helper or service extraction when it does not, keep touched code nullable-aware, and avoid broad helper types with fuzzy ownership.

## Current Findings

### Editor-side hotspots

- `LuaEditor.Completion.cs` owns too much in one place: debounce scheduling, request orchestration, popup lifecycle, tooltip lifecycle, reflection-based AvalonEdit integration, width measurement, and query-offset calculation.
- `LuaEditor.SignatureHelp.cs` owns both presentation and workflow state: popup UI elements, request-in-flight state, refresh timer, positioning, and active-parameter formatting.
- `LuaEditor.Hover.cs` mixes request orchestration, fallback selection, and tooltip composition.
- `LuaEditor.Navigation.cs` duplicates parts of the editor request pipeline and shares state patterns with hover and completion.
- `LuaEditor.Intellisense.cs` still acts as a central wiring file for many unrelated concerns, so field ownership is difficult to follow even though the code is split into partials.

### LuaLS service-side hotspots

- `TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense` is currently a flat folder containing client transport, protocol, response parsing, workspace watching, document synchronization, semantic tokens, and provider orchestration.
- `LuaLanguageServerIntellisenseProvider` is still a large composition point with significant state and workflow spread across partials.
- Several feature-specific files already exist, but they are not grouped together on disk in a way that helps maintainers navigate by feature.

### Tuple and result-shape hotspots

The following methods are good candidates for named result types because their return values carry multiple meanings or mixed null and boolean state:

- `LuaIndentationStrategy.BuildEnterInsertion()`
- `LuaIndentationStrategy.NormalizeCompletionInsertion()`
- `LuaLanguageServerResponseParser.StripSnippetPlaceholders()`
- `LuaLanguageServerIntellisenseProvider.DecodeSemanticTokensResponse()`
- `LuaLanguageServerIntellisenseProvider.SynchronizeDocumentCoreAsync()`
- `LuaIntellisenseDocumentManager.GetSemanticTokensDeltaState()`

Small, local coordinate pairs such as line and column can remain tuples until a wider text-position abstraction is clearly justified. Not every two-value tuple needs to become a record.

## Cleanup Rules

- Keep `LuaEditor` as the public control and composition root. Do not move public editor behavior into a maze of tiny pass-through wrappers.
- Extract collaborators only when they own real state, workflow, or policy.
- Use `record struct` for small immutable result carriers and state snapshots.
- Prefer one top-level type per new file.
- Keep partial classes only where the split still represents one type with one coherent ownership boundary.
- Avoid introducing a generic cross-language framework before at least two editors need the same abstraction with similar complexity.

## Phase 0 - Safeguards And Baseline

Purpose: lock down behavior before structural work begins.

Work items:

1. Add focused tests for the current behavior that is easiest to break during extraction.
2. Cover completion request debounce, completion window refresh, completion tooltip resolve, hover vs diagnostic fallback, signature help open and refresh and dismiss, definition navigation, document sync, and semantic-token delta fallback.
3. Add a short maintainer note near the Lua editor and LuaLS service roots describing which file currently owns which feature.
4. Adopt the naming convention before creating new collaborators: use `Controller` for editor-side stateful owners, `Coordinator` for provider-side workflow owners, and keep `Service` for stable external integration boundaries or shared utilities.

Exit criteria:

- Refactor-sensitive behavior has direct tests.
- Maintainers can see the intended cleanup direction before code starts moving.

## Phase 1 - Replace Ambiguous Tuple Returns

Purpose: improve readability first with low-risk changes that clarify intent.

Recommended result types:

- `LuaEnterInsertionResult`
  - replaces the tuple returned by `BuildEnterInsertion()`
- `LuaCompletionNormalizationResult`
  - replaces the tuple returned by `NormalizeCompletionInsertion()`
- `LuaSnippetPlaceholderResult`
  - replaces the tuple returned by `StripSnippetPlaceholders()`
- `LuaSemanticTokensDecodeResult`
  - replaces the four-part tuple returned by `DecodeSemanticTokensResponse()`
- `LuaDocumentSynchronizationResult`
  - replaces the tuple returned by `SynchronizeDocumentCoreAsync()`
- `LuaSemanticTokensDeltaState`
  - replaces the tuple returned by `GetSemanticTokensDeltaState()`

Notes:

- Favor `internal readonly record struct` for these types.
- Keep the first pass narrow: rename only the returns that improve comprehension immediately.
- Do not convert short, obvious local tuples just for consistency.

Exit criteria:

- Mixed-state tuple returns are replaced with named result types.
- Method call sites read as domain concepts instead of index-based unpacking.

## Phase 2 - Extract Editor Feature Controllers

Purpose: remove intertwined state from `LuaEditor` without breaking its public role.

Target shape:

- `LuaEditor` remains the host control, public API surface, and event source.
- Each major editor feature gets one internal owner for its fields, timers, request tokens, and UI state.

Recommended extractions:

1. `LuaCompletionController`
   - Owns completion debounce scheduling.
   - Owns popup lifecycle.
   - Owns tooltip update scheduling and resolve flow.
   - Owns width measurement and query-offset calculation.
   - Owns AvalonEdit-specific reflection and non-activating window behavior.

2. `LuaSignatureHelpController`
   - Owns signature popup elements.
   - Owns refresh timer and pending-offset state.
   - Owns request-in-flight state.
   - Owns positioning and active-parameter rendering.

3. `LuaHoverController`
   - Owns hover request cancellation and tokens.
   - Owns hover-offset validation.
   - Owns hover vs diagnostic fallback policy.
   - Owns combined tooltip rendering decisions.

4. `LuaDefinitionNavigationController`
   - Owns F12 and Ctrl+Click navigation flow.
   - Owns definition request cancellation and freshness checks.

5. Optional shared internal helper after the first extractions only
   - `LuaEditorRequestGate` or `LuaEditorAsyncState`
   - Create this only if completion, hover, signature help, and navigation still duplicate freshness logic after the first controller pass.

Recommended folder direction:

```text
TombLib/TombLib.Scripting.Lua/
  Editor/
    LuaEditor.cs
    Completion/
    Hover/
    Navigation/
    SignatureHelp/
    SemanticHighlighting/
  Objects/
  Resources/
  Utils/
```

Notes:

- Do not extract theme application or semantic-highlighting attachment until the feature controllers settle. Those areas are smaller and less entangled.
- Keep event wiring in `LuaEditor` or one small composition file, not spread across every controller.

Exit criteria:

- No single feature stores its timers and request state directly on `LuaEditor` unless that state is trivial.
- The owner of each field is easy to infer from the file name.

## Phase 3 - Re-slice The LuaLS Implementation In TombIDE

Purpose: turn the flat LuaLS service folder into a discoverable feature-oriented structure.

Recommended folder direction:

```text
TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/
  Infrastructure/
    Client/
    Pathing/
    Startup/
    Workspace/
  Documents/
  Provider/
  Completion/
  Hover/
  Navigation/
  SignatureHelp/
  Diagnostics/
  SemanticTokens/
```

Suggested file moves:

- `Infrastructure/Client/`
  - `ILuaLanguageServerClient.cs`
  - `LuaLanguageServerClient.cs`
  - `LuaLanguageServerClient.Protocol.cs`
  - `LuaLanguageServerClient.Transport.cs`

- `Infrastructure/Pathing/`
  - `LuaLanguageServerPathHelper.cs`
  - `LuaLanguageServerLocator.cs`
  - `LuaLanguageServerSettingsFactory.cs`

- `Infrastructure/Startup/`
  - `LuaLanguageServerStartupFailure.cs`
  - `LuaProcessJobObject.cs`

- `Infrastructure/Workspace/`
  - `LuaWorkspaceFileWatcher.cs`
  - `LuaWorkspaceWatcherFailure.cs`

- `Documents/`
  - `LuaIntellisenseDocumentManager.cs`
  - `LuaDocumentSnapshot.cs`
  - `LuaDocumentSynchronizationRequest.cs`
  - `LuaDocumentRenameRequest.cs`
  - `LuaDocumentLineOffsets.cs`
  - `LuaIncrementalEditCalculator.cs`
  - `LuaTextDocumentSyncKind.cs`

- `Provider/`
  - `LuaLanguageServerIntellisenseProvider.cs`
  - `LuaLanguageServerIntellisenseProvider.Documents.cs`
  - `LuaLanguageServerIntellisenseProvider.Requests.cs`
  - `LuaLanguageServerIntellisenseProvider.SemanticTokens.cs`

- `Completion/`
  - `LuaLanguageServerResponseParser.Completion.cs`

- `Navigation/`
  - `LuaLanguageServerResponseParser.Navigation.cs`

- `SignatureHelp/`
  - `LuaLanguageServerResponseParser.SignatureHelp.cs`

- `Diagnostics/`
  - `LuaLanguageServerDiagnosticsParser.cs`
  - `LuaPublishedDiagnostics.cs`
  - shared response-parser pieces that are diagnostics-specific if more are split later

- `SemanticTokens/`
  - `LuaLanguageServerSemanticTokensDeltaParser.cs`
  - future semantic-token decoder or result helpers

Implementation order inside this phase:

1. Move files without behavioral changes.
2. Rename partials only when the new file names better match the responsibility.
3. Extract dedicated stateful coordinators only where there is real state ownership pressure.

Recommended coordinator candidates inside the provider layer:

- `LuaDocumentSynchronizationCoordinator`
- `LuaSemanticTokensCoordinator`
- `LuaWorkspaceWatcherCoordinator`

Non-goal for this phase:

- Do not create separate completion, hover, navigation, and signature-help service classes if they remain thin request wrappers around parser calls. That would add indirection without paying down much complexity.

Exit criteria:

- A maintainer can find document sync, semantic tokens, workspace watching, or client transport without scanning the whole folder.
- Provider partials and helper types match clear feature boundaries.

## Phase 4 - Consolidate Shared AvalonEdit Infrastructure Carefully

Purpose: extract only the reusable pieces that are already proven to repeat.

Shared abstractions that are worth considering:

- `TextDocumentPosition`
  - only if Lua editor code and LuaLS infrastructure both need the same line and character concept in multiple places
- `AsyncRequestGate`
  - only if multiple editor features or multiple editors share the same request freshness and lifecycle pattern
- `CompletionWindowHost`
  - only if another async editor needs the same styled completion popup and tooltip behavior
- `HoverToolTipFactory`
  - only if tooltip composition starts repeating across more than one editor

Shared abstractions that are not worth building now:

- A generic `IIntellisenseProvider<TCompletion, THover, TDefinition, TSignature>`
- A generic editor framework for completion, hover, definition, and signature help across all AvalonEdit editors

Reason:

- Lua currently has the only rich async LSP-backed pipeline.
- `GameFlowEditor` and `ClassicScriptEditor` are much simpler and mostly synchronous.
- A shared full-framework abstraction would force weaker editors into stub implementations and increase indirection before there is a second real consumer.

Recommendation:

- Keep the Lua feature contracts Lua-specific for now.
- Revisit a broader abstraction only when at least one other editor reaches a Lua-like capability set or moves to an LSP-backed async model.

Exit criteria:

- Shared helpers exist only where they already have at least two concrete consumers.
- No new abstraction is introduced just to make the architecture diagram look cleaner.

## Phase 5 - Final Simplification And Maintainer Handoff

Purpose: finish the cleanup by reducing leftover friction for future contributors.

Work items:

1. Remove partials that no longer improve navigation after service extraction.
2. Ensure touched files follow nullable guidance and remove redundant null-forgiving usage where practical.
3. Remove unused `using` directives and redundant framework qualification in touched files.
4. Add a short `README.md` inside the LuaLS service root describing the slices and their ownership.
5. Add a small regression checklist for completion, hover, signature help, definition navigation, document rename, diagnostics, and semantic tokens.

Exit criteria:

- A new maintainer can identify the owner of a behavior in one or two guesses.
- The cleanup leaves fewer hidden couplings, not just more files.

## Recommended Execution Order

1. Phase 0 baseline tests and notes.
2. Phase 1 named result types.
3. Phase 2 completion controller extraction.
4. Phase 2 signature-help controller extraction.
5. Phase 2 hover and navigation extraction.
6. Phase 3 file moves for LuaLS slices.
7. Phase 3 targeted provider coordinators where state remains dense.
8. Phase 4 shared infrastructure only if the first three phases reveal a real repeated pattern.
9. Phase 5 final simplification, docs, and regression pass.

## Decision On A Shared IntelliSense Abstraction

Short answer: not as a full framework yet.

The worthwhile cleanup right now is to separate Lua editor features and LuaLS infrastructure into well-owned pieces. That work will make later sharing easier if another editor actually grows into the same capability set. Building a generic abstraction for completion, hover, definition, signature help, and related plumbing before that point would most likely add complexity instead of removing duplication.

The pragmatic middle ground is:

- extract Lua-specific controllers and provider coordinators first
- promote only small reusable building blocks when a second consumer appears
- delay any broad cross-language IntelliSense contract until there is a second editor with comparable async behavior and lifecycle needs
