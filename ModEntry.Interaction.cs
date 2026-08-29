using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace HeyYoureCursed;

internal sealed partial class ModEntry
{
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;

        if (Game1.activeClickableMenu is OccultCabinetMenu occultCabinetMenu)
        {
            if (occultCabinetMenu.HandleSmapiInput(e.Button))
                this.Helper.Input.Suppress(e.Button);
            return;
        }

        if (Game1.activeClickableMenu is WizardRevealMenu wizardRevealMenu)
        {
            if (wizardRevealMenu.HandleSmapiInput(e.Button))
                this.Helper.Input.Suppress(e.Button);
            return;
        }

        if (Game1.activeClickableMenu is SaloonPrologueMenu saloonPrologueMenu)
        {
            if (saloonPrologueMenu.HandleSmapiInput(e.Button))
                this.Helper.Input.Suppress(e.Button);
            return;
        }

        if (Game1.activeClickableMenu is SudokuMenu sudokuMenu)
        {
            if (sudokuMenu.HandleSmapiInput(e.Button))
                this.Helper.Input.Suppress(e.Button);
            return;
        }

        if (Game1.activeClickableMenu is SudokuConversationMenu conversationMenu)
        {
            if (conversationMenu.HandleSmapiInput(e.Button))
                this.Helper.Input.Suppress(e.Button);
            return;
        }

        if (Game1.activeClickableMenu is SudokuChoiceMenu choiceMenu)
        {
            if (choiceMenu.HandleSmapiInput(e.Button))
                this.Helper.Input.Suppress(e.Button);
            return;
        }

        if (Game1.activeClickableMenu is SudokuStageSelectMenu stageSelectMenu)
        {
            if (stageSelectMenu.HandleSmapiInput(e.Button))
                this.Helper.Input.Suppress(e.Button);
            return;
        }

        if (this.sequenceActive || Game1.activeClickableMenu is not null)
            return;

        bool isActionButton = e.Button.IsActionButton() || e.Button == SButton.ControllerA;
        bool isMouseClick = e.Button == SButton.MouseLeft;
        if (!isActionButton && !isMouseClick)
            return;

        if (this.TryHandleOccultCabinetInteraction(e))
            return;

        if (!ModIdentity.HasArrivalBeenSeen(Game1.player) || this.IsSudokuSealed())
            return;

        if (this.IsAtSpiritEveFestival())
            return;

        NPC? sudoku = this.FindSudoku(currentLocationOnly: true);
        if (sudoku is null)
            return;

        if (ModIdentity.HasDailySignalRunToday(Game1.player))
            sudoku.IsInvisible = false;

        if (sudoku.IsInvisible)
            return;

        float distance = Vector2.Distance(Game1.player.Tile, sudoku.Tile);
        if (distance > 3.25f || !this.IsSudokuInteractionTarget(sudoku, e))
            return;

        Vector2 lookDelta = Game1.player.Tile - sudoku.Tile;
        if (Math.Abs(lookDelta.X) > Math.Abs(lookDelta.Y))
            sudoku.faceDirection(lookDelta.X > 0 ? 1 : 3);
        else if (Math.Abs(lookDelta.Y) > 0.01f)
            sudoku.faceDirection(lookDelta.Y > 0 ? 2 : 0);

        this.Helper.Input.Suppress(e.Button);

        if (!ModIdentity.HasFirstConversationCompleted(Game1.player))
        {
            this.ShowFirstConversation();
            return;
        }

