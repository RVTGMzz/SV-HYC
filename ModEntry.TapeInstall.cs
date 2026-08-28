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
        {
            this.RemoveAllCursedVhsFromInventory();
            return;
        }

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

        int removedCopies = this.RemoveAllCursedVhsFromInventory();
        Game1.player.modData[ModIdentity.CursedVhsInstalledKey] = "true";

        Game1.playSound("smallSelect");
        Game1.showGlobalMessage("*Cạch.* Cuộn VHS biến mất vào trong TV. Nút eject không phản hồi.");

        this.TryStartArrival(force: true);

        this.Monitor.Log(
            $"Cursed VHS installed ({removedCopies} inventory copy/copies consumed). First signal started immediately; future daily signals are armed for {this.Config.DailySignalTime}.",
            LogLevel.Info
        );
    }
}
