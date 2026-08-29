using StardewModdingAPI;
using StardewValley;

namespace HeyYoureCursed;

internal sealed partial class ModEntry
{
    private void RegisterAlpha22TestCommands(IModHelper helper)
    {
        helper.ConsoleCommands.Add("heyyourecursed_test_solve", "Auto-solve the currently open Sudoku board through normal completion logic.", this.OnTestSolveCommand);
        helper.ConsoleCommands.Add("heyyourecursed_test_profile", "Apply a quick progression profile: fresh, early, mid, late, or complete.", this.OnTestProfileCommand);
        helper.ConsoleCommands.Add("heyyourecursed_test_trust", "Set Sudoku Trust directly (0-30).", this.OnTestTrustCommand);
        helper.ConsoleCommands.Add("heyyourecursed_test_stages", "Set the number of unique cleared Sudoku Stages directly.", this.OnTestStagesCommand);
        helper.ConsoleCommands.Add("heyyourecursed_test_endless", "Mark all current Stages clear and unlock Endless Practice.", this.OnTestEndlessCommand);
        helper.ConsoleCommands.Add("heyyourecursed_test_spiriteve", "Set Spirit's Eve test mode: gentle, wild, lewis, or reset.", this.OnTestSpiritEveCommand);
        helper.ConsoleCommands.Add("heyyourecursed_test_help", "List Hey! You’re Cursed! developer/test commands.", this.OnTestHelpCommand);
    }

