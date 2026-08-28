using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.Locations;

namespace ChuyenTamLinhKoDuaDuocDau;

internal sealed class ModEntry : Mod
{
    private const string NpcId = "ronvotri.chuyentamlinhkoduaduocdau_Sudoku";
    private const string SeenKey = "ronvotri.chuyentamlinhkoduaduocdau/SudokuArrivalSeen";

    private const int FrameWidth = 64;
    private const int FrameHeight = 64;
    private const int FrameCount = 6;

    private ModConfig Config = null!;
    private Texture2D? tvArrivalSheet;

    private bool sequenceActive;
    private int elapsedTicks;
    private int frameIndex;

    public override void Entry(IModHelper helper)
    {
        this.Config = helper.ReadConfig<ModConfig>();

        helper.Events.Content.AssetRequested += this.OnAssetRequested;
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
            "Clear Sudoku's arrival flag so the sequence can be tested again.",
            this.OnResetArrivalCommand
        );

        helper.ConsoleCommands.Add(
            "sudoku_unlocknpc",
            "Set Sudoku's arrival flag. The real NPC should be added on the next save load/day rollover.",
            this.OnUnlockNpcCommand
        );

        helper.ConsoleCommands.Add(
            "sudoku_status",
            "Print Sudoku prototype state for the current save.",
            this.OnStatusCommand
        );
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo($"Characters/{NpcId}"))
        {
            e.LoadFromModFile<Texture2D>(
                "assets/Characters/Sudoku.png",
                AssetLoadPriority.Exclusive
            );
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo($"Portraits/{NpcId}"))
        {
            e.LoadFromModFile<Texture2D>(
                "assets/Portraits/Sudoku.png",
                AssetLoadPriority.Exclusive
            );
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo($"Characters/Dialogue/{NpcId}"))
        {
            e.LoadFromModFile<Dictionary<string, string>>(
                "assets/Dialogue/Sudoku.json",
                AssetLoadPriority.Exclusive
            );
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo($"Characters/schedules/{NpcId}"))
        {
            e.LoadFromModFile<Dictionary<string, string>>(
                "assets/Schedules/Sudoku.json",
                AssetLoadPriority.Exclusive
            );
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo("Data/Characters"))
        {
            e.Edit(asset =>
            {
                Dictionary<string, CharacterData>? custom =
                    this.Helper.Data.ReadJsonFile<Dictionary<string, CharacterData>>(
                        "assets/Data/Sudoku.character.json"
                    );

                if (custom is null || !custom.TryGetValue(NpcId, out CharacterData? sudoku))
                {
                    this.Monitor.Log(
                        "Couldn't read Sudoku.character.json; Sudoku NPC data was not injected.",
                        LogLevel.Error
                    );
                    return;
                }

                asset.AsDictionary<string, CharacterData>().Data[NpcId] = sudoku;
            });
        }
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        this.tvArrivalSheet = this.Helper.ModContent.Load<Texture2D>(
            "assets/Events/Sudoku_TV.png"
        );

        this.ResetSequenceState();

        this.Monitor.Log(
            "Sudoku prototype v0.0.2 loaded. Arrival test remains available at 7:00 AM in the farmhouse.",
            LogLevel.Info
        );

        if (Game1.player.modData.TryGetValue(SeenKey, out string? raw) && raw == "true")
        {
            this.Monitor.Log(
                "Sudoku has already arrived in this save. Her custom NPC data is unlocked.",
                LogLevel.Info
            );
        }
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

        int staticEnd = Math.Max(1, this.Config.StaticTicks);
        int wellEnd = staticEnd + Math.Max(1, this.Config.WellTicks);
        int glitchEnd = wellEnd + Math.Max(1, this.Config.GlitchTicks);

        if (this.elapsedTicks == staticEnd)
            Game1.playSound("thunder");

        if (this.elapsedTicks < glitchEnd)
            return;

        int safeFrameDuration = Math.Max(1, this.Config.FrameDurationTicks);
        int rawFrame = (this.elapsedTicks - glitchEnd) / safeFrameDuration;

        if (rawFrame >= FrameCount)
        {
            this.FinishSequence();
            return;
        }

        this.frameIndex = Math.Clamp(rawFrame, 0, FrameCount - 1);
    }

    private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
    {
        if (!this.sequenceActive || this.tvArrivalSheet is null)
            return;

        int staticEnd = Math.Max(1, this.Config.StaticTicks);
        int wellEnd = staticEnd + Math.Max(1, this.Config.WellTicks);
        int glitchEnd = wellEnd + Math.Max(1, this.Config.GlitchTicks);

        this.DrawDarkBackdrop(e.SpriteBatch);

        if (this.elapsedTicks < staticEnd)
        {
            this.DrawStatic(e.SpriteBatch, strength: 1f);
            return;
        }

        if (this.elapsedTicks < wellEnd)
        {
            this.DrawWellGlimpse(e.SpriteBatch);

            if ((this.elapsedTicks / 5) % 2 == 0)
                this.DrawStatic(e.SpriteBatch, strength: 0.22f);

            return;
        }

        if (this.elapsedTicks < glitchEnd)
        {
            this.DrawWellGlimpse(e.SpriteBatch);
            this.DrawGlitch(e.SpriteBatch);
            return;
        }

        this.DrawEmergenceFrame(e.SpriteBatch);
    }

    private void DrawDarkBackdrop(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            Game1.staminaRect,
            new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height),
            Color.Black * 0.68f
        );
    }

    private void DrawStatic(SpriteBatch spriteBatch, float strength)
    {
        Random random = new(this.elapsedTicks * 7919 + 17);
        int width = Game1.viewport.Width;
        int height = Game1.viewport.Height;

        for (int i = 0; i < 90; i++)
        {
            int y = random.Next(0, Math.Max(1, height));
            int x = random.Next(0, Math.Max(1, width));
            int lineWidth = random.Next(8, Math.Max(9, width / 5));
            int lineHeight = random.Next(1, 5);

            Color tint = random.NextDouble() > 0.5
                ? new Color(170, 205, 225)
                : new Color(80, 105, 130);

            spriteBatch.Draw(
                Game1.staminaRect,
                new Rectangle(x, y, lineWidth, lineHeight),
                tint * (0.16f * strength)
            );
        }
    }

    private void DrawWellGlimpse(SpriteBatch spriteBatch)
    {
        int cx = Game1.viewport.Width / 2;
        int cy = Game1.viewport.Height / 2;

        Rectangle screen = new(cx - 150, cy - 110, 300, 220);
        spriteBatch.Draw(Game1.staminaRect, screen, new Color(25, 37, 48) * 0.96f);

        Rectangle inner = new(cx - 132, cy - 92, 264, 184);
        spriteBatch.Draw(Game1.staminaRect, inner, new Color(90, 125, 145) * 0.35f);

        for (int i = 0; i < 11; i++)
        {
            int width = 132 - (i * 7);
            int y = cy + 28 + (i * 3);

            spriteBatch.Draw(
                Game1.staminaRect,
                new Rectangle(cx - width / 2, y, width, 4),
                new Color(105, 110, 118) * (0.85f - i * 0.035f)
            );
        }

        spriteBatch.Draw(
            Game1.staminaRect,
            new Rectangle(cx - 45, cy + 39, 90, 23),
            Color.Black * 0.92f
        );

        spriteBatch.Draw(
            Game1.staminaRect,
            new Rectangle(cx - 8, cy - 50, 16, 76),
            new Color(7, 10, 14) * 0.92f
        );
        spriteBatch.Draw(
            Game1.staminaRect,
            new Rectangle(cx - 24, cy - 58, 48, 34),
            new Color(7, 10, 14) * 0.92f
        );
    }

    private void DrawGlitch(SpriteBatch spriteBatch)
    {
        Random random = new(this.elapsedTicks * 3571 + 91);

        for (int i = 0; i < 18; i++)
        {
            int y = random.Next(0, Math.Max(1, Game1.viewport.Height));
            int h = random.Next(4, 22);

            Color tint = i % 3 switch
            {
                0 => new Color(170, 210, 230),
                1 => new Color(70, 95, 120),
                _ => Color.White
            };

            spriteBatch.Draw(
                Game1.staminaRect,
                new Rectangle(0, y, Game1.viewport.Width, h),
                tint * 0.12f
            );
        }

        this.DrawStatic(spriteBatch, strength: 0.8f);
    }

    private void DrawEmergenceFrame(SpriteBatch spriteBatch)
    {
        if (this.tvArrivalSheet is null)
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

        spriteBatch.Draw(
            this.tvArrivalSheet,
            destination,
            source,
            Color.White
        );

        if ((this.elapsedTicks / 4) % 3 == 0)
            this.DrawStatic(spriteBatch, strength: 0.15f);
    }

    private void TryStartArrival(bool force)
    {
        if (!Context.IsWorldReady || this.sequenceActive)
            return;

        if (Game1.currentLocation is not FarmHouse)
            return;

        if (!force && Game1.activeClickableMenu is not null)
            return;

        if (!force)
        {
            if (Game1.player.modData.TryGetValue(SeenKey, out string? raw) && raw == "true")
                return;

            if (Game1.timeOfDay < this.Config.TestArrivalTime)
                return;
        }

        this.sequenceActive = true;
        this.elapsedTicks = 0;
        this.frameIndex = 0;

        this.Monitor.Log("Sudoku's haunted-TV arrival sequence started.", LogLevel.Info);
    }

    private void FinishSequence()
    {
        this.ResetSequenceState();

        Game1.player.modData[SeenKey] = "true";

        Game1.drawObjectDialogue(
            "......\n\nNgươi...\n\n...có bút chì không?"
        );

        this.Monitor.Log(
            "Sudoku's arrival finished. The save flag is now set; the custom NPC should be eligible to spawn on the next save load/day rollover.",
            LogLevel.Info
        );
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
            "Sudoku arrival flag cleared. If Sudoku already spawned in this save, this command intentionally doesn't delete her NPC instance.",
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

        Game1.player.modData[SeenKey] = "true";

        this.Monitor.Log(
            "Sudoku unlock flag set. Save/reload or sleep to the next day to let Data/Characters spawn-if-missing logic add her.",
            LogLevel.Info
        );
    }

    private void OnStatusCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("No save is loaded.", LogLevel.Info);
            return;
        }

        bool seen =
            Game1.player.modData.TryGetValue(SeenKey, out string? raw)
            && raw == "true";

        NPC? sudoku = Utility.getAllCharacters()
            .FirstOrDefault(p => p.Name == NpcId);

        this.Monitor.Log(
            $"Sudoku status: arrivalSeen={seen}, npcPresent={sudoku is not null}, sequenceActive={this.sequenceActive}, time={Game1.timeOfDay}, location={Game1.currentLocation?.NameOrUniqueName}.",
            LogLevel.Info
        );
    }
}
