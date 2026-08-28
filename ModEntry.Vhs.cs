using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Objects;
using StardewValley.Locations;

namespace CursedSignal;

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

        if (firstArrival)
        {
            Game1.drawObjectDialogue(
                "......^Ngươi...^...có bút chì không?"
            );
        }
        else
        {
            Game1.showGlobalMessage("TV nhiễu lên rồi tắt phụt. Sudoku đã đứng cạnh bạn.");
        }

        this.Monitor.Log(
            sudoku is null
                ? "Cursed Signal finished, but Sudoku could not be materialized immediately."
                : firstArrival
                    ? "First Cursed Signal finished. Sudoku materialized beside the player."
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
                Game1.player.addItemByMenuIfNecessary(tape);
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
            Game1.drawObjectDialogue(
                "Bạn nhận được một cuộn VHS cũ.^Có lẽ nó sẽ hoạt động nếu bạn dùng trực tiếp lên TV trong nhà."
            );
        }
    }

    private void CancelSequence()
    {
        this.ResetSequenceState();
        this.Monitor.Log(
            "Sudoku's TV signal was cancelled because the player left the farmhouse.",
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
