# Lua IntelliSense Production Readiness Plan

## Scope
This document is a clean-room handoff for the next agent. It re-verifies the current Lua IntelliSense concerns against the codebase as it exists now and turns them into a phased implementation plan.

Date: 2026-05-09.

## Executive Summary
The current blocker set is narrower than the original checklist, but a few items are real production-readiness problems and should be treated as the first slice:

- LuaLS request timeouts do not trigger recovery, so a hung server can leave IntelliSense dead until restart.
- Old transport loops can still interact with the shared pending-request map after restart.
- Save As / external rename do not rebind tracked IntelliSense documents to the new path.
- Completion insertion does not honor LSP `textEdit` replacement semantics.
- Signature help dismissal and refresh are incomplete enough to produce visible UX failures.
- Stale diagnostics and semantic tokens survive normal edits until the delayed sync fires, which explains the shifted underline / stale color complaints better than a pure TextMate failure.

The Lua indentation complaints are also real, but they belong in the completion / Enter-handling phase rather than the transport phase.

## Re-Verification Matrix

### 1. LuaLS Startup, Transport, And Recovery

#### Concern: repeated request timeouts leave IntelliSense permanently dead.
Verdict: verified blocker.

Evidence:

- [SendBoundedRequestAsync](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerIntellisenseProvider.Requests.cs#L185) times out after 10 seconds, logs, and returns `default`.
- That path does not mark the client unhealthy, invalidate startup state, or trigger restart.
- Restart today depends on `_client.IsReady` becoming false, which a hung but still-running process will not do.

#### Concern: a restarted LuaLS receives all currently open documents again.
Verdict: already implemented and worth preserving.

Evidence:

- [EnsureStartedAsync](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerIntellisenseProvider.cs#L159) calls `_documents.PrepareForRestart()` and then [ReopenTrackedDocumentsAsync](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerIntellisenseProvider.Documents.cs#L102).
- Existing coverage: [GetHoverAsync_ReplaysTrackedDocumentsAfterLanguageServerRestart](TombLib/TombLib.Test/LuaLanguageServerIntellisenseProviderTests.cs#L153).

#### Concern: stale / old process read loops cannot fail requests belonging to a newer process.
Verdict: verified blocker.

Evidence:

- The client keeps one shared `_pendingRequests` map in [LuaLanguageServerClient](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerClient.cs#L35).
- Requests are added in [SendRequestCoreAsync](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerClient.Protocol.cs#L17).
- Old read loops can still call [FailPendingRequests](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerClient.cs#L480) from [ReadLoopAsync](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerClient.Transport.cs#L18) after restart.
- `ResetProcessState()` clears streams and task references, but it does not replace the pending-request store or scope failures to a transport generation.

#### Concern: disposal of LuaLS hangs on stderr / diagnostic pumps / background loops.
Verdict: not a blocker, but keep an eye on it.

Evidence:

- [Dispose](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerClient.cs#L559) cancels lifetime, fails pending requests, and waits with [DisposeWaitTimeout](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerClient.cs#L21).
- [WaitForBackgroundLoopsAsync](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerClient.cs#L506) is timeout-bounded and logs rather than hanging indefinitely.

### 2. Document Lifetime And Tab Ownership

#### Concern: Save As / rename must move diagnostics, semantic tokens, hover, completion, and definition to the new path.
Verdict: verified blocker.

Evidence:

- [SaveFileAs(TabPage)](TombIDE/TombIDE.ScriptingStudio/Controls/EditorTabControl.cs#L363) directly changes `editor.FilePath` and saves, but it does not tell IntelliSense to close the old URI and open the new one.
- [RenameDocumentTabPage](TombIDE/TombIDE.ScriptingStudio/Controls/EditorTabControl.cs#L691) also rewrites `editor.FilePath` only.
- Lua editors are wired through [LuaStudio.Intellisense.cs](TombIDE/TombIDE.ScriptingStudio/LuaStudio.Intellisense.cs#L76), but there is no rename/rekey hook.

#### Concern: request-only hover / definition / signature documents are closed afterward.
Verdict: not a current UI blocker.

Evidence:

- Request paths use `acquireOpenReference: false` in [SendPositionRequestAsync](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerIntellisenseProvider.Requests.cs#L145).
- In the actual UI flow, Lua editors are opened through [EditorTabControl_LuaFileOpened](TombIDE/TombIDE.ScriptingStudio/LuaStudio.Intellisense.cs#L76), which already calls `OpenDocument`.
- A non-UI caller could still create a request-only tracked document, but that is not the dominant production path.

#### Concern: closing a tab while requests are in flight must not update a disposed editor.
Verdict: mixed.

Evidence:

- Completion and hover cancel tokens on unload in [LuaEditor_Unloaded](TombLib/TombLib.Scripting.Lua/LuaEditor.Intellisense.cs#L60).
- Signature help and definition navigation still lack the same level of stale-result guarding.

### 3. Stale Async Editor Requests

#### Concern: stale completion / hover / signature / definition responses should be discarded after edits.
Verdict: partially verified, still missing in important paths.

Evidence:

- Completion uses `_completionRequestToken` in [RequestCompletionAsync](TombLib/TombLib.Scripting.Lua/LuaEditor.Completion.cs#L92).
- Hover uses `_hoverRequestToken` in [HandleMouseHover](TombLib/TombLib.Scripting.Lua/LuaEditor.Hover.cs#L27).
- Signature help in [RequestSignatureHelpAsync](TombLib/TombLib.Scripting.Lua/LuaEditor.SignatureHelp.cs#L146) has cancellation only, no request-generation token.
- Definition navigation in [TryNavigateToDefinitionAsync](TombLib/TombLib.Scripting.Lua/LuaEditor.Navigation.cs#L55) has no generation/version guard at all.

### 4. Completion Insertion

#### Concern: `textEdit.range`, `textEdit.insert`, and `textEdit.replace` must be honored.
Verdict: verified blocker.

Evidence:

- [ParseCompletionItem](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerResponseParser.Completion.cs#L145) reads `textEdit.newText` only.
- It ignores `textEdit.range`, `textEdit.insert`, and `textEdit.replace` entirely.
- [LuaCompletionData.Complete](TombLib/TombLib.Scripting.Lua/Objects/LuaCompletionData.cs#L117) always replaces AvalonEdit's `completionSegment`, not an LSP-provided edit range.

#### Concern: text edits must not apply after the document version changed.
Verdict: verified gap.

Evidence:

- `LuaCompletionItem` has no document version or request-generation metadata in [LuaCompletionItem](TombLib/TombLib.Scripting.Lua/Objects/LuaCompletionItem.cs#L10).
- `LuaCompletionData.Complete` has no stale-version guard.

#### Concern: snippets, `$0`, multiline insertion, and caret placement are correct.
Verdict: not correct if snippet or multiline completion text reaches the editor.

Evidence:

- [StripSnippetPlaceholders](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerResponseParser.Completion.cs#L359) strips `$1`, `${1:default}`, and `$0`, but it does not preserve caret intent.
- [LuaCompletionData.Complete](TombLib/TombLib.Scripting.Lua/Objects/LuaCompletionData.cs#L117) inserts plain text only.
- There is no Lua-aware multiline reindent pass anywhere in [TextEditorBase](TombLib/TombLib.Scripting/Bases/TextEditorBase.cs#L562) or [LuaEditor](TombLib/TombLib.Scripting.Lua/LuaEditor.cs#L10).

Important nuance:

- The client advertises `snippetSupport = false` in [LuaLanguageServerClient](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerClient.cs#L280).
- Settings also set `callSnippet = "Disable"` in [LuaLanguageServerSettingsFactory](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerSettingsFactory.cs#L34).
- That lowers the frequency of snippet-shaped results, but it does not make the current insertion code correct if LuaLS still emits keyword snippets or multiline `newText`.

#### Concern: completion-item resolve must preserve insertion text and ranges.
Verdict: future implementation requirement.

Evidence:

- Resolve reparses a new `LuaCompletionItem` in [ResolveCompletionItemAsync](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerIntellisenseProvider.Requests.cs#L116).
- Once range metadata is added, resolve must merge detail/documentation onto the unresolved insertion metadata rather than replacing it blindly.

### 5. Lua Enter / Indentation Strategy

#### Concern: pressing Enter after `then` is missing one extra indent level.
Verdict: verified missing feature.

Evidence:

- [TextEditorBase](TombLib/TombLib.Scripting/Bases/TextEditorBase.cs#L562) only implements bracket / quote auto-closing.
- There is no Lua-specific Enter handler, indentation strategy, or block-aware newline logic in [LuaEditor](TombLib/TombLib.Scripting.Lua/LuaEditor.cs#L10) or its partials.

#### Concern: `if ... then ... end` autocomplete puts `end` at the wrong indentation.
Verdict: verified gap for any multiline completion text.

Evidence:

- Multiline completion text is inserted verbatim by [LuaCompletionData.Complete](TombLib/TombLib.Scripting.Lua/Objects/LuaCompletionData.cs#L117).
- No reindent or caret-placement normalization exists before or after insertion.

### 6. Signature Help And Navigation UX

#### Concern: closing a method with `)` should dismiss signature help.
Verdict: verified UX bug.

Evidence:

- Lua dismisses signature help on `)` only in [TextArea_TextEntered](TombLib/TombLib.Scripting.Lua/LuaEditor.Intellisense.cs#L104).
- The base editor can consume `)` earlier in [TryPerformElementSkip](TombLib/TombLib.Scripting/Bases/TextEditorBase.cs#L596) when skipping over an auto-inserted close paren.
- That means the editor can move past `)` without the Lua-specific dismissal path running.

#### Concern: signature help argument highlighting can desync.
Verdict: verified high-priority issue.

Evidence:

- Signature refresh happens on `(`, `,`, Backspace, and Delete only. See [TextArea_TextEntered](TombLib/TombLib.Scripting.Lua/LuaEditor.Intellisense.cs#L87) and [TextEditor_KeyDown](TombLib/TombLib.Scripting.Lua/LuaEditor.Navigation.cs#L21).
- Normal typing inside an argument list does not refresh signature help.
- Signature help also lacks a request token in [RequestSignatureHelpAsync](TombLib/TombLib.Scripting.Lua/LuaEditor.SignatureHelp.cs#L146), so stale responses can repaint older active-parameter state.

#### Concern: go-to-definition should not act on stale results.
Verdict: verified hardening need.

Evidence:

- [TryNavigateToDefinitionAsync](TombLib/TombLib.Scripting.Lua/LuaEditor.Navigation.cs#L55) has no generation or version check before invoking `DefinitionNavigationRequested`.

### 7. Semantic Tokens, Syntax Coloring, And Diagnostic Lag

#### Concern: quick edits like `local -> locl -> local` break colors, possibly due to autocomplete popup interaction.
Verdict: partially reclassified.

Verified part:

- Stale semantic tokens remain visible until the delayed document sync runs, because edits do not immediately clear semantic overlays.
- Lua semantic tokens are updated only after [TextChangedDelayed](TombIDE/TombIDE.ScriptingStudio/LuaStudio.Intellisense.cs#L92), which is driven by the 300 ms timer in [TextEditorBase](TombLib/TombLib.Scripting/Bases/TextEditorBase.cs#L185).
- `ClearSemanticTokens()` is called on unload only, not on ordinary edits. See [LuaEditor.SemanticHighlighting](TombLib/TombLib.Scripting.Lua/LuaEditor.SemanticHighlighting.cs#L16).

Not directly proven:

- The TextMate path itself looks change-driven and should retokenize lines through [TextMateDocumentLineList](TombLib/TombLib.Scripting/Highlighting/TextMateDocumentLineList.cs#L48) and [TextMateColorizingTransformer](TombLib/TombLib.Scripting/Highlighting/TextMateColorizingTransformer.cs#L24).
- There is no strong code evidence that the completion popup itself blocks TextMate recoloring.

Working conclusion:

- Treat the reported recolor bug primarily as stale semantic-token / stale overlay behavior until a concrete TextMate-only repro proves otherwise.

#### Concern: errors should clear immediately on text change because underlines can shift.
Verdict: verified high-priority UX defect.

Evidence:

- [TextEditor_TextChanged](TombLib/TombLib.Scripting/Bases/TextEditorBase.cs#L269) only marks content changed and restarts the delayed timer.
- Diagnostics are not cleared on edit.
- Provider-side diagnostics are refreshed only through [LuaEditor_TextChangedDelayed](TombIDE/TombIDE.ScriptingStudio/LuaStudio.Intellisense.cs#L92).

#### Concern: semantic-token version association / stale overwrite after rapid edits.
Verdict: normal version gating is present, but stale-visibility still exists.

Evidence:

- [TryStoreSemanticTokens](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaIntellisenseDocumentManager.cs#L216) rejects older token versions.
- However, the old tokens remain painted until a newer token set arrives or the editor clears them.

#### Concern: failed semantic-token delta requests should preserve or clear state intentionally.
Verdict: verified hardening issue.

Evidence:

- On delta failure, [DecodeSemanticTokensResponse](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerIntellisenseProvider.SemanticTokens.cs#L89) can return `previousData` paired with a new `resultId`.
- That is internally inconsistent and should be replaced by a forced full refresh path.

### 8. TextMate And Highlighting Fallback

#### Concern: Lua should remain highlighted if TextMate grammar install fails.
Verdict: verified gap in the main editor.

Evidence:

- [LuaTextMateSyntaxHighlighting.TryInstall](TombLib/TombLib.Scripting/Highlighting/LuaTextMateSyntaxHighlighting.cs#L14) returns false when grammar load fails.
- [LuaEditor.UpdateSettings](TombLib/TombLib.Scripting.Lua/LuaEditor.cs#L51) always sets `SyntaxHighlighting = null` after trying TextMate.
- Markdown tooltips do have a fallback in [MarkdownToolTipRenderer](TombLib/TombLib.Scripting/Rendering/MarkdownToolTipRenderer.cs#L309), but the main editor does not.

### 9. Tooltips, Watcher, Disposal, And Packaging

#### Concern: tooltip embedded editors can steal focus.
Verdict: worth hardening.

Evidence:

- The FlowDocument viewer and hyperlinks are made non-focusable in [MarkdownToolTipRenderer](TombLib/TombLib.Scripting/Rendering/MarkdownToolTipRenderer.cs#L85) and [MarkdownToolTipRenderer](TombLib/TombLib.Scripting/Rendering/MarkdownToolTipRenderer.cs#L218).
- The embedded `TextEditor` created in [CreateCodeBlockElement](TombLib/TombLib.Scripting/Rendering/MarkdownToolTipRenderer.cs#L309) is read-only but not explicitly made non-focusable.

#### Concern: watcher recovery / disposal safety.
Verdict: watch coverage is good, disposal race is real, recovery behavior is limited.

Evidence:

- The watcher covers `*.lua`, `.API`, and `.luarc.*` in [LuaWorkspaceFileWatcher.Start](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaWorkspaceFileWatcher.cs#L43).
- [DispatchPendingChangesAsync](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaWorkspaceFileWatcher.cs#L131) can still be in flight while `Dispose()` disposes `_dispatchGate`.
- `Error` currently logs only; it does not restart or visibly disable the watcher.

#### Concern: event subscribers throwing exceptions should not starve others.
Verdict: mixed.

Evidence:

- Startup failures are protected in [ReportStartupFailure](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerIntellisenseProvider.cs#L320).
- The client diagnostics pump isolates handler exceptions in [PumpDiagnosticsAsync](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerClient.Transport.cs#L301).
- Provider fan-out for [DiagnosticsUpdated](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerIntellisenseProvider.Documents.cs#L271) and [SemanticTokensUpdated](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerIntellisenseProvider.SemanticTokens.cs#L59) is not wrapped.

#### Concern: LuaLS attribution when bundled.
Verdict: verified documentation gap, not installer blocker.

Evidence:

- Bundling is build-backed in [TombIDE.Shared.csproj](TombIDE/TombIDE.Shared/TombIDE.Shared.csproj#L9).
- [ExternalResources.md](ExternalResources.md) does not currently list LuaLS.

## Phase Plan

### Phase 1: Transport Resilience Blockers
Goal: make restarts safe and automatic when LuaLS becomes unhealthy.

Tasks:

- Add a transport generation / session concept in [LuaLanguageServerClient](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerClient.cs).
- Scope pending requests and read-loop failure propagation to that generation.
- Ensure an old read loop or process exit cannot fault requests created by a newer process.
- Add timeout health tracking in [LuaLanguageServerIntellisenseProvider.Requests.cs](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerIntellisenseProvider.Requests.cs).
- Restart after a small threshold of consecutive request timeouts on the same generation.
- Preserve the existing tracked-document replay path.

Tests:

- Extend [LuaLanguageServerClientTests](TombLib/TombLib.Test/LuaLanguageServerClientTests.cs).
- Extend [LuaLanguageServerIntellisenseProviderTests](TombLib/TombLib.Test/LuaLanguageServerIntellisenseProviderTests.cs).
- Add tests for timeout-triggered restart, document replay, and old-generation isolation.

### Phase 2: Document Identity And Path Ownership
Goal: keep all IntelliSense state attached to the real file path.

Tasks:

- Add a provider-level rename / rekey API rather than hand-rolling `CloseDocument` + `OpenDocument` in multiple UI call sites.
- Wire Save As in [EditorTabControl.SaveFileAs(TabPage)](TombIDE/TombIDE.ScriptingStudio/Controls/EditorTabControl.cs#L363).
- Wire external rename in [EditorTabControl.RenameDocumentTabPage](TombIDE/TombIDE.ScriptingStudio/Controls/EditorTabControl.cs#L691).
- Preserve diagnostics / semantic-token cache ownership when the path changes.

Tests:

- Save As of an open Lua file.
- External rename of an open Lua file.
- Multiple tabs referencing the same Lua file.

### Phase 3: Editor Request Freshness And Signature UX
Goal: stale results must never drive UI.

Tasks:

- Add a signature request token similar to completion / hover.
- Refresh signature help on more than `(`, `,`, Backspace, and Delete.
- Dismiss signature help when `)` is typed or skipped over through the base auto-close path.
- Make signature-help and definition-result application no-op once the editor has unloaded or is otherwise no longer valid.
- Add a generation / version guard to definition navigation.
- Keep the policy as cancel-and-discard rather than allow-finish-and-apply.

Tests:

- Typing `)` after an auto-inserted close paren dismisses signature help.
- Typing and deleting inside an argument list refreshes the highlighted parameter.
- Closing a tab during an in-flight signature or definition request does not reopen UI or navigate.
- Editing before a definition response returns does not navigate.

### Phase 4: Completion Insertion Correctness
Goal: completion insertion must match LSP semantics, not AvalonEdit guesses.

Tasks:

- Extend `LuaCompletionItem` with edit metadata: document version, request generation, and edit range information.
- Parse `textEdit.range` and `InsertReplaceEdit` (`insert` / `replace`) in [LuaLanguageServerResponseParser.Completion.cs](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerResponseParser.Completion.cs#L145).
- Apply those ranges in [LuaCompletionData.Complete](TombLib/TombLib.Scripting.Lua/Objects/LuaCompletionData.cs#L117).
- Reject stale completion application after a relevant document version change.
- When resolve is used, merge resolved detail/documentation into the unresolved insertion metadata.

Tests:

- Range replacement.
- Insert-vs-replace behavior.
- Stale version rejection.
- Resolve preserving insertion semantics.

### Phase 5: Lua Indentation And Multiline Completion Behavior
Goal: Lua should indent blocks correctly whether the user presses Enter or commits a multiline completion.

Tasks:

- Introduce a Lua-specific Enter / indentation strategy in [LuaEditor](TombLib/TombLib.Scripting.Lua/LuaEditor.cs).
- Indent one extra level after block openers such as `then`, `do`, `function`, `repeat`, and table / paren continuations as appropriate.
- Dedent on lines starting with `end`, `until`, `else`, or `elseif` as appropriate.
- Normalize multiline completion insertion indentation relative to the current line.
- Decide explicitly what to do with snippet `$0` intent when snippet support is disabled.

Practical guidance:

- Do not build a full tabstop snippet engine in this phase.
- Do preserve correct relative indentation and sensible caret placement for the common multiline Lua blocks users actually insert.

Tests:

- Pressing Enter after `then` increases indentation.
- `if ... then ... end` completion places `end` at the expected indentation.
- Multiline insertion preserves CRLF and caret placement intent as far as the plain-text model allows.

### Phase 6: Immediate Stale-UI Cleanup During Editing
Goal: avoid showing obviously wrong colors and diagnostics while edits are in progress.

Tasks:

- Clear diagnostics immediately on text change in Lua editors.
- Decide whether to clear semantic tokens immediately on text change or keep them only when a lightweight remapping strategy exists.
- If immediate clearing is too visually noisy, add a very short debounce distinct from the 300 ms document sync.
- Keep provider-side version gating unchanged.

Tests:

- Underlines disappear immediately after edit.
- No stale semantic overlay remains after quick keyword edits.
- Rapid `local -> locl -> local` edits do not leave visually stale coloring.

### Phase 7: Semantic Token And Highlighting Hardening
Goal: make fallback behavior intentional and deterministic.

Tasks:

- Fix delta-failure handling in [LuaLanguageServerIntellisenseProvider.SemanticTokens.cs](TombIDE/TombIDE.ScriptingStudio/Services/LuaIntellisense/LuaLanguageServerIntellisenseProvider.SemanticTokens.cs#L89) so previous data is not paired with a new result id.
- Add a main-editor fallback highlighting path when TextMate install fails.
- Recheck lone-CR affected-line handling in [TextMateDocumentLineList](TombLib/TombLib.Scripting/Highlighting/TextMateDocumentLineList.cs#L164).
- Add resolver tests for the selector behavior the bundled themes actually depend on.

Tests:

- Failed delta forces full refresh.
- Main editor still has baseline Lua highlighting if TextMate is unavailable.
- CRLF, LF, and lone CR changes invalidate the correct line range.

### Phase 8: Watcher, Disposal, Tooltip, And Packaging Hardening
Goal: finish the non-blocking safety and polish tasks.

Tasks:

- Make watcher disposal safe while dispatch is active.
- Decide whether watcher `Error` should restart, back off, or surface a disabled state.
- Wrap provider fan-out so one subscriber exception does not suppress later subscribers.
- Make tooltip code-block editors explicitly non-focusable and confirm resource lifetime behavior.
- Add LuaLS attribution to [ExternalResources.md](ExternalResources.md).

Tests:

- Watcher disposal during active dispatch.
- Subscriber exception isolation.
- Tooltip content does not steal focus.

## Items Reclassified As Lower Priority Or Non-Blockers

- Open-document replay after LuaLS restart is already present.
- Watch coverage for `*.lua`, `.luarc.*`, and `.API` is already present.
- Timeout-bounded LuaLS disposal is already present.
- The bundled themes do not currently rely on advanced TextMate parent-scope or excluded-selector semantics strongly enough to make that a first-wave blocker.
- A full snippet engine is not required for the first implementation pass.

## Coverage Of Verified Issues

Every concern below was re-verified as a real issue and has an explicit home in the plan:

- Timeout-based server recovery: Phase 1.
- Old-generation transport isolation: Phase 1.
- Save As / rename document rekeying: Phase 2.
- Signature stale-response guards: Phase 3.
- Definition stale / disposed-editor guards: Phase 3.
- Signature-help dismissal on skipped `)`: Phase 3.
- Signature active-parameter refresh and desync: Phase 3.
- `textEdit.range` / `insert` / `replace` support: Phase 4.
- Completion stale-version application guard: Phase 4.
- Completion resolve preserving insertion metadata: Phase 4.
- Enter indentation after `then`: Phase 5.
- Multiline `if ... then ... end` completion indentation: Phase 5.
- Plain-text handling of snippet `$0` intent and caret placement: Phase 5.
- Immediate clearing of stale diagnostics after edit: Phase 6.
- Immediate handling of stale semantic overlays after edit: Phase 6.
- Semantic-token delta fallback consistency: Phase 7.
- Main-editor TextMate fallback: Phase 7.
- Watcher disposal / recovery hardening: Phase 8.
- Diagnostics / semantic-token fan-out isolation: Phase 8.
- Tooltip embedded-editor focus hardening: Phase 8.
- LuaLS attribution in packaged resources: Phase 8.

The remaining concerns were intentionally not phased as defects because they were either already implemented, not reproduced as current production issues, or were narrower follow-on improvements rather than independently verified bugs.

## Suggested Test Inventory

- [LuaLanguageServerClientTests](TombLib/TombLib.Test/LuaLanguageServerClientTests.cs)
- [LuaLanguageServerIntellisenseProviderTests](TombLib/TombLib.Test/LuaLanguageServerIntellisenseProviderTests.cs)
- [LuaLanguageServerResponseParserTests](TombLib/TombLib.Test/LuaLanguageServerResponseParserTests.cs)
- [LuaIncrementalEditCalculatorTests](TombLib/TombLib.Test/LuaIncrementalEditCalculatorTests.cs)
- [LuaThemeConfigurationTests](TombLib/TombLib.Test/LuaThemeConfigurationTests.cs)

Add one optional integration test that starts bundled LuaLS only when the executable exists in the test environment.

## Recommended First Implementation Slice

If the next agent wants the smallest safe slice with maximum user impact, do this order:

1. Phase 1 transport resilience.
2. Phase 2 path rekeying.
3. Phase 3 signature / navigation stale guards.
4. Phase 4 completion `textEdit` correctness.
5. Phase 6 immediate diagnostic and semantic cleanup.

That order addresses the largest correctness and survivability problems before moving into indentation polish and secondary hardening.
