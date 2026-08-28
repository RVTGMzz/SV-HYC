# Hey! You’re Cursed! — v0.0.6-alpha.11

A paranormal Stardew Valley mod about a cursed VHS, Sudoku, and a ghost who treats your farmhouse like a very inconvenient address.

## alpha.11 — Reliable NPC interaction + ghost visual pass

- Old test saves which already reached day 2 with the cursed-TV arrival completed automatically recover the missing intro-complete flag, so talking to Sudoku opens the daily board.
- Controller A is accepted explicitly as an action input.
- NPC targeting now uses a short facing cone instead of an overly strict exact-tile comparison. Mouse targeting stays tight so clicks on the TV beside Sudoku still belong to the TV.
- After the intro, interacting with Sudoku always opens today’s Sudoku board, even if today’s reward was already claimed; duplicate rewards remain blocked by the puzzle service.
- Sudoku’s world sprite is 85% opaque and shifted upward by one source pixel (about four screen pixels at normal scale). Her shadow remains disabled, so she reads as slightly translucent and hovering instead of planted on the floor.

## Controller layout

- D-pad or left stick: move the selected cell.
- A: open the number picker for an editable cell.
- Left/right while the picker is open: choose 1–9.
- A again: place the highlighted number.
- X: erase the selected cell.
- Y or Start: check the board.
- B: cancel the number picker, or close the menu when the picker is closed.
- LB/RB or LT/RT: jump to the previous/next editable cell.

## Current core flow

1. Receive the prototype Cursed VHS.
2. Hold it and interact with the farmhouse TV.
3. The VHS is consumed and installed permanently.
4. First signal runs immediately: static → glitch → Sudoku emergence.
5. Sudoku materializes beside the player.
6. The one-time two-way introduction runs.
7. The first Sudoku board opens after the intro.
8. From then on, talking to Sudoku opens the current daily board directly.
9. On later days Sudoku stays hidden before 8:00, then materializes after the short daily signal.

The old procedural well scene remains removed.

## Build

Double-click `Build_HeyYoureCursed.bat`.

Successful builds deploy to `Mods/HeyYoureCursed`.

## Useful test commands

- `heyyourecursed_resetarrival`
- `heyyourecursed_status`
- `heyyourecursed_open`
- `heyyourecursed_resetdaily`
- `heyyourecursed_unlocknpc`
- `heyyourecursed_testarrival`
- `heyyourecursed_givevhs`

`heyyourecursed_open` remains the direct menu diagnostic.
