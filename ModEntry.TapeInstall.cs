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

        // If installed after 8:00, don't fire instantly; wait until tomorrow morning.
        if (Game1.timeOfDay >= this.Config.DailySignalTime)
            Game1.player.modData[ModIdentity.DailySignalDayKey] = Game1.Date.TotalDays.ToString();
        else
            Game1.player.modData.Remove(ModIdentity.DailySignalDayKey);

        Game1.playSound("smallSelect");
        Game1.drawObjectDialogue(
            "Bạn đẩy cuộn VHS vào TV.^*Cạch.*^Cuộn băng biến mất vào trong.^Nút eject không phản hồi."
        );

        this.Monitor.Log(
            $"Cursed VHS installed into a farmhouse TV. Daily signal armed for {this.Config.DailySignalTime}.",
            LogLevel.Info
        );
    }
}
