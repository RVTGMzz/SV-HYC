# Chuyện Tâm Linh Ko Đùa Được Đâu — Sudoku prototype

Development branch for **v0.0.2: TV Arrival + Custom NPC foundation**.

## What changed from v0.0.1

- the base64 event image workaround was removed;
- real PNG assets are now used;
- a brief haunted-VHS presentation was added:
  1. static;
  2. a corrupted glimpse of a well;
  3. glitch;
  4. six TV-emergence frames;
- Sudoku now has `Data/Characters` data, a 16×32 movement sheet, portraits, dialogue, and a basic stay-at-home schedule;
- Sudoku is locked behind the per-save arrival flag;
- after the arrival flag is set, the game's spawn-if-missing system should make her eligible to appear as a normal NPC on the next save load/day rollover.

## Important prototype limitation

This branch has **not been tested in-game yet**.

Custom NPC spawning is deliberately deferred to the game's normal `Data/Characters` spawn pass. That means the arrival cutscene can finish at 7:00 AM, but the real NPC is expected to become persistent after sleeping/reloading rather than being force-inserted mid-day.

This avoids creating a duplicate or partially initialized NPC while the prototype is still untested.

## Console commands

- `sudoku_testarrival` — run the arrival immediately in the farmhouse.
- `sudoku_resetarrival` — clear the per-save arrival flag.
- `sudoku_unlocknpc` — set the flag without playing the arrival; sleep/reload afterwards.
- `sudoku_status` — print the flag / NPC / sequence state.

## Current asset sizes

- `assets/Characters/Sudoku.png`: 64×128, 4×4 movement sheet, 16×32 per frame.
- `assets/Portraits/Sudoku.png`: 256×128, 8 portraits, 64×64 each.
- `assets/Events/Sudoku_TV.png`: 192×128, 6 event frames, 64×64 each.

## Still intentionally deferred

- exact TV location anchoring in every farmhouse upgrade/map replacement;
- cursed VHS as an inventory item;
- gifts and friendship balancing;
- daily Sudoku minigame;
- daily rewards;
- roommate progression;
- localization/i18n pass.
