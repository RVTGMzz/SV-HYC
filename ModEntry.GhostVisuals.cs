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
        this.RegisterAlpha22TestCommands(helper);
    }

    private void OnGhostVisualUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        this.UpdateSudokuGhostVisuals();
    }

    private const float GhostHoverBaseLiftPixels = 11f;
    private const float GhostHoverAmplitudePixels = 3.5f;
    private const double GhostHoverCycleSeconds = 2.8;

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

        // Data/Characters already declares Shadow.Visible=false. Keep shadow offset disabled too,
        // so other game code can't accidentally make the shadow follow the floating sprite.
        sudoku.shouldShadowBeOffset = false;

        if (sudoku.IsInvisible)
        {
            sudoku.drawOffset.Value = Vector2.Zero;
            return;
        }

        double seconds = Game1.currentGameTime.TotalGameTime.TotalSeconds;
        float bob = (float)Math.Sin(seconds * Math.PI * 2d / GhostHoverCycleSeconds)
            * GhostHoverAmplitudePixels;

        sudoku.drawOffset.Value = new Vector2(0f, -GhostHoverBaseLiftPixels + bob);
    }
}
