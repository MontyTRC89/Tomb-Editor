# Lua Language Server Ownership Notes

This note is a maintainer map for the current LuaLS integration layout after the phase 3 re-slice. It is intentionally short and should be updated as cleanup work lands.

## Current ownership

- `Provider/`: provider composition root, startup and restart flow, request orchestration, document replay, and semantic-token refresh coordination.
- `Documents/`: tracked document snapshots, version and open-reference state, synchronization requests and results, incremental edit calculation, and text-sync kinds.
- `Infrastructure/Client/`: process lifetime, JSON-RPC session state, transport I/O, and server capability tracking.
- `Infrastructure/Pathing/`: path normalization, server discovery, and settings generation.
- `Infrastructure/Startup/`: startup failure reporting and process-host job-object integration.
- `Infrastructure/Workspace/`: workspace file watching, external change batching, and watcher failure reporting.
- `Completion/`: completion response parsing and snippet-placeholder result shaping.
- `Hover/`: hover response parsing.
- `Navigation/`: definition-location response parsing.
- `References/`: reference-location response parsing for `textDocument/references`.
- `Rename/`: workspace-edit parsing for `textDocument/rename` results.
- `Formatting/`: document-formatting edit parsing for `textDocument/formatting` results.
- `SignatureHelp/`: signature-help response parsing.
- `Diagnostics/`: diagnostics payload mapping and published diagnostics snapshots.
- `SemanticTokens/`: semantic-tokens delta parsing plus cached delta and decode result carriers.
- `LuaLanguageServerResponseParser.Shared.cs`: shared markup parsing helpers used by the response-parser partials.
- `LuaStudio.Formatting.cs` plus `LuaWorkspaceEditApplier`: host-side reformat command ownership, including selection preservation when formatting edits are applied back into the editor.

## Extraction naming

- Use `Coordinator` for new provider-side owners of document sync, semantic tokens, restart handling, or workspace watching.
- Avoid introducing feature-specific `Service` types when the result would only wrap one parser call and add another hop.
- Keep parser and transport types focused on one protocol concern each.

## Cleanup guardrails

- Move files by feature first, then extract new stateful owners only where state is still dense.
- Keep document tracking centralized instead of splitting version and cache state across several helpers.
- Prefer feature-oriented folders over a larger set of flat provider partials.
- Keep XML docs current on provider-facing contracts, result carriers, and other maintainer-facing entry points when responsibilities change.

## Regression checklist

- Completion: verify `.` `:` and manual `Ctrl+Space` requests still open the completion list, refresh correctly after edits, and continue to resolve selected-item tooltip content.
- Hover: verify symbol hover still shows hover content, and that hover falls back to diagnostics when no hover payload is available.
- Signature help: verify `(` and `,` open or refresh the popup, `)` dismisses it, and editor deactivation clears transient UI.
- Definition navigation: verify `F12` and `Ctrl+Click` navigate for valid identifiers and stay inert for comments, strings, or unresolved symbols.
- References: verify Find References opens or refreshes the references results pane, groups locations by file, and keeps activation navigation aligned with the selected result.
- Rename: verify Rename Symbol applies workspace edits across open and unopened files, preserves diagnostics ownership after the rename, and keeps the global undo action working for the full rename batch.
- Formatting: verify the existing cleanup command routes Lua documents through LuaLS formatting, preserves caret or selection state in the active editor, and falls back cleanly when formatting is unavailable.
- Document lifecycle: verify open, update, rename, close, and restart replay keep document versions, URIs, and open references synchronized with LuaLS.
- Diagnostics: verify published diagnostics replace stale results, clear when documents change or close, and stay attached to the correct file after rename and restart flows.
- Semantic tokens: verify full refresh, delta apply, and delta fallback all keep coloring stable after edits, file changes, and server restart.
