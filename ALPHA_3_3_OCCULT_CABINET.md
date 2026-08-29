# v0.0.7-alpha.3.3 — Seven-Day Wizard Reveal + Occult Cabinet

This pass adds the second major story milestone after Sudoku's Pencil activation.

## Seven-day clock

The clock begins only when Sudoku actually accepts the Pencil and the first conversation completes. Receiving the VHS, inserting it, or merely seeing Sudoku does **not** start this timer.

After seven full in-game days have elapsed, the Wizard's reveal is queued. The scene waits until the farmer is inside the farmhouse with no other menu/event active, so it won't interrupt festivals or another cutscene.

## Wizard reveal

The Wizard admits he heard the farmer at the Stardrop Saloon. He asks whether the farmer has learned the lesson, notices that the farmer may have grown attached to Sudoku, and gives the farmer an **Occult Cabinet**.

The farmer can answer one of three ways:

- `Rồi. Tôi tin rồi.`
- `Ông làm chuyện này thật à?`
- `...Tôi cũng quen với cô ấy rồi.`

The choice is stored for future story callbacks but doesn't branch progression yet.

## Occult Cabinet

- Custom placeable big-craftable, intended for the farmhouse.
- Indoor-only.
- The farmer chooses where to place it; the mod does not rearrange the farmhouse.
- Interacting with it shows the haunting-system status.
- Sudoku remains the active haunting after the unlock.
- Future Cases use this cabinet to seal the active anchor and switch hauntings without resetting each entity's progress.

## Canon unlock rule

The Cabinet and free haunting-switch framework unlock after **seven days with Sudoku**, not after Sudoku reaches 18/18. Each future haunting can still have its own 18-minigame completion track, but players aren't forced to finish one entire ghost before trying another.

## Test commands

- `heyyourecursed_test_wizard` — marks the activation clock as seven days old and arms the Wizard scene.
- `heyyourecursed_givecabinet` — gives another Occult Cabinet for placement testing.
- `heyyourecursed_status` — reports days since Sudoku activation, Wizard reveal state, Cabinet unlock state, and active haunting ID.
