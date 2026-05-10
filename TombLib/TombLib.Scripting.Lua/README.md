# Lua Editor Ownership Notes

This note is a phase 0 maintainer map for the current Lua editor layout. It is intentionally short and should be updated as cleanup work lands.

## Current ownership

- `LuaEditor.cs`: public control surface, composition root, theme application, syntax-highlighting setup, and provider hookup.
- `LuaEditor.Intellisense.cs`: event wiring, trigger routing, document/request generation invalidation, unload cleanup, and shared editor-side request helpers.
- `Editor/Completion/LuaCompletionController.cs`: editor-facing completion entry points, debounce scheduling, popup lifecycle, completion tooltip resolve flow, query-offset calculation, width measurement, and AvalonEdit popup integration details.
- `Editor/Hover/LuaHoverController.cs`: editor hover entry points, hover request cancellation, hover eligibility checks, tooltip rendering, and hover versus diagnostic tooltip selection.
- `Editor/Navigation/LuaDefinitionNavigationController.cs`: F12 and Ctrl+Click definition navigation flow and definition-request cancellation state.
- `Editor/SignatureHelp/LuaSignatureHelpController.cs`: editor-facing signature help entry points, signature popup UI, refresh timer, pending-request state, popup positioning, and request flow.
- `LuaEditor.Navigation.cs`: keyboard and mouse navigation event handling plus the public navigation entry point.
- `LuaEditor.SemanticHighlighting.cs`: semantic-token subscriptions, token application, and editor-side highlighting refresh.
- `Utils/`: editor-local rules and pure helpers such as line parsing, indentation, and hover/completion/definition interaction guards.
- `Services/ILuaIntellisenseProvider.cs`: editor-facing contract for the Lua language-service backend.

## Extraction naming

- Use `Controller` for new stateful editor-owned feature workflows.
- Reserve `Coordinator` for provider-side workflows that own document queues, background refresh, or restart-sensitive state.
- Keep `Service` for stable external integration surfaces or shared utilities that already represent a service boundary.

## Cleanup guardrails

- Keep `LuaEditor` as the public control and composition root.
- Keep partials only when the state still clearly belongs to `LuaEditor` itself.
- Prefer moving real feature state and workflow into a named owner instead of spreading it across another partial.