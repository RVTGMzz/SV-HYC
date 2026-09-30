# HANDOFF CURRENT — Hey! You’re Cursed!

Updated: 2026-09-30

## Repository / account authority

- Canonical repository: `RVTGMzz/SV-HYC`
- GitHub write account for this project: profile `RVTGMzz`, email `lengochung28191@gmail.com`
- Do NOT use the roneditor08 GitHub account for writes to this project.
- Repository remains **public for now**.

## Important repository state

The current `main` branch is still an old prototype/content-pack state and is **not** the latest shipped Hey! You’re Cursed! source.

Do not start new release work from `main` until the latest v1.0.x source is synchronized into the repository.

The existing compatibility branch:

`compat/teamup-party-control`

contains the Team Up runtime ownership handshake work. Known compatibility commits:

- `37246f988af5ce1bf4de78c061927479afd10c52`
- `8f133f74f1f6ede6cf20754718d08e168ea58e22`

This branch was originally based on older HYC source, so it should be treated as a compatibility reference/patch source until merged into the current release source.

## Current HYC identity

Current shipped/canonical IDs before the planned migration:

- Mod UniqueID: `ronvotri.HeyYoureCursed`
- Sudoku NPC ID: `ronvotri.HeyYoureCursed_Sudoku`
- Team Up marker: `Ronvotri.TeamUp/PartyControlled = true`

## Planned ID migration

The next identity cleanup is intended to move public/canonical IDs to:

- Mod UniqueID: `RVTGMzz.HeyYoureCursed`
- Sudoku NPC ID: `RVTGMzz.HeyYoureCursed_Sudoku`
- Team Up marker: `RVTGMzz.TeamUp/PartyControlled = true`
- Asset prefix: `Mods/RVTGMzz.HeyYoureCursed/...`
- Object IDs such as:
  - `RVTGMzz.HeyYoureCursed_CursedVHS`
  - `RVTGMzz.HeyYoureCursed_Pencil`
- modData prefix: `RVTGMzz.HeyYoureCursed/...`

**This migration is planned, not yet implemented in the repository.**

Backward compatibility is required. Old `ronvotri.HeyYoureCursed...` identifiers may remain only inside migration/legacy compatibility code so existing saves do not lose:

- Trust
- Puzzle Bond
- story progression
- Sudoku NPC state
- VHS/Pencil and other custom items
- roommate/finale state

Migration must also prevent duplicate Sudoku NPCs.

Do not rename the DLL or C# namespace just for this identity change unless technically required.

## Team Up compatibility contract

Team Up must not read or write HYC story state, Trust, Puzzle Bond, `SudokuNpcEnabled`, ArrivalSeen, or other HYC internal save data.

While Team Up controls Sudoku, it marks the live Sudoku NPC instance through `modData`. HYC then yields only its roommate movement/activity control so the two mods do not fight over:

- Halt calls
- pathfinding
- direct Position writes
- roommate activity reposition/warp
- movement-facing refreshes

When Team Up releases Sudoku, the runtime marker is removed and HYC resumes roommate behavior.

When the ID migration is implemented, both HYC and Team Up must move the runtime marker to the new `RVTGMzz...` key while preserving any needed transition compatibility.

## Sudoku progression reminder

HYC intentionally has two different progression systems:

- **Trust 0–30**: daily relationship/life bond.
- **Puzzle Bond 0–18**: first-time clears of the 18 main Sudoku stages.

Daily Challenge, Endless Practice, and replayed stages do not increase Puzzle Bond.

Sudoku answer checking should distinguish:

1. At least one entered value wrong → **Wrong**
2. All entered values correct but blanks remain → **Incomplete**
3. Full and correct → complete normally

## Planned post-game update

Highest-priority content direction after the source is synchronized:

**Sudoku post-game content after Trust 30/30 + Puzzle Bond 18/18.**

Target scope discussed:

- around 6 main post-game events
- around 25 micro-interactions / roommate dialogue variants
- post-game `What are you doing?` dialogue pool
- subtle relationship evolution without adding another grind bar
- one rare supernatural teaser that can foreshadow Case 02

Core event ideas:

1. **The Quiet Screen** — Sudoku can remain outside the TV without relying on the signal.
2. **Still Here** — late-night waiting scene.
3. **A Normal Day** — deliberately ordinary domestic event.
4. **Don’t Do That Again** — follow-up after Sudoku has rescued the player.
5. **The Empty Tape** — revisit the cursed VHS after the bond is complete.
6. **Home** — player confirms whether Sudoku still has a place with them now that she is no longer bound there.

Possible end-state UI flavor:

`Home: ♥`

No extra 31/30-style progression bar.

Marriage/romance may be considered later, but the immediate priority is meaningful post-game content rather than simply enabling vanilla marriage flags.

## Next-session instruction

Before implementing post-game or ID migration:

1. Verify the GitHub connection is using the `RVTGMzz` profile tied to `lengochung28191@gmail.com`.
2. Work only in `RVTGMzz/SV-HYC`.
3. Do not treat `main` as current release source.
4. Synchronize or otherwise obtain the latest v1.0.x release source first.
5. Preserve the Team Up compatibility contract.
6. Implement ID migration with save compatibility before switching canonical IDs.
7. Then start the Sudoku post-game branch, suggested name:
   `dev/v1.1-sudoku-postgame`

## Current privacy decision

The repository is intentionally left public for now. Renaming IDs removes the old public identity trail, but it does **not** hide source code. Privacy can be revisited later by making the repository private and distributing only release builds.
