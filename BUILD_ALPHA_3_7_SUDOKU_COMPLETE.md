# Build alpha.3.7 — Sudoku Complete

This build completes the Sudoku-only route. It includes the Spirit's Eve visible payoff, the 18/18 + Trust 30/30 capstone, post-capstone Channel 18 broadcasts, inventory-safe VHS delivery, and English/Vietnamese fallback localization. Case 2 and other ghosts are intentionally not included.

## Windows build

Run `Build_HeyYoureCursed.bat` from the source folder. The CS9057 analyzer message is a warning; any CS0103/CS#### error must be fixed before testing.

## Focused smoke tests

1. `heyyourecursed_test_profile complete`, then talk to Sudoku: capstone appears once, choice is saved, and the follow-up line returns to the hub.
2. `heyyourecursed_test_capstone`: repeat the capstone scene from a clean state.
3. `heyyourecursed_test_channel18`: open Channel 18; check season text, wrap, keyboard, and controller input.
4. `heyyourecursed_test_spiriteve_scene gentle`, `free`, and `lewis`: each scene advances by click/Enter/A and exits by Esc/B.
5. Switch English and Vietnamese and repeat the capstone, Channel 18, Spirit's Eve, stage list, and full-inventory VHS checks.

## Regression checklist

- Intro/pencil flow and existing alpha.3.4 story.
- Gifts, tutorial, daily challenge, 18 sequential stages, replay, and Endless Practice.
- Trust 30/30 rescue for death and exhaustion, including cooldown and save reload.
- Spirit's Eve plan, visible prank, festival dialogue, and next-day aftermath.
- Sealing/unsealing through the Occult Cabinet.
- No duplicate VHS when the inventory is full.

