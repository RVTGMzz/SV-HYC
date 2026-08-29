# v0.0.7-alpha.3.2 — Persistent Saloon Prologue Story Gate

## Canon flow

1. A fresh/reset story state starts with **no cursed VHS**.
2. Each morning Gus leaves an invitation/reminder asking the farmer to visit the Stardrop Saloon around 18:00.
3. If the farmer skips the gathering, nothing paranormal happens yet. The invitation returns on later days, with HUD reminders at 17:30 and 18:00.
4. Entering the Saloon at/after 18:00 starts the prologue. If the farmer entered early and stayed inside, it starts when the clock reaches 18:00.
5. The villagers swap paranormal stories. The farmer makes a careless remark; Gus and Abigail warn that paranormal matters are not something to joke about.
6. The Wizard happens to pass outside, overhears the remark, and says: “Hừm... chưa từng gặp sao? Vậy thì ta sẽ cho ngươi thấy.”
7. The VHS does **not** appear that same evening. The following morning an anonymous brown-paper parcel appears with the cursed tape, the “DO NOT REWIND” note, and the almost-erased `18` clue.
8. From there the existing TV → Sudoku → real Pencil gate continues unchanged. Pierre sells the Pencil for 100g; Sudoku never tells the player that Pierre has it.

## Non-linear safety

The Saloon scene is a persistent story gate, not a one-night missable event. The player can delay it as long as desired, but the haunting cannot begin until the gathering has actually been attended.

## Compatibility

Existing saves that already have pre-alpha.3.2 VHS/Sudoku progress are automatically grandfathered past the Saloon prologue. Use `heyyourecursed_resetarrival` to deliberately replay the new flow from the invitation.

## Test commands

- `heyyourecursed_resetarrival` — reset to the Saloon invitation; no VHS is granted.
- `heyyourecursed_test_prologue` — while standing in the Saloon, force the clock to 18:00 if needed and replay the scene.
- `heyyourecursed_givevhs` — debug bypass that can still grant the VHS directly.
- `heyyourecursed_givepencil` — give one Pencil directly.

Sudoku's successful Pencil acceptance now stores `SudokuActivatedDay`, which will be used by the later seven-day Occult Cabinet unlock pass.
