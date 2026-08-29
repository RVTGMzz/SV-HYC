using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace HeyYoureCursed;

internal sealed partial class ModEntry
{
    private bool pendingWizardReveal;

    private bool HasWizardRevealSeen()
    {
        return Game1.player.modData.TryGetValue(ModIdentity.WizardRevealSeenKey, out string? raw)
            && raw == "true";
    }

    private bool IsOccultCabinetUnlocked()
    {
        return Game1.player.modData.TryGetValue(ModIdentity.OccultCabinetUnlockedKey, out string? raw)
            && raw == "true";
    }

    private bool IsSudokuSealed()
    {
        return Game1.player.modData.TryGetValue(ModIdentity.SudokuSealedKey, out string? raw)
            && raw == "true";
    }

    private string GetActiveHauntingId()
    {
        if (!this.IsOccultCabinetUnlocked())
            return "Sudoku";

        if (Game1.player.modData.TryGetValue(ModIdentity.ActiveHauntingIdKey, out string? raw)
            && !string.IsNullOrWhiteSpace(raw))
        {
            return raw;
        }

        return this.IsSudokuSealed() ? "none" : "Sudoku";
    }

    private bool IsSudokuActiveHaunting()
    {
        return string.Equals(this.GetActiveHauntingId(), "Sudoku", StringComparison.OrdinalIgnoreCase);
    }

    private int GetDaysSinceSudokuActivation()
    {
        if (!Game1.player.modData.TryGetValue(ModIdentity.SudokuActivatedDayKey, out string? raw)
            || !int.TryParse(raw, out int activatedDay))
        {
            return -1;
        }

        return Math.Max(0, Game1.Date.TotalDays - activatedDay);
    }

    private bool IsWizardRevealEligible()
    {
        return ModIdentity.HasFirstConversationCompleted(Game1.player)
            && !this.HasWizardRevealSeen()
            && this.GetDaysSinceSudokuActivation() >= 7;
    }

    private void PrepareOccultCabinetOnSaveLoaded()
    {
        this.pendingWizardReveal = false;

        if (ModIdentity.HasFirstConversationCompleted(Game1.player)
            && !Game1.player.modData.ContainsKey(ModIdentity.SudokuActivatedDayKey))
        {
            Game1.player.modData[ModIdentity.SudokuActivatedDayKey] = Game1.Date.TotalDays.ToString();
        }

        if (this.HasWizardRevealSeen())
        {
            Game1.player.modData[ModIdentity.OccultCabinetUnlockedKey] = "true";
            if (!Game1.player.modData.ContainsKey(ModIdentity.ActiveHauntingIdKey))
                Game1.player.modData[ModIdentity.ActiveHauntingIdKey] = this.IsSudokuSealed() ? "none" : "Sudoku";

            if (this.IsSudokuActiveHaunting())
            {
                Game1.player.modData.Remove(ModIdentity.SudokuSealedKey);
                Game1.player.modData.Remove(ModIdentity.SudokuSealedDayKey);
            }
            else if (ModIdentity.HasFirstConversationCompleted(Game1.player))
            {
                Game1.player.modData[ModIdentity.SudokuSealedKey] = "true";
            }

            bool cabinetGifted = Game1.player.modData.TryGetValue(ModIdentity.OccultCabinetGiftedKey, out string? giftedRaw)
                && giftedRaw == "true";
            if (!cabinetGifted)
                this.GiveOccultCabinet(showMessage: false);
            return;
        }

        if (this.IsWizardRevealEligible())
            this.pendingWizardReveal = true;
    }

    private void PrepareOccultCabinetOnDayStarted()
    {
        this.pendingWizardReveal = this.IsWizardRevealEligible();
    }

    private void ResetOccultCabinetRuntime()
    {
        this.pendingWizardReveal = false;
    }

    private void UpdateOccultCabinetStoryUi()
    {
        if (!this.pendingWizardReveal
            || this.sequenceActive
            || Game1.eventUp
            || Game1.activeClickableMenu is not null
            || Game1.currentLocation is not FarmHouse)
        {
            return;
        }

        this.pendingWizardReveal = false;
        Game1.player.Halt();
        Game1.playSound("doorClose");
        Game1.activeClickableMenu = new WizardRevealMenu(
            onChoice: choice => Game1.player.modData[ModIdentity.WizardRevealChoiceKey] = choice,
            onComplete: this.CompleteWizardReveal
        );
    }

