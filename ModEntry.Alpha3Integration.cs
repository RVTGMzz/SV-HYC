using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;

namespace HeyYoureCursed;

/// <summary>Bridges the alpha.3 story/haunting systems into the repository's older split-partial architecture.</summary>
internal sealed partial class ModEntry
{
    private const string PencilTextureAsset = "Mods/ronvotri.HeyYoureCursed/Pencil";
    private bool pendingAlpha3VhsOriginDialogue;

    private void RegisterAlpha3Features(IModHelper helper)
    {
        helper.Events.Content.AssetRequested += this.OnAlpha3AssetRequested;
        helper.Events.GameLoop.SaveLoaded += this.OnAlpha3SaveLoaded;
        helper.Events.GameLoop.DayStarted += this.OnAlpha3DayStarted;
        helper.Events.GameLoop.UpdateTicked += this.OnAlpha3UpdateTicked;
        helper.Events.GameLoop.ReturnedToTitle += this.OnAlpha3ReturnedToTitle;

        helper.ConsoleCommands.Add("heyyourecursed_givepencil", "Give one Hey! You're Cursed! Pencil for story testing.", this.OnGivePencilCommand);
        helper.ConsoleCommands.Add("heyyourecursed_test_prologue", "Replay the Stardrop Saloon prologue while standing in the Saloon.", this.OnTestPrologueCommand);
        helper.ConsoleCommands.Add("heyyourecursed_test_wizard", "Arm the seven-day Wizard reveal immediately for testing inside the farmhouse.", this.OnTestWizardRevealCommand);
        helper.ConsoleCommands.Add("heyyourecursed_givecabinet", "Give one Occult Cabinet immediately for testing.", this.OnGiveCabinetCommand);
        helper.ConsoleCommands.Add("heyyourecursed_test_seal", "Seal Sudoku into the Occult Cabinet immediately for testing.", this.OnTestSealCommand);
        helper.ConsoleCommands.Add("heyyourecursed_test_unseal", "Unseal Sudoku from the Occult Cabinet immediately for testing.", this.OnTestUnsealCommand);
    }

