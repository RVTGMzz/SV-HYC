using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace HeyYoureCursed;

internal sealed partial class ModEntry
{
    private int sudokuHubSelectedIndex;

    private void OnOpenTalkCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save before using heyyourecursed_talk.", StardewModdingAPI.LogLevel.Warn);
            return;
        }

        this.sudokuHubSelectedIndex = 0;
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
                Label = T("hub.option.play"),
                Action = this.ShowSudokuStageSelect
            },
            new SudokuChoiceOption
            {
                Label = T("hub.option.talk"),
                Action = this.ShowNaturalSudokuTalk
            },
            new SudokuChoiceOption
            {
                Label = T("hub.option.activity"),
                Action = this.ShowSudokuActivityTalk
            },
            new SudokuChoiceOption
            {
                Label = T("hub.option.gift"),
                Action = this.ShowSudokuGiftOffer
            },
            new SudokuChoiceOption
            {
                Label = T(spiritEve ? "hub.option.spiriteve" : "hub.option.strange"),
                Action = spiritEve ? this.ShowSpiritEvePlan : this.ShowSudokuStrangeTalk
            },
            new SudokuChoiceOption
            {
                Label = T("ui.common.later"),
                Action = () => this.sudokuHubSelectedIndex = 0
            }
        };

        this.sudokuHubSelectedIndex = Math.Clamp(this.sudokuHubSelectedIndex, 0, Math.Max(0, options.Count - 1));

        Texture2D? portraits = this.LoadSudokuPortraitTexture();
        Game1.activeClickableMenu = new SudokuChoiceMenu(
            portraits,
            portraitIndex,
            openingLines ?? new[] { T("hub.opening") },
            question: question ?? T("hub.question"),
            progressText: T("hub.progress", new
            {
                cleared = stageClears,
                total = totalStages,
                trust,
                label = trustLabel
            }),
            options: options,
            initialSelectedIndex: this.sudokuHubSelectedIndex,
            onOptionActivated: index => this.sudokuHubSelectedIndex = index,
            onClosed: () => this.sudokuHubSelectedIndex = 0
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
            ? T("hub.trust.gained", new { trust })
            : T("hub.trust.already", new { trust });

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
            ? T("hub.activity.gained", new { activity = activityLabel, trust })
            : T("hub.activity.normal", new { activity = activityLabel, trust });

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
            ? T("hub.strange.gained", new { trust })
            : T("hub.strange.normal", new { trust });

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
            question: returnToHub ? T("dialogue.more") : "...",
            primaryLabel: returnToHub ? T("dialogue.stay") : T("dialogue.ok"),
            secondaryLabel: T("ui.common.later"),
            progressText: progressText,
            onPrimary: returnToHub ? () => this.ShowSudokuInteractionHub() : () => { },
            onSecondary: () => this.sudokuHubSelectedIndex = 0
        );
    }
}
