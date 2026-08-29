# Hey! You’re Cursed! — v0.0.6-alpha.13

A paranormal Stardew Valley mod about a cursed VHS, Sudoku, and a ghost who slowly stops pretending she does not enjoy your company.

## alpha.13 — Stage Select + unique-clear progression

This build turns the Sudoku system into two connected modes instead of one random daily board.

### Flow

1. Talk to Sudoku.
2. Her portrait conversation plays for the day.
3. Choose **Chơi**.
4. The **Sudoku Stage Select** hub opens.
5. Choose either:
   - **Daily Challenge** — repeatable once per day for the normal item/gold reward.
   - **Stage 01–18** — permanent progression; clearing a unique Stage advances Sudoku's bond dialogue.

### Stage progression

There are currently 18 fixed Stages:

- Easy 01–06
- Normal 07–12
- Hard 13–18

Stages unlock sequentially. Replaying a cleared Stage is allowed, but it does **not** increase progression again and does not grant the Daily Challenge reward.

The bond/progression counter is now the number of **unique Stages cleared**, not the number of daily rewards claimed:

- 0–2: creepy / threatening
- 3–6: less hostile
- 7–12: familiar
- 13–17: close
- 18/18: completion dialogue pool

Old alpha.12 `SudokuSolvedCount` save data is migrated once into the first N cleared Stages (capped at 18), then the unique Stage-clear set becomes the source of truth.

### Daily Challenge

Daily Challenge remains the only repeatable daily reward source.

Its difficulty follows Stage progress instead of vanilla friendship:

- 0–5 unique clears → Easy
- 6–11 → Normal
- 12+ → Hard

Finishing Daily Challenge does not increase Stage/bond progress.

### Menu/controller

Stage Select supports mouse, keyboard, D-pad, and left stick.

- D-pad / Left Stick: move selection
- A: open selected Daily Challenge / Stage
- B: close
- Cleared Stages show a completion mark
- Locked Stages cannot be opened
- The first uncleared unlocked Stage is preferred when moving down from Daily Challenge

The Sudoku board keeps the controller-first controls from alpha.12:

- D-pad / Left Stick: move cell
- A: open number picker
- Left/Right: select 1–9
- A again: place number
- X: erase
- Y / Start: check
- LB/RB or LT/RT: previous/next editable cell
- B / Esc: return to Stage Select when the board was opened from the hub

The footer remains separated from the action buttons so instructions do not overlap the UI.

## Sudoku content

Current puzzle bank: **18 boards total**.

- 6 Easy
- 6 Normal
- 6 Hard

These 18 boards are the complete current Stage set. No Chapter 2/Stage 19–36 content is being added yet.

## Ghost presentation

Sudoku keeps the alpha.11 ghost pass:

- 85% world-sprite opacity
- sprite shifted slightly upward
- no shadow

Her portrait remains fully opaque in conversation UI.

## Build

Close Stardew Valley and SMAPI, then double-click:

`Build_HeyYoureCursed.bat`

Successful builds deploy to:

`Mods/HeyYoureCursed`

## Useful test commands

- `heyyourecursed_stages` — open Stage Select directly
- `heyyourecursed_open` — open today's Daily Challenge directly
- `heyyourecursed_status`
- `heyyourecursed_resetdaily`
- `heyyourecursed_resetarrival`
- `heyyourecursed_unlocknpc`
- `heyyourecursed_testarrival`
- `heyyourecursed_givevhs`

`heyyourecursed_status` reports Stage clear progress and today's Daily Challenge state.
