# Hey! You’re Cursed! — v0.0.6-alpha.12

A paranormal Stardew Valley mod about a cursed VHS, Sudoku, and a ghost who slowly becomes less threatening the more puzzles you solve together.

## alpha.12 — Daily portrait dialogue + Sudoku UI polish

This build replaces the fragile vanilla question-dialogue handoff with a purpose-built Sudoku conversation menu.

### Daily Sudoku conversation

- Talking to Sudoku no longer jumps straight into the board.
- The first interaction each day shows 1–2 short lines with Sudoku's portrait, then asks whether you want to play.
- `Chơi` opens today's Sudoku board; `Để sau` closes the conversation cleanly.
- Talking again on the same day uses a shorter repeat line instead of replaying the full daily dialogue.
- If today's puzzle was already solved, the prompt becomes an invitation to reopen the board instead of granting another reward.

### Relationship through puzzle progress

Sudoku's tone is driven by the number of daily boards successfully solved:

- 0–2 solves: threatening / unsettling.
- 3–6 solves: less hostile, still defensive.
- 7–12 solves: familiar and noticeably softer.
- 13+ solves: openly attached, though she still tries to deny it.

Dialogue is deterministic per save/day so reloading doesn't constantly reroll her mood. Multiple portrait frames are used across progression stages instead of leaving the portrait sheet unused.

### First meeting

The one-time introduction now uses the same custom portrait conversation UI, including the TV joke and pencil/hoe branch. After the intro, Sudoku immediately continues into the daily invitation flow. This removes the old `DialogueBox -> callback -> SudokuMenu` failure path entirely.

### Sudoku menu layout

- Responsive grid size based on viewport height.
- `XÓA` and `KIỂM TRA` stay in their own action row.
- Controller/keyboard instructions now live in a dedicated footer panel below the buttons.
- Status text is wrapped inside the footer instead of overlapping the controls.
- Controller-first controls remain intact.

### Ghost visual

- World sprite stays at 85% opacity.
- Sprite remains lifted slightly above the floor.
- No shadow, preserving the floating ghost look.

## Controller layout

Conversation:
- D-pad / left stick: choose response.
- A: confirm.
- B: decline / close.

Sudoku:
- D-pad or left stick: move cell.
- A: open number picker.
- Left/right: choose 1–9.
- A again: place number.
- X: erase.
- Y or Start: check board.
- B: cancel picker / close board.
- LB/RB or LT/RT: jump to previous/next editable cell.

## Build

Double-click `Build_HeyYoureCursed.bat` while Stardew Valley and SMAPI are closed.

Successful builds deploy to:

`Stardew Valley/Mods/HeyYoureCursed`

## Useful test commands

- `heyyourecursed_open`
- `heyyourecursed_status`
- `heyyourecursed_resetdaily`
- `heyyourecursed_resetarrival`
- `heyyourecursed_unlocknpc`
- `heyyourecursed_testarrival`
- `heyyourecursed_givevhs`

`heyyourecursed_status` now also reports total solved Sudoku boards and whether today's full daily conversation has already played.
