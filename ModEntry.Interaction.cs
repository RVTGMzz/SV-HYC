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
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady || this.sequenceActive)
            return;

        if (Game1.activeClickableMenu is SudokuMenu sudokuMenu)
        {
            if (sudokuMenu.HandleSmapiInput(e.Button))
                this.Helper.Input.Suppress(e.Button);

            return;
        }

        if (Game1.activeClickableMenu is not null)
            return;

        bool isActionButton = e.Button.IsActionButton();
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

        if (!ModIdentity.HasFirstConversationCompleted(Game1.player))
        {
            this.Helper.Input.Suppress(e.Button);
            this.ShowFirstConversation();
            return;
        }

        if (!this.Config.EnableDailySudoku || this.dailySudoku is null)
        {
            if (isMouseClick)
            {
                this.Helper.Input.Suppress(e.Button);
                Game1.drawObjectDialogue("Sudoku nhìn bạn.^\"...Hôm nay không có bảng.\"");
            }

            return;
        }

        if (this.dailySudoku.IsRewardClaimedToday())
        {
            if (isMouseClick)
            {
                this.Helper.Input.Suppress(e.Button);
                Game1.drawObjectDialogue("Sudoku nhìn bạn một lúc.^\"...Mai 8 giờ.\"");
            }

            return;
        }

        // After the one-time introduction, talking to Sudoku is the board action itself.
        // This bypasses the fragile DialogueBox -> custom-menu transition entirely.
        this.Helper.Input.Suppress(e.Button);
        this.OpenDailySudoku(force: false);
    }

    private bool IsSudokuInteractionTarget(NPC sudoku, ButtonPressedEventArgs e)
    {
        if (e.Button == SButton.MouseLeft)
        {
            Vector2 clickedTile = e.Cursor.GrabTile;
            return Vector2.Distance(clickedTile, sudoku.Tile) <= 0.70f;
        }

        Vector2 facingOffset = Game1.player.FacingDirection switch
        {
            0 => new Vector2(0f, -1f),
            1 => new Vector2(1f, 0f),
            2 => new Vector2(0f, 1f),
            3 => new Vector2(-1f, 0f),
            _ => Vector2.Zero
        };

        Vector2 targetTile = Game1.player.Tile + facingOffset;
        return Vector2.Distance(targetTile, sudoku.Tile) <= 0.70f;
    }

    private static bool AnswerMatches(string? answer, string key)
    {
        if (string.IsNullOrWhiteSpace(answer))
            return false;

        string value = answer.Trim();
        return value.Equals(key, StringComparison.OrdinalIgnoreCase)
            || value.EndsWith("_" + key, StringComparison.OrdinalIgnoreCase)
            || value.EndsWith("." + key, StringComparison.OrdinalIgnoreCase)
            || value.EndsWith("/" + key, StringComparison.OrdinalIgnoreCase);
    }

    private void QueueDailySudokuOpen()
    {
        this.pendingSudokuMenuOpen = true;
        this.pendingSudokuMenuDelayTicks = 12;
        this.pendingSudokuMenuWaitTicks = 0;
        this.Monitor.Log("Daily Sudoku menu queued; delayed dialogue handoff armed.", LogLevel.Info);
    }

    private void ShowFirstConversation()
    {
        if (!Context.IsWorldReady || ModIdentity.HasFirstConversationCompleted(Game1.player))
            return;

        Response[] responses =
        {
            new("yes", "Ừ. Rõ lắm."),
            new("tv", "Không. Tôi đang nói chuyện với cái TV.")
        };

        NPC? sudoku = this.FindSudoku(currentLocationOnly: true);
        Game1.currentLocation.createQuestionDialogue(
            "Sudoku nhìn bạn không chớp mắt.^\"...Ngươi thấy ta?\"",
            responses,
            new GameLocation.afterQuestionBehavior(this.OnFirstConversationSightAnswered),
            sudoku
        );
    }

    private void OnFirstConversationSightAnswered(Farmer who, string answer)
    {
        string prompt = AnswerMatches(answer, "tv")
            ? "\"Tốt.\"^Sudoku liếc sang chiếc TV.^\"...Nó nói chuyện dễ hiểu hơn ngươi.\"^\"...Có bút chì không?\""
            : "Sudoku im lặng vài giây.^\"...Phiền thật.\"^\"...Ngươi có bút chì không?\"";

        Response[] responses =
        {
            new("pencil", "Có."),
            new("hoe", "Không, nhưng tôi có cuốc.")
        };

        NPC? sudoku = this.FindSudoku(currentLocationOnly: true);
        Game1.currentLocation.createQuestionDialogue(
            prompt,
            responses,
            new GameLocation.afterQuestionBehavior(this.OnFirstConversationPencilAnswered),
            sudoku
        );
    }

    private void OnFirstConversationPencilAnswered(Farmer who, string answer)
    {
        Game1.player.modData[ModIdentity.FirstConversationCompletedKey] = "true";

        if (AnswerMatches(answer, "hoe"))
            Game1.showGlobalMessage("Sudoku nhìn cây cuốc của bạn. '...Đừng dùng thứ đó lên bảng.'");
        else
            Game1.showGlobalMessage("Sudoku khẽ gật đầu rồi đặt một bảng 9×9 trước mặt bạn.");

        // End the intro here and queue the first board immediately. This removes the
        // redundant third question-dialogue layer that was causing the handoff failure.
        this.QueueDailySudokuOpen();
    }
}
