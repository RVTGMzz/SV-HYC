using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Objects;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace HeyYoureCursed;

internal sealed partial class ModEntry
{
    private void ShowSudokuStageSelect()
    {
        if (!Context.IsWorldReady || this.dailySudoku is null)
            return;

        Game1.activeClickableMenu = new SudokuStageSelectMenu(
            this.dailySudoku,
            onDaily: () => this.OpenDailySudoku(force: false, returnToStageSelect: true),
            onStage: this.OpenStageSudoku
        );

        this.Monitor.Log(
            $"Sudoku Stage Select opened: cleared={this.dailySudoku.GetSolvedCount()}/{this.dailySudoku.GetStageCount()}, dailyClaimed={this.dailySudoku.IsRewardClaimedToday()}.",
            LogLevel.Trace
        );
    }

    private void OpenDailySudoku(bool force, bool returnToStageSelect = false)
    {
        if (!Context.IsWorldReady || this.dailySudoku is null)
            return;

        if (!force && (!this.Config.EnableDailySudoku || !ModIdentity.HasArrivalBeenSeen(Game1.player)))
            return;

        SudokuPuzzle? puzzle = this.dailySudoku.EnsureToday();
        if (puzzle is null)
        {
            this.Monitor.Log(
                "Couldn't open Daily Sudoku because no valid puzzle was available.",
                LogLevel.Error
            );
            return;
        }

        Game1.activeClickableMenu = new SudokuMenu(
            this.dailySudoku,
            puzzle,
            SudokuPlayMode.DailyChallenge,
            stageIndex: -1,
            returnToStageSelect: returnToStageSelect ? this.ShowSudokuStageSelect : null
        );

        this.Monitor.Log(
            $"Daily Sudoku menu opened: puzzle={puzzle.Id}, difficulty={puzzle.Difficulty}, rewardClaimed={this.dailySudoku.IsRewardClaimedToday()}.",
            LogLevel.Info
        );
    }

    private void OpenStageSudoku(int stageIndex)
    {
        if (!Context.IsWorldReady || this.dailySudoku is null)
            return;

        if (!this.dailySudoku.IsStageUnlocked(stageIndex))
        {
            this.Monitor.Log($"Stage {stageIndex + 1:00} is still locked.", LogLevel.Trace);
            this.ShowSudokuStageSelect();
            return;
        }

        SudokuPuzzle? puzzle = this.dailySudoku.GetStage(stageIndex);
        if (puzzle is null)
        {
            this.Monitor.Log($"Couldn't open Sudoku Stage {stageIndex + 1:00}: no puzzle exists.", LogLevel.Error);
            this.ShowSudokuStageSelect();
            return;
        }

        this.dailySudoku.PrepareStageForPlay(stageIndex);

        Game1.activeClickableMenu = new SudokuMenu(
            this.dailySudoku,
            puzzle,
            SudokuPlayMode.Stage,
            stageIndex,
            returnToStageSelect: this.ShowSudokuStageSelect
        );

        this.Monitor.Log(
            $"Sudoku Stage opened: stage={stageIndex + 1}/{this.dailySudoku.GetStageCount()}, puzzle={puzzle.Id}, difficulty={puzzle.Difficulty}, cleared={this.dailySudoku.IsStageCleared(stageIndex)}.",
            LogLevel.Info
        );
    }

    private void OnOpenDailyCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save before using heyyourecursed_open.", LogLevel.Warn);
            return;
        }

        this.OpenDailySudoku(force: true);
    }

    private void OnOpenStagesCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady || this.dailySudoku is null)
        {
            this.Monitor.Log("Load a save before using heyyourecursed_stages.", LogLevel.Warn);
            return;
        }

        this.ShowSudokuStageSelect();
    }

    private void OnResetDailyCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady || this.dailySudoku is null)
        {
            this.Monitor.Log("Load a save before using heyyourecursed_resetdaily.", LogLevel.Warn);
            return;
        }

        this.dailySudoku.ResetTodayForTesting();
        SudokuPuzzle? puzzle = this.dailySudoku.EnsureToday();
        this.Monitor.Log(
            $"Today's Sudoku reset. Current puzzle={puzzle?.Id ?? "none"}.",
            LogLevel.Info
        );
    }
}
