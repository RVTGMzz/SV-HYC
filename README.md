# Hey! You’re Cursed! — v0.0.7-alpha.2.1

## alpha.2.1 — Compile hotfix

- Added the missing `StardewModdingAPI` namespace imports used by the roommate and Spirit's Eve partial files, fixing the `Context` CS0103 errors.
- Narrowed the nullable Sudoku service field to local variables in roommate dialogue methods, removing the two CS8602 warnings from those call sites.
- No gameplay or progression behavior changed from alpha.2.

A paranormal Stardew Valley mod about a cursed VHS, Sudoku, and a ghost who slowly becomes a strange little roommate instead of just a minigame dispenser.

## v0.0.7-alpha.2.1 — Active roommate behavior pass

This build keeps the centered Sudoku UI, parallel Puzzle Bond/Trust progression, Endless Practice, and Spirit's Eve foundation from alpha.1, then makes Sudoku actually **live around the farmhouse** instead of standing in one fixed tile.

### 1. Sudoku now has real farmhouse activity states

After the daily TV materialization, Sudoku chooses a deterministic activity for each part of the day. The current activity pool includes:

- watching the TV;
- observing furniture;
- watching a pet when one is available;
- listening to the farmhouse;
- standing near the door;
- hiding in a quiet corner;
- at higher Trust, waiting for the player to come home.

The activity selection changes with Trust, so early Sudoku spends more time near the TV or in quiet corners, while a close Sudoku is more willing to occupy shared spaces and openly wait for the player.

### 2. Ghost movement instead of normal NPC walking

Sudoku does not use a normal villager walking schedule inside the farmhouse. The mod resolves safe tiles from the player's **actual current farmhouse map and furniture layout**, builds a short path, then smoothly glides her tile-to-tile while keeping her normal ghost sprite still.

Safety rules:

- no movement before the day's TV materialization;
- pause movement while a menu/event is open;
- pause if the player is standing too close;
- use `CanSpawnCharacterHere` for destination/path tiles;
- recalculate when furniture/player position invalidates a path;
- unusual farmhouse layouts can use a distant ghost blink fallback instead of leaving Sudoku permanently stuck;
- leaving the farmhouse cancels transient movement state.

This is intentionally a **ghost drift**, not standard NPC walking animation.

### 3. `Hôm nay cô đang làm gì?` now matches what she is actually doing

The roommate menu no longer pulls a generic random activity line. It reads Sudoku's live state and uses matching dialogue, for example:

- TV → comments about the screen/static;
- pet → comments about the animal being able to see her;
- door → comments about hearing the player return;
- quiet corner → explains why she picked that spot;
- waiting state → admits (or tries not to admit) that she was waiting for the player.

Trust can still increase only once per day from ordinary roommate interaction, so repeatedly asking about the same activity does not farm relationship progress.

### 4. Welcome-home reactions

Once Trust is at least 3, returning to the farmhouse after 11:00 can trigger one short Sudoku reaction **once per day**. The wording becomes less defensive as Trust rises. This is deliberately non-modal: it gives the house a lived-in feeling without forcing a dialogue box every time the player enters.

### 5. Puzzle and relationship remain parallel

The existing alpha.1 structure stays intact:

- **Puzzle Bond** = 18 unique Stage clears;
- **Trust** = ordinary time/interactions with Sudoku;
- Daily Challenge remains the repeatable reward source;
- 18/18 unlocks Endless Practice;
- a player can focus on puzzles, character interaction, or both.

### 6. Centered Sudoku UI retained

Sudoku board, Stage Select, portrait conversation, and roommate choice menu still use Stardew's UI viewport, so the Sudoku frame stays centered even when world/UI viewport sizes differ.

### 7. Late-game Endless Practice

After **18/18 Stage**, Stage Select unlocks **ENDLESS PRACTICE**.

For the current content bank it cycles through the existing 18 validated puzzles in a persistent sequence. Endless Practice:

- is always available after 18/18;
- gives no item/gold reward;
- does not increase Puzzle Bond;
- does not affect Daily Challenge reward eligibility;
- returns to Stage Select when closed.

This gives late-game players a permanent reason to keep playing Sudoku even after the fixed progression is complete. More puzzle packs/chapters can extend this later without changing the basic flow.

### 8. Spirit's Eve outing foundation

On **Fall 27**, the roommate menu changes `Có gì lạ không?` into `Spirit's Eve tối nay?`.

Sudoku asks:

> "Hôm nay người sống giả làm ma?"
> "...Ta muốn đi."
> "Ta được phép dọa họ chứ?"

The player can choose:

- **Nhẹ thôi.**
- **Cứ tự nhiên.**
- **Đừng làm Lewis ngất.**

Choosing a plan:

- records the plan for that year;
- gives a small one-time Trust bump for the outing;
- invalidates the Fall 27 festival data so the selected plan is used when the festival loads;
- adds Sudoku to Spirit's Eve using `Set-Up_additionalCharacters`;
- adds Sudoku festival dialogue and extra reaction lines for Abigail, Sebastian, Wizard, Krobus, and Lewis;
- leaves normal Stardew festival interaction in control while the player is at the festival;
- adds a one-time Fall 28 aftermath line where Sudoku asks whether she can go again next year.

Vanilla Spirit's Eve uses a standard custom-NPC position. A separate known-open SVE festival coordinate is used when Stardew Valley Expanded is detected.

This is the first pass of the outing system; later builds can add more animation/cutscene choreography without replacing the save-state foundation.

### 9. Ghost presentation retained

Sudoku still keeps the existing ghost pass:

- 85% world-sprite opacity;
- sprite lifted slightly above the floor;
- no shadow;
- portraits remain fully opaque.

## Current puzzle rewards

Only **Daily Challenge** gives repeatable item/gold rewards.

Stage clears give permanent progression and unlocks. Endless Practice is reward-free.

## Controller

All new roommate/choice menus are controller-first:

- D-pad / Left Stick: move selection
- A: confirm
- B: close/back

Sudoku board controls remain:

- D-pad / Left Stick: move cell
- A: open number picker
- Left/Right: choose 1–9
- A again: place number
- X: erase
- Y / Start: check
- LB/RB or LT/RT: previous/next editable cell
- B / Esc: back

## Build

Close Stardew Valley and SMAPI, then double-click:

`Build_HeyYoureCursed.bat`

Successful builds deploy to:

`Mods/HeyYoureCursed`

## Useful test commands

- `heyyourecursed_talk` — open the new roommate interaction hub
- `heyyourecursed_stages` — open Stage Select
- `heyyourecursed_open` — open today's Daily Challenge
- `heyyourecursed_status` — reports Puzzle Bond, Trust, current roommate activity, Endless unlock, and Spirit's Eve mode
- `heyyourecursed_resetdaily`
- `heyyourecursed_resetarrival`
- `heyyourecursed_unlocknpc`
- `heyyourecursed_testarrival`
- `heyyourecursed_givevhs`
