# Cursed Signal — v0.0.4 prototype

Vietnamese subtitle: **Chuyện Tâm Linh Không Đùa Được Đâu**

> Status: source-complete prototype, **not yet compiled or tested inside Stardew Valley/SMAPI**.

## v0.0.4 — Cursed VHS & Daily Interaction

This branch builds on the identity-cleanup work from v0.0.3.1.

### Cursed VHS

Sudoku's arrival now leaves behind a real custom object:

- item ID: `(O)ronvotri.CursedSignal_CursedVHS`
- display name: `Cursed VHS`
- custom 16×16 item icon
- unsellable / unshippable / ungiftable story item
- label description hints at the 9×9 Sudoku motif

The VHS is granted once after Sudoku's TV arrival. Existing prototype saves which already have the arrival flag receive it on load/day start if they haven't received it before.

### Daily interaction

Talking to Sudoku before today's reward has been claimed no longer jumps straight into the board.

The prototype now asks:

- `Giải.`
- `Để sau.`

Choosing `Giải.` opens today's saved 9×9 board. Choosing `Để sau.` closes the interaction and leaves the puzzle available for later.

After today's reward has already been claimed, the mod stops intercepting the action button so normal NPC dialogue can happen.

### Item reward pool

Daily Sudoku rewards are now primarily **items** instead of only gold.

`assets/Data/Sudoku.rewards.json` contains weighted reward pools for:

- Easy
- Normal
- Hard

Current examples include Coffee, Omni Geodes, gems, Battery Packs, Iridium Bars, and food. The reward is deterministic for a given save/day and is still claimable only once per day.

The old gold values remain in `config.json` as a safe fallback if the item reward pool is missing or an item can't be created.

### Save compatibility

v0.0.4 keeps the v0.0.3.1 identity migration:

- new keyspace: `ronvotri.CursedSignal/...`
- legacy keyspace is read and copied forward
- old keys are intentionally not deleted during this prototype phase
- legacy Sudoku NPC IDs are still recognized to avoid duplicate NPCs

## Existing prototype flow

1. Stay in the farmhouse until 7:00 AM (temporary test trigger).
2. TV static begins.
3. A distorted well appears.
4. The screen glitches.
5. Sudoku crawls out of the TV.
6. The arrival flag is saved.
7. A Cursed VHS is recovered.
8. Sudoku becomes eligible as a persistent custom NPC.
9. Each day gets a deterministic Sudoku puzzle.
10. Talk to Sudoku → choose `Giải.` or `Để sau.`
11. Solve correctly → receive one daily item reward.

## Console commands

- `sudoku_testarrival`
- `sudoku_resetarrival`
- `sudoku_unlocknpc`
- `sudoku_status`
- `sudoku_open`
- `sudoku_resetdaily`
- `cursedsignal_givevhs`

## Still intentionally deferred

- final cursed-tape acquisition quest before the TV event;
- putting the VHS into/using it on the TV as an actual interaction;
- full i18n/localization;
- friendship/heart-event progression;
- animated portrait reactions;
- pencil-note candidates;
- conflict/mistake highlighting;
- multiplayer authority and sync validation;
- controller polish and real-device testing;
- final reward balance.
