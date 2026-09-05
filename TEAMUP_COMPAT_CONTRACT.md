# Team Up compatibility contract: Sudoku party control

This is an intentionally tiny runtime handshake. Neither mod needs a hard DLL/API dependency on the other.

## Canonical NPC

- Sudoku canonical NPC ID: `ronvotri.HeyYoureCursed_Sudoku`

Team Up should only recruit/control the real NPC after Hey! You're Cursed! has unlocked and spawned her normally. Team Up must not read or write Hey! You're Cursed! story state, Trust, `SudokuNpcEnabled`, ArrivalSeen, or other internal save data.

## Runtime ownership marker

While Team Up is actively controlling Sudoku's follow/pathfinding/combat movement, set this marker on the **Sudoku NPC instance itself**:

```text
Ronvotri.TeamUp/PartyControlled = true
```

Contract details:

- Key comparison is exact and case-sensitive: `Ronvotri.TeamUp/PartyControlled`.
- Hey! You're Cursed! treats the value `true` case-insensitively.
- Remove the marker as soon as Team Up stops controlling Sudoku.
- This marker is runtime ownership only. Team Up owns setting/removing it and should not use it as persistent party save state.
- On save/load, Team Up should restore its own party state through its own system, then reapply the marker to the live Sudoku NPC if she is still actively party-controlled.
- On teardown/leave-party, remove the marker from the NPC.

## Hey! You're Cursed! behavior

When the marker is `true`, Hey! You're Cursed! yields only Sudoku's **roommate movement/activity controller**:

- no roommate `Halt()` calls;
- no roommate path generation/dequeue;
- no roommate glide `Position` writes;
- no roommate activity reposition/warp;
- no roommate-facing refresh tied to activity movement;
- in-flight roommate path/glide/activity runtime is cleared once when ownership transfers to Team Up.

Hey! You're Cursed! does **not** delete Sudoku, modify Trust/story progression, alter `SudokuNpcEnabled`, read Team Up services/save data, or require Team Up to be installed.

When the marker disappears, Hey! You're Cursed! clears stale roommate runtime again and resumes normal roommate behavior with a short refresh cooldown.

## Integration scenarios

1. Recruit Sudoku while both player and Sudoku are in FarmHouse. Team Up should take movement ownership immediately and HYC must not pull her toward TV/furniture.
2. Remove Sudoku from Party. Remove the marker; HYC should rebuild a clean roommate activity/path and resume.
3. Warp in/out of FarmHouse while recruited. Keep/reapply the marker on the live NPC that Team Up controls.
4. Save/reload while recruited. Reconstruct Team Up party state normally, then mark the live Sudoku NPC again. No HYC save keys are required.
5. Run Hey! You're Cursed! without Team Up. With no marker present, behavior is unchanged.
