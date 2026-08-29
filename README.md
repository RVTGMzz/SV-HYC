# Hey! You’re Cursed! — v0.0.7-alpha.3.4

A paranormal Stardew Valley mod built around one simple lesson: **chuyện tâm linh không đùa được đâu** — don't mess around with the supernatural.

## Current alpha.3 story flow

1. **Persistent Saloon invitation** — a fresh/reset save does not receive the VHS immediately. Gus asks the farmer to attend a gathering at the Stardrop Saloon around 18:00. If the player skips it, the invitation returns on later days; the paranormal story simply waits.
2. **The careless remark** — at the gathering, villagers swap paranormal stories. The farmer can claim they've never experienced anything supernatural or dismiss the stories. Gus and Abigail warn them not to joke about such things. The Wizard happens to overhear from outside.
3. **The cursed VHS** — the following morning, and only after the gathering was actually attended, a brown-paper parcel appears with an old VHS, a `DO NOT REWIND` note, and an almost-erased `18` written in pencil.
4. **Sudoku appears** — the VHS/TV sequence introduces Sudoku, the first haunting.
5. **Real Pencil gate** — Sudoku will not complete the activation without a real Pencil. Pierre sells the Pencil for **100g**. Sudoku never tells the farmer where to buy it; the farmer thinks, `Bút chì à... chắc chú Pierre có bán.`
6. **Seven-day milestone** — the clock starts only when Sudoku actually accepts the Pencil. Seven full days later, the Wizard visits the farmhouse and asks whether the farmer has learned the lesson.
7. **Occult Cabinet** — the Wizard gives a placeable indoor Occult Cabinet. The player chooses where to put it in the farmhouse.
8. **Active Haunting system** — the Cabinet manages one active haunting at a time. In alpha.3.4, `Cursed VHS — Sudoku` can be sealed and unsealed. Trust, all 18 Stage clears, Daily/Endless progress, and story state are preserved.

## Long-term haunting rule

Sudoku is the first/tutorial haunting, but future ghosts should **not** form a forced A → B → C campaign.

After the seven-day Cabinet unlock, future Cases can be entered non-linearly. Each ghost can have its own set of **18 unique minigames/challenges**, while 18/18 represents completion of that Case rather than permission to try the next ghost.

Only one haunting can be actively attached at a time. Other known hauntings are sealed through their anchor items in the Occult Cabinet.

## Sudoku progression

- **Gắn kết / Puzzle Bond:** 18 unique Stage clears.
- **Tin cậy / Trust:** social progression with Sudoku, 0–30.
- One new Stage can be cleared for progression per in-game day; cleared Stages can be replayed freely.
- Daily Challenge remains the repeatable reward source.
- 18/18 unlocks Endless Practice for Sudoku, but does **not** gate access to future haunting Cases after the Cabinet milestone.

## Active Haunting — alpha.3.4

Current states:

- `ActiveHauntingId = Sudoku` — Sudoku can materialize and behave normally.
- Seal `Cursed VHS — Sudoku` — Sudoku is removed from the world and the active slot becomes `none`.
- Unseal — Sudoku returns immediately with the same Trust/Stage/story progress.
- A sealed Sudoku cannot be respawned by the daily TV signal and is not injected into Spirit's Eve.

Sudoku has Trust-sensitive reactions to being sealed and restored.

## Main test commands

- `heyyourecursed_resetarrival` — reset all the way back to the persistent Saloon invitation; no free VHS is returned.
- `heyyourecursed_test_prologue` — replay/start the Saloon scene while standing in the Saloon.
- `heyyourecursed_givevhs` — debug VHS bypass.
- `heyyourecursed_givepencil` — give one Pencil.
- `heyyourecursed_test_wizard` — arm the seven-day Wizard reveal immediately.
- `heyyourecursed_givecabinet` — give an Occult Cabinet.
- `heyyourecursed_test_seal` / `heyyourecursed_test_unseal` — test the Active Haunting state directly.
- `heyyourecursed_status` — report story, Sudoku, Cabinet, and Active Haunting state.
- Existing Sudoku helpers remain available: `heyyourecursed_test_solve`, `heyyourecursed_test_profile`, `heyyourecursed_test_trust`, `heyyourecursed_test_stages`, `heyyourecursed_test_endless`, and `heyyourecursed_test_spiriteve`.

## Story design documents

- `ALPHA_3_2_STORY_GATE.md` — persistent Saloon prologue and VHS story gate.
- `ALPHA_3_3_OCCULT_CABINET.md` — seven-day Wizard reveal and free haunting unlock rule.
- `ALPHA_3_4_ACTIVE_HAUNTING.md` — one-active-haunting contract and Sudoku seal/unseal behavior.

## Build

Close Stardew Valley and SMAPI, then run:

`Build_HeyYoureCursed.bat`

The current test package is **v0.0.7-alpha.3.4 — Active Haunting + Seal/Unseal Sudoku**.

This branch has been synchronized to the alpha.3.4 story/system design, but the current alpha.3.4 pass still requires the Windows compile/game test before it should be treated as a stable checkpoint.
