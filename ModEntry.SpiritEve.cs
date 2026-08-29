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
            "free" => new[]
            {
                "\"Người sống la rất to khi bị dọa.\"",
                "Sudoku im lặng một lúc. \"...Năm sau ta đi nữa được không?\""
            },
            "lewis" => new[]
            {
                "\"Lewis tránh ta đến tận cuối lễ hội.\"",
                "\"...Ta nghĩ ông ấy vẫn còn tỉnh. Năm sau ta đi nữa được không?\""
            },
            _ => new[]
            {
                "\"Người sống rất lạ.\"",
                "Sudoku nhìn sang chỗ khác. \"...Năm sau ta đi nữa được không?\""
            }
        };

        portraitIndex = mode == "free" ? 3 : 4;
        ModIdentity.MarkSpiritEveAftermathSeen(Game1.player);
        return true;
    }

    private void ShowSpiritEvePlan()
    {
        if (!Context.IsWorldReady)
            return;

        Texture2D? portraits = this.LoadSudokuPortraitTexture();
        string currentMode = ModIdentity.GetSpiritEveScareMode(Game1.player);
        string current = ModIdentity.HasSpiritEvePlanForCurrentYear(Game1.player)
            ? $"Kế hoạch hiện tại: {GetScareModeLabel(currentMode)}"
            : "Sudoku chưa từng đi Spirit's Eve cùng bạn năm nay.";

        Game1.activeClickableMenu = new SudokuChoiceMenu(
            portraits,
            portraitIndex: 4,
            lines: new[]
            {
                "\"Hôm nay người sống giả làm ma?\"",
                "\"...Ta muốn đi.\"",
                "Sudoku nghiêng đầu. \"Ta được phép dọa họ chứ?\""
            },
            question: "Bạn dặn Sudoku thế nào?",
            progressText: current,
            options: new[]
            {
                SpiritOption("Nhẹ thôi.", "gentle"),
                SpiritOption("Cứ tự nhiên.", "free"),
                SpiritOption("Đừng làm Lewis ngất.", "lewis"),
                new SudokuChoiceOption { Label = "Để sau", Action = () => this.ShowSudokuInteractionHub() }
            }
        );
    }

    private SudokuChoiceOption SpiritOption(string label, string mode)
    {
        return new SudokuChoiceOption
        {
            Label = label,
            Action = () => this.ApplySpiritEvePlan(mode)
        };
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
            "free" => new[] { "Sudoku khẽ gật đầu.", "\"...Ta sẽ cố không làm ai chạy quá xa.\"" },
            "lewis" => new[] { "Sudoku im lặng vài giây.", "\"Ta không hứa về Lewis.\"" },
            _ => new[] { "\"Nhẹ thôi.\"", "Sudoku thử mỉm cười. Điều đó không giúp cô ấy bớt đáng sợ hơn." }
        };

        this.ShowSimpleSudokuDialogue(
            portraitIndex: mode == "free" ? 6 : 4,
            lines: response,
            progressText: "Spirit's Eve • Sudoku sẽ xuất hiện ở lễ hội tối nay",
            returnToHub: true
        );
    }

    private void EditSpiritEveFestival(IDictionary<string, string> data)
    {
        if (!Context.IsWorldReady
            || !ModIdentity.HasArrivalBeenSeen(Game1.player)
            || !ModIdentity.HasFirstConversationCompleted(Game1.player)
            || !ModIdentity.HasSpiritEvePlanForCurrentYear(Game1.player))
        {
            return;
        }

        bool yearTwoVariant = Game1.year % 2 == 0;
        string setupKey = yearTwoVariant ? "Set-Up_additionalCharacters_y2" : "Set-Up_additionalCharacters";
        string dialogueSuffix = yearTwoVariant ? "_y2" : string.Empty;
        bool sve = this.Helper.ModRegistry.IsLoaded("FlashShifter.StardewValleyExpandedCP");

        // Vanilla Spirit's Eve custom-NPC convention uses 67,36. SVE replaces the map, so use
        // a known open festival coordinate from its additional-character layout instead.
        string placement = sve ? "47 88 up" : "67 36 up";
        AppendSlashEntry(data, setupKey, $"{ModIdentity.SudokuNpcId} {placement}");

        string mode = ModIdentity.GetSpiritEveScareMode(Game1.player);
        string sudokuKey = ModIdentity.SudokuNpcId + dialogueSuffix;
        data[sudokuKey] = mode switch
        {
            "free" => "...Họ thật sự trả tiền để giả làm ma?$1#$e#Ta đã dọa ba người. Abigail muốn ta làm lại.$3",
            "lewis" => "Lewis đang tránh ta.$4#$e#Ta không hiểu. Ngươi chỉ bảo ta đừng làm ông ấy ngất.$1",
            _ => "...Ta đang dọa họ rất nhẹ.$4#$e#Abigail nói ta có 'costume' đẹp. Ta chưa sửa cô ấy.$1"
        };

        AppendFestivalDialogue(data, "Abigail" + dialogueSuffix,
            "Cô gái đi cùng cậu có bộ đồ đáng sợ thật! ...Khoan, đó là bộ đồ thật à?$1");
        AppendFestivalDialogue(data, "Sebastian" + dialogueSuffix,
            "Sudoku không nói nhiều. Tôi thấy vậy khá dễ chịu.$0");
        AppendFestivalDialogue(data, "Wizard" + dialogueSuffix,
            "Người bạn mới của con không thuộc về cõi của chúng ta. Ta nghĩ con đã biết điều đó.$0");
        AppendFestivalDialogue(data, "Krobus" + dialogueSuffix,
            "Người bạn tóc đen của cậu rất yên tĩnh. Tôi thích cô ấy.$1");

        string lewisLine = mode switch
        {
            "free" => "Cô gái đi cùng cậu vừa xuất hiện sau lưng ta ba lần. Ba lần, @!$2",
            "lewis" => "@... làm ơn nói với bạn của cậu rằng đứng im sau lưng thị trưởng không phải trò lễ hội.$2",
            _ => "Bạn của cậu có... tinh thần lễ hội rất thuyết phục. Rất, rất thuyết phục.$2"
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

        if (existing.Contains(ModIdentity.SudokuNpcId, StringComparison.OrdinalIgnoreCase))
            return;

        data[key] = existing.TrimEnd('/') + "/" + value;
    }

    private static void AppendFestivalDialogue(IDictionary<string, string> data, string key, string extraLine)
    {
        if (!data.TryGetValue(key, out string? existing) || string.IsNullOrWhiteSpace(existing))
        {
            data[key] = extraLine;
            return;
        }

        if (existing.Contains(extraLine, StringComparison.Ordinal))
            return;

        data[key] = existing + "#$e#" + extraLine;
    }

    private static string GetScareModeLabel(string mode)
    {
        return mode switch
        {
            "free" => "Cứ tự nhiên",
            "lewis" => "Đừng làm Lewis ngất",
            _ => "Nhẹ thôi"
        };
    }
}
