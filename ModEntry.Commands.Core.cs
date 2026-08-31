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

        DailySudokuService sudokuService = this.dailySudoku;
        int cleared = sudokuService.GetSolvedCount();
        int total = sudokuService.GetStageCount();
        bool practiceUnlocked = sudokuService.IsEndlessPracticeUnlocked();

        List<SudokuChoiceOption> options = new()
        {
            new SudokuChoiceOption
            {
                Label = T("stage.option.daily"),
                Action = () => this.OpenDailySudoku(force: false, returnToStageSelect: true)
            },
            new SudokuChoiceOption
            {
                Label = T("stage.option.stages"),
                Action = this.ShowSudokuStageGrid
            },
            new SudokuChoiceOption
            {
                Label = practiceUnlocked ? T("stage.option.practice") : T("stage.option.practice-locked"),
                Action = practiceUnlocked
                    ? this.OpenPracticeSudoku
                    : () => this.ShowSudokuStageSelect()
            },
        };

        if (ModIdentity.HasSudokuCapstoneSeen(Game1.player))
        {
            options.Add(new SudokuChoiceOption
            {
                Label = T("channel18.option"),
                Action = this.ShowChannel18Broadcast
            });
        }

        options.Add(new SudokuChoiceOption
        {
            Label = T("ui.common.later"),
            Action = () => { }
        });

        Game1.activeClickableMenu = new SudokuChoiceMenu(
            this.LoadSudokuPortraitTexture(),
            portraitIndex: practiceUnlocked ? 4 : 0,
            lines: new[]
            {
                T("stage.hub.line.1"),
                practiceUnlocked
                    ? T("stage.hub.line.2.complete")
                    : T("stage.hub.line.2.incomplete")
            },
            question: T("stage.hub.question"),
            progressText: T("stage.hub.progress", new { cleared, total, unlocked = practiceUnlocked ? T("stage.hub.unlocked") : string.Empty }),
            options: options
        );
    }

    private void ShowSudokuStageGrid()
    {
        if (!Context.IsWorldReady || this.dailySudoku is null)
            return;

        Game1.activeClickableMenu = new SudokuStageSelectMenu(
            this.dailySudoku,
            onDaily: () => this.OpenDailySudoku(force: false, returnToStageSelect: true),
            onStage: this.OpenStageSudoku
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
            this.Monitor.Log("Couldn't open Daily Sudoku because no valid puzzle was available.", LogLevel.Error);
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

    private void OpenPracticeSudoku()
    {
        if (!Context.IsWorldReady || this.dailySudoku is null)
            return;

        SudokuPuzzle? puzzle = this.dailySudoku.PrepareNextPracticePuzzle();
        if (puzzle is null)
        {
            Game1.playSound("cancel");
            this.ShowSudokuStageSelect();
            return;
        }

        Game1.activeClickableMenu = new SudokuMenu(
            this.dailySudoku,
            puzzle,
            SudokuPlayMode.Practice,
            stageIndex: -1,
            returnToStageSelect: this.ShowSudokuStageSelect
        );

        this.Monitor.Log($"Endless Practice opened: puzzle={puzzle.Id}, difficulty={puzzle.Difficulty}.", LogLevel.Info);
    }

    private void OpenStageSudoku(int stageIndex)
    {
        if (!Context.IsWorldReady || this.dailySudoku is null)
            return;

        if (!this.dailySudoku.IsStageUnlocked(stageIndex))
        {
            this.Monitor.Log($"Stage {stageIndex + 1:00} is still locked.", LogLevel.Trace);
            this.ShowSudokuStageGrid();
            return;
        }

        SudokuPuzzle? puzzle = this.dailySudoku.GetStage(stageIndex);
        if (puzzle is null)
        {
            this.Monitor.Log($"Couldn't open Sudoku Stage {stageIndex + 1:00}: no puzzle exists.", LogLevel.Error);
            this.ShowSudokuStageGrid();
            return;
        }

        this.dailySudoku.PrepareStageForPlay(stageIndex);
        Game1.activeClickableMenu = new SudokuMenu(
            this.dailySudoku,
            puzzle,
            SudokuPlayMode.Stage,
            stageIndex,
            returnToStageSelect: this.ShowSudokuStageGrid
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
        this.Monitor.Log($"Today's Sudoku reset. Current puzzle={puzzle?.Id ?? "none"}.", LogLevel.Info);
    }
}
