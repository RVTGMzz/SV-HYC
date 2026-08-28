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
    private void OpenDailySudoku(bool force)
    {
        if (!Context.IsWorldReady || this.dailySudoku is null)
            return;

        if (!force)
        {
            if (!this.Config.EnableDailySudoku || !ModIdentity.HasArrivalBeenSeen(Game1.player))
                return;
        }

        SudokuPuzzle? puzzle = this.dailySudoku.EnsureToday();
        if (puzzle is null)
        {
            this.Monitor.Log(
                "Couldn't open Daily Sudoku because no valid puzzle was available.",
                LogLevel.Error
            );
            return;
        }

        Game1.activeClickableMenu = new SudokuMenu(this.dailySudoku, puzzle);
        this.Monitor.Log($"Daily Sudoku menu opened: puzzle={puzzle.Id}, difficulty={puzzle.Difficulty}.", LogLevel.Info);
    }

    private void OnOpenDailyCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save before using sudoku_open.", LogLevel.Warn);
            return;
        }

        this.OpenDailySudoku(force: true);
    }

    private void OnResetDailyCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady || this.dailySudoku is null)
        {
            this.Monitor.Log("Load a save before using sudoku_resetdaily.", LogLevel.Warn);
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
