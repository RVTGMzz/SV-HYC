using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace ChuyenTamLinhKoDuaDuocDau;

internal sealed class ModEntry : Mod
{
    private const string SeenKey = "ronvotri.chuyentamlinhkoduaduocdau/SudokuArrivalSeen";

    private const int FrameWidth = 64;
    private const int FrameHeight = 64;
    private const int FrameCount = 6;
    private const int IntroDelayTicks = 30;

    private ModConfig Config = null!;
    private Texture2D? tvArrivalSheet;

    private bool sequenceActive;
    private int elapsedTicks;
    private int frameIndex;

    public override void Entry(IModHelper helper)
    {
        this.Config = helper.ReadConfig<ModConfig>();

        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.GameLoop.TimeChanged += this.OnTimeChanged;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        helper.Events.Player.Warped += this.OnWarped;
        helper.Events.Display.RenderedWorld += this.OnRenderedWorld;

        helper.ConsoleCommands.Add(
            "sudoku_testarrival",
            "Start Sudoku's haunted-TV arrival immediately while inside the farmhouse.",
            this.OnTestArrivalCommand
        );

        helper.ConsoleCommands.Add(
            "sudoku_resetarrival",
            "Clear the prototype arrival flag so the event can be tested again.",
            this.OnResetArrivalCommand
        );
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        string encodedPath = Path.Combine(
            this.Helper.DirectoryPath,
            "assets",
            "Events",
            "Sudoku_TV.b64"
        );

        string encodedPng = File.ReadAllText(encodedPath).Trim();
        byte[] pngBytes = Convert.FromBase64String(encodedPng);

        using MemoryStream stream = new(pngBytes, writable: false);
        this.tvArrivalSheet?.Dispose();
        this.tvArrivalSheet = Texture2D.FromStream(Game1.graphics.GraphicsDevice, stream);

        this.sequenceActive = false;
        this.elapsedTicks = 0;
        this.frameIndex = 0;

        this.Monitor.Log(
            "Sudoku TV Arrival v0.0.1 loaded. Test trigger: 7:00 AM in the farmhouse.",
            LogLevel.Info
        );
    }

    private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
    {
        if (!this.Config.EnableArrivalTest)
            return;

        if (e.NewTime >= this.Config.TestArrivalTime)
            this.TryStartArrival(force: false);
    }

    private void OnWarped(object? sender, WarpedEventArgs e)
    {
        if (this.sequenceActive && e.OldLocation is FarmHouse && e.NewLocation is not FarmHouse)
        {
            this.CancelSequence();
            return;
        }

        if (!this.Config.EnableArrivalTest || e.NewLocation is not FarmHouse)
            return;

        if (Game1.timeOfDay >= this.Config.TestArrivalTime)
            this.TryStartArrival(force: false);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!this.sequenceActive || !Context.IsWorldReady)
            return;

        if (Game1.currentLocation is not FarmHouse)
        {
            this.CancelSequence();
            return;
        }

        this.elapsedTicks++;

        if (this.elapsedTicks == 1)
            Game1.playSound("ghost");
        else if (this.elapsedTicks == IntroDelayTicks)
            Game1.playSound("thunder");

        if (this.elapsedTicks < IntroDelayTicks)
            return;

        int safeFrameDuration = Math.Max(1, this.Config.FrameDurationTicks);
        int rawFrame = (this.elapsedTicks - IntroDelayTicks) / safeFrameDuration;

        if (rawFrame >= FrameCount)
        {
            this.FinishSequence();
            return;
        }

        this.frameIndex = Math.Clamp(rawFrame, 0, FrameCount - 1);
    }

    private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
    {
        if (!this.sequenceActive || this.tvArrivalSheet is null || this.elapsedTicks < IntroDelayTicks)
            return;

        int sourceColumn = this.frameIndex % 3;
        int sourceRow = this.frameIndex / 3;

        Rectangle source = new(
            sourceColumn * FrameWidth,
            sourceRow * FrameHeight,
            FrameWidth,
            FrameHeight
        );

        float scale = Math.Max(0.5f, this.Config.EventScale);
        int drawWidth = (int)(FrameWidth * scale);
        int drawHeight = (int)(FrameHeight * scale);

        Rectangle destination = new(
            (Game1.viewport.Width - drawWidth) / 2,
            (Game1.viewport.Height - drawHeight) / 2,
            drawWidth,
            drawHeight
        );

        e.SpriteBatch.Draw(
            this.tvArrivalSheet,
            destination,
            source,
            Color.White
        );
    }

    private void TryStartArrival(bool force)
    {
        if (!Context.IsWorldReady || this.sequenceActive)
            return;

        if (Game1.currentLocation is not FarmHouse)
            return;

        if (!force)
        {
            if (Game1.player.modData.ContainsKey(SeenKey))
                return;

            if (Game1.timeOfDay < this.Config.TestArrivalTime)
                return;
        }

        this.sequenceActive = true;
        this.elapsedTicks = 0;
        this.frameIndex = 0;

        this.Monitor.Log("Sudoku's TV arrival sequence started.", LogLevel.Info);
    }

    private void FinishSequence()
    {
        this.sequenceActive = false;
        this.elapsedTicks = 0;
        this.frameIndex = 0;

        Game1.player.modData[SeenKey] = "true";

        Game1.drawObjectDialogue(
            "......\n\nNgươi...\n\n...có bút chì không?"
        );

        this.Monitor.Log(
            "Sudoku's TV arrival sequence finished and was marked as seen for this save.",
            LogLevel.Info
        );
    }

    private void CancelSequence()
    {
        this.sequenceActive = false;
        this.elapsedTicks = 0;
        this.frameIndex = 0;

        this.Monitor.Log(
            "Sudoku's TV arrival sequence was cancelled because the player left the farmhouse.",
            LogLevel.Trace
        );
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
        Game1.player.modData.Remove(SeenKey);

        this.Monitor.Log(
            "Sudoku arrival flag cleared. Stay in or re-enter the farmhouse at/after 7:00 AM to test again.",
            LogLevel.Info
        );
    }
}
