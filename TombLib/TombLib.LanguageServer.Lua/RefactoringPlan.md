# TombLib.LanguageServer.Lua Cleanup Plan

## Goal

- Bring `TombLib.LanguageServer.Lua` closer to the structural quality of `TombLib.LanguageServer.Core` without reopening the finished `.Core` extraction.
- Improve folder boundaries, naming, XML documentation coverage, and test layout while preserving current behavior.
- Follow the repository guidance from `AGENTS.md`, especially file-scoped namespaces, sorted usings, nullability discipline, and XML docs on public surface area.

## Current Assessment

- `Documents/` is already on the right abstraction level because `LuaIntellisenseDocumentManager` now sits on top of `TrackedDocumentStore<LuaTrackedDocumentState>` from `.Core`.
- `Provider/` still mixes the main provider surface with workspace watching and recovery logic because `LuaWorkspaceChangeCoordinator` lives beside `LuaLanguageServerIntellisenseProvider`.
- Lua-specific parsing is reasonably feature-sliced, but shared parsing helpers still sit at the project root in `LuaLanguageServerResponseParser.Shared.cs`.
- The LuaLS configuration builders in `Infrastructure/` and `Infrastructure/Client/` are thin and useful, but their naming is longer than necessary and their type-level XML docs are inconsistent.
- Tests already have decent behavioral coverage, but the `TombLib.Test` layout is flatter than the production layout, which makes the remaining cleanup work harder to navigate.

## Keep As-Is

- Keep the `LuaLanguageServerIntellisenseProvider` partial-class split. It already matches the main concern boundaries well enough.
- Keep the feature folders such as `Completion/`, `Hover/`, `Rename/`, `References/`, `Formatting/`, and `SignatureHelp/`.
- Keep the `LuaLanguageServerResponseParser` partial approach unless there is a deliberate API-breaking rename. The main problem is helper placement, not the partial layout itself.
- Avoid extracting more generic functionality back into `.Core` unless a remaining type is clearly language-agnostic and reused by more than Lua.

## Recommended Target Structure

```text
TombLib.LanguageServer.Lua/
  Completion/
  Diagnostics/
  Documents/
  Formatting/
  Hover/
  Infrastructure/
    Client/
    Configuration/
  Markup/
  Navigation/
  Provider/
    LuaLanguageServerIntellisenseProvider.cs
    LuaLanguageServerIntellisenseProvider.Documents.cs
    LuaLanguageServerIntellisenseProvider.Lifecycle.cs
    LuaLanguageServerIntellisenseProvider.Requests.cs
    LuaLanguageServerIntellisenseProvider.SemanticTokens.cs
  References/
  Rename/
  SemanticTokens/
  SignatureHelp/
  Workspace/
```

Notes:

- `Provider/` should contain only provider-facing orchestration.
- `Workspace/` should contain watcher coordination, snapshotting, and external workspace change forwarding.
- `Infrastructure/Configuration/` should hold LuaLS settings and initialization payload builders.
- `Markup/` should hold shared Lua-specific markup normalization helpers so parser partials stop depending on a root-level orphan file.

## Phase 1 - Documentation And Style Baseline

### Scope

- All public types in `TombLib.LanguageServer.Lua`.
- Internal types with non-trivial behavior or concurrency rules.

### Work

- Add type-level XML docs to public types that still lack them, especially:
  - `LuaLanguageServerSettingsFactory`
  - `LuaLanguageServerClientCapabilitiesFactory`
  - `LuaLanguageServerInitializationOptionsFactory`
- Add XML docs to internal complex types whose behavior is not obvious from names alone:
  - `LuaWorkspaceChangeCoordinator`
  - `LuaTrackedDocumentState`
  - `LuaDocumentDiagnosticsCache`
  - `LuaDocumentSemanticTokensCache`
- Normalize touched files to match `AGENTS.md`:
  - sort `using` directives
  - remove unused imports
  - keep file-scoped namespaces
  - keep nullable annotations accurate
  - avoid the null-forgiving operator unless it expresses a real invariant

### Exit Criteria

- Every public type and public member in the touched `.Lua` files has XML documentation.
- Every touched file follows the current C# style guidance from `AGENTS.md`.

## Phase 2 - Folder Boundary Cleanup

### Scope

- File placement only. No behavioral changes beyond namespace-neutral moves.

### Work

- Move `Provider/LuaWorkspaceChangeCoordinator.cs` into a new `Workspace/` folder.
- Move `Infrastructure/LuaLanguageServerSettingsFactory.cs` into `Infrastructure/Configuration/`.
- Move the logic from `LuaLanguageServerResponseParser.Shared.cs` into a dedicated helper under `Markup/`.
- Keep provider partials together under `Provider/`.
- Keep parser partials in their existing feature folders instead of moving everything into a single `Parsing/` folder.

### Exit Criteria

- `Provider/` contains only provider code.
- No shared helper remains stranded at the project root except `GlobalUsings.cs` and the project file.
- The folder layout reads similarly to `.Core` at a glance.

## Phase 3 - Naming Cleanup

### Scope

- Internal names first.
- Public-name changes only when the API churn is acceptable.

### Work

