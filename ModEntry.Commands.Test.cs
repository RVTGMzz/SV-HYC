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
    private void OnGiveVhsCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save before using cursedsignal_givevhs.", LogLevel.Warn);
            return;
        }

        if (ModIdentity.IsCursedVhsInstalled(Game1.player))
        {
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
        ModIdentity.ClearArrivalFlags(Game1.player);
        this.Monitor.Log(
            "Sudoku arrival, VHS installation, and daily signal flags were cleared. Existing NPC/item instances are intentionally left alone for testing.",
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

        string npcLocation = sudoku?.currentLocation?.NameOrUniqueName ?? "none";
        string npcTile = sudoku is null ? "none" : sudoku.Tile.ToString();

        this.Monitor.Log(
            $"Sudoku status: arrivalSeen={seen}, npcEnabled={npcEnabled}, npcPresent={sudoku is not null}, npcId={sudoku?.Name ?? "none"}, npcLocation={npcLocation}, npcTile={npcTile}, vhsGranted={vhsGranted}, vhsInstalled={vhsInstalled}, signalRanToday={signalRanToday}, sequenceActive={this.sequenceActive}, dailyPuzzle={dailyPuzzle?.Id ?? "none"}, dailyClaimed={dailyClaimed}, time={Game1.timeOfDay}, location={Game1.currentLocation?.NameOrUniqueName}.",
            LogLevel.Info
        );
    }
}
