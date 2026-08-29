# alpha.3.4 Windows Test Snapshot

## Package

`HeyYoureCursed_v0.0.7-alpha.3.4_ActiveHaunting_SealUnseal_Source.zip`

SHA-256:

`c466558a52cf2d70f45fbda010abd2f77d2809024781d57e87fd60220043becf`

## Status

**Pre-test / not yet promoted as a stable checkpoint.**

The package passed the static packaging checks used during development (JSON/key/ZIP integrity), but the development environment that produced it does not have the Windows .NET/SMAPI build toolchain available. The Windows compile and in-game pass must still be done before this snapshot is called stable.

## Expected feature set

- Persistent Stardrop Saloon prologue; skipping the invitation delays the haunting instead of skipping the story.
- Wizard overhears the farmer's careless paranormal remark.
- Cursed VHS appears on the following morning.
- Real Pencil gate; Pierre sells Pencil for 100g; the Pierre hint is the farmer's thought, not Sudoku's knowledge.
- Seven-day timer starts only after Sudoku accepts the Pencil.
- Seven-day Wizard reveal and placeable Occult Cabinet.
- Free haunting-framework unlock after seven days; Sudoku 18/18 is not required to switch future Cases.
- One Active Haunting at a time.
- Seal/unseal `Cursed VHS — Sudoku` without losing Trust, Stage, Daily/Endless, or story progress.
- Sealed Sudoku stays absent across day changes/save reloads and is not injected into Spirit's Eve.

## Primary test pass

1. `heyyourecursed_resetarrival`.
2. Skip the Saloon invitation for one day and verify the reminder returns.
3. Attend the Saloon after 18:00 and finish the prologue.
4. Sleep; verify the VHS package appears the following morning, not the same night.
5. Trigger Sudoku without a Pencil; verify the Pencil gate and farmer thought.
6. Buy Pencil from Pierre for 100g and complete activation.
7. Use `heyyourecursed_test_wizard` or wait seven full days.
8. Finish the Wizard reveal and place the Occult Cabinet.
9. Seal Sudoku; sleep a day; verify she does not respawn and progress is unchanged.
10. Unseal Sudoku; verify she returns immediately with the same progress.
11. Save/reload once while sealed and once while active.

When this pass is clean, create a stable alpha.3.4 checkpoint/tag from the tested commit rather than treating this pre-test snapshot as final.
