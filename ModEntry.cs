using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Objects;
using StardewValley.Locations;

namespace CursedSignal;

internal sealed partial class ModEntry : Mod
{
    private const int FrameWidth = 64;
    private const int FrameHeight = 64;
    private const int FrameCount = 6;

    private ModConfig Config = null!;
    private Texture2D? tvArrivalSheet;
    private Texture2D? wellBroadcastTexture;
    private DailySudokuService? dailySudoku;

    private bool sequenceActive;
    private bool sequenceIsFirstArrival;
    private int elapsedTicks;
    private int frameIndex;

    public override void Entry(IModHelper helper)
    {
        this.Config = helper.ReadConfig<ModConfig>();
        this.dailySudoku = new DailySudokuService(helper, this.Monitor, this.Config);

        helper.Events.Content.AssetRequested += this.OnAssetRequested;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
        helper.Events.GameLoop.TimeChanged += this.OnTimeChanged;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        helper.Events.Player.Warped += this.OnWarped;
        helper.Events.Display.RenderedWorld += this.OnRenderedWorld;
        helper.Events.Input.ButtonPressed += this.OnTapeButtonPressed;
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;

        helper.ConsoleCommands.Add("sudoku_testarrival", "Start the current Cursed Signal sequence immediately while inside the farmhouse.", this.OnTestArrivalCommand);
        helper.ConsoleCommands.Add("sudoku_resetarrival", "Reset the full VHS/signal/Sudoku core flow and return one fresh test tape.", this.OnResetArrivalCommand);
        helper.ConsoleCommands.Add("sudoku_unlocknpc", "Set Sudoku's arrival flag and create the NPC immediately if possible.", this.OnUnlockNpcCommand);
        helper.ConsoleCommands.Add("sudoku_status", "Print stabilized Cursed Signal core state for the current save.", this.OnStatusCommand);
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
        this.ResetSequenceState();
        this.LoadEventTexturesSafely();

        this.Monitor.Log(
            "Cursed Signal v0.0.6-alpha.2 loaded. Core stabilization pass is active.",
            LogLevel.Info
        );

        if (migratedKeys > 0)
        {
            this.Monitor.Log(
                $"Migrated {migratedKeys} legacy prototype state key(s) into the ronvotri.CursedSignal keyspace. Legacy keys were kept for rollback safety.",
                LogLevel.Info
            );
        }

        this.ReconcileCoreState();

        if (!ModIdentity.IsCursedVhsInstalled(Game1.player))
            this.EnsureCursedVhsGranted(showDialogue: false);

        if (ModIdentity.HasArrivalBeenSeen(Game1.player) && this.Config.EnableDailySudoku)
            this.dailySudoku?.EnsureToday();
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        this.ResetSequenceState();
        this.ReconcileCoreState();

        if (!ModIdentity.IsCursedVhsInstalled(Game1.player))
            this.EnsureCursedVhsGranted(showDialogue: false);

        if (this.Config.EnableDailySudoku && this.dailySudoku is not null && ModIdentity.HasArrivalBeenSeen(Game1.player))
            this.dailySudoku.EnsureToday();
    }

    private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        this.ResetSequenceState();
        this.tvArrivalSheet = null;
        this.wellBroadcastTexture = null;
    }

    private void LoadEventTexturesSafely()
    {
        this.tvArrivalSheet = null;
        this.wellBroadcastTexture = null;

        try
        {
            Texture2D sheet = this.Helper.ModContent.Load<Texture2D>("assets/Events/Sudoku_TV.png");
            if (sheet.Width < FrameWidth * 3 || sheet.Height < FrameHeight * 2)
            {
                this.Monitor.Log(
                    $"assets/Events/Sudoku_TV.png is too small ({sheet.Width}x{sheet.Height}); expected at least {FrameWidth * 3}x{FrameHeight * 2}. The sequence will continue without emergence frames.",
                    LogLevel.Error
                );
            }
            else
            {
                this.tvArrivalSheet = sheet;
            }
        }
        catch (Exception ex)
        {
            this.Monitor.Log(
                $"Couldn't load assets/Events/Sudoku_TV.png. The sequence will continue with fallback static/glitch only. {ex.Message}",
                LogLevel.Error
            );
        }

        try
        {
            this.wellBroadcastTexture = this.Helper.ModContent.Load<Texture2D>("assets/Events/WellBroadcast.png");
        }
        catch (Exception ex)
        {
            this.Monitor.Log(
                $"Couldn't load assets/Events/WellBroadcast.png. The first signal will skip the well image instead of crashing. {ex.Message}",
                LogLevel.Error
            );
        }
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
        if (sudoku is null || sudoku.IsInvisible)
            return;

        float distance = Vector2.Distance(Game1.player.Tile, sudoku.Tile);
        if (distance > 2.1f)
            return;

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
}
