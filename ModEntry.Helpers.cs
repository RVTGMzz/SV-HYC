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

    private NPC? EnsureSudokuCharacterExists()
    {
        if (!Context.IsWorldReady)
            return null;

        NPC? existing = this.FindSudoku(currentLocationOnly: false);
        if (existing is not null)
            return existing;

        try
        {
            // Stardew 1.6 can materialize a Data/Characters entry immediately.
            // bypassConditions is safe here because the arrival flag has already been set by Cursed Signal.
            Game1.AddCharacterIfNecessary(ModIdentity.SudokuNpcId, bypassConditions: true);

            NPC? sudoku = Game1.getCharacterFromName(ModIdentity.SudokuNpcId);
            if (sudoku is null)
            {
                this.Monitor.Log(
                    "Sudoku's Data/Characters entry is unlocked, but Game1.AddCharacterIfNecessary did not create an NPC instance.",
                    LogLevel.Warn
                );
            }

            return sudoku;
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"Couldn't spawn Sudoku immediately. {ex}", LogLevel.Error);
            return null;
        }
    }

    private NPC? PlaceSudokuAfterArrival()
    {
        NPC? sudoku = this.EnsureSudokuCharacterExists();
        if (sudoku is null)
            return null;

        if (Game1.currentLocation is not FarmHouse farmHouse)
            return sudoku;

        // Keep the first appearance predictable and visible. Her normal schedule can take over tomorrow.
        Vector2 arrivalTile = new(6f, 5f);
        Game1.warpCharacter(sudoku, farmHouse, arrivalTile);
        sudoku.ignoreScheduleToday = true;
        sudoku.Halt();
        sudoku.faceDirection(2);

        this.Monitor.Log(
            $"Sudoku placed in {farmHouse.NameOrUniqueName} at tile {arrivalTile} after the TV arrival.",
            LogLevel.Info
        );

        return sudoku;
    }
}
