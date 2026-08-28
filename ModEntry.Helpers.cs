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
    private NPC? FindSudoku(bool currentLocationOnly)
    {
        IEnumerable<NPC> candidates = Utility.getAllCharacters()
            .Where(p => ModIdentity.IsSudokuNpcId(p.Name));

        if (currentLocationOnly)
            candidates = candidates.Where(p => p.currentLocation == Game1.currentLocation);

        return candidates
            .OrderBy(p => p.Name == ModIdentity.SudokuNpcId ? 0 : 1)
            .FirstOrDefault();
    }

    private void SyncNpcIdentityFlag()
    {
        if (!Context.IsWorldReady)
            return;

        if (!ModIdentity.HasArrivalBeenSeen(Game1.player))
        {
            Game1.player.modData.Remove(ModIdentity.SudokuNpcEnabledKey);
            return;
        }

        bool newNpcPresent = Utility.getAllCharacters().Any(p => p.Name == ModIdentity.SudokuNpcId);
        bool legacyNpcPresent = Utility.getAllCharacters().Any(p => p.Name == ModIdentity.LegacySudokuNpcId);

        if (legacyNpcPresent && !newNpcPresent)
        {
            Game1.player.modData.Remove(ModIdentity.SudokuNpcEnabledKey);
            this.Monitor.Log(
                "Legacy Sudoku NPC detected in this save. Cursed Signal will keep using that instance instead of spawning a duplicate new-ID Sudoku.",
                LogLevel.Info
            );
            return;
        }

        Game1.player.modData[ModIdentity.SudokuNpcEnabledKey] = "true";
    }

}
