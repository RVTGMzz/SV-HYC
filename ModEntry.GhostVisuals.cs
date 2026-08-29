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

        // Data/Characters already declares Shadow.Visible=false. Keep shadow offset disabled too,
        // so other game code can't accidentally make a shadow follow the floating sprite.
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
