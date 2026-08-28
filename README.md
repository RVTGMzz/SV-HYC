# Hey! You’re Cursed! — v0.0.6-alpha.9

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

## alpha.9 — Dialogue → Sudoku handoff fix

- The direct `heyyourecursed_open` command already proved the Sudoku menu itself is healthy.
- Stardew may return prefixed response keys from question dialogues, so answer handling now accepts `board` / `solve` plus common prefixed forms instead of requiring an exact string.
- After `Đưa đây.`, the mod waits briefly for the answer click to settle, dismisses only the completed dialogue box, then opens `SudokuMenu` on the following update tick.
- SMAPI now logs the actual response key for the first-board and daily-board prompts so any remaining dialogue edge case is visible immediately.

## alpha.8 — Controller-first Sudoku controls

Controller handling is now routed through SMAPI while `SudokuMenu` is active, so the menu no longer depends only on Stardew forwarding gamepad buttons to `receiveGamePadButton`.

Controller layout:

- D-pad or left stick: move the selected cell.
- A: open the number picker for an editable cell.
- Left/right while the picker is open: choose 1–9.
- A again: place the highlighted number.
- X: erase the selected cell.
- Y or Start: check the board.
- B: cancel the number picker, or close the menu when the picker is closed.
- LB/RB or LT/RT: jump to the previous/next editable cell.

The board now highlights the selected row, column, 3×3 box, and matching values so controller navigation is easier to read.

## alpha.7 — Interaction + portrait + menu handoff hotfix

- TV/furniture actions are no longer stolen just because Sudoku is standing on a neighboring tile. Action-button interaction now requires Sudoku's actual tile to be targeted.
- Question dialogues pass Sudoku as the speaker so her portrait can be used by Stardew's dialogue UI.
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
