# Cursed Signal — v0.0.6-alpha.2

Vietnamese subtitle: **Chuyện Tâm Linh Không Đùa Được Đâu**

> Core stabilization checkpoint. Build and runtime validation are still required on the real Stardew Valley + SMAPI install.

## Goal of this pass

Lock down the complete core loop before adding more content:

`Cursed VHS → install into TV → immediate first signal → Sudoku beside player → save/load/day rollover → short 8:00 AM materialization on later days`

## Core invariants

The stabilization pass tries to keep these states deterministic:

- Before VHS installation, the player should have exactly one prototype Cursed VHS and Sudoku should not exist yet.
- Installing the tape consumes every stray duplicate copy and marks the VHS as permanently installed.
- The first signal starts immediately and uses the full static → well → glitch → emergence sequence.
- Finishing the first signal creates exactly one Sudoku and places her on a valid nearby tile when possible.
- That first signal counts as the current day's signal, so 8:00 does not trigger twice on the installation day.
- On later days Sudoku stays invisible before the daily signal, then materializes beside the player after the short 8:00 burst.
- If the player is away at 8:00, the signal waits until the first farmhouse re-entry after 8:00.
- Save/load and day rollover reconcile the story flags, VHS inventory, and Sudoku instance instead of blindly creating another NPC.

## Stabilization changes

- Duplicate Sudoku instances are collapsed to one canonical NPC.
- Legacy-ID Sudoku instances are preserved instead of spawning a second new-ID NPC.
- Stale Sudoku instances are removed when the save has no completed arrival state.
- Pre-signal hiding now uses the NPC invisibility flag instead of parking Sudoku at negative map coordinates.
- Materialization searches nearby valid NPC-spawn tiles before using a safe farmhouse fallback.
- VHS inventory is normalized: one tape before installation, zero after installation.
- `sudoku_resetarrival` now performs a deterministic full core reset and returns one fresh test tape.
- Event textures load through a guarded path. Missing/bad event art logs an error and falls back instead of crashing the whole save flow.
- Returning to title clears transient sequence/texture state.
- The player is halted while a TV signal sequence is running to reduce movement/warp edge cases.
- Project and manifest versions are aligned to `0.0.6-alpha.2`.
- `Build_CursedSignal.bat` performs a clean Debug build before launching the test cycle.

## Test matrix

### A — Fresh flow

1. Load a test save and run `sudoku_resetarrival`.
2. Run `sudoku_status`.
3. Confirm the VHS is in inventory and Sudoku is absent.
4. Hold the VHS, stand close to a farmhouse TV, and press the action button.
5. Confirm the VHS disappears, the full first signal runs, and exactly one Sudoku appears beside the player.

### B — Same-day duplicate prevention

1. After the first arrival, wait until/past 8:00 on the same day.
2. Confirm the TV does not trigger a second time.
3. Run `sudoku_status` and confirm `npcCount=1`, `vhsInstalled=true`, `vhsInventoryCount=0`, and `signalRanToday=true`.

### C — Next morning

1. Sleep to the next day.
2. At 6:00, Sudoku should be hidden.
3. At 8:00, the short static/glitch signal should run once.
4. Sudoku should materialize beside the player.

### D — Away at 8:00

1. Leave the farmhouse before 8:00.
2. Stay outside past 8:00.
3. Re-enter the farmhouse.
4. The short signal should run then, once, and Sudoku should materialize beside the player.

### E — Save/load reconciliation

Test save/load both before and after the daily signal. The core state should survive without duplicate Sudoku NPCs, extra VHS copies, or a repeated signal on the same day.

### F — Diagnostics

Run `sudoku_status` whenever the state looks wrong. Important fields are:

- `npcCount` — should never exceed 1.
- `npcInvisible` — normally true while waiting for the morning signal, false after materialization.
- `vhsInstalled` — true after the tape has been used on the TV.
- `vhsInventoryCount` — 1 before installation, 0 after installation.
- `signalRanToday` — prevents duplicate daily activations.
- `sequenceActive` / `firstSequence` — helps diagnose interrupted animations.

## Existing Daily Sudoku

Daily Sudoku and weighted item rewards remain intact. Talk to Sudoku and choose `Giải.` or `Để sau.`; a solved board grants one reward per day.

## Console commands

- `cursedsignal_givevhs`
- `sudoku_resetarrival`
- `sudoku_testarrival`
- `sudoku_status`
- `sudoku_open`
- `sudoku_resetdaily`
- `sudoku_unlocknpc`

## Still deferred

- final quest/location where the player discovers the tape;
- custom TV/VCR visual showing the inserted tape;
- living/idle behavior around the farmhouse;
- full i18n/localization;
- friendship/heart-event progression;
- multiplayer validation and controller polish.
