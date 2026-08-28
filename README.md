# Cursed Signal — Sudoku prototype v0.0.3

> Vietnamese subtitle: **Chuyện Tâm Linh Ko Đùa Được Đâu**

Development branch for **v0.0.3: Daily Sudoku**.

> Status: source-complete prototype, **not yet compiled or tested in Stardew Valley/SMAPI**.

## Current flow

1. The v0.0.2 haunted-TV arrival remains intact.
2. After the per-save `SudokuArrivalSeen` flag is set, Sudoku can exist as a custom NPC in the farmhouse.
3. Each in-game day gets one deterministic Sudoku puzzle.
4. Interact near Sudoku before claiming today's reward to open the 9×9 board.
5. Fill all editable cells and press **KIỂM TRA**.
6. A correct solution gives one reward for that day; reopening can't grant a second reward.
7. Closing the menu preserves the board in player `modData`, so the same puzzle can be resumed later that day.

## Puzzle bank

`assets/Data/Sudoku.puzzles.json` contains **18 pre-validated puzzles**:

- 6 Easy
- 6 Normal
- 6 Hard

The prototype chooses difficulty from friendship points:

- below 4 hearts: Easy
- 4–7 hearts: Normal
- 8+ hearts: Hard

If friendship data isn't available yet, it safely falls back to Easy.

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

## Daily rewards

For v0.0.3 rewards are deliberately simple and low-risk while the UI is untested:

- Easy: `250g`
- Normal: `500g`
- Hard: `900g`

The intended later design is to replace/augment this with thematic item rewards and friendship progression.

## Console commands

Existing:

- `sudoku_testarrival`
- `sudoku_resetarrival`
- `sudoku_unlocknpc`
- `sudoku_status`

New in v0.0.3:

- `sudoku_open` — open today's board immediately, even before the NPC flow is ready;
- `sudoku_resetdaily` — reset today's board and reward flag for repeated testing.

## Still intentionally deferred

- a proper `Giải / Để sau` dialogue choice before opening the menu;
- item rewards / cursed VHS rewards;
- pencil-note candidates inside cells;
- mistakes/highlighting assistance;
- animated portrait reactions while solving;
- final controller navigation polish;
- multiplayer authority/sync rules;
- full localization/i18n;
- balancing and final friendship progression.
