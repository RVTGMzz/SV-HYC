using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Objects;
using StardewValley.Locations;

namespace CursedSignal;

internal sealed class ModEntry : Mod
{
    private const int FrameWidth = 64;
    private const int FrameHeight = 64;
    private const int FrameCount = 6;

    private ModConfig Config = null!;
    private Texture2D? tvArrivalSheet;
    private DailySudokuService? dailySudoku;

    private bool sequenceActive;
    private int elapsedTicks;
    private int frameIndex;

    public override void Entry(IModHelper helper)
    {
        this.Config = helper.ReadConfig<ModConfig>();
        this.dailySudoku = new DailySudokuService(helper, this.Monitor, this.Config);

        helper.Events.Content.AssetRequested += this.OnAssetRequested;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.GameLoop.TimeChanged += this.OnTimeChanged;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        helper.Events.Player.Warped += this.OnWarped;
        helper.Events.Display.RenderedWorld += this.OnRenderedWorld;
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;

        helper.ConsoleCommands.Add("sudoku_testarrival", "Start Sudoku's haunted-TV arrival immediately while inside the farmhouse.", this.OnTestArrivalCommand);
        helper.ConsoleCommands.Add("sudoku_resetarrival", "Clear Sudoku's arrival flag so the sequence can be tested again.", this.OnResetArrivalCommand);
        helper.ConsoleCommands.Add("sudoku_unlocknpc", "Set Sudoku's arrival flag. The real NPC should be added on the next save load/day rollover.", this.OnUnlockNpcCommand);
        helper.ConsoleCommands.Add("sudoku_status", "Print Sudoku prototype state for the current save.", this.OnStatusCommand);
        helper.ConsoleCommands.Add("sudoku_open", "Open today's Sudoku board immediately for testing.", this.OnOpenDailyCommand);
        helper.ConsoleCommands.Add("sudoku_resetdaily", "Reset today's Sudoku board and reward flag for testing.", this.OnResetDailyCommand);
        helper.ConsoleCommands.Add("cursedsignal_givevhs", "Give the Cursed VHS story item to the current player for testing.", this.OnGiveVhsCommand);
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        bool IsNpcAsset(string prefix)
        {
            return e.NameWithoutLocale.IsEquivalentTo($"{prefix}/{ModIdentity.SudokuNpcId}")
                || e.NameWithoutLocale.IsEquivalentTo($"{prefix}/{ModIdentity.LegacySudokuNpcId}");
        }

        if (e.NameWithoutLocale.IsEquivalentTo(ModIdentity.ItemTextureAsset))
        {
            e.LoadFromModFile<Texture2D>("assets/Items/CursedVHS.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (IsNpcAsset("Characters"))
        {
            e.LoadFromModFile<Texture2D>("assets/Characters/Sudoku.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (IsNpcAsset("Portraits"))
        {
            e.LoadFromModFile<Texture2D>("assets/Portraits/Sudoku.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (IsNpcAsset("Characters/Dialogue"))
        {
            e.LoadFromModFile<Dictionary<string, string>>("assets/Dialogue/Sudoku.json", AssetLoadPriority.Exclusive);
            return;
        }

        if (IsNpcAsset("Characters/schedules"))
        {
            e.LoadFromModFile<Dictionary<string, string>>("assets/Schedules/Sudoku.json", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo("Data/Characters"))
        {
            e.Edit(asset =>
            {
                Dictionary<string, CharacterData>? custom = this.Helper.Data.ReadJsonFile<Dictionary<string, CharacterData>>(
                    "assets/Data/Sudoku.character.json"
                );

                if (custom is null || !custom.TryGetValue(ModIdentity.SudokuNpcId, out CharacterData? sudoku))
                {
                    this.Monitor.Log("Couldn't read Sudoku.character.json; Sudoku NPC data was not injected.", LogLevel.Error);
                    return;
                }

                asset.AsDictionary<string, CharacterData>().Data[ModIdentity.SudokuNpcId] = sudoku;
            });
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
        {
            e.Edit(asset =>
            {
                Dictionary<string, ObjectData>? custom = this.Helper.Data.ReadJsonFile<Dictionary<string, ObjectData>>(
                    "assets/Data/CursedSignal.objects.json"
                );

                if (custom is null || !custom.TryGetValue(ModIdentity.CursedVhsItemId, out ObjectData? vhs))
                {
                    this.Monitor.Log("Couldn't read CursedSignal.objects.json; the Cursed VHS item was not injected.", LogLevel.Error);
                    return;
                }

                asset.AsDictionary<string, ObjectData>().Data[ModIdentity.CursedVhsItemId] = vhs;
            });
        }
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        int migratedKeys = ModIdentity.MigrateLegacyPlayerData(Game1.player);
        this.SyncNpcIdentityFlag();

        this.tvArrivalSheet = this.Helper.ModContent.Load<Texture2D>("assets/Events/Sudoku_TV.png");
        this.ResetSequenceState();

        this.Monitor.Log(
            "Cursed Signal v0.0.4 loaded. Cursed VHS, Daily Interaction, and item reward pools are enabled.",
            LogLevel.Info
        );

        if (migratedKeys > 0)
        {
            this.Monitor.Log(
                $"Migrated {migratedKeys} legacy prototype state key(s) into the ronvotri.CursedSignal keyspace. Legacy keys were kept for rollback safety.",
                LogLevel.Info
            );
        }

        if (ModIdentity.HasArrivalBeenSeen(Game1.player))
        {
            this.Monitor.Log("Sudoku has already arrived in this save. Her Cursed Signal NPC data is unlocked.", LogLevel.Info);
            this.EnsureCursedVhsGranted(showDialogue: false);

            if (this.Config.EnableDailySudoku)
                this.dailySudoku?.EnsureToday();
        }
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        this.SyncNpcIdentityFlag();

        if (ModIdentity.HasArrivalBeenSeen(Game1.player))
            this.EnsureCursedVhsGranted(showDialogue: false);

        if (!this.Config.EnableDailySudoku || this.dailySudoku is null)
            return;

        if (ModIdentity.HasArrivalBeenSeen(Game1.player))
            this.dailySudoku.EnsureToday();
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!this.Config.EnableDailySudoku || this.dailySudoku is null)
            return;

        if (!Context.IsWorldReady || this.sequenceActive || Game1.activeClickableMenu is not null)
            return;

        if (!e.Button.IsActionButton())
            return;

        if (!ModIdentity.HasArrivalBeenSeen(Game1.player))
            return;

        NPC? sudoku = this.FindSudoku(currentLocationOnly: true);
        if (sudoku is null)
            return;

        float distance = Vector2.Distance(Game1.player.Tile, sudoku.Tile);
        if (distance > 2.1f)
            return;

        // Once today's reward has been claimed, don't intercept the action button.
        // The game's normal NPC dialogue can happen instead.
        if (this.dailySudoku.IsRewardClaimedToday())
            return;

        this.Helper.Input.Suppress(e.Button);
        this.ShowDailySudokuPrompt();
    }

    private void ShowDailySudokuPrompt()
    {
        Response[] responses =
        {
            new("solve", "Giải."),
            new("later", "Để sau.")
        };

        Game1.currentLocation.createQuestionDialogue(
            "......^Bảng hôm nay.",
            responses,
            new GameLocation.afterQuestionBehavior(this.OnDailySudokuPromptAnswered)
        );
    }

    private void OnDailySudokuPromptAnswered(Farmer who, string answer)
    {
        if (answer == "solve")
        {
            this.OpenDailySudoku(force: false);
            return;
        }

        Game1.drawObjectDialogue("Sudoku nhìn bạn vài giây.^\"...Đừng điền bừa khi quay lại.\"");
    }

    private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
    {
        if (this.Config.EnableArrivalTest && e.NewTime >= this.Config.TestArrivalTime)
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
            this.DrawStatic(e.SpriteBatch, 1f);
            return;
        }

        if (this.elapsedTicks < wellEnd)
        {
            this.DrawWellGlimpse(e.SpriteBatch);
            if ((this.elapsedTicks / 5) % 2 == 0)
                this.DrawStatic(e.SpriteBatch, 0.22f);
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

        spriteBatch.Draw(
            Game1.staminaRect,
            new Rectangle(cx - 150, cy - 110, 300, 220),
            new Color(25, 37, 48) * 0.96f
        );
        spriteBatch.Draw(
            Game1.staminaRect,
            new Rectangle(cx - 132, cy - 92, 264, 184),
            new Color(90, 125, 145) * 0.35f
        );

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

        this.DrawStatic(spriteBatch, 0.8f);
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

        spriteBatch.Draw(this.tvArrivalSheet, destination, source, Color.White);

        if ((this.elapsedTicks / 4) % 3 == 0)
            this.DrawStatic(spriteBatch, 0.15f);
    }

    private void TryStartArrival(bool force)
    {
        if (!Context.IsWorldReady || this.sequenceActive || Game1.currentLocation is not FarmHouse)
            return;

        if (!force && Game1.activeClickableMenu is not null)
            return;

        if (!force)
        {
            if (ModIdentity.HasArrivalBeenSeen(Game1.player))
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

    private void OpenDailySudoku(bool force)
    {
        if (!Context.IsWorldReady || this.dailySudoku is null)
            return;

        if (!force)
        {
            if (!this.Config.EnableDailySudoku || !ModIdentity.HasArrivalBeenSeen(Game1.player))
                return;
        }

        SudokuPuzzle? puzzle = this.dailySudoku.EnsureToday();
        if (puzzle is null)
        {
            this.Monitor.Log(
                "Couldn't open Daily Sudoku because no valid puzzle was available.",
                LogLevel.Error
            );
            return;
        }

        Game1.activeClickableMenu = new SudokuMenu(this.dailySudoku, puzzle);
    }

    private void OnOpenDailyCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save before using sudoku_open.", LogLevel.Warn);
            return;
        }

        this.OpenDailySudoku(force: true);
    }

    private void OnResetDailyCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady || this.dailySudoku is null)
        {
            this.Monitor.Log("Load a save before using sudoku_resetdaily.", LogLevel.Warn);
            return;
        }

        this.dailySudoku.ResetTodayForTesting();
        SudokuPuzzle? puzzle = this.dailySudoku.EnsureToday();
        this.Monitor.Log(
            $"Today's Sudoku reset. Current puzzle={puzzle?.Id ?? "none"}.",
            LogLevel.Info
        );
    }

    private void OnGiveVhsCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save before using cursedsignal_givevhs.", LogLevel.Warn);
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
            "Sudoku arrival/VHS flags cleared in the Cursed Signal and legacy prototype keyspaces. Existing NPC/item instances are intentionally left alone.",
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
        this.EnsureCursedVhsGranted(showDialogue: false);

        this.Monitor.Log(
            "Sudoku unlock flags set under ronvotri.CursedSignal. Save/reload or sleep to the next day to let Data/Characters add her.",
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

        this.Monitor.Log(
            $"Sudoku status: arrivalSeen={seen}, npcEnabled={npcEnabled}, npcPresent={sudoku is not null}, npcId={sudoku?.Name ?? "none"}, vhsGranted={vhsGranted}, sequenceActive={this.sequenceActive}, dailyPuzzle={dailyPuzzle?.Id ?? "none"}, dailyClaimed={dailyClaimed}, time={Game1.timeOfDay}, location={Game1.currentLocation?.NameOrUniqueName}.",
            LogLevel.Info
        );
    }
}
