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

        bool isActionButton = e.Button.IsActionButton() || e.Button == SButton.ControllerA;
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

        // After the one-time introduction, talking to Sudoku always opens today's board,
        // even if today's reward was already claimed. The menu itself prevents duplicate
        // rewards, so there is no reason for NPC interaction to silently do nothing.

        // After the one-time introduction, talking to Sudoku is the board action itself.
        // Open the custom menu directly instead of routing through another DialogueBox;
        // the direct console command already proves SudokuMenu is healthy, so this removes
        // the fragile dialogue-to-menu handoff from the normal daily path.
        this.Helper.Input.Suppress(e.Button);
        this.OpenDailySudoku(force: false);
    }

    private bool IsSudokuInteractionTarget(NPC sudoku, ButtonPressedEventArgs e)
    {
        if (e.Button == SButton.MouseLeft)
        {
            // Mouse interaction stays tight so clicking the TV next to Sudoku still belongs
            // to the TV, not the ghost.
            Vector2 clickedTile = e.Cursor.GrabTile;
            return Vector2.Distance(clickedTile, sudoku.Tile) <= 0.85f;
        }

        // NPC tile positions can be fractional, so comparing only against the single tile
        // directly in front of the farmer was too strict on some saves/controllers. Instead
        // use a short facing cone: Sudoku must be genuinely in front of the player, not merely
        // standing nearby. This keeps the TV beside her usable while making NPC talk reliable.
        Vector2 delta = sudoku.Tile - Game1.player.Tile;
        float forward;
        float sideways;

        switch (Game1.player.FacingDirection)
        {
            case 0: // up
                forward = -delta.Y;
                sideways = Math.Abs(delta.X);
                break;
            case 1: // right
                forward = delta.X;
                sideways = Math.Abs(delta.Y);
                break;
            case 2: // down
                forward = delta.Y;
                sideways = Math.Abs(delta.X);
                break;
            case 3: // left
                forward = -delta.X;
                sideways = Math.Abs(delta.Y);
                break;
            default:
                return false;
        }

        return forward >= 0.10f && forward <= 1.75f && sideways <= 0.80f;
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

        // This callback is already proven to run because it advances the intro. Queue the
        // board immediately here and remove the extra third question-dialogue layer.
        this.QueueDailySudokuOpen();
    }



}
