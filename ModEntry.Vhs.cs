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
        this.ResetSequenceState();
        Game1.player.modData[ModIdentity.ArrivalSeenKey] = "true";
        Game1.player.modData[ModIdentity.SudokuNpcEnabledKey] = "true";

        this.EnsureCursedVhsGranted(showDialogue: false);

        Game1.drawObjectDialogue(
            "......^Ngươi...^...có bút chì không?^^Một cuộn băng lạnh ngắt nằm cạnh TV. Trên nhãn là một lưới 9×9."
        );

        this.Monitor.Log(
            "Sudoku's arrival finished. The Cursed Signal save/NPC flags are set and the Cursed VHS has been granted.",
            LogLevel.Info
        );
    }

    private void EnsureCursedVhsGranted(bool showDialogue)
    {
        if (!Context.IsWorldReady)
            return;

        bool alreadyFlagged =
            Game1.player.modData.TryGetValue(ModIdentity.CursedVhsGrantedKey, out string? raw)
            && raw == "true";

        if (alreadyFlagged)
            return;

        bool alreadyInInventory = Game1.player.Items.Any(
            item => item?.QualifiedItemId == ModIdentity.CursedVhsQualifiedItemId
        );

        if (!alreadyInInventory)
        {
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
                "Bạn nhặt được một cuộn VHS không nhãn.^Ai đó đã vẽ một lưới 9×9 lên mặt băng."
            );
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
        this.elapsedTicks = 0;
        this.frameIndex = 0;
    }

}
