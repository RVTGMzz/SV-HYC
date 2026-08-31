# Hey! You’re Cursed! — alpha.3.6 development handoff

Date: 2026-08-31  
Repo: `ronvotri/HeyYoureCursed`  
Branch: `dev/alpha3-story-hook`

## Baseline

Alpha.3.4 remains the Windows/game-tested rollback checkpoint. Preserve the Saloon story gate, next-morning parcel, deliberate TV activation, real Pencil gate, day-7 temporary Wizard actor, placeable Occult Cabinet, one Active Haunting, seal/unseal persistence, and `farmerPassesThrough` behavior.

The approved 64×128 RGBA Sudoku sprite is restored in this source. Required SHA-256:

`9b51eafc4abb5faaa2277543fde30cda1b6267304debaa7e8af011944b00e99a`

## Alpha.3.5 merged

- Custom one-gift-per-day Sudoku flow in `ModEntry.Gifts.cs`.
- Trust deltas: Pencil/Loved +2, Liked +1, Neutral 0, Disliked -1, Hated -2.
- Story anchors/tools/furniture are rejected; vanilla friendship hearts remain unused.
- Beginner guide overlay on all Sudoku boards via mouse, H/F1, and controller Back/View.
- Gift/tutorial text lives in `Alpha3TranslationFallback.cs` to fit the split-source branch; fallback token interpolation was added in `ModEntry.I18n.cs`.
- The Spirit's Eve keys present in the attached alpha.3.5 package were also restored to the fallback so the existing festival flow cannot display raw key names.

## Alpha.3.6 implemented in source

- Trust 30/30 max-Trust protection in `ModEntry.Rescue.cs`.
- Detects lethal HP loss and real exhaustion collapse before 2:00 AM.
- Does not trigger for the normal 2:00 AM/oversleep pass-out.
- Restores full HP/Energy at the collapse location and suppresses the vanilla death/pass-out warp and penalties.
- Cooldown is 15 `TotalDays`, crossing seasons/years.
- Brief global pause, darkening, VHS/static lines, Sudoku apparition/afterimages, and signature line.
- Roommate hub shows ready/remaining-day status at max Trust.
- Test commands: `heyyourecursed_test_rescue [death|exhaustion]` and `heyyourecursed_reset_rescue`.
- `heyyourecursed_status` now reports gift and rescue state.

## Validation status

- JSON files parse successfully.
- `git diff --check` passes.
- Approved sprite checksum and PNG decoding pass.
- This environment has no .NET SDK, so alpha.3.5/3.6 is **not yet Windows compiled or game-tested**.
- Windows compile is authoritative. Ignore `CS9057` as a warning and fix actual `error CS...` first.

Use `BUILD_ALPHA_3_6_MAX_TRUST_RESCUE.md` as the Windows/game test checklist.

## Next priority after alpha.3.6 passes

Add a visible 20–40 second Spirit’s Eve prank/payoff scene for the selected plan, then preserve the existing festival dialogue and day-28 aftermath. Do not begin Case 2 or multi-ghost architecture yet.
