# v0.0.7-alpha.3.4 — TESTED CHECKPOINT

Status: Windows compile + in-game integration test PASS (reported 2026-08-30).

Validated flow:
- Persistent Stardrop Saloon invitation/event gate.
- Native Saloon cutscene with villagers present; hidden observer identity not revealed until day 7.
- Morning-after unmarked package delivery flow.
- Player opens package to receive Cursed VHS.
- Player voluntarily installs VHS into a farmhouse TV before Sudoku haunting begins.
- Pencil activation starts the 7-day timer.
- Seven-day Wizard reveal works as an in-world farmhouse event actor; Wizard leaves with no persistent farmhouse/minimap NPC marker.
- Occult Cabinet gift/unlock.
- Active Haunting state.
- Sudoku seal/unseal preserves Trust, 18-stage progress, Daily/Endless and story progress.
- Sealed Sudoku does not respawn through TV/Spirit's Eve paths.
- Farmer can pass through Sudoku (ghost collision behavior).
- Vietnamese Cabinet/VHS terminology reviewed.

Last Windows source package used for this test series:
`HeyYoureCursed_v0.0.7-alpha.3.4_PackageTV_WizardEvent_GhostPass_Source_STATUSFIX.zip`

Package SHA-256:
`51ae02c22b84869fb564fa94dd4211f210b73e04096d739723301e7d902469dd`

Approved hand-fixed Sudoku sprite:
- target path: `assets/Characters/Sudoku.png`
- dimensions: 64x128 RGBA
- approved source SHA-256: `9b51eafc4abb5faaa2277543fde30cda1b6267304debaa7e8af011944b00e99a`

Canon lock after this checkpoint:
- Saloon -> package -> VHS -> TV -> Sudoku -> Pencil -> 7-day Wizard reveal -> Occult Cabinet.
- Only one Active Haunting at a time.
- Cabinet controls seal/unseal of anchor objects.
- Future ghosts are freely selectable Cases after Cabinet unlock; no mandatory A->B->C route.
- Do not require Sudoku 18/18 to advance to another Case.

Do not change alpha.3.4 behavior after this checkpoint except to fix regressions/bugs.
