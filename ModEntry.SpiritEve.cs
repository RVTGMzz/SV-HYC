using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace HeyYoureCursed;

internal sealed partial class ModEntry
{
    private bool IsSpiritEveToday()
    {
        return Context.IsWorldReady
            && Game1.currentSeason.Equals("fall", StringComparison.OrdinalIgnoreCase)
            && Game1.dayOfMonth == 27;
    }

    private bool IsAtSpiritEveFestival()
    {
        return this.IsSpiritEveToday()
            && Game1.timeOfDay >= 2200
            && Game1.currentLocation is not null
            && Game1.currentLocation.NameOrUniqueName.StartsWith("Town", StringComparison.OrdinalIgnoreCase);
    }

    private bool TryConsumeSpiritEveAftermath(out string[] lines, out int portraitIndex)
    {
        lines = Array.Empty<string>();
        portraitIndex = 4;

        if (!Context.IsWorldReady
            || this.IsSudokuSealed()
            || !Game1.currentSeason.Equals("fall", StringComparison.OrdinalIgnoreCase)
            || Game1.dayOfMonth != 28
            || !ModIdentity.HasSpiritEvePlanForCurrentYear(Game1.player)
            || ModIdentity.HasSpiritEveAftermathSeenForCurrentYear(Game1.player))
        {
            return false;
        }

        string mode = ModIdentity.GetSpiritEveScareMode(Game1.player);
        lines = mode switch
        {
            "free" => new[] { T("spiriteve.aftermath.free.1"), T("spiriteve.aftermath.free.2") },
            "lewis" => new[] { T("spiriteve.aftermath.lewis.1"), T("spiriteve.aftermath.lewis.2") },
            _ => new[] { T("spiriteve.aftermath.gentle.1"), T("spiriteve.aftermath.gentle.2") }
        };

        portraitIndex = mode == "free" ? 3 : 4;
        ModIdentity.MarkSpiritEveAftermathSeen(Game1.player);
        return true;
    }

    private void ShowSpiritEvePlan()
    {
        if (!Context.IsWorldReady || this.IsSudokuSealed())
            return;

        Texture2D? portraits = this.LoadSudokuPortraitTexture();
        string currentMode = ModIdentity.GetSpiritEveScareMode(Game1.player);
        string current = ModIdentity.HasSpiritEvePlanForCurrentYear(Game1.player)
            ? T("spiriteve.current", new { mode = GetScareModeLabel(currentMode) })
            : T("spiriteve.none");

        Game1.activeClickableMenu = new SudokuChoiceMenu(
            portraits,
            portraitIndex: 4,
            lines: new[] { T("spiriteve.plan.1"), T("spiriteve.plan.2"), T("spiriteve.plan.3") },
            question: T("spiriteve.question"),
            progressText: current,
            options: new[]
            {
                SpiritOption(T("spiriteve.option.gentle"), "gentle"),
                SpiritOption(T("spiriteve.option.free"), "free"),
                SpiritOption(T("spiriteve.option.lewis"), "lewis"),
                new SudokuChoiceOption { Label = T("ui.common.later"), Action = () => this.ShowSudokuInteractionHub() }
            }
        );
    }

    private SudokuChoiceOption SpiritOption(string label, string mode)
    {
        return new SudokuChoiceOption { Label = label, Action = () => this.ApplySpiritEvePlan(mode) };
    }

    private void ApplySpiritEvePlan(string mode)
    {
        bool firstPlan = ModIdentity.SetSpiritEveScareMode(Game1.player, mode);
        if (firstPlan)
        {
            int currentTrust = ModIdentity.GetSudokuTrust(Game1.player);
            Game1.player.modData[ModIdentity.SudokuTrustKey] = Math.Clamp(currentTrust + 2, 0, 30).ToString();
        }

        this.Helper.GameContent.InvalidateCache("Data/Festivals/fall27");
        string[] response = mode switch
        {
            "free" => new[] { T("spiriteve.response.free.1"), T("spiriteve.response.free.2") },
            "lewis" => new[] { T("spiriteve.response.lewis.1"), T("spiriteve.response.lewis.2") },
            _ => new[] { T("spiriteve.response.gentle.1"), T("spiriteve.response.gentle.2") }
        };

        this.ShowSimpleSudokuDialogue(mode == "free" ? 6 : 4, response, T("spiriteve.progress"), true);
    }

