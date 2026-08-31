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
    private void FinishSequence()
    {
        bool firstArrival = this.sequenceIsFirstArrival;

        this.ResetSequenceState();
        Game1.player.modData[ModIdentity.ArrivalSeenKey] = "true";
        Game1.player.modData[ModIdentity.SudokuNpcEnabledKey] = "true";
        ModIdentity.MarkDailySignalRunToday(Game1.player);

        NPC? sudoku = this.PlaceSudokuAfterArrival();
        this.BeginSudokuRoommateBehaviorAfterArrival();

        if (firstArrival)
            this.ShowFirstConversation();
        else
            Game1.showGlobalMessage(T("story.vhs.signal-repeat"));

        this.Monitor.Log(
            sudoku is null
                ? "Hey! You’re Cursed! sequence finished, but Sudoku could not be materialized immediately."
                : firstArrival
                    ? "First cursed-TV sequence finished. Sudoku materialized beside the player."
                    : "Repeat morning signal finished. Sudoku materialized beside the player.",
            sudoku is null ? LogLevel.Warn : LogLevel.Info
        );
    }

    private void EnsureCursedVhsGranted(bool showDialogue)
    {
        if (!Context.IsWorldReady)
            return;

        if (ModIdentity.IsCursedVhsInstalled(Game1.player))
        {
            this.RemoveAllCursedVhsFromInventory();
            return;
        }

        int inventoryCount = this.CountCursedVhsInInventory();
        if (inventoryCount != 1)
        {
            this.RemoveAllCursedVhsFromInventory();
            try
            {
                Item tape = ItemRegistry.Create(ModIdentity.CursedVhsQualifiedItemId, 1);
                // Don't open Stardew's overflow ItemGrabMenu here. If the backpack is
                // full, that menu keeps the tape outside the inventory and another
                // SaveLoaded/DayStarted pass can create a second copy.
                if (!Game1.player.addItemToInventoryBool(tape))
                {
                    this.Monitor.Log("Couldn't grant the Cursed VHS because the player's inventory is full; it will be retried later.", LogLevel.Warn);
                    if (showDialogue)
                        Game1.showGlobalMessage(T("story.vhs.inventory-full"));
                    return;
                }
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Couldn't grant the Cursed VHS item. {ex}", LogLevel.Error);
                return;
            }
        }

        Game1.player.modData[ModIdentity.CursedVhsGrantedKey] = "true";
        if (showDialogue)
        {
            Game1.drawObjectDialogue(T("story.vhs.received"));
        }
    }

    private void CancelSequence()
    {
        this.ResetSequenceState();
        this.Monitor.Log(
            "Sudoku's TV arrival sequence was cancelled because the player left the farmhouse.",
            LogLevel.Trace
        );
    }

    private void ResetSequenceState()
    {
        this.sequenceActive = false;
        this.sequenceIsFirstArrival = false;
        this.elapsedTicks = 0;
        this.frameIndex = 0;
    }
}
