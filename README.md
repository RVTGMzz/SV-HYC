# Cursed Signal — v0.0.5-alpha.1

Vietnamese subtitle: **Chuyện Tâm Linh Không Đùa Được Đâu**

> v0.0.4 compiled successfully on a real Stardew/SMAPI setup. v0.0.5 changes the VHS/TV story gate and still needs one compile/runtime pass.

## v0.0.5 — VHS Installation Gate

The Cursed VHS is now the thing that arms the haunting instead of being a reward after Sudoku appears.

### Prototype flow

1. The player receives the `Cursed VHS` story item for testing.
2. Its description hints that it should be used directly on the farmhouse TV.
3. Hold the tape, stand close to a TV, and press the action button.
4. The tape is consumed and marked as permanently installed in the TV.
5. Before installation, no automatic haunted-TV signal can run.
6. Once installed, the signal is armed for **8:00 AM every day**.
7. If the player is outside at 8:00, the signal can run the first time they return to the farmhouse that day.
8. The first successful signal unlocks/spawns Sudoku; later days can activate the signal again.

If the tape is installed after 8:00 AM, the first automatic activation waits until the next morning instead of firing immediately.

### Story state

- `ronvotri.CursedSignal/CursedVHSGranted`
- `ronvotri.CursedSignal/CursedVHSInstalled`
- `ronvotri.CursedSignal/DailySignalDay`
- existing Sudoku arrival/NPC keys remain for save compatibility.

### Daily Sudoku

Daily Sudoku and the weighted item reward pools from v0.0.4 remain intact. Talk to Sudoku and choose `Giải.` or `Để sau.`; a solved board grants one reward per day.

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
