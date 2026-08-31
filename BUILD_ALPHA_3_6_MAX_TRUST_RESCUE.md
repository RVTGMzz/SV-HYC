# v0.0.7-alpha.3.6 — Sudoku Max-Trust Rescue

## Contract

- Unlocks only at Trust 30/30 while Sudoku is the active, unsealed haunting.
- Triggers on lethal HP loss or a real exhaustion collapse (`Stamina <= -15`) before 2:00 AM.
- Never triggers for the normal 2:00 AM/oversleep pass-out.
- Revives at the collapse location with full HP and full Energy.
- Cancels the vanilla death/pass-out flow before the hospital/bed warp and loss penalties.
- Cooldown is 15 in-game days based on `Game1.Date.TotalDays`, so it crosses seasons and years.
- Presentation: brief time-stop and darkening, VHS/static glitch, Sudoku apparition/afterimages, `...Ta chưa cho phép ngươi chết.`, then release with short invincibility.

## Test commands

- `heyyourecursed_test_trust 30`
- `heyyourecursed_test_rescue death`
- `heyyourecursed_test_rescue exhaustion`
- `heyyourecursed_reset_rescue`
- `heyyourecursed_status`

## Windows/game checklist

1. Compile on Windows. Treat `CS9057` as a warning; fix any actual `error CS...` first.
2. Set Trust to 29 and verify neither death nor exhaustion is intercepted.
3. Set Trust to 30, reset rescue, and trigger `death` in a mine. Confirm the farmer remains on the same tile/location with full HP/Energy and loses no money/items.
4. Confirm the visual lasts only a few seconds and Sudoku disappears cleanly afterward.
5. Confirm status reports a 15-day cooldown on the rescue day.
6. Advance 14 in-game days; confirm the cooldown is not ready. Advance one more day; confirm it is ready.
7. Repeat across a season boundary.
8. At 2:00 AM, confirm the normal late-night pass-out still occurs and does not consume protection.
9. Seal Sudoku and confirm protection cannot trigger. Unseal and confirm eligibility returns if the cooldown is ready.
10. Re-test the alpha.3.4 Saloon → parcel → deliberate TV VHS → Pencil → day-7 Wizard → Cabinet flow for regressions.
