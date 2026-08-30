# alpha.3.5 — Sudoku custom gifts

Sudoku-only release polish. Multi-ghost/Case work remains deferred.

- Vanilla NPC gift friendship stays disabled (`CanReceiveGifts=false`).
- Roommate hub adds **Đưa cô ấy thứ gì đó / Give her something**.
- Uses the currently held ordinary object item.
- One gift per in-game day, separate from the normal daily social Trust gain.
- Trust: Pencil/Loved +2, Liked +1, Neutral 0, Disliked -1, Hated -2.
- Pencil has a unique callback reaction.
- Minerals/gems/monster loot are generally liked; cooked food/fish disliked; junk hated.
- `heyyourecursed_resetgift` resets the daily gift limit for testing.
- Approved hand-fixed Sudoku sprite remains the canonical sprite asset.
