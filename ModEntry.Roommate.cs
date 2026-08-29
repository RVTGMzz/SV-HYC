using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace HeyYoureCursed;

internal sealed partial class ModEntry
{
    private void OnOpenTalkCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save before using heyyourecursed_talk.", StardewModdingAPI.LogLevel.Warn);
            return;
        }

        this.ShowDailySudokuConversation(forceFresh: true);
    }

    private void ShowSudokuInteractionHub(
        IEnumerable<string>? openingLines = null,
        int portraitIndex = 4,
        string? question = null,
        bool markDailyDialogue = false
    )
    {
        DailySudokuService? sudokuService = this.dailySudoku;
        if (!Context.IsWorldReady || sudokuService is null)
            return;

        if (markDailyDialogue && !ModIdentity.HasDailyDialogueRunToday(Game1.player))
            ModIdentity.MarkDailyDialogueRunToday(Game1.player);

        int stageClears = sudokuService.GetSolvedCount();
        int totalStages = sudokuService.GetStageCount();
        int trust = ModIdentity.GetSudokuTrust(Game1.player);
        string trustLabel = SudokuRoommateLibrary.GetTrustLabel(trust);
        bool spiritEve = this.IsSpiritEveToday();

        List<SudokuChoiceOption> options = new()
        {
            new SudokuChoiceOption
            {
                Label = "Chơi Sudoku",
                Action = this.ShowSudokuStageSelect
            },
            new SudokuChoiceOption
            {
                Label = "Nói chuyện",
                Action = this.ShowNaturalSudokuTalk
            },
            new SudokuChoiceOption
            {
                Label = "Hôm nay cô đang làm gì?",
                Action = this.ShowSudokuActivityTalk
            },
            new SudokuChoiceOption
            {
                Label = spiritEve ? "Spirit's Eve tối nay?" : "Có gì lạ không?",
                Action = spiritEve ? this.ShowSpiritEvePlan : this.ShowSudokuStrangeTalk
            },
            new SudokuChoiceOption
            {
                Label = "Để sau",
                Action = () => { }
            }
        };

        Texture2D? portraits = this.LoadSudokuPortraitTexture();
        Game1.activeClickableMenu = new SudokuChoiceMenu(
            portraits,
            portraitIndex,
            openingLines ?? new[] { "Sudoku nhìn bạn, chờ bạn lên tiếng." },
            question: question ?? "...Hôm nay ngươi muốn làm gì?",
            progressText: $"Puzzle Bond {stageClears}/{totalStages}  •  Trust {trust}/30 — {trustLabel}",
            options: options
        );
    }

    private void ShowNaturalSudokuTalk()
    {
        DailySudokuService? sudokuService = this.dailySudoku;
        if (!Context.IsWorldReady || sudokuService is null)
            return;

        bool gained = ModIdentity.TryGainDailyTrust(Game1.player);
        int trust = ModIdentity.GetSudokuTrust(Game1.player);
        int stageClears = sudokuService.GetSolvedCount();
        SudokuRoommateDialogue dialogue = SudokuRoommateLibrary.GetNaturalTalk(trust, stageClears);

        string trustNote = gained
            ? $"Trust tăng lên {trust}/30."
            : $"Trust {trust}/30 • hôm nay hai người đã dành thời gian nói chuyện rồi.";

        this.ShowSimpleSudokuDialogue(
            dialogue.PortraitIndex,
            dialogue.Lines,
            trustNote,
            returnToHub: true
        );
    }

    private void ShowSudokuActivityTalk()
    {
        if (!Context.IsWorldReady)
            return;

        bool gained = ModIdentity.TryGainDailyTrust(Game1.player);
        int trust = ModIdentity.GetSudokuTrust(Game1.player);
        SudokuActivityKind activity = this.GetCurrentSudokuActivityKind();
        SudokuRoommateDialogue dialogue = SudokuRoommateLibrary.GetActivity(activity, trust);
        string activityLabel = SudokuRoommateLibrary.GetActivityLabel(activity);
        string note = gained
            ? $"{activityLabel} • Trust tăng lên {trust}/30"
            : $"{activityLabel} • Trust {trust}/30";
        this.ShowSimpleSudokuDialogue(
            dialogue.PortraitIndex,
            dialogue.Lines,
            note,
            returnToHub: true
        );
    }

    private void ShowSudokuStrangeTalk()
    {
        if (!Context.IsWorldReady)
            return;

        bool gained = ModIdentity.TryGainDailyTrust(Game1.player);
        int trust = ModIdentity.GetSudokuTrust(Game1.player);
        SudokuRoommateDialogue dialogue = SudokuRoommateLibrary.GetStrangeThing(trust);
        string note = gained
            ? $"Paranormal note • Trust tăng lên {trust}/30"
            : $"Paranormal note • Trust {trust}/30";
        this.ShowSimpleSudokuDialogue(
            dialogue.PortraitIndex,
            dialogue.Lines,
            note,
            returnToHub: true
        );
    }

    private void ShowSimpleSudokuDialogue(
        int portraitIndex,
        IEnumerable<string> lines,
        string progressText,
        bool returnToHub
    )
    {
        Texture2D? portraits = this.LoadSudokuPortraitTexture();
        Game1.activeClickableMenu = new SudokuConversationMenu(
            portraits,
            portraitIndex,
            lines,
            question: returnToHub ? "...Còn gì nữa không?" : "...",
            primaryLabel: returnToHub ? "Ở lại" : "Được.",
            secondaryLabel: "Để sau",
            progressText: progressText,
            onPrimary: returnToHub ? () => this.ShowSudokuInteractionHub() : () => { },
            onSecondary: () => { }
        );
    }
}
