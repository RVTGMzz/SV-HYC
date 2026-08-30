using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace HeyYoureCursed;

internal sealed partial class ModEntry
{
    private enum SudokuGiftReaction
    {
        Pencil,
        Loved,
        Liked,
        Neutral,
        Disliked,
        Hated
    }

    private static readonly HashSet<string> SudokuLovedGiftNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Ghost Crystal",
        "Fairy Stone",
        "Strange Doll",
        "Strange Doll (green)",
        "Strange Doll (yellow)"
    };

    private static readonly HashSet<string> SudokuLikedGiftNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Void Essence",
        "Solar Essence",
        "Bat Wing",
        "Bone Fragment",
        "Obsidian",
        "Frozen Tear",
        "Fire Quartz",
        "Earth Crystal",
        "Quartz",
        "Pumpkin",
        "Jack-O-Lantern"
    };

    private void ShowSudokuGiftOffer()
    {
        if (!Context.IsWorldReady)
            return;

        int trust = ModIdentity.GetSudokuTrust(Game1.player);

        if (ModIdentity.HasGiftedSudokuToday(Game1.player))
        {
            this.ShowSimpleSudokuDialogue(
                portraitIndex: 0,
                lines: new[] { T("gift.already.1"), T("gift.already.2") },
                progressText: T("gift.progress", new { trust }),
                returnToHub: true
            );
            return;
        }

        Item? gift = Game1.player.CurrentItem;
        if (gift is null)
        {
            this.ShowSimpleSudokuDialogue(
                portraitIndex: 1,
                lines: new[] { T("gift.empty.1"), T("gift.empty.2") },
                progressText: T("gift.progress", new { trust }),
                returnToHub: true
            );
            return;
        }

        if (!gift.QualifiedItemId.StartsWith("(O)", StringComparison.Ordinal)
            || gift.QualifiedItemId == ModIdentity.CursedVhsQualifiedItemId
            || gift.QualifiedItemId == ModIdentity.OccultCabinetQualifiedItemId)
        {
            this.ShowSimpleSudokuDialogue(
                portraitIndex: 6,
                lines: new[] { T("gift.invalid.1"), T("gift.invalid.2") },
                progressText: T("gift.progress", new { trust }),
                returnToHub: true
            );
            return;
        }

        string qualifiedId = gift.QualifiedItemId;
        string displayName = gift.DisplayName;
        Texture2D? portraits = this.LoadSudokuPortraitTexture();
        Game1.activeClickableMenu = new SudokuConversationMenu(
            portraits,
            portraitIndex: 0,
            lines: new[] { T("gift.offer.1", new { item = displayName }) },
            question: T("gift.offer.question", new { item = displayName }),
            primaryLabel: T("gift.offer.give"),
            secondaryLabel: T("gift.offer.cancel"),
            progressText: T("gift.progress", new { trust }),
            onPrimary: () => this.CompleteSudokuGift(qualifiedId),
            onSecondary: () => this.ShowSudokuInteractionHub()
        );
    }

    private void CompleteSudokuGift(string expectedQualifiedId)
    {
        if (!Context.IsWorldReady || ModIdentity.HasGiftedSudokuToday(Game1.player))
        {
            this.ShowSudokuGiftOffer();
            return;
        }

        Item? gift = Game1.player.CurrentItem;
        if (gift is null || gift.QualifiedItemId != expectedQualifiedId)
        {
            this.ShowSudokuGiftOffer();
            return;
        }

        SudokuGiftReaction reaction = GetSudokuGiftReaction(gift);
        int requestedDelta = reaction switch
        {
            SudokuGiftReaction.Pencil => 2,
            SudokuGiftReaction.Loved => 2,
            SudokuGiftReaction.Liked => 1,
            SudokuGiftReaction.Disliked => -1,
            SudokuGiftReaction.Hated => -2,
            _ => 0
        };

        if (!ModIdentity.TryApplySudokuGiftTrust(Game1.player, requestedDelta, out int trust, out int actualDelta))
        {
            this.ShowSudokuGiftOffer();
            return;
        }

        string itemName = gift.DisplayName;
        this.ConsumeOneGiftItem(gift);
        Game1.playSound("smallSelect");

        (int portrait, string key1, string key2) = reaction switch
        {
            SudokuGiftReaction.Pencil => (4, "gift.pencil.1", "gift.pencil.2"),
            SudokuGiftReaction.Loved => (3, "gift.loved.1", "gift.loved.2"),
            SudokuGiftReaction.Liked => (4, "gift.liked.1", "gift.liked.2"),
            SudokuGiftReaction.Disliked => (1, "gift.disliked.1", "gift.disliked.2"),
            SudokuGiftReaction.Hated => (6, "gift.hated.1", "gift.hated.2"),
            _ => (0, "gift.neutral.1", "gift.neutral.2")
        };

        string progress = actualDelta == 0
            ? T("gift.progress", new { trust })
            : T("gift.progress.changed", new
            {
                delta = actualDelta > 0 ? $"+{actualDelta}" : actualDelta.ToString(),
                trust
            });

        this.ShowSimpleSudokuDialogue(
            portrait,
            new[]
            {
                T(key1, new { item = itemName }),
                T(key2, new { item = itemName })
            },
            progress,
            returnToHub: true
        );
    }

    private static SudokuGiftReaction GetSudokuGiftReaction(Item gift)
    {
        if (gift.QualifiedItemId == ModIdentity.PencilQualifiedItemId)
            return SudokuGiftReaction.Pencil;

        if (SudokuLovedGiftNames.Contains(gift.Name))
            return SudokuGiftReaction.Loved;

        if (SudokuLikedGiftNames.Contains(gift.Name))
            return SudokuGiftReaction.Liked;

        if (gift is StardewValley.Object obj)
        {
            return obj.Category switch
            {
                -20 => SudokuGiftReaction.Hated,
                -28 => SudokuGiftReaction.Liked,
                -12 => SudokuGiftReaction.Liked,
                -2 => SudokuGiftReaction.Liked,
                -7 => SudokuGiftReaction.Disliked,
                -4 => SudokuGiftReaction.Disliked,
                _ => SudokuGiftReaction.Neutral
            };
        }

        return SudokuGiftReaction.Neutral;
    }

    private void ConsumeOneGiftItem(Item gift)
    {
        for (int i = 0; i < Game1.player.Items.Count; i++)
        {
            Item? item = Game1.player.Items[i];
            if (!ReferenceEquals(item, gift))
                continue;

            item.Stack--;
            if (item.Stack <= 0)
                Game1.player.Items[i] = null;
            return;
        }

        for (int i = 0; i < Game1.player.Items.Count; i++)
        {
            Item? item = Game1.player.Items[i];
            if (item?.QualifiedItemId != gift.QualifiedItemId)
                continue;

            item.Stack--;
            if (item.Stack <= 0)
                Game1.player.Items[i] = null;
            return;
        }
    }

    private void OnResetGiftCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save before using heyyourecursed_resetgift.", StardewModdingAPI.LogLevel.Warn);
            return;
        }

        Game1.player.modData.Remove(ModIdentity.SudokuGiftDayKey);
        this.Monitor.Log("Sudoku's gift limit was reset for today.", StardewModdingAPI.LogLevel.Info);
    }
}