    private void OnAlpha3AssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo(PencilTextureAsset))
        {
            e.LoadFromModFile<Texture2D>("assets/Items/Pencil.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo(ModIdentity.BigCraftableTextureAsset))
        {
            e.LoadFromModFile<Texture2D>("assets/Items/OccultCabinet.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftables"))
        {
            e.Edit(asset =>
            {
                Dictionary<string, BigCraftableData> data = asset.AsDictionary<string, BigCraftableData>().Data;
                data[ModIdentity.OccultCabinetItemId] = new BigCraftableData
                {
                    Name = ModIdentity.OccultCabinetItemId,
                    DisplayName = T("item.cabinet.name"),
                    Description = T("item.cabinet.description"),
                    Price = 0,
                    Fragility = 0,
                    CanBePlacedIndoors = true,
                    CanBePlacedOutdoors = false,
                    IsLamp = false,
                    Texture = ModIdentity.BigCraftableTextureAsset,
                    SpriteIndex = 0
                };
            }, AssetEditPriority.Late);
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
        {
            e.Edit(asset =>
            {
                Dictionary<string, ObjectData>? custom = this.Helper.Data.ReadJsonFile<Dictionary<string, ObjectData>>("assets/Data/HeyYoureCursed.objects.json");
                if (custom is null)
                {
                    this.Monitor.Log("Couldn't read HeyYoureCursed.objects.json; alpha.3 story items were not injected.", LogLevel.Error);
                    return;
                }

                Dictionary<string, ObjectData> data = asset.AsDictionary<string, ObjectData>().Data;
                foreach ((string id, ObjectData item) in custom)
                {
                    if (id == ModIdentity.CursedVhsItemId)
                    {
                        item.DisplayName = T("item.vhs.name");
                        item.Description = T("item.vhs.description");
                    }
                    else if (id == ModIdentity.PencilItemId)
                    {
                        item.DisplayName = T("item.pencil.name");
                        item.Description = T("item.pencil.description");
                    }
                    data[id] = item;
                }
            }, AssetEditPriority.Late);
            return;
        }

        if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
        {
            e.Edit(asset =>
            {
                Dictionary<string, ShopData> shops = asset.AsDictionary<string, ShopData>().Data;
                if (!shops.TryGetValue("SeedShop", out ShopData? pierre))
                {
                    this.Monitor.Log("Pierre's SeedShop wasn't found; the story Pencil couldn't be added.", LogLevel.Warn);
                    return;
                }

                pierre.Items ??= new List<ShopItemData>();
                const string stockId = ModIdentity.UniqueId + "_PencilStock";
                if (pierre.Items.Any(item => item.Id == stockId))
                    return;

                pierre.Items.Add(new ShopItemData
                {
                    Id = stockId,
                    ItemId = ModIdentity.PencilQualifiedItemId,
                    Price = 100,
                    ApplyProfitMargins = false
                });
            }, AssetEditPriority.Late);
        }
    }

    private void OnAlpha3SaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        this.CorrectLegacyAutoVhsBeforePrologue();
        this.PrepareSaloonPrologueOnSaveLoaded();
        this.PrepareOccultCabinetOnSaveLoaded();
        this.ApplySealedSudokuState();
        this.QueueVhsOriginDialogueIfReady();

        this.Monitor.Log("alpha.3.4 integration active: persistent Saloon gate, Pencil activation, seven-day Cabinet unlock, and Active Haunting state.", LogLevel.Info);
    }

    private void OnAlpha3DayStarted(object? sender, DayStartedEventArgs e)
    {
        this.CorrectLegacyAutoVhsBeforePrologue();
        this.PrepareSaloonPrologueOnDayStarted();
        this.PrepareOccultCabinetOnDayStarted();
        this.ApplySealedSudokuState();
        this.QueueVhsOriginDialogueIfReady();
    }

    private void OnAlpha3UpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady
            || !this.pendingAlpha3VhsOriginDialogue
            || this.sequenceActive
            || Game1.eventUp
            || Game1.activeClickableMenu is not null)
        {
            return;
        }

        this.pendingAlpha3VhsOriginDialogue = false;
        Game1.player.modData[ModIdentity.VhsOriginStorySeenKey] = "true";
        Game1.drawObjectDialogue(T("story.vhs.origin-package"));
    }

    private void OnAlpha3ReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        this.pendingAlpha3VhsOriginDialogue = false;
        this.ResetSaloonPrologueRuntime();
        this.ResetOccultCabinetRuntime();
    }

    private void QueueVhsOriginDialogueIfReady()
    {
        if (!this.HasSaloonPrologueSeen()
            || Game1.player.modData.ContainsKey(ModIdentity.VhsOriginStorySeenKey)
            || !Game1.player.modData.TryGetValue(ModIdentity.CursedVhsGrantedKey, out string? granted)
            || granted != "true")
        {
            return;
        }

        if (!Game1.player.modData.TryGetValue(ModIdentity.SaloonPrologueCompletedDayKey, out string? raw)
            || !int.TryParse(raw, out int completedDay)
            || Game1.Date.TotalDays <= completedDay)
        {
            return;
        }

        this.pendingAlpha3VhsOriginDialogue = true;
    }

    /// <summary>Undo the old build's automatic VHS grant on genuinely fresh saves until the player attends the Saloon gathering.</summary>
    private void CorrectLegacyAutoVhsBeforePrologue()
    {
        bool prologueSeen = Game1.player.modData.TryGetValue(ModIdentity.SaloonPrologueSeenKey, out string? seen) && seen == "true";
        if (prologueSeen)
            return;

        bool realPriorStoryProgress = ModIdentity.HasArrivalBeenSeen(Game1.player)
            || ModIdentity.IsCursedVhsInstalled(Game1.player)
            || ModIdentity.HasFirstConversationCompleted(Game1.player)
            || Game1.player.modData.ContainsKey(ModIdentity.PencilAcceptedKey);

        if (realPriorStoryProgress)
            return;

        this.RemoveAllCursedVhsFromInventory();
        Game1.player.modData.Remove(ModIdentity.CursedVhsGrantedKey);
    }

    private void ApplySealedSudokuState()
    {
        if (!this.IsSudokuSealed())
            return;

        this.ResetSequenceState();
        this.ResetSudokuRoommateBehavior();
        this.RemoveAllSudokuInstances();
        Game1.player.modData.Remove(ModIdentity.SudokuNpcEnabledKey);
        Game1.player.modData[ModIdentity.ActiveHauntingIdKey] = "none";
    }

    private void OnGivePencilCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save before using heyyourecursed_givepencil.", LogLevel.Warn);
            return;
        }

        try
        {
            Item pencil = ItemRegistry.Create(ModIdentity.PencilQualifiedItemId, 1);
            Game1.player.addItemByMenuIfNecessary(pencil);
            this.Monitor.Log("One story Pencil was added to the player's inventory.", LogLevel.Info);
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"Couldn't give the story Pencil. {ex}", LogLevel.Error);
        }
    }
}