    private void CompleteWizardReveal()
    {
        Game1.player.modData[ModIdentity.WizardRevealSeenKey] = "true";
        Game1.player.modData[ModIdentity.OccultCabinetUnlockedKey] = "true";
        Game1.player.modData[ModIdentity.ActiveHauntingIdKey] = "Sudoku";
        Game1.player.modData.Remove(ModIdentity.SudokuSealedKey);
        Game1.player.modData.Remove(ModIdentity.SudokuSealedDayKey);

        this.GiveOccultCabinet(showMessage: true);
        this.Monitor.Log(
            "Seven-day Wizard reveal completed. Occult Cabinet unlocked; active haunting initialized to Sudoku.",
            LogLevel.Info
        );
    }

    private void GiveOccultCabinet(bool showMessage)
    {
        if (!Context.IsWorldReady)
            return;

        try
        {
            Item cabinet = ItemRegistry.Create(ModIdentity.OccultCabinetQualifiedItemId, 1);
            Game1.player.addItemByMenuIfNecessary(cabinet);
            Game1.player.modData[ModIdentity.OccultCabinetGiftedKey] = "true";

            if (showMessage)
                Game1.showGlobalMessage(T("story.cabinet.received"));
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"Couldn't give the Occult Cabinet. {ex}", LogLevel.Error);
        }
    }

    private bool TryHandleOccultCabinetInteraction(ButtonPressedEventArgs e)
    {
        if (!this.IsOccultCabinetUnlocked() || Game1.currentLocation is null)
            return false;

        bool isActionButton = e.Button.IsActionButton() || e.Button == SButton.ControllerA;
        bool isMouseClick = e.Button == SButton.MouseLeft;
        if (!isActionButton && !isMouseClick)
            return false;

        Vector2 tile = isMouseClick ? e.Cursor.GrabTile : Game1.player.GetGrabTile();
        StardewValley.Object? cabinet = this.FindOccultCabinetAt(tile);
        if (cabinet is null)
            return false;

        this.Helper.Input.Suppress(e.Button);
        Game1.playSound("shiny4");
        this.ShowOccultCabinetMenu();
        return true;
    }

    private Texture2D? LoadOccultCabinetTexture()
    {
        try
        {
            return this.Helper.GameContent.Load<Texture2D>(ModIdentity.BigCraftableTextureAsset);
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"Couldn't load the Occult Cabinet texture for its menu. {ex.Message}", LogLevel.Warn);
            return null;
        }
    }

    private void ShowOccultCabinetMenu()
    {
        if (!Context.IsWorldReady || !this.IsOccultCabinetUnlocked())
            return;

        string active = this.GetActiveHauntingId();
        bool sudokuSealed = this.IsSudokuSealed();
        string activeDisplay = string.Equals(active, "Sudoku", StringComparison.OrdinalIgnoreCase)
            ? T("cabinet.menu.sudoku")
            : string.Equals(active, "none", StringComparison.OrdinalIgnoreCase)
                ? T("cabinet.menu.none")
                : active;
        string sealedDisplay = sudokuSealed ? T("cabinet.menu.sudoku") : T("cabinet.menu.none");

        List<OccultCabinetOption> options = new();
        if (this.IsSudokuActiveHaunting())
        {
            options.Add(new OccultCabinetOption
            {
                Label = T("cabinet.option.seal-sudoku"),
                Action = this.BeginSealSudoku
            });
        }
        else if (sudokuSealed && string.Equals(active, "none", StringComparison.OrdinalIgnoreCase))
        {
            options.Add(new OccultCabinetOption
            {
                Label = T("cabinet.option.unseal-sudoku"),
                Action = this.UnsealSudoku
            });
        }

        options.Add(new OccultCabinetOption
        {
            Label = T("cabinet.option.close"),
            Action = () => { }
        });

        Game1.activeClickableMenu = new OccultCabinetMenu(
            this.LoadOccultCabinetTexture(),
            T("cabinet.menu.title"),
            T(this.IsSudokuActiveHaunting() ? "cabinet.menu.intro.active" : "cabinet.menu.intro.sealed"),
            T("cabinet.menu.active-header"),
            activeDisplay,
            T("cabinet.menu.sealed-header"),
            sealedDisplay,
            options
        );
    }

    private void BeginSealSudoku()
    {
        if (!Context.IsWorldReady || !this.IsSudokuActiveHaunting())
            return;

        int trust = ModIdentity.GetSudokuTrust(Game1.player);
        string[] lines = trust switch
        {
            <= 6 => new[] { T("cabinet.sudoku.seal.low.1"), T("cabinet.sudoku.seal.low.2") },
            <= 18 => new[] { T("cabinet.sudoku.seal.mid.1"), T("cabinet.sudoku.seal.mid.2") },
            _ => new[] { T("cabinet.sudoku.seal.high.1"), T("cabinet.sudoku.seal.high.2") }
        };

        Game1.activeClickableMenu = new SudokuConversationMenu(
            this.LoadSudokuPortraitTexture(),
            portraitIndex: trust >= 19 ? 4 : trust >= 7 ? 1 : 6,
            lines: lines,
            question: T("cabinet.sudoku.seal.question"),
            primaryLabel: T("cabinet.sudoku.seal.confirm"),
            secondaryLabel: T("cabinet.sudoku.seal.cancel"),
            progressText: T("cabinet.sudoku.seal.progress"),
            onPrimary: this.SealSudoku,
            onSecondary: this.ShowOccultCabinetMenu
        );
    }

    private void SealSudoku()
    {
        if (!Context.IsWorldReady || !this.IsOccultCabinetUnlocked())
            return;

        this.ResetSequenceState();
        this.ResetSudokuRoommateBehavior();
        int removed = this.RemoveAllSudokuInstances();
        Game1.player.modData.Remove(ModIdentity.SudokuNpcEnabledKey);
        Game1.player.modData[ModIdentity.SudokuSealedKey] = "true";
        Game1.player.modData[ModIdentity.SudokuSealedDayKey] = Game1.Date.TotalDays.ToString();
        Game1.player.modData[ModIdentity.ActiveHauntingIdKey] = "none";
        this.Helper.GameContent.InvalidateCache("Data/Festivals/fall27");

        Game1.playSound("ghost");
        Game1.drawObjectDialogue(T("cabinet.sudoku.sealed"));
        this.Monitor.Log($"Sudoku sealed into the Occult Cabinet. Removed NPC instances={removed}; progression/Trust preserved.", LogLevel.Info);
    }

    private void UnsealSudoku()
    {
        if (!Context.IsWorldReady || !this.IsOccultCabinetUnlocked())
            return;

        string active = this.GetActiveHauntingId();
        if (!string.Equals(active, "none", StringComparison.OrdinalIgnoreCase))
        {
            Game1.drawObjectDialogue(T("cabinet.unseal.blocked"));
            return;
        }

        Game1.player.modData[ModIdentity.ActiveHauntingIdKey] = "Sudoku";
        Game1.player.modData.Remove(ModIdentity.SudokuSealedKey);
        Game1.player.modData.Remove(ModIdentity.SudokuSealedDayKey);
        Game1.player.modData[ModIdentity.SudokuNpcEnabledKey] = "true";
        ModIdentity.MarkDailySignalRunToday(Game1.player);
        this.Helper.GameContent.InvalidateCache("Data/Festivals/fall27");

        NPC? sudoku = Game1.currentLocation is FarmHouse
            ? this.PlaceSudokuAfterArrival()
            : this.EnsureSudokuCharacterExists();

        if (sudoku is not null)
        {
            sudoku.IsInvisible = false;
            sudoku.ignoreScheduleToday = true;
            sudoku.Halt();
        }

        this.BeginSudokuRoommateBehaviorAfterArrival();

        int trust = ModIdentity.GetSudokuTrust(Game1.player);
        string[] lines = trust switch
        {
            <= 6 => new[] { T("cabinet.sudoku.return.low") },
            <= 18 => new[] { T("cabinet.sudoku.return.mid") },
            _ => new[] { T("cabinet.sudoku.return.high") }
        };

        Game1.playSound("ghost");
        Game1.activeClickableMenu = new SudokuConversationMenu(
            this.LoadSudokuPortraitTexture(),
            portraitIndex: trust >= 19 ? 4 : 1,
            lines: lines,
            question: T("cabinet.sudoku.return.question"),
            primaryLabel: T("dialogue.ok"),
            secondaryLabel: T("ui.common.later"),
            progressText: T("cabinet.sudoku.return.progress"),
            onPrimary: () => { },
            onSecondary: () => { }
        );

        this.Monitor.Log("Sudoku unsealed from the Occult Cabinet and restored as the active haunting.", LogLevel.Info);
    }

    private void OnTestSealCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save before using heyyourecursed_test_seal.", LogLevel.Warn);
            return;
        }
        Game1.player.modData[ModIdentity.OccultCabinetUnlockedKey] = "true";
        Game1.player.modData[ModIdentity.ActiveHauntingIdKey] = "Sudoku";
        Game1.player.modData.Remove(ModIdentity.SudokuSealedKey);
        this.SealSudoku();
    }

    private void OnTestUnsealCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save before using heyyourecursed_test_unseal.", LogLevel.Warn);
            return;
        }
        Game1.player.modData[ModIdentity.OccultCabinetUnlockedKey] = "true";
        Game1.player.modData[ModIdentity.ActiveHauntingIdKey] = "none";
        Game1.player.modData[ModIdentity.SudokuSealedKey] = "true";
        this.UnsealSudoku();
    }

    private StardewValley.Object? FindOccultCabinetAt(Vector2 tile)
    {
        if (Game1.currentLocation.objects.TryGetValue(tile, out StardewValley.Object? direct)
            && direct.QualifiedItemId == ModIdentity.OccultCabinetQualifiedItemId)
            return direct;

        Vector2 below = tile + Vector2.UnitY;
        if (Game1.currentLocation.objects.TryGetValue(below, out StardewValley.Object? lower)
            && lower.QualifiedItemId == ModIdentity.OccultCabinetQualifiedItemId)
            return lower;
        return null;
    }

    private int RemoveAllOccultCabinetsForTesting()
    {
        int removed = 0;
        for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
        {
            Item? item = Game1.player.Items[i];
            if (item?.QualifiedItemId != ModIdentity.OccultCabinetQualifiedItemId) continue;
            removed += Math.Max(1, item.Stack); Game1.player.Items[i] = null;
        }

        FarmHouse? home = Utility.getHomeOfFarmer(Game1.player);
        if (home is not null)
        {
            foreach (Vector2 tile in home.objects.Pairs.Where(pair => pair.Value.QualifiedItemId == ModIdentity.OccultCabinetQualifiedItemId).Select(pair => pair.Key).ToArray())
            { home.objects.Remove(tile); removed++; }
        }
        return removed;
    }

    private void OnTestWizardRevealCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady) { this.Monitor.Log("Load a save before using heyyourecursed_test_wizard.", LogLevel.Warn); return; }
        this.RemoveAllOccultCabinetsForTesting();
        Game1.player.modData[ModIdentity.FirstConversationCompletedKey] = "true";
        Game1.player.modData[ModIdentity.PencilAcceptedKey] = "true";
        Game1.player.modData[ModIdentity.SudokuActivatedDayKey] = Math.Max(0, Game1.Date.TotalDays - 7).ToString();
        Game1.player.modData.Remove(ModIdentity.WizardRevealSeenKey);
        Game1.player.modData.Remove(ModIdentity.WizardRevealChoiceKey);
        Game1.player.modData.Remove(ModIdentity.OccultCabinetUnlockedKey);
        Game1.player.modData.Remove(ModIdentity.OccultCabinetGiftedKey);
        Game1.player.modData.Remove(ModIdentity.SudokuSealedKey);
        Game1.player.modData.Remove(ModIdentity.SudokuSealedDayKey);
        this.pendingWizardReveal = true;
        this.Monitor.Log("Wizard reveal armed. Enter/stay inside the farmhouse with no menu open; the seven-day scene will start automatically.", LogLevel.Info);
    }

    private void OnGiveCabinetCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady) { this.Monitor.Log("Load a save before using heyyourecursed_givecabinet.", LogLevel.Warn); return; }
        Game1.player.modData[ModIdentity.OccultCabinetUnlockedKey] = "true";
        if (!Game1.player.modData.ContainsKey(ModIdentity.ActiveHauntingIdKey))
            Game1.player.modData[ModIdentity.ActiveHauntingIdKey] = this.IsSudokuSealed() ? "none" : "Sudoku";
        this.GiveOccultCabinet(showMessage: false);
        this.Monitor.Log("One Occult Cabinet was added for testing.", LogLevel.Info);
    }
}
