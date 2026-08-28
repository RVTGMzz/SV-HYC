# Chuyện Tâm Linh Ko Đùa Được Đâu — Sudoku prototype

Development branch for **v0.0.1: Sudoku TV Arrival Test**.

## Current test

1. Load a save and stay inside the farmhouse.
2. At **7:00 AM**, the haunted-TV sequence should start.
3. Six TV-emergence frames play in order. The prototype stores the small PNG as base64 text so the source branch can remain entirely text-based.
4. The prototype dialogue appears:
   - `......`
   - `Ngươi...`
   - `...có bút chì không?`
5. The event is marked as seen for that save.

If the player leaves home before 7:00 and returns later, the prototype will still try to start after re-entering.

## Console commands

- `sudoku_testarrival` — run the arrival immediately while inside the farmhouse.
- `sudoku_resetarrival` — clear the per-save seen flag so the event can trigger again.

## Config

SMAPI will generate `config.json` automatically.

Defaults:

```json
{
  "EnableArrivalTest": true,
  "TestArrivalTime": 700,
  "FrameDurationTicks": 18,
  "EventScale": 4.0
}
```

## Scope of v0.0.1

This branch tests only the haunted-TV arrival presentation and trigger/reset logic.

Not implemented yet:
- the brief well image/glitch before the TV emergence;
- Sudoku as a persistent custom NPC;
- portraits/dialogue progression;
- the cursed VHS item;
- the daily Sudoku minigame and rewards.

The old root `content.json` is a pre-prototype Content Patcher experiment and is not loaded by the SMAPI manifest on this branch.