        this.ShowDailySudokuConversation();
    }

    private bool IsSudokuInteractionTarget(NPC sudoku, ButtonPressedEventArgs e)
    {
        if (e.Button == SButton.MouseLeft)
        {
            Vector2 clickedTile = e.Cursor.GrabTile;
            return Vector2.Distance(clickedTile, sudoku.Tile) <= 0.85f;
        }

        Vector2 delta = sudoku.Tile - Game1.player.Tile;
        float forward;
        float sideways;

        switch (Game1.player.FacingDirection)
        {
            case 0: forward = -delta.Y; sideways = Math.Abs(delta.X); break;
            case 1: forward = delta.X; sideways = Math.Abs(delta.Y); break;
            case 2: forward = delta.Y; sideways = Math.Abs(delta.X); break;
            case 3: forward = -delta.X; sideways = Math.Abs(delta.Y); break;
            default: return false;
        }

        return forward >= 0.10f && forward <= 1.75f && sideways <= 0.80f;
    }

    private Texture2D? LoadSudokuPortraitTexture()
    {
        try
        {
            return this.Helper.GameContent.Load<Texture2D>($"Portraits/{ModIdentity.SudokuNpcId}");
        }
        catch (Exception gameContentError)
        {
            try
            {
                return this.Helper.ModContent.Load<Texture2D>("assets/Portraits/Sudoku.png");
            }
            catch (Exception modContentError)
            {
                this.Monitor.Log($"Couldn't load Sudoku portraits for conversation UI. GameContent: {gameContentError.Message}; ModContent: {modContentError.Message}", LogLevel.Error);
                return null;
            }
        }
    }

    private void ShowFirstConversation()
    {
        Texture2D? portraits = this.LoadSudokuPortraitTexture();
        Game1.activeClickableMenu = new SudokuConversationMenu(
            portraits,
            portraitIndex: 6,
            lines: new[] { T("intro.0.line1"), T("intro.0.line2") },
            question: T("intro.0.question"),
            primaryLabel: T("intro.0.primary"),
            secondaryLabel: T("intro.0.secondary"),
            progressText: T("intro.progress"),
            onPrimary: () => this.ShowFirstConversationPencil(tvAnswer: false),
            onSecondary: () => this.ShowFirstConversationPencil(tvAnswer: true)
        );
    }

    private void ShowFirstConversationPencil(bool tvAnswer)
    {
        Texture2D? portraits = this.LoadSudokuPortraitTexture();
        string[] lines = tvAnswer
            ? new[] { T("intro.pencil.tv.1"), T("intro.pencil.tv.2"), T("intro.pencil.tv.3") }
            : new[] { T("intro.pencil.normal.1"), T("intro.pencil.normal.2"), T("intro.pencil.normal.3") };

        Game1.activeClickableMenu = new SudokuConversationMenu(
            portraits,
            portraitIndex: tvAnswer ? 1 : 0,
            lines: lines,
            question: T("intro.pencil.question"),
            primaryLabel: T(this.HasStoryPencil() ? "intro.pencil.primary" : "intro.pencil.missing"),
            secondaryLabel: T("intro.pencil.secondary"),
            progressText: T("intro.progress"),
            onPrimary: () => this.CompleteFirstConversation(broughtHoe: false),
            onSecondary: () => this.CompleteFirstConversation(broughtHoe: true)
        );
    }

    private void CompleteFirstConversation(bool broughtHoe)
    {
        if (broughtHoe)
        {
            this.ShowPencilRequiredWarning(usedHoe: true);
            return;
        }

        if (!this.ConsumeOneStoryPencil())
        {
            this.ShowPencilRequiredWarning(usedHoe: false);
            return;
        }

        Game1.player.modData[ModIdentity.PencilAcceptedKey] = "true";
        Game1.player.modData[ModIdentity.FirstConversationCompletedKey] = "true";
        if (!Game1.player.modData.ContainsKey(ModIdentity.SudokuActivatedDayKey))
            Game1.player.modData[ModIdentity.SudokuActivatedDayKey] = Game1.Date.TotalDays.ToString();

        this.ShowDailySudokuConversation(T("intro.complete.pencil"), forceFresh: true);
    }

    private void ShowPencilRequiredWarning(bool usedHoe)
    {
        Texture2D? portraits = this.LoadSudokuPortraitTexture();
        string[] lines = usedHoe
            ? new[] { T("intro.pencil.warning.hoe.1"), T("intro.pencil.warning.hoe.2"), T("intro.pencil.warning.hoe.3") }
            : new[] { T("intro.pencil.warning.missing.1"), T("intro.pencil.warning.missing.2"), T("intro.pencil.warning.missing.3") };

        Game1.activeClickableMenu = new SudokuConversationMenu(
            portraits,
            portraitIndex: usedHoe ? 6 : 1,
            lines: lines,
            question: T("intro.pencil.warning.question"),
            primaryLabel: T("intro.pencil.warning.buy"),
            secondaryLabel: T("ui.common.later"),
            progressText: T("intro.progress"),
            onPrimary: this.ShowPencilShopThought,
            onSecondary: this.ShowPencilShopThought
        );
    }

    private void ShowPencilShopThought()
    {
        if (Context.IsWorldReady)
            Game1.drawObjectDialogue(T("story.player-thought.pencil-shop"));
    }

    private bool HasStoryPencil() => Game1.player.Items.Any(item => item?.QualifiedItemId == ModIdentity.PencilQualifiedItemId);

    private bool ConsumeOneStoryPencil()
    {
        for (int i = 0; i < Game1.player.Items.Count; i++)
        {
            Item? item = Game1.player.Items[i];
            if (item?.QualifiedItemId != ModIdentity.PencilQualifiedItemId)
                continue;

            item.Stack--;
            if (item.Stack <= 0)
                Game1.player.Items[i] = null;
            Game1.playSound("coin");
            return true;
        }
        return false;
    }

    private void ShowDailySudokuConversation(string? preface = null, bool forceFresh = false)
    {
        if (!Context.IsWorldReady)
            return;

        if (!this.Config.EnableDailySudoku || this.dailySudoku is null)
        {
            Texture2D? disabledPortraits = this.LoadSudokuPortraitTexture();
            Game1.activeClickableMenu = new SudokuConversationMenu(
                disabledPortraits,
                portraitIndex: 1,
                lines: new[] { T("daily.disabled.line") },
                question: T("daily.disabled.question"),
                primaryLabel: T("dialogue.ok"),
                secondaryLabel: T("ui.common.later"),
                progressText: string.Empty,
                onPrimary: () => { },
                onSecondary: () => { }
            );
            return;
        }

        int solvedCount = this.dailySudoku.GetSolvedCount();
        bool solvedToday = this.dailySudoku.IsRewardClaimedToday();
        bool repeatTalk = !forceFresh && ModIdentity.HasDailyDialogueRunToday(Game1.player);

        if (!repeatTalk)
            ModIdentity.MarkDailyDialogueRunToday(Game1.player);

        SudokuDailyDialogue dialogue = SudokuDialogueLibrary.GetForToday(solvedCount, repeatTalk, solvedToday);
        List<string> lines = new();
        if (!string.IsNullOrWhiteSpace(preface))
            lines.Add(preface);

        int portraitIndex = dialogue.PortraitIndex;
        if (this.TryConsumeSpiritEveAftermath(out string[] aftermathLines, out int aftermathPortrait))
        {
            lines.AddRange(aftermathLines);
            portraitIndex = aftermathPortrait;
        }
        else
        {
            lines.AddRange(dialogue.Lines);
        }

        this.ShowSudokuInteractionHub(
            openingLines: lines,
            portraitIndex: portraitIndex,
            question: dialogue.Question,
            markDailyDialogue: false
        );

        this.Monitor.Log($"Sudoku roommate hub opened: stageClears={solvedCount}, trust={ModIdentity.GetSudokuTrust(Game1.player)}, repeatTalk={repeatTalk}, solvedToday={solvedToday}, portrait={portraitIndex}.", LogLevel.Trace);
    }
}
