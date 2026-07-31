# Lua Language-Server Integration Tests

These tests launch the bundled Lua language server from `TombIDE/TombIDE.Shared/TIDE/LuaLS.zip` and run live end-to-end provider scenarios against it.

Prerequisites:

- The repository layout must still include `TombIDE/TombIDE.Shared/TIDE/LuaLS.zip` relative to the built test output.
- The extracted archive must contain `bin/lua-language-server.exe`.
- The tests are Windows-oriented because the bundled archive currently provides the Windows executable.

Runtime expectations:

- Each integration test can take several seconds because it extracts the LuaLS bundle, launches a real process, waits for diagnostics and semantic token round-trips, and may simulate a transport crash plus restart.
- Run them as focused integration slices rather than as part of every tight inner-loop unit-test run.

Skip behavior:

- When the bundled archive or executable is unavailable, the tests mark themselves inconclusive instead of failing unrelated development environments.