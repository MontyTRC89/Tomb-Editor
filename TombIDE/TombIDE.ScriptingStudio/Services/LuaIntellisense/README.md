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
- `SignatureHelp/`: signature-help response parsing.
- `Diagnostics/`: diagnostics payload mapping and published diagnostics snapshots.
- `SemanticTokens/`: semantic-tokens delta parsing plus cached delta and decode result carriers.
- `LuaLanguageServerResponseParser.Shared.cs`: shared markup parsing helpers used by the response-parser partials.

## Extraction naming

- Use `Coordinator` for new provider-side owners of document sync, semantic tokens, restart handling, or workspace watching.
- Avoid introducing feature-specific `Service` types when the result would only wrap one parser call and add another hop.
- Keep parser and transport types focused on one protocol concern each.

## Cleanup guardrails

- Move files by feature first, then extract new stateful owners only where state is still dense.
- Keep document tracking centralized instead of splitting version and cache state across several helpers.
- Prefer feature-oriented folders over a larger set of flat provider partials.
