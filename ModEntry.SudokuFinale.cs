using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace HeyYoureCursed;

internal sealed partial class ModEntry
{
    internal static string LocalizeDifficulty(string difficulty)
    {
        string key = difficulty.ToLowerInvariant() switch
        {
            "easy" => "difficulty.easy",
            "normal" => "difficulty.normal",
            "hard" => "difficulty.hard",
            _ => string.Empty
        };
        return string.IsNullOrEmpty(key) ? difficulty : T(key);
    }

    private bool TryShowSudokuCapstone()
    {
        if (!Context.IsWorldReady || this.dailySudoku is null || ModIdentity.HasSudokuCapstoneSeen(Game1.player))
            return false;

        if (!ModIdentity.HasFirstConversationCompleted(Game1.player)
            || this.IsSudokuSealed()
            || !this.IsSudokuActiveHaunting()
            || this.dailySudoku.GetSolvedCount() < this.dailySudoku.GetStageCount()
            || ModIdentity.GetSudokuTrust(Game1.player) < 30)
        {
            return false;
        }

        Game1.activeClickableMenu = new SudokuCapstoneMenu(
            this.LoadSudokuPortraitTexture(),
            choice => Game1.player.modData[ModIdentity.SudokuCapstoneChoiceKey] = choice,
            () => this.FinishSudokuCapstone(Game1.player.modData.TryGetValue(ModIdentity.SudokuCapstoneChoiceKey, out string? saved) ? saved : "stay")
        );
        Game1.playSound("ghost");
        this.Monitor.Log("Sudoku capstone opened: all stages clear and Trust 30/30.", LogLevel.Info);
        return true;
    }

    private void FinishSudokuCapstone(string choice)
    {
        if (ModIdentity.HasSudokuCapstoneSeen(Game1.player))
            return;

        ModIdentity.MarkSudokuCapstoneSeen(Game1.player, choice);
        Game1.playSound("yoba");
        this.Monitor.Log($"Sudoku capstone completed with choice={choice}.", LogLevel.Info);
        this.ShowDailySudokuConversation(T("capstone.after"), forceFresh: true);
    }

    private void ShowChannel18Broadcast()
    {
        if (!Context.IsWorldReady || !ModIdentity.HasSudokuCapstoneSeen(Game1.player))
            return;

        string season = Game1.currentSeason.ToLowerInvariant() switch
        {
            "spring" => "spring",
            "summer" => "summer",
            "fall" => "fall",
            _ => "winter"
        };
        int variant = Math.Abs((Game1.Date.TotalDays + (int)(Game1.uniqueIDForThisGame % 97)) % 2);
        string prefix = $"channel18.broadcast.{season}.{variant}";
        Texture2D? portraits = this.LoadSudokuPortraitTexture();
        Game1.activeClickableMenu = new SudokuConversationMenu(
            portraits,
            portraitIndex: 4,
            lines: new[] { T($"{prefix}.a"), T($"{prefix}.b") },
            question: T("channel18.progress"),
            primaryLabel: T("ui.common.continue"),
            secondaryLabel: T("ui.common.later"),
            progressText: T("channel18.title"),
            onPrimary: this.ShowSudokuStageSelect,
            onSecondary: this.ShowSudokuStageSelect
        );
    }
}
