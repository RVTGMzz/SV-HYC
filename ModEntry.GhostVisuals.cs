using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace HeyYoureCursed;

internal sealed partial class ModEntry
{
    private void RegisterAlpha22Features(IModHelper helper)
    {
        I18n = helper.Translation;
        helper.Events.GameLoop.UpdateTicked += this.OnGhostVisualUpdateTicked;
        helper.Events.Input.ButtonPressed += this.OnAlpha224ControllerFallbackButtonPressed;
        this.RegisterAlpha22TestCommands(helper);
        this.RegisterAlpha3Features(helper);
    }

    private void OnGhostVisualUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady || this.IsSudokuSealed())
            return;

        this.UpdateSudokuGhostVisuals();
    }

    private void OnAlpha224ControllerFallbackButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady || !this.sequenceActive)
            return;

        bool handled = Game1.activeClickableMenu switch
        {
            OccultCabinetMenu menu => menu.HandleSmapiInput(e.Button),
            WizardRevealMenu menu => menu.HandleSmapiInput(e.Button),
            SaloonPrologueMenu menu => menu.HandleSmapiInput(e.Button),
            SpiritEvePrankMenu menu => menu.HandleSmapiInput(e.Button),
            SudokuCapstoneMenu menu => menu.HandleSmapiInput(e.Button),
            SudokuMenu menu => menu.HandleSmapiInput(e.Button),
            SudokuConversationMenu menu => menu.HandleSmapiInput(e.Button),
            SudokuChoiceMenu menu => menu.HandleSmapiInput(e.Button),
            SudokuStageSelectMenu menu => menu.HandleSmapiInput(e.Button),
            _ => false
        };

        if (handled)
            this.Helper.Input.Suppress(e.Button);
    }

    private const float GhostHoverBaseLift = 5f;
    private const float GhostHoverVerticalAmplitude = 2.5f;
    private const float GhostHoverHorizontalAmplitude = 1.8f;

    private void UpdateSudokuGhostVisuals()
    {
        if (!Context.IsWorldReady || this.IsSudokuSealed())
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
