# v0.0.7-alpha.3.1 — Story Hook + Player Thought

This branch begins the first real story chapter for Hey! You’re Cursed!.

## The cursed VHS

The player doesn't simply receive the tape without explanation anymore.

At the farmhouse, a brown-paper parcel appears that the player doesn't remember ordering. It has no stamp and no sender name. The unsettling detail is the return address: it is the player's own farm address.

Inside are only:

- an old VHS tape;
- a handwritten note: **“DO NOT REWIND.” / “ĐỪNG TUA LẠI.”**
- one small unresolved clue: the number **18** is written in pencil on the tape label, then almost completely erased.

The sender, the erased 18, and the reason the package returns to this farm are intentionally unresolved. Later story chapters can reveal those answers gradually.

## Pencil gate

Sudoku asking for a pencil is a real story requirement, not a cosmetic dialogue choice.

- Pierre's shop sells a custom **Pencil / Bút chì** for **100g**.
- Sudoku will not complete the first activation and will not unlock the normal Sudoku loop until the player actually owns the Pencil.
- Giving the Pencil consumes one Pencil and marks the first activation complete.
- Arriving empty-handed does not complete the intro.
- Offering/answering with the hoe also does not complete the intro.

Vietnamese no-pencil/hoe reaction includes:

> “...Ngươi không muốn sống nữa hở?”

Sudoku only knows that she needs a pencil. She does **not** know Pierre sells one.

After the warning conversation closes, the farmer supplies the practical game-world knowledge as an internal thought:

> **“Bút chì à... chắc chú Pierre có bán.”**
>
> **“A pencil... Pierre probably sells those.”**

This keeps Sudoku's knowledge believable while still giving the player a clear hint.

## Save behavior

- Existing progressed saves are not forced backwards through the Pencil gate.
- A fresh alpha.3 intro that is still waiting for a Pencil remains incomplete across save/load.
- Debug reset clears the fresh-story VHS/Pencil state for repeat testing.

## Test shortcuts

- `heyyourecursed_givepencil` — give one story Pencil immediately.
- `heyyourecursed_resetarrival` — reset the VHS/Sudoku arrival flow so the first chapter can be replayed.

## Test priority

1. Fresh/reset flow shows the mysterious parcel origin text and the erased `18` clue.
2. Trigger the VHS/Sudoku arrival without a Pencil; Sudoku refuses activation without naming Pierre.
3. Close the warning and verify the farmer thought appears: `Bút chì à... chắc chú Pierre có bán.`
4. Verify Pierre sells Pencil for 100g.
5. Return with the Pencil; one Pencil is consumed and Sudoku activates normally.
6. Save/reload while still missing the Pencil; the gate should remain intact.

The existing Spirit's Eve foundation remains in place. Full festival choreography is the next alpha.3 pass after this opening story hook is compile/game-tested.
