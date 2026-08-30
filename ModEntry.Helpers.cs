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
    private List<NPC> FindAllSudoku()
    {
        return Utility.getAllCharacters()
            .Where(p => ModIdentity.IsSudokuNpcId(p.Name))
            .ToList();
    }

    private static void ConfigureSudokuGhostPhysics(NPC sudoku)
    {
        // Sudoku is a ghost: the farmer can walk through her instead of being body-blocked.
        // Keep normal map collision for Sudoku herself so roommate wandering still respects the farmhouse layout.
        sudoku.farmerPassesThrough = true;
    }

    private NPC? FindSudoku(bool currentLocationOnly)
    {
        IEnumerable<NPC> candidates = this.FindAllSudoku();

        if (currentLocationOnly)
            candidates = candidates.Where(p => p.currentLocation == Game1.currentLocation);

        return candidates
            .OrderBy(p => p.Name == ModIdentity.LegacySudokuNpcId ? 0 : 1)
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

        bool legacyNpcPresent = this.FindAllSudoku().Any(p => p.Name == ModIdentity.LegacySudokuNpcId);

        if (legacyNpcPresent)
        {
            Game1.player.modData.Remove(ModIdentity.SudokuNpcEnabledKey);
            return;
        }

        Game1.player.modData[ModIdentity.SudokuNpcEnabledKey] = "true";
    }

    private NPC? EnsureSudokuCharacterExists()
    {
        if (!Context.IsWorldReady)
            return null;

        NPC? existing = this.NormalizeSudokuInstances();
        if (existing is not null)
        {
            ConfigureSudokuGhostPhysics(existing);
            return existing;
        }

        try
        {
            Game1.player.modData[ModIdentity.SudokuNpcEnabledKey] = "true";
            Game1.AddCharacterIfNecessary(ModIdentity.SudokuNpcId, bypassConditions: true);

            NPC? sudoku = this.NormalizeSudokuInstances()
                ?? Game1.getCharacterFromName(ModIdentity.SudokuNpcId);

            if (sudoku is null)
            {
                this.Monitor.Log(
                    "Sudoku's Data/Characters entry is unlocked, but the game did not create an NPC instance.",
                    LogLevel.Warn
                );
            }
            else
            {
                ConfigureSudokuGhostPhysics(sudoku);
            }

            return sudoku;
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"Couldn't spawn Sudoku immediately. {ex}", LogLevel.Error);
            return null;
        }
    }

    private void HideSudokuUntilDailySignal()
    {
        NPC? sudoku = this.EnsureSudokuCharacterExists();
        if (sudoku is null)
            return;

        sudoku.IsInvisible = true;
        sudoku.ignoreScheduleToday = true;
        sudoku.Halt();

        this.Monitor.Log(
            "Sudoku is hidden until today's Hey! You’re Cursed! materializes her.",
            LogLevel.Trace
        );
    }

    private NPC? PlaceSudokuAfterArrival()
    {
        NPC? sudoku = this.EnsureSudokuCharacterExists();
        if (sudoku is null)
            return null;

        if (Game1.currentLocation is not FarmHouse farmHouse)
            return sudoku;

        Vector2 playerTile = Game1.player.Tile;
        Vector2[] offsets =
        {
            new(1f, 0f), new(-1f, 0f), new(0f, 1f), new(0f, -1f),
            new(1f, 1f), new(-1f, 1f), new(1f, -1f), new(-1f, -1f),
            new(2f, 0f), new(-2f, 0f), new(0f, 2f), new(0f, -2f)
        };

        Vector2? arrivalTile = null;
        foreach (Vector2 offset in offsets)
        {
            Vector2 candidate = playerTile + offset;
            if (farmHouse.CanSpawnCharacterHere(candidate))
            {
                arrivalTile = candidate;
                break;
            }
        }

        Vector2 fallbackTile = new(6f, 5f);
        Vector2 finalTile = arrivalTile
            ?? (farmHouse.CanSpawnCharacterHere(fallbackTile) ? fallbackTile : playerTile);

        Game1.warpCharacter(sudoku, farmHouse, finalTile);
        ConfigureSudokuGhostPhysics(sudoku);
        sudoku.IsInvisible = false;
        sudoku.ignoreScheduleToday = true;
        sudoku.Halt();

        Vector2 delta = playerTile - finalTile;
        int facingDirection;
        if (Math.Abs(delta.X) > Math.Abs(delta.Y))
            facingDirection = delta.X > 0 ? 1 : 3;
        else
            facingDirection = delta.Y > 0 ? 2 : 0;

        sudoku.faceDirection(facingDirection);

        this.Monitor.Log(
            $"Sudoku materialized in {farmHouse.NameOrUniqueName} at tile {finalTile}, beside the player when possible.",
            LogLevel.Info
        );

        return sudoku;
    }

    private NPC? NormalizeSudokuInstances()
    {
        List<NPC> all = this.FindAllSudoku();
        if (all.Count == 0)
            return null;

        NPC canonical = all
            .OrderBy(p => p.Name == ModIdentity.LegacySudokuNpcId ? 0 : 1)
            .First();

        ConfigureSudokuGhostPhysics(canonical);

        int removed = 0;
        foreach (NPC duplicate in all)
        {
            if (ReferenceEquals(duplicate, canonical))
                continue;

            if (duplicate.currentLocation is not null && duplicate.currentLocation.characters.Remove(duplicate))
                removed++;
        }

        if (removed > 0)
        {
            this.Monitor.Log(
                $"Removed {removed} duplicate Sudoku NPC instance(s); keeping {canonical.Name}.",
                LogLevel.Warn
            );
        }

        return canonical;
    }

    private int RemoveAllSudokuInstances()
    {
        int removed = 0;

        foreach (NPC sudoku in this.FindAllSudoku())
        {
            if (sudoku.currentLocation is not null && sudoku.currentLocation.characters.Remove(sudoku))
                removed++;
        }

        return removed;
    }

    private int CountCursedVhsInInventory()
    {
        int count = 0;

        foreach (Item? item in Game1.player.Items)
        {
            if (item?.QualifiedItemId == ModIdentity.CursedVhsQualifiedItemId)
                count += Math.Max(1, item.Stack);
        }

        return count;
    }

    private int RemoveAllCursedVhsFromInventory()
    {
        int removed = 0;

        for (int i = 0; i < Game1.player.Items.Count; i++)
        {
            Item? item = Game1.player.Items[i];
            if (item?.QualifiedItemId != ModIdentity.CursedVhsQualifiedItemId)
                continue;

            removed += Math.Max(1, item.Stack);
            Game1.player.Items[i] = null;
        }

        return removed;
    }

    private void ReconcileCoreState()
    {
        if (!Context.IsWorldReady)
            return;

        bool arrived = ModIdentity.HasArrivalBeenSeen(Game1.player);

        if (!arrived)
        {
            int staleNpcCount = this.RemoveAllSudokuInstances();
            Game1.player.modData.Remove(ModIdentity.SudokuNpcEnabledKey);

            if (staleNpcCount > 0)
            {
                this.Monitor.Log(
                    $"Removed {staleNpcCount} stale Sudoku NPC instance(s) because this save has no completed arrival.",
                    LogLevel.Warn
                );
            }

            return;
        }

        this.NormalizeSudokuInstances();
        this.SyncNpcIdentityFlag();

        NPC? sudoku = this.EnsureSudokuCharacterExists();
        if (sudoku is null)
            return;

        ConfigureSudokuGhostPhysics(sudoku);

        bool waitingForSignal =
            ModIdentity.IsCursedVhsInstalled(Game1.player)
            && !ModIdentity.HasDailySignalRunToday(Game1.player);

        if (waitingForSignal)
            this.HideSudokuUntilDailySignal();
        else
            sudoku.IsInvisible = false;
    }
}
