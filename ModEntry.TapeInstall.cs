using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace CursedSignal;

internal sealed partial class ModEntry
{
    private void OnTapeButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady || this.sequenceActive || Game1.activeClickableMenu is not null)
            return;

        if (!e.Button.IsActionButton())
            return;

        if (ModIdentity.IsCursedVhsInstalled(Game1.player))
            return;

        Item? heldItem = Game1.player.CurrentItem;
        if (heldItem?.QualifiedItemId != ModIdentity.CursedVhsQualifiedItemId)
            return;

        if (Game1.currentLocation is not FarmHouse farmHouse)
            return;

        TV? nearestTv = farmHouse.furniture
            .OfType<TV>()
            .OrderBy(tv => Vector2.Distance(Game1.player.Tile, tv.TileLocation))
            .FirstOrDefault(tv => Vector2.Distance(Game1.player.Tile, tv.TileLocation) <= 3f);

        if (nearestTv is null)
            return;

        this.Helper.Input.Suppress(e.Button);
        Game1.player.reduceActiveItemByOne();
        Game1.player.modData[ModIdentity.CursedVhsInstalledKey] = "true";

        Game1.playSound("smallSelect");
        Game1.showGlobalMessage("*Cạch.* Cuộn VHS biến mất vào trong TV. Nút eject không phản hồi.");

        // First activation happens immediately when the tape is inserted, regardless of the clock.
        // FinishSequence marks today's signal as completed, so 8:00 won't trigger a second time today.
        this.TryStartArrival(force: true);

        this.Monitor.Log(
            $"Cursed VHS installed. First signal started immediately; future daily signals are armed for {this.Config.DailySignalTime}.",
            LogLevel.Info
        );
    }
}
