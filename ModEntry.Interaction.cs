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

        // Route controller input explicitly through SMAPI while the custom Sudoku menu is open.
        // This avoids relying only on Stardew's receiveGamePadButton forwarding, which can be
        // inconsistent depending on gamepad mode/controller setup.
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

        // A completed signal should never leave an on-screen Sudoku stuck as non-interactive.
        if (ModIdentity.HasDailySignalRunToday(Game1.player))
            sudoku.IsInvisible = false;

        if (sudoku.IsInvisible)
            return;

        float distance = Vector2.Distance(Game1.player.Tile, sudoku.Tile);
        if (distance > 3.25f)
            return;

        // Do not hijack every action button just because Sudoku happens to be nearby.
        // The player must actually target/click Sudoku. This keeps TV/furniture interaction
        // working normally when she is standing beside the television.
        if (!this.IsSudokuInteractionTarget(sudoku, e))
            return;

        // Make the interaction feel responsive even while the stabilization build keeps
        // Sudoku in a simple idle state instead of giving her a full house-wandering AI yet.
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

        // Once today's reward has been claimed, preserve Stardew's normal right-click/action
        // dialogue. Left-click gets a small fallback line so both interaction styles work.
        if (this.dailySudoku.IsRewardClaimedToday())
        {
            if (isMouseClick)
            {
                this.Helper.Input.Suppress(e.Button);
                Game1.drawObjectDialogue("Sudoku nhìn bạn một lúc.^\"...Mai 8 giờ.\"");
            }

            return;
        }

        this.Helper.Input.Suppress(e.Button);
        this.ShowDailySudokuPrompt();
    }


    private bool IsSudokuInteractionTarget(NPC sudoku, ButtonPressedEventArgs e)
    {
        if (e.Button == SButton.MouseLeft)
        {
            // Mouse clicks only belong to Sudoku when the clicked tile is actually her tile.
            // A loose radius here caused clicks on the TV next to her to open her dialogue.
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

        // Action-button interaction must target Sudoku's own tile. Do not fall back to
        // “Sudoku is merely adjacent to the player”, because that steals actions meant
        // for TVs, furniture, chests, etc.
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

        string prompt = AnswerMatches(answer, "hoe")
            ? "Sudoku nhìn xuống cây cuốc của bạn.^\"......\"^\"Đừng dùng thứ đó lên bảng.\"^Cô ấy rút ra một tờ giấy đầy ô vuông.^\"...Muốn thử luôn không?\""
            : "Sudoku khẽ gật đầu.^\"...Được.\"^Cô ấy rút ra một tờ giấy đầy ô vuông.^\"...Thử một bảng?\"";

        Response[] responses =
        {
            new("board", "Đưa đây."),
            new("later", "Để mai.")
        };

        NPC? sudoku = this.FindSudoku(currentLocationOnly: true);
        Game1.currentLocation.createQuestionDialogue(
            prompt,
            responses,
            new GameLocation.afterQuestionBehavior(this.OnFirstConversationBoardAnswered),
            sudoku
        );
    }


    private void OnFirstConversationBoardAnswered(Farmer who, string answer)
    {
        this.Monitor.Log($"First-board dialogue answered with key '{answer}'.", LogLevel.Info);

        if (AnswerMatches(answer, "board"))
        {
            this.QueueDailySudokuOpen();
            return;
        }

        Game1.drawObjectDialogue("Sudoku gấp tờ giấy lại.^\"...Ngày mai. 8 giờ.\"");
    }


    private void ShowDailySudokuPrompt()
    {
        Response[] responses =
        {
            new("solve", "Đưa đây."),
            new("later", "Để sau.")
        };

        NPC? sudoku = this.FindSudoku(currentLocationOnly: true);
        Game1.currentLocation.createQuestionDialogue(
            "Sudoku đưa cho bạn một tờ giấy.^\"...Bảng hôm nay.\"",
            responses,
            new GameLocation.afterQuestionBehavior(this.OnDailySudokuPromptAnswered),
            sudoku
        );
    }


    private void OnDailySudokuPromptAnswered(Farmer who, string answer)
    {
        this.Monitor.Log($"Daily-board dialogue answered with key '{answer}'.", LogLevel.Info);

        if (AnswerMatches(answer, "solve"))
        {
            this.QueueDailySudokuOpen();
            return;
        }

        Game1.drawObjectDialogue("Sudoku nhìn bạn vài giây.^\"...Đừng điền bừa khi quay lại.\"");
    }
}
