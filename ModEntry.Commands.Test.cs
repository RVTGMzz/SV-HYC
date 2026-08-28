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
    private void OnGiveVhsCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save before using heyyourecursed_givevhs.", LogLevel.Warn);
            return;
        }

        if (ModIdentity.IsCursedVhsInstalled(Game1.player))
        {
            this.RemoveAllCursedVhsFromInventory();
            this.Monitor.Log("The Cursed VHS is already installed in the TV for this save.", LogLevel.Info);
            return;
        }

        Game1.player.modData.Remove(ModIdentity.CursedVhsGrantedKey);
        this.EnsureCursedVhsGranted(showDialogue: true);
    }

    private void OnTestArrivalCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save before using sudoku_testarrival.", LogLevel.Warn);
            return;
        }

        if (Game1.currentLocation is not FarmHouse)
        {
            this.Monitor.Log("Enter the farmhouse before using sudoku_testarrival.", LogLevel.Warn);
            return;
        }

        this.TryStartArrival(force: true);
    }

    private void OnResetArrivalCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save before using sudoku_resetarrival.", LogLevel.Warn);
            return;
        }

        this.CancelSequence();

        int removedNpcCount = this.RemoveAllSudokuInstances();
        int removedTapeCount = this.RemoveAllCursedVhsFromInventory();

        ModIdentity.ClearArrivalFlags(Game1.player);
        this.dailySudoku?.ResetTodayForTesting();
        this.EnsureCursedVhsGranted(showDialogue: false);

        this.Monitor.Log(
            $"Core reset complete. Removed NPCs={removedNpcCount}, removed VHS copies={removedTapeCount}. One fresh Cursed VHS was returned to the player.",
            LogLevel.Info
        );
    }

    private void OnUnlockNpcCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save before using sudoku_unlocknpc.", LogLevel.Warn);
            return;
        }

        Game1.player.modData[ModIdentity.ArrivalSeenKey] = "true";
        Game1.player.modData[ModIdentity.SudokuNpcEnabledKey] = "true";
        NPC? sudoku = this.EnsureSudokuCharacterExists();

        this.Monitor.Log(
            sudoku is null
                ? "Sudoku unlock flags are set, but the NPC instance could not be created immediately."
                : $"Sudoku unlocked and present as {sudoku.Name} in {sudoku.currentLocation?.NameOrUniqueName ?? "unknown"}.",
            sudoku is null ? LogLevel.Warn : LogLevel.Info
        );
    }

    private void OnStatusCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("No save is loaded.", LogLevel.Info);
            return;
        }

        bool seen = ModIdentity.HasArrivalBeenSeen(Game1.player);
        List<NPC> allSudoku = this.FindAllSudoku();
        NPC? sudoku = this.FindSudoku(currentLocationOnly: false);
        bool dailyClaimed = this.dailySudoku?.IsRewardClaimedToday() ?? false;
        SudokuPuzzle? dailyPuzzle = this.dailySudoku?.EnsureToday();
        bool npcEnabled =
            Game1.player.modData.TryGetValue(ModIdentity.SudokuNpcEnabledKey, out string? enabled)
            && enabled == "true";
        bool vhsGranted =
            Game1.player.modData.TryGetValue(ModIdentity.CursedVhsGrantedKey, out string? vhs)
            && vhs == "true";
        bool vhsInstalled = ModIdentity.IsCursedVhsInstalled(Game1.player);
        bool signalRanToday = ModIdentity.HasDailySignalRunToday(Game1.player);
        int vhsInventoryCount = this.CountCursedVhsInInventory();

        string npcLocation = sudoku?.currentLocation?.NameOrUniqueName ?? "none";
        string npcTile = sudoku is null ? "none" : sudoku.Tile.ToString();
        string npcInvisible = sudoku is null ? "n/a" : sudoku.IsInvisible.ToString();

        this.Monitor.Log(
            $"Hey! You’re Cursed! core status: arrivalSeen={seen}, npcEnabled={npcEnabled}, npcCount={allSudoku.Count}, npcId={sudoku?.Name ?? "none"}, npcLocation={npcLocation}, npcTile={npcTile}, npcInvisible={npcInvisible}, vhsGranted={vhsGranted}, vhsInstalled={vhsInstalled}, vhsInventoryCount={vhsInventoryCount}, signalRanToday={signalRanToday}, sequenceActive={this.sequenceActive}, firstSequence={this.sequenceIsFirstArrival}, dailyPuzzle={dailyPuzzle?.Id ?? "none"}, dailyClaimed={dailyClaimed}, pendingSudokuMenu={this.pendingSudokuMenuOpen}, activeMenu={Game1.activeClickableMenu?.GetType().Name ?? "none"}, time={Game1.timeOfDay}, location={Game1.currentLocation?.NameOrUniqueName}.",
            LogLevel.Info
        );
    }
}
