# v0.0.7-alpha.3 — Story Hook + Pencil Gate

This branch begins the first real story chapter for Hey! You’re Cursed!.

## The cursed VHS

The player doesn't simply receive the tape without explanation anymore.

At the farmhouse, a brown-paper parcel appears that the player doesn't remember ordering. It has no stamp and no sender name. The unsettling detail is the return address: it is the player's own farm address.

Inside are only:

- an old VHS tape;
- a handwritten note: **“DO NOT REWIND.” / “ĐỪNG TUA LẠI.”**

The sender and the reason the package returns to this farm are intentionally unresolved. Later story chapters can reveal those answers gradually.

## Pencil gate

Sudoku asking for a pencil is now a real story requirement, not a cosmetic dialogue choice.

- Pierre's shop sells a custom **Pencil / Bút chì** for **100g**.
- Sudoku will not complete the first activation and will not unlock the normal Sudoku loop until the player actually owns the Pencil.
- Giving the Pencil consumes one Pencil and marks the first activation complete.
- Arriving empty-handed does not complete the intro.
- Offering/answering with the hoe also does not complete the intro.

Vietnamese no-pencil/hoe reaction includes:

> “...Ngươi không muốn sống nữa hở?”

Sudoku then tells the player to buy a pencil from Pierre and return.

## Save behavior

- Existing progressed saves are not forced backwards through the Pencil gate.
- A fresh alpha.3 intro that is still waiting for a Pencil remains incomplete across save/load.
- Debug reset clears the fresh-story VHS/Pencil state for repeat testing.

## Test shortcuts

- `heyyourecursed_givepencil` — give one story Pencil immediately.
- `heyyourecursed_resetarrival` — reset the VHS/Sudoku arrival flow so the first chapter can be replayed.

## Test priority

1. Fresh/reset flow shows the mysterious parcel origin text.
2. Trigger the VHS/Sudoku arrival without a Pencil; Sudoku refuses activation.
3. Verify Pierre sells Pencil for 100g.
4. Return with the Pencil; one Pencil is consumed and Sudoku activates normally.
5. Save/reload while still missing the Pencil; the gate should remain intact.

The existing Spirit's Eve foundation remains in place. Full festival choreography is the next alpha.3 pass after this opening story hook is compile/game-tested.
