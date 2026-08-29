# v0.0.7-alpha.3.4 — Active Haunting + Seal/Unseal Sudoku

> Windows test ZIP is the canonical alpha.3.4 test source until this pass is compile/game-tested and promoted into the repository's older split-source layout.

## Active Haunting foundation

- The Occult Cabinet is the permanent switchboard for hauntings.
- Only one haunting can be actively connected at a time.
- `Cursed VHS — Sudoku` can move between **Active Connection** and **Sealed**.
- Sealing Sudoku removes her NPC from the world but preserves Trust, all 18 Stage clears, Daily Sudoku data, Endless progression, and story flags.
- Unsealing restores Sudoku immediately with the same progression.
- A sealed Sudoku does not respawn from the daily TV signal and does not appear at Spirit's Eve.

## Cabinet UI

The placed Occult Cabinet now exposes two slots:

- **Active Connection**
- **Sealed**

With only Sudoku implemented, the player can seal/unseal the Cursed VHS. Future ghosts can reuse the same one-active-haunting contract without resetting existing entities.

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
