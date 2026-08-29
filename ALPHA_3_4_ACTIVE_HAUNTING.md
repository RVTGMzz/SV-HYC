# v0.0.7-alpha.3.4 — Active Haunting + Seal/Unseal Sudoku

## Active Haunting foundation

- The Occult Cabinet is the permanent switchboard for hauntings.
- Only one haunting can be actively connected at a time.
- `Cursed VHS — Sudoku` can move between **Active Connection** and **Sealed**.
- Sealing Sudoku removes her NPC from the world but preserves Trust, all 18 Stage clears, Daily Sudoku data, Endless progression, and story flags.
- Unsealing restores Sudoku immediately with the same progression.
- A sealed Sudoku does not respawn from the daily TV signal and does not appear at Spirit's Eve.
- The framework becomes freely available after the seven-day Wizard/Cabinet milestone; 18/18 is completion of Sudoku's Case, not a prerequisite to switch hauntings.

## Cabinet UI

The placed Occult Cabinet exposes two slots:

- **Active Connection**
- **Sealed**

With only Sudoku implemented, the player can seal/unseal the Cursed VHS. Future ghosts reuse the same one-active-haunting contract and keep their own progression/minigame state.

## Future Case rule

After the initial seven-day tutorial period with Sudoku, future hauntings should be selectable non-linearly. A future ghost may have its own 18-minigame track, but the mod should not force Ghost A 18/18 → Ghost B → Ghost C.

## Character response

Sudoku's seal and return lines vary by Trust. High-Trust Sudoku can admit that she does not want to stay inside the tape for too long.

## Test shortcuts

- `heyyourecursed_test_seal`
- `heyyourecursed_test_unseal`
- `heyyourecursed_status` reports `activeHaunting` and `sudokuSealed`.

## Test checklist

1. Finish/skip to the Wizard reveal and place the Occult Cabinet.
2. Seal Sudoku and confirm she disappears.
3. Verify Trust and Stage progress do not change.
4. Sleep one day and confirm the daily signal does not respawn her.
5. Unseal the VHS and confirm Sudoku returns immediately.
6. Save/reload while sealed and while active to verify persistence.
