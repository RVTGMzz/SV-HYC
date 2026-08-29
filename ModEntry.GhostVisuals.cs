using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace HeyYoureCursed;

internal sealed partial class ModEntry
{
    private void RegisterAlpha22Features(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked += this.OnGhostVisualUpdateTicked;
        helper.Events.Input.ButtonPressed += this.OnAlpha224ControllerFallbackButtonPressed;
        this.RegisterAlpha22TestCommands(helper);
    }

    private void OnGhostVisualUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        this.UpdateSudokuGhostVisuals();
    }

    // The main input router historically returned early while the TV sequence flag was active.
    // If a custom Sudoku menu is already visible during that transient state, give it a fallback
    // controller path so mouse and controller never disagree about whether the menu is usable.
    private void OnAlpha224ControllerFallbackButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady || !this.sequenceActive)
            return;

        bool handled = Game1.activeClickableMenu switch
        {
            SudokuMenu menu => menu.HandleSmapiInput(e.Button),
            SudokuConversationMenu menu => menu.HandleSmapiInput(e.Button),
            SudokuChoiceMenu menu => menu.HandleSmapiInput(e.Button),
            SudokuStageSelectMenu menu => menu.HandleSmapiInput(e.Button),
            _ => false
        };

        if (handled)
            this.Helper.Input.Suppress(e.Button);
    }

    // Match the motion language that already works well for ChaCha in Cardcha:
    // a small horizontal fairy/ghost sway plus a faster vertical bob around a modest lift.
    // These are drawOffset units, not world/tile movement, so interaction and pathfinding stay fixed.
    private const float GhostHoverBaseLift = 5f;
    private const float GhostHoverVerticalAmplitude = 2.5f;
    private const float GhostHoverHorizontalAmplitude = 1.8f;

    /// <summary>
    /// Applies a visual-only floating offset to Sudoku. Her world position, tile, collision,
    /// interaction distance, pathfinding, and save state stay untouched.
    /// </summary>
    private void UpdateSudokuGhostVisuals()
    {
        if (!Context.IsWorldReady)
            return;

        NPC? sudoku = this.FindSudoku(currentLocationOnly: false);
        if (sudoku is null)
            return;

        sudoku.shouldShadowBeOffset = false;

        if (sudoku.IsInvisible)
        {
            sudoku.drawOffset = Vector2.Zero;
            return;
        }

        double seconds = Game1.currentGameTime.TotalGameTime.TotalSeconds;
        float swayX = (float)Math.Sin(seconds * 2.2d + 0.4d) * GhostHoverHorizontalAmplitude;
        float bobY = (float)Math.Sin(seconds * 3.6d) * GhostHoverVerticalAmplitude - GhostHoverBaseLift;

        sudoku.drawOffset = new Vector2(swayX, bobY);
    }
}
