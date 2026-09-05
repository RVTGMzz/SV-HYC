using StardewValley;

namespace HeyYoureCursed;

/// <summary>
/// Lightweight runtime-only compatibility handshake for external party controllers.
/// No Team Up DLL/API or save data is referenced; only a marker on the canonical Sudoku NPC is observed.
/// </summary>
internal sealed partial class ModEntry
{
    internal const string TeamUpPartyControlledMarkerKey = "Ronvotri.TeamUp/PartyControlled";

    private bool sudokuRoommateYieldedToTeamUp;

    private static bool IsSudokuPartyControlledByTeamUp(NPC sudoku)
    {
        return sudoku.modData.TryGetValue(TeamUpPartyControlledMarkerKey, out string? raw)
            && string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns true while Team Up owns Sudoku's movement. Transitions clear only Hey! You're Cursed!
    /// roommate runtime state so an in-flight path/glide cannot fight the external controller.
    /// </summary>
    private bool ShouldYieldSudokuRoommateControl(NPC sudoku)
    {
        bool controlled = IsSudokuPartyControlledByTeamUp(sudoku);
        if (controlled)
        {
            if (!this.sudokuRoommateYieldedToTeamUp)
            {
                this.ClearSudokuRoommateActivityRuntime();
                this.sudokuRoommateYieldedToTeamUp = true;
                this.Monitor.Log(
                    $"Team Up party-control marker detected on {ModIdentity.SudokuNpcId}; yielding Sudoku roommate movement/activity control.",
                    StardewModdingAPI.LogLevel.Trace
                );
            }

            return true;
        }

        if (this.sudokuRoommateYieldedToTeamUp)
        {
            this.sudokuRoommateYieldedToTeamUp = false;
            this.ClearSudokuRoommateActivityRuntime();
            this.roommateBehaviorCooldownTicks = 30;
            this.Monitor.Log(
                $"Team Up party-control marker cleared on {ModIdentity.SudokuNpcId}; Sudoku roommate behavior will refresh cleanly.",
                StardewModdingAPI.LogLevel.Trace
            );
        }

        return false;
    }
}
