# Cursed Signal — v0.0.5-alpha.2

Vietnamese subtitle: **Chuyện Tâm Linh Không Đùa Được Đâu**

> v0.0.4 compiled successfully on a real Stardew/SMAPI setup. v0.0.5 changes the VHS/TV story gate and still needs a compile/runtime pass.

## v0.0.5 — VHS Installation Gate

The Cursed VHS now arms the haunting instead of being a reward after Sudoku appears.

### Prototype flow

1. The player receives the `Cursed VHS` story item for testing.
2. Its description hints that it should be used directly on the farmhouse TV.
3. Hold the tape, stand close to a TV, and press the action button.
4. The tape is consumed and permanently installed in the TV.
5. **The first cursed signal begins immediately at the moment the tape is inserted, regardless of the current time.**
6. When that first sequence ends, Sudoku is spawned and placed on a clear tile directly beside the player, facing them.
7. That immediate activation counts as today's signal, so the TV won't activate a second time at 8:00 on the same day.
8. From the following day onward, the installed tape activates at **8:00 AM every day**.
9. If the player is outside at 8:00, the signal can run the first time they return to the farmhouse that day.

### Story state

- `ronvotri.CursedSignal/CursedVHSGranted`
- `ronvotri.CursedSignal/CursedVHSInstalled`
- `ronvotri.CursedSignal/DailySignalDay`
- existing Sudoku arrival/NPC keys remain for save compatibility.

### Daily Sudoku

Daily Sudoku and weighted item rewards remain intact. Talk to Sudoku and choose `Giải.` or `Để sau.`; a solved board grants one reward per day.

## Console commands

- `cursedsignal_givevhs` — get the VHS for testing before installation.
- `sudoku_resetarrival` — clear arrival, VHS-install, and daily-signal story flags for a fresh test.
- `sudoku_testarrival` — force the haunted-TV sequence regardless of the tape gate.
- `sudoku_status` — prints VHS installed state, whether today's signal ran, and Sudoku NPC state.
- `sudoku_open`
- `sudoku_resetdaily`
- `sudoku_unlocknpc`

## Still deferred

- the final quest/location where the player discovers the tape;
- a custom TV/VCR visual showing the tape physically inserted;
- a shorter repeat-day signal animation;
- full i18n/localization;
- friendship/heart-event progression;
- multiplayer validation and controller polish.
