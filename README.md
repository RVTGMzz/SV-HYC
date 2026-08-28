# Hey! You’re Cursed! — v0.0.6-alpha.7

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

## alpha.7 — Interaction + portrait + menu handoff hotfix

- TV/furniture actions are no longer stolen just because Sudoku is standing on a neighboring tile. Action-button interaction now requires Sudoku's actual tile to be targeted.
- Question dialogues pass Sudoku as the speaker so her portrait can be used by Stardew's dialogue UI.
- Choosing `Đưa đây.` no longer force-replaces the active `DialogueBox` in the same update frame. The mod waits until Stardew closes the dialogue naturally, then opens `SudokuMenu` on the next clean frame.
- All debug commands now use the `heyyourecursed_` prefix so an old prototype can't crash mod entry by registering the same command names.

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

- `heyyourecursed_resetarrival`
- `heyyourecursed_status`
- `heyyourecursed_open`
- `heyyourecursed_resetdaily`
- `heyyourecursed_unlocknpc`
- `heyyourecursed_testarrival`
- `heyyourecursed_givevhs`

`heyyourecursed_open` is the direct diagnostic: if it opens the board, the menu itself is healthy and any remaining issue is in the conversation transition.
