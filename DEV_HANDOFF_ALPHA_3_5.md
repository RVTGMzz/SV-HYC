# Hey! You’re Cursed! — Cross-account development handoff

Date: 2026-08-31
Repo: ronvotri/HeyYoureCursed
Branch: `dev/alpha3-story-hook`

## Where we are

**alpha.3.4 is TESTED/PASS on Windows/game.** Do not regress it. The tested baseline includes:
- Persistent Stardrop Saloon prologue at 18:00.
- Farmer dismisses/denies supernatural experiences; Wizard overhears from outside.
- Next morning cursed VHS arrives via brown-paper parcel at farmhouse door.
- VHS must be deliberately used with a farmhouse TV to start Sudoku haunting.
- Real Pencil gate: Pierre sells Pencil for 100g; seven-day clock starts when Sudoku accepts the Pencil.
- Day-7 Wizard reveal is a real in-world/event presentation; Wizard must disappear cleanly and must NOT remain as a real NPC/minimap icon.
- Occult Cabinet is a placeable Big Craftable.
- One Active Haunting; Sudoku seal/unseal preserves Trust, 18 Stage progress, Daily/Endless progress, and story state.
- Sudoku is a ghost and Farmer can pass through her (`farmerPassesThrough`); she still respects normal world collision.
- Sudoku sprite approved by user: 64x128 RGBA hand-fixed sheet. Approved sprite SHA-256: `9b51eafc4abb5faaa2277543fde30cda1b6267304debaa7e8af011944b00e99a`.

## Current alpha.3.5 work

User wants to finish Sudoku as a standalone release for the Halloween/Spirit’s Eve period before building a multi-ghost framework.

Already designed/implemented in the latest local test package:
- Sudoku gift system: one gift/day; uses the item currently held; categories Loved/Liked/Neutral/Disliked/Hated; Trust deltas planned/implemented as +2/+1/0/-1/-2; no vanilla heart friendship system.
- Special Pencil gift reaction should exist.
- Sudoku tutorial overlay in the puzzle menu: explain 9x9, numbers 1-9, no repeats in row/column/3x3 box, simple visual example, controls; open via `? Hướng dẫn`, keyboard H/F1, controller Back/View.

Latest local package built for source handoff:
`HeyYoureCursed_v0.0.7-alpha.3.5_SudokuGifts_Tutorial_Source.zip`

## Next planned feature: max-Trust rescue

User approved design:
- Unlock only at Trust 30/30.
- When Farmer truly dies/gets knocked out or faints from exhaustion, Sudoku can rescue them.
- Do NOT trigger for late-night 2:00 AM/oversleep.
- Rescue at the location of collapse.
- Restore 100% HP + 100% Energy.
- No death-loss money/item penalty from that rescue.
- Cooldown is **15 in-game days**, crossing seasons; does not reset at season change.
- Suggested UI: `Sự che chở: Sẵn sàng` / `Sự che chở: Còn X ngày`.
- Test commands planned: `heyyourecursed_test_rescue`, `heyyourecursed_reset_rescue`, and status reporting.
- User wants a special signature presentation: brief time-stop/darkening, VHS/static glitch, ghostly Sudoku apparition/afterimage, short line such as `...Ta chưa cho phép ngươi chết.`, then instant revival at full HP/Energy and Sudoku disappears. Keep it stylish and short, not a boss spell.

## Halloween / Spirit’s Eve status

There is already Spirit’s Eve logic: planning on day 27 with 3 choices (gentle scare / let her roam / target Lewis), first-choice-per-year bonus, Sudoku festival presence, altered dialogue for Sudoku/Abigail/Sebastian/Wizard/Krobus/Lewis, and day-28 aftermath. However, it is not yet considered fully polished because the actual festival payoff is mostly dialogue/selection rather than a visible prank scene.

Planned polish: add a 20–40 second visible prank/payoff scene for the selected plan, then keep existing dialogue and day-28 aftermath.

## Late-game Sudoku direction

18/18 unlocks Endless Practice; Daily Challenge is repeatable. Current late-game needs a stronger emotional/gameplay capstone.

Planned after gifts/tutorial/rescue:
- 18/18 + Trust 30/30 capstone event.
- Potential `Channel 18` TV content as a long-term reason to keep Sudoku around: seasonal/random broadcasts, small challenges, occasional item, rare dialogue. This is an idea, not yet implemented/locked.

## Explicit scope priority

**Do NOT start Case 2 / multi-ghost architecture yet.** Finish Sudoku for release first:
1. Gift system + tutorial compile/game test.
2. Max-Trust rescue with 15-day cooldown and special effect.
3. Spirit’s Eve visible payoff scene.
4. 18/18 + Trust 30 capstone.
5. Optional Channel 18 if scope permits.
6. Final localization/polish/release package.

## Build/test rule

Windows compile is authoritative. `CS9057` from Pathoschild Stardew ModBuildConfig 4.4.0 referencing compiler 4.9 while current compiler is 4.3 is a warning, not the build blocker. Fix actual `error CS...` first.

Before modifying source, fetch the current branch. Do not rely on old chat source or old ZIPs. Keep the tested alpha.3.4 checkpoint as rollback.

## Story canon

Theme: **“Chuyện tâm linh không đùa được đâu.”**

Farmer receives Saloon invitation for 18:00. If they skip it, invitation persists/reappears; no Saloon attendance means no haunting/VHS. During gathering, farmer says they have never experienced anything supernatural / people scare themselves. Wizard overhears and decides to teach them. Wizard's identity stays hidden until day 7. Sudoku is first haunting. Future ghosts are independent Cases, not A→B→C, but that framework is future scope.

## Important implementation cautions

- Never spawn Wizard as a persistent real NPC just for the day-7 cutscene; use a temporary event actor so no minimap icon remains.
- Keep Sudoku's `farmerPassesThrough` true whenever her instance is created/restored/unsealed.
- Keep VHS activation deliberate through TV; do not auto-insert it into inventory or auto-trigger from merely owning it.
- Keep Pencil gate and seven-day timer semantics unchanged.