    private void OnTestSolveCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save before using heyyourecursed_test_solve.", LogLevel.Warn);
            return;
        }

        if (Game1.activeClickableMenu is not SudokuMenu sudokuMenu)
        {
            this.Monitor.Log("Open a Sudoku board first, then run heyyourecursed_test_solve.", LogLevel.Warn);
            return;
        }

        sudokuMenu.SolveInstantlyForTesting();
        this.Monitor.Log("Current Sudoku board auto-solved through normal completion/reward logic.", LogLevel.Info);
    }

    private void OnTestTrustCommand(string command, string[] args)
    {
        if (!this.TryRequireWorld(command))
            return;

        if (args.Length < 1 || !int.TryParse(args[0], out int trust))
        {
            this.Monitor.Log("Usage: heyyourecursed_test_trust <0-30>", LogLevel.Info);
            return;
        }

        trust = Math.Clamp(trust, 0, 30);
        Game1.player.modData[ModIdentity.SudokuTrustKey] = trust.ToString();
        Game1.player.modData.Remove(ModIdentity.SocialInteractionDayKey);
        this.RefreshSudokuRoommateActivity(force: true);
        this.Monitor.Log($"Sudoku Trust set to {trust}/30. Daily social-gain lock cleared for testing.", LogLevel.Info);
    }

    private void OnTestStagesCommand(string command, string[] args)
    {
        if (!this.TryRequireWorld(command) || this.dailySudoku is null)
            return;

        if (args.Length < 1 || !int.TryParse(args[0], out int requested))
        {
            this.Monitor.Log($"Usage: heyyourecursed_test_stages <0-{this.dailySudoku.GetStageCount()}>", LogLevel.Info);
            return;
        }

        int count = Math.Clamp(requested, 0, this.dailySudoku.GetStageCount());
        this.SetStageClearsForTesting(count);
        this.Monitor.Log($"Sudoku Stage progress set to {count}/{this.dailySudoku.GetStageCount()} unique clears.", LogLevel.Info);
    }

    private void OnTestProfileCommand(string command, string[] args)
    {
        if (!this.TryRequireWorld(command) || this.dailySudoku is null)
            return;

        string profile = args.Length > 0 ? args[0].Trim().ToLowerInvariant() : "";
        (int stages, int trust)? values = profile switch
        {
            "fresh" => (0, 0),
            "early" => (3, 3),
            "mid" => (7, 8),
            "late" => (13, 18),
            "complete" => (this.dailySudoku.GetStageCount(), 30),
            _ => null
        };

        if (!values.HasValue)
        {
            this.Monitor.Log("Usage: heyyourecursed_test_profile <fresh|early|mid|late|complete>", LogLevel.Info);
            return;
        }

        this.PrepareCoreForFeatureTesting();
        this.SetStageClearsForTesting(values.Value.stages);
        Game1.player.modData[ModIdentity.SudokuTrustKey] = values.Value.trust.ToString();
        Game1.player.modData.Remove(ModIdentity.SocialInteractionDayKey);
        Game1.player.modData.Remove(ModIdentity.DailyDialogueDayKey);
        this.dailySudoku.ResetTodayForTesting();

        NPC? sudoku = this.EnsureSudokuCharacterExists();
        if (sudoku is not null)
        {
            sudoku.IsInvisible = false;
            if (Game1.currentLocation is StardewValley.Locations.FarmHouse)
                this.PlaceSudokuAfterArrival();
        }

        this.RefreshSudokuRoommateActivity(force: true);
        this.Monitor.Log(
            $"Test profile '{profile}' applied: stages={values.Value.stages}/{this.dailySudoku.GetStageCount()}, trust={values.Value.trust}/30, Endless={(this.dailySudoku.IsEndlessPracticeUnlocked() ? "unlocked" : "locked")}.",
            LogLevel.Info
        );
    }

    private void OnTestEndlessCommand(string command, string[] args)
    {
        if (!this.TryRequireWorld(command) || this.dailySudoku is null)
            return;

        this.PrepareCoreForFeatureTesting();
        this.SetStageClearsForTesting(this.dailySudoku.GetStageCount());
        this.Monitor.Log("All current Stages marked clear. Endless Practice is unlocked for testing.", LogLevel.Info);
    }

    private void OnTestSpiritEveCommand(string command, string[] args)
    {
        if (!this.TryRequireWorld(command))
            return;

        string mode = args.Length > 0 ? args[0].Trim().ToLowerInvariant() : "";
        if (mode == "reset")
        {
            Game1.player.modData.Remove(ModIdentity.SpiritEvePlanYearKey);
            Game1.player.modData.Remove(ModIdentity.SpiritEveScareModeKey);
            Game1.player.modData.Remove(ModIdentity.SpiritEveAftermathYearKey);
            this.Helper.GameContent.InvalidateCache("Data/Festivals/fall27");
            this.Monitor.Log("Spirit's Eve test state reset for the current save.", LogLevel.Info);
            return;
        }

        if (mode is not ("gentle" or "wild" or "lewis"))
        {
            this.Monitor.Log("Usage: heyyourecursed_test_spiriteve <gentle|wild|lewis|reset>", LogLevel.Info);
            return;
        }

        this.PrepareCoreForFeatureTesting();
        ModIdentity.SetSpiritEveScareMode(Game1.player, mode);
        Game1.player.modData.Remove(ModIdentity.SpiritEveAftermathYearKey);
        this.Helper.GameContent.InvalidateCache("Data/Festivals/fall27");
        this.Monitor.Log($"Spirit's Eve scare mode set to '{mode}' for year {Game1.year}.", LogLevel.Info);
    }

    private void OnTestHelpCommand(string command, string[] args)
    {
        this.Monitor.Log(
            "Hey! You’re Cursed! test commands:\n"
            + "  heyyourecursed_test_solve — auto-solve the currently open Sudoku board.\n"
            + "  heyyourecursed_test_profile <fresh|early|mid|late|complete> — jump to a progression/Trust profile.\n"
            + "  heyyourecursed_test_trust <0-30> — set Trust directly.\n"
            + "  heyyourecursed_test_stages <0-18> — set unique Stage clears directly.\n"
            + "  heyyourecursed_test_endless — clear all current Stages and unlock Endless Practice.\n"
            + "  heyyourecursed_test_spiriteve <gentle|wild|lewis|reset> — set/reset Spirit's Eve test state.\n"
            + "Existing: heyyourecursed_talk, heyyourecursed_stages, heyyourecursed_open, heyyourecursed_status, heyyourecursed_resetdaily, heyyourecursed_resetarrival.",
            LogLevel.Info
        );
    }

    private bool TryRequireWorld(string command)
    {
        if (Context.IsWorldReady)
            return true;

        this.Monitor.Log($"Load a save before using {command}.", LogLevel.Warn);
        return false;
    }

    private void PrepareCoreForFeatureTesting()
    {
        Game1.player.modData[ModIdentity.ArrivalSeenKey] = "true";
        Game1.player.modData[ModIdentity.SudokuNpcEnabledKey] = "true";
        Game1.player.modData[ModIdentity.FirstConversationCompletedKey] = "true";
        Game1.player.modData[ModIdentity.CursedVhsInstalledKey] = "true";
        ModIdentity.MarkDailySignalRunToday(Game1.player);
        this.RemoveAllCursedVhsFromInventory();
    }

    private void SetStageClearsForTesting(int count)
    {
        if (this.dailySudoku is null)
            return;

        this.dailySudoku.ResetStageProgressForTesting();
        IReadOnlyList<SudokuPuzzle> stages = this.dailySudoku.GetStages();
        int clamped = Math.Clamp(count, 0, stages.Count);

        for (int i = 0; i < clamped; i++)
            Game1.player.modData[ModIdentity.StageClearedPrefix + stages[i].Id] = "true";

        Game1.player.modData[ModIdentity.SudokuSolvedCountKey] = clamped.ToString();
        Game1.player.modData[ModIdentity.StageProgressMigratedKey] = "true";
        Game1.player.modData.Remove(ModIdentity.PracticePuzzleIdKey);
        Game1.player.modData.Remove(ModIdentity.PracticeBoardKey);
        Game1.player.modData.Remove(ModIdentity.PracticeCursorKey);
    }
}