    private void EditSpiritEveFestival(IDictionary<string, string> data)
    {
        if (!Context.IsWorldReady
            || !ModIdentity.HasArrivalBeenSeen(Game1.player)
            || !ModIdentity.HasFirstConversationCompleted(Game1.player)
            || (this.IsOccultCabinetUnlocked() && !this.IsSudokuActiveHaunting())
            || !ModIdentity.HasSpiritEvePlanForCurrentYear(Game1.player))
        {
            return;
        }

        bool yearTwoVariant = Game1.year % 2 == 0;
        string setupKey = yearTwoVariant ? "Set-Up_additionalCharacters_y2" : "Set-Up_additionalCharacters";
        string dialogueSuffix = yearTwoVariant ? "_y2" : string.Empty;
        bool sve = this.Helper.ModRegistry.IsLoaded("FlashShifter.StardewValleyExpandedCP");
        string placement = sve ? "47 88 up" : "67 36 up";
        AppendSlashEntry(data, setupKey, $"{ModIdentity.SudokuNpcId} {placement}");

        string mode = ModIdentity.GetSpiritEveScareMode(Game1.player);
        string sudokuKey = ModIdentity.SudokuNpcId + dialogueSuffix;
        data[sudokuKey] = mode switch
        {
            "free" => T("spiriteve.festival.sudoku.free"),
            "lewis" => T("spiriteve.festival.sudoku.lewis"),
            _ => T("spiriteve.festival.sudoku.gentle")
        };

        AppendFestivalDialogue(data, "Abigail" + dialogueSuffix, T("spiriteve.festival.abigail"));
        AppendFestivalDialogue(data, "Sebastian" + dialogueSuffix, T("spiriteve.festival.sebastian"));
        AppendFestivalDialogue(data, "Wizard" + dialogueSuffix, T("spiriteve.festival.wizard"));
        AppendFestivalDialogue(data, "Krobus" + dialogueSuffix, T("spiriteve.festival.krobus"));

        string lewisLine = mode switch
        {
            "free" => T("spiriteve.festival.lewis.free"),
            "lewis" => T("spiriteve.festival.lewis.lewis"),
            _ => T("spiriteve.festival.lewis.gentle")
        };
        AppendFestivalDialogue(data, "Lewis" + dialogueSuffix, lewisLine);
    }

    private static void AppendSlashEntry(IDictionary<string, string> data, string key, string value)
    {
        if (!data.TryGetValue(key, out string? existing) || string.IsNullOrWhiteSpace(existing))
        {
            data[key] = value;
            return;
        }
        if (existing.Contains(ModIdentity.SudokuNpcId, StringComparison.OrdinalIgnoreCase)) return;
        data[key] = existing.TrimEnd('/') + "/" + value;
    }

    private static void AppendFestivalDialogue(IDictionary<string, string> data, string key, string extraLine)
    {
        if (!data.TryGetValue(key, out string? existing) || string.IsNullOrWhiteSpace(existing))
        {
            data[key] = extraLine;
            return;
        }
        if (existing.Contains(extraLine, StringComparison.Ordinal)) return;
        data[key] = existing + "#$e#" + extraLine;
    }

    private static string GetScareModeLabel(string mode)
    {
        return mode switch
        {
            "free" => T("spiriteve.mode.free"),
            "lewis" => T("spiriteve.mode.lewis"),
            _ => T("spiriteve.mode.gentle")
        };
    }
}
