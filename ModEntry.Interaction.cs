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
        if (!Context.IsWorldReady || this.sequenceActive)
            return;

        // Custom menus get an explicit SMAPI controller path so gamepad behavior stays
        // consistent even when Stardew doesn't forward every controller button itself.
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

        if (Game1.activeClickableMenu is SudokuStageSelectMenu stageSelectMenu)
        {
            if (stageSelectMenu.HandleSmapiInput(e.Button))
                this.Helper.Input.Suppress(e.Button);

            return;
        }

        if (Game1.activeClickableMenu is not null)
            return;

        bool isActionButton = e.Button.IsActionButton() || e.Button == SButton.ControllerA;
        bool isMouseClick = e.Button == SButton.MouseLeft;
        if (!isActionButton && !isMouseClick)
            return;

        if (!ModIdentity.HasArrivalBeenSeen(Game1.player))
            return;

        NPC? sudoku = this.FindSudoku(currentLocationOnly: true);
        if (sudoku is null)
            return;

        if (ModIdentity.HasDailySignalRunToday(Game1.player))
            sudoku.IsInvisible = false;

        if (sudoku.IsInvisible)
            return;

        float distance = Vector2.Distance(Game1.player.Tile, sudoku.Tile);
        if (distance > 3.25f)
            return;

        if (!this.IsSudokuInteractionTarget(sudoku, e))
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
            case 0:
                forward = -delta.Y;
                sideways = Math.Abs(delta.X);
                break;
            case 1:
                forward = delta.X;
                sideways = Math.Abs(delta.Y);
                break;
            case 2:
                forward = delta.Y;
                sideways = Math.Abs(delta.X);
                break;
            case 3:
                forward = -delta.X;
                sideways = Math.Abs(delta.Y);
                break;
            default:
                return false;
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
                this.Monitor.Log(
                    $"Couldn't load Sudoku portraits for conversation UI. GameContent: {gameContentError.Message}; ModContent: {modContentError.Message}",
                    LogLevel.Error
                );
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
            lines: new[]
            {
                "Sudoku nhìn bạn không chớp mắt.",
                "\"...Ngươi thấy ta?\""
            },
            question: "Trả lời thế nào?",
            primaryLabel: "Ừ. Rõ lắm.",
            secondaryLabel: "Tôi đang nói với TV.",
            progressText: "Lần gặp đầu tiên",
            onPrimary: () => this.ShowFirstConversationPencil(tvAnswer: false),
            onSecondary: () => this.ShowFirstConversationPencil(tvAnswer: true)
        );
    }

    private void ShowFirstConversationPencil(bool tvAnswer)
    {
        Texture2D? portraits = this.LoadSudokuPortraitTexture();

        string[] lines = tvAnswer
            ? new[]
            {
                "\"Tốt.\"",
                "Sudoku liếc sang chiếc TV. \"...Nó nói chuyện dễ hiểu hơn ngươi.\"",
                "\"...Có bút chì không?\""
            }
            : new[]
            {
                "Sudoku im lặng vài giây.",
                "\"...Phiền thật.\"",
                "\"...Ngươi có bút chì không?\""
            };

        Game1.activeClickableMenu = new SudokuConversationMenu(
            portraits,
            portraitIndex: tvAnswer ? 1 : 0,
            lines: lines,
            question: "Bạn đưa gì cho cô ấy?",
            primaryLabel: "Bút chì.",
            secondaryLabel: "Tôi có cuốc.",
            progressText: "Lần gặp đầu tiên",
            onPrimary: () => this.CompleteFirstConversation(broughtHoe: false),
            onSecondary: () => this.CompleteFirstConversation(broughtHoe: true)
        );
    }

    private void CompleteFirstConversation(bool broughtHoe)
    {
        Game1.player.modData[ModIdentity.FirstConversationCompletedKey] = "true";

        string preface = broughtHoe
            ? "\"......\"  \"Đừng dùng thứ đó lên bảng.\""
            : "\"...Được.\"";

        this.ShowDailySudokuConversation(preface, forceFresh: true);
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
                lines: new[] { "\"...Hôm nay không có bảng.\"" },
                question: "Sudoku nhìn sang chỗ khác.",
                primaryLabel: "Được.",
                secondaryLabel: "Để sau.",
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

        SudokuDailyDialogue dialogue = SudokuDialogueLibrary.GetForToday(
            solvedCount,
            repeatTalk,
            solvedToday
        );

        List<string> lines = new();
        if (!string.IsNullOrWhiteSpace(preface))
            lines.Add(preface);
        lines.AddRange(dialogue.Lines);

        int totalStages = this.dailySudoku.GetStageCount();
        string progressText = solvedCount switch
        {
            0 => $"Tiến độ Stage: 0/{totalStages} • Sudoku vẫn chưa tin bạn lắm.",
            1 => $"Tiến độ Stage: 1/{totalStages} • Mối liên kết vừa bắt đầu.",
            _ => $"Tiến độ Stage: {solvedCount}/{totalStages}"
        };

        Texture2D? portraits = this.LoadSudokuPortraitTexture();
        Game1.activeClickableMenu = new SudokuConversationMenu(
            portraits,
            dialogue.PortraitIndex,
            lines,
            dialogue.Question,
            primaryLabel: "Chơi",
            secondaryLabel: "Để sau",
            progressText: progressText,
            onPrimary: this.ShowSudokuStageSelect,
            onSecondary: () => { }
        );

        this.Monitor.Log(
            $"Sudoku daily conversation opened: solvedCount={solvedCount}, repeatTalk={repeatTalk}, solvedToday={solvedToday}, portrait={dialogue.PortraitIndex}.",
            LogLevel.Trace
        );
    }
}
