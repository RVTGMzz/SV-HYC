# Cursed Signal

> **Chuyện Tâm Linh Ko Đùa Được Đâu** — Vietnamese subtitle / original working title.

Development branch for **v0.0.3.1: Cursed Signal Identity Cleanup**.

> Status: source-complete prototype, **not yet compiled or tested in Stardew Valley/SMAPI**.

## Identity cleanup in v0.0.3.1

The project now uses the Cursed Signal identity consistently:

- mod name: `Cursed Signal`;
- SMAPI UniqueID: `ronvotri.CursedSignal`;
- DLL / project: `CursedSignal.dll` / `CursedSignal.csproj`;
- C# namespace: `CursedSignal`;
- Sudoku NPC ID: `ronvotri.CursedSignal_Sudoku`;
- save-state prefix: `ronvotri.CursedSignal/...`.

### Legacy save compatibility

Older prototype builds used `ronvotri.chuyentamlinhkoduaduocdau` in NPC IDs and `modData` keys. v0.0.3.1 keeps one-way compatibility helpers:

- old arrival state is copied to the new Cursed Signal key automatically;
- old Daily Sudoku day, puzzle, board, and claimed-reward state are copied if the new keys don't exist;
- legacy keys are **not deleted**, so rolling back to an older development branch is safer;
- runtime asset lookup still recognizes the old Sudoku NPC ID if a test save contains it;
- Daily Sudoku difficulty can still read friendship data stored under the legacy NPC ID.

The test commands `sudoku_resetarrival` and `sudoku_resetdaily` intentionally clear both new and legacy prototype flags.

## Current gameplay prototype

1. Stay in the farmhouse until **7:00 AM** to trigger the haunted-TV arrival test.
2. TV static → brief well glimpse → glitch → Sudoku crawls out.
3. After arrival, Sudoku can exist as a custom NPC in the farmhouse.
4. Each in-game day gets one deterministic 9×9 Sudoku board.
5. Interact near Sudoku before claiming the reward to open the board.
6. Correct solutions grant one reward for that day.
7. Closing the menu preserves the board so it can be resumed later that day.

## Puzzle bank

`assets/Data/Sudoku.puzzles.json` currently contains **18 pre-validated puzzles**:

- 6 Easy;
- 6 Normal;
- 6 Hard.

Prototype difficulty by friendship:

- below 4 hearts: Easy;
- 4–7 hearts: Normal;
- 8+ hearts: Hard.

If friendship data isn't available yet, it falls back to Easy.

## Controls

### Mouse / keyboard

- click an editable cell;
- click `1–9` or press number keys;
- `Backspace`, `Delete`, or `0` clears the selected cell;
- arrow keys move selection;
- `Enter` checks the board;
- `Esc` closes the board.

### Controller prototype

- D-pad: move selection;
- `A`: cycle selected cell forward;
- `LB/RB`: cycle backward/forward;
- `X`: clear;
- `Y`: check;
- `B`: close.

## Test commands

- `sudoku_testarrival`
- `sudoku_resetarrival`
- `sudoku_unlocknpc`
- `sudoku_status`
- `sudoku_open`
- `sudoku_resetdaily`

## Still intentionally deferred

- proper `Giải / Để sau` dialogue choice;
- cursed VHS item and lore flow;
- item reward pool;
- pencil-note candidates;
- mistake/highlight assistance;
- animated portrait reactions;
- multiplayer authority/sync rules;
- full i18n/localization;
- final friendship progression and balance.
