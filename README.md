# Cursed Signal — v0.0.6-alpha.1

Vietnamese subtitle: **Chuyện Tâm Linh Không Đùa Được Đâu**

> v0.0.4 compiled successfully on a real Stardew/SMAPI setup. v0.0.6 still needs one compile/runtime pass.

## v0.0.6 — Daily Materialization

The first VHS activation and the later 8:00 AM hauntings now behave differently.

### First activation

1. Receive the `Cursed VHS` prototype item.
2. Hold it, stand close to the farmhouse TV, and press the action button.
3. The tape is consumed and permanently installed in the TV.
4. The full signal starts immediately: static → well → glitch → TV emergence animation.
5. When the sequence ends, Sudoku materializes on a clear tile directly beside the player and faces them.
6. The activation counts as today's signal, so 8:00 AM won't fire a second time that same day.

### Repeat mornings

From the next day onward:

1. Sudoku is kept off-map before the daily signal so she isn't already standing in the farmhouse at 6:00 AM.
2. At **8:00 AM**, the TV performs a much shorter static/glitch burst instead of replaying the full well/crawl scene.
3. Sudoku materializes beside the player when the burst ends.
4. If the player is outside at 8:00, Sudoku remains hidden and the short signal runs the first time the player returns to the farmhouse that day.
5. The signal can only complete once per in-game day.

Repeat signals use a lightweight global message instead of interrupting the player with a full dialogue box every morning.

### Config

- `DailySignalTime` = `800`
- `RepeatStaticTicks` = `22`
- `RepeatGlitchTicks` = `14`

The first-arrival timings remain separately configurable.

### Daily Sudoku

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

- the final quest/location where the player discovers the tape;
- a custom TV/VCR visual showing the tape physically inserted;
- full i18n/localization;
- friendship/heart-event progression;
- multiplayer validation and controller polish.