- Rename `LuaIntellisenseDocumentManager` to a store-oriented name.
  - Preferred option: `LuaDocumentStore`.
  - Alternate option if explicit symmetry is preferred: `LuaTrackedDocumentStore`.
- Rename cache types to match their actual role and reduce redundant wording.
  - `LuaDocumentDiagnosticsCache` -> `LuaDiagnosticsCache`
  - `LuaDocumentSemanticTokensCache` -> `LuaSemanticTokensCache`
- Review whether `LuaTrackedDocumentState` should stay as-is or become `LuaDocumentState` for consistency with the store rename.
- Shorten factory names only if public API churn is acceptable.
  - `LuaLanguageServerClientCapabilitiesFactory` -> `LuaClientCapabilitiesFactory`
  - `LuaLanguageServerInitializationOptionsFactory` -> `LuaInitializationOptionsFactory`
  - `LuaLanguageServerSettingsFactory` -> `LuaSettingsFactory`

### Exit Criteria

- Type names describe concrete roles such as `Store`, `Cache`, `Coordinator`, and `Factory`.
- The word `Manager` disappears unless a type really owns multiple cross-cutting responsibilities.

## Phase 4 - Workspace Slice Extraction

### Scope

- `LuaWorkspaceChangeCoordinator` and related tests.

### Work

- Extract the workspace snapshot capture and comparison logic from `LuaWorkspaceChangeCoordinator` into a dedicated helper.
  - Recommended name: `LuaWorkspaceSnapshotTracker`.
- Keep watcher lifecycle, recovery, and reporting inside `LuaWorkspaceChangeCoordinator`.
- Let the new helper own only snapshot capture, diffing, and snapshot updates after forwarded changes.
- Add or update focused tests around snapshot diff behavior and watcher recovery boundaries.

### Exit Criteria

- `LuaWorkspaceChangeCoordinator` reads as an orchestrator instead of a state container plus orchestrator.
- Workspace snapshot logic can be tested without going through the whole provider path.

## Phase 5 - Document State Cleanup

### Scope

- `Documents/` only.

### Work

- After the store rename, review `LuaTrackedDocumentState` and keep it intentionally small.
- Avoid adding more behavior to the state type beyond state mutation bridges and Lua-specific caches.
- If the stale-version rule remains duplicated between the diagnostics and semantic-token caches, extract that rule into a very small internal helper instead of introducing a generic inheritance hierarchy.
- Keep cache ownership in the state object unless a later change proves that diagnostics and semantic-token state need different lifetimes.

### Exit Criteria

- `Documents/` has a clear split between store, state, and cache helpers.
- No type in this folder acts as both coordinator and data container.

## Phase 6 - Parsing And Request Polish

### Scope

- `LuaLanguageServerResponseParser` partials.
- `LuaLanguageServerIntellisenseProvider.Requests.cs`.

### Work

- Move markup normalization into a dedicated helper in `Markup/` and let parser partials consume it.
- Leave the feature parser partial files where they are.
- Review `LuaLanguageServerIntellisenseProvider.Requests.cs` after the naming and folder passes.
- Split `Requests.cs` further only if one of these becomes true:
  - the file starts mixing unrelated request families heavily
  - one request path needs enough private helpers to justify its own partial
- Replace non-obvious magic strings or numeric values in the configuration builders with named constants when that improves readability.

### Exit Criteria

- Request code reads top-down without hidden shared helpers living outside the relevant feature area.
- Parsing helpers are discoverable by folder and name.

## Phase 7 - Test Layout Alignment

### Scope

- `TombLib.Test` layout for Lua language-server tests.

### Work

- Reorganize Lua language-server tests into folders that mirror the production slices.
  - `LanguageServer/Lua/Documents/`
  - `LanguageServer/Lua/Provider/`
  - `LanguageServer/Lua/Workspace/`
  - `LanguageServer/Lua/Parsing/`
  - `LanguageServer/Lua/Infrastructure/`
- Keep the existing provider partial test-class pattern, but place the files in a folder that matches the concern.
- Add focused tests for any new helper introduced in phases 4 through 6.
- Prefer narrow tests for extracted helpers over broader provider integration tests when both would cover the same behavior.

### Exit Criteria

- Test navigation mirrors production navigation.
- Every structural extraction lands with direct slice tests.

## Suggested Delivery Order

1. Phase 1 as a small cleanup PR.
2. Phase 2 and the internal-only part of Phase 3 as a second PR.
3. Phase 4 as a focused workspace refactor PR with targeted tests.
4. Phase 5 as a documents-only cleanup PR.
5. Phase 6 as a polish PR after earlier renames have stabilized.
6. Phase 7 either alongside each phase or as a final layout-only cleanup.

## Practical Guardrails

- Do not split `LuaLanguageServerIntellisenseProvider` again just to match `.Core`. Its current partial boundaries are already useful.
- Do not introduce inheritance-heavy cache abstractions. The current code is small enough that a tiny helper beats a hierarchy.
- Do not move behavior into `.Core` unless the result is clearly language-neutral and broadly reusable.
- Prefer behavior-preserving renames and file moves before deeper logic extraction.
- Treat XML docs as part of the refactor definition of done, not a follow-up.
