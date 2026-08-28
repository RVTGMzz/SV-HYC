# Hey! You’re Cursed! — v0.0.6-alpha.6

A paranormal Stardew Valley mod about a cursed VHS, Sudoku, and a ghost who treats your farmhouse like a very inconvenient address.

## Identity cleanup

This build completes the rename while the project is still pre-release:

- display name: `Hey! You’re Cursed!`
- repository: `ronvotri/HeyYoureCursed`
- SMAPI UniqueID: `ronvotri.HeyYoureCursed`
- assembly/DLL: `HeyYoureCursed.dll`
- namespace/project: `HeyYoureCursed`
- deployed Mods folder and release ZIP name: `HeyYoureCursed`

Early `ronvotri.CursedSignal` prototype keys/NPC IDs are still recognized for test-save cleanup/migration, but all new state uses the final ID.

## alpha.6 — Sudoku menu hardening

The final dialogue choice now queues the Sudoku board for a short delay. If Stardew leaves the final `DialogueBox` alive after its response callback, the mod dismisses only that stale dialogue and opens `SudokuMenu` on a clean update frame. This fixes the alpha.5 case where choosing `Đưa đây.` could leave the minigame queued forever.

Diagnostics now log when the menu is queued/opened and `sudoku_status` includes the pending-menu and active-menu state.

## Current core flow

1. Receive the prototype Cursed VHS.
2. Hold it and interact with the farmhouse TV.
3. The VHS is consumed and installed permanently.
4. First signal runs immediately: static → glitch → Sudoku emergence.
5. Sudoku materializes beside the player.
6. First conversation runs before the puzzle is offered.
7. Choosing `Đưa đây.` opens the actual 9×9 Sudoku menu.
8. On later days Sudoku stays hidden before 8:00, then materializes after the short daily signal.

The old procedural well scene remains removed. If a well shot returns later, it should be a purpose-built pixel-art event asset rather than a fake procedural overlay.

## Build

Double-click `Build_HeyYoureCursed.bat`.

`Stardew.ModBuildConfig` is explicitly configured with `<ModFolderName>HeyYoureCursed</ModFolderName>`, so successful builds deploy to `Mods/HeyYoureCursed` and use the same clean name for release ZIPs.

## Useful test commands

- `sudoku_resetarrival`
- `sudoku_status`
- `sudoku_open`
- `sudoku_resetdaily`
- `sudoku_unlocknpc`
- `heyyourecursed_givevhs`

`sudoku_open` is the direct diagnostic: if it opens the board, the menu itself is healthy and any remaining issue is in the conversation transition.
