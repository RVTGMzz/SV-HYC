# v0.0.7-alpha.3.6 — Inventory and Locale Fix

## Build

1. Extract this source folder on Windows.
2. Run `Build_HeyYoureCursed.bat`.
3. `CS9057` from `SMAPI.ModBuildConfig.Analyzer.dll` is only a compiler-version warning. The build is successful when there are no `error CS...` lines.
4. Copy the generated `bin\Debug\net6.0\HeyYoureCursed` folder into the Stardew Valley `Mods` folder.

## Regression checks

- Set the game language to English, load a save, and talk to Sudoku on Tuesday. The line should read: `The number 7 can't appear twice in the same row.`
- Set the game language to English and install the VHS. The install message and all Sudoku dialogue should be English.
- Fill every inventory slot, then save and reload. No `ItemGrabMenu` should open with a second Cursed VHS. Leave one empty slot and use `heyyourecursed_givevhs` or advance to the next grant attempt; exactly one VHS should be added.
- Reopen the first Sudoku conversation. The two long answer buttons should wrap inside their panels without overlapping.
- Run `heyyourecursed_status` and confirm `VHS inventory: 0/1` after installation, or `1/1` before installation.

