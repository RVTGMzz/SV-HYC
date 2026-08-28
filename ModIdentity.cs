using StardewValley;

namespace HeyYoureCursed;

/// <summary>Stable identity constants for Hey! You're Cursed!, with migration from early prototype IDs.</summary>
internal static class ModIdentity
{
    public const string UniqueId = "ronvotri.HeyYoureCursed";
    public const string LegacyUniqueId = "ronvotri.CursedSignal";
    public const string OlderLegacyUniqueId = "ronvotri.chuyentamlinhkoduaduocdau";

    public const string SudokuNpcId = UniqueId + "_Sudoku";
    public const string LegacySudokuNpcId = LegacyUniqueId + "_Sudoku";
    public const string OlderLegacySudokuNpcId = OlderLegacyUniqueId + "_Sudoku";

    public const string ArrivalSeenKey = UniqueId + "/SudokuArrivalSeen";
    public const string SudokuNpcEnabledKey = UniqueId + "/SudokuNpcEnabled";
    public const string FirstConversationCompletedKey = UniqueId + "/FirstConversationCompleted";
    public const string DailyDialogueDayKey = UniqueId + "/DailyDialogueDay";
    public const string SudokuSolvedCountKey = UniqueId + "/SudokuSolvedCount";

    public const string CursedVhsItemId = UniqueId + "_CursedVHS";
    public const string CursedVhsQualifiedItemId = "(O)" + CursedVhsItemId;
    public const string CursedVhsGrantedKey = UniqueId + "/CursedVHSGranted";
    public const string CursedVhsInstalledKey = UniqueId + "/CursedVHSInstalled";
    public const string DailySignalDayKey = UniqueId + "/DailySignalDay";
    public const string ItemTextureAsset = "Mods/" + UniqueId + "/Items";

    public const string DailySudokuPrefix = UniqueId + "/DailySudoku/";

    private static readonly string[] LegacyPrefixes =
    {
        LegacyUniqueId,
        OlderLegacyUniqueId
    };

    private static readonly string[] DailySudokuSuffixes =
    {
        "Day",
        "PuzzleId",
        "Board",
        "ClaimedDay"
    };

    public static bool IsSudokuNpcId(string? name)
    {
        return name == SudokuNpcId
            || name == LegacySudokuNpcId
            || name == OlderLegacySudokuNpcId;
    }

    public static bool HasArrivalBeenSeen(Farmer player)
    {
        if (player.modData.TryGetValue(ArrivalSeenKey, out string? current) && current == "true")
            return true;

        foreach (string legacyPrefix in LegacyPrefixes)
        {
            string key = legacyPrefix + "/SudokuArrivalSeen";
            if (player.modData.TryGetValue(key, out string? value) && value == "true")
            {
                player.modData[ArrivalSeenKey] = "true";
                return true;
            }
        }

        return false;
    }

    public static int MigrateLegacyPlayerData(Farmer player)
    {
        int migrated = 0;
        string[] scalarSuffixes =
        {
            "SudokuArrivalSeen",
            "SudokuNpcEnabled",
            "FirstConversationCompleted",
            "CursedVHSGranted",
            "CursedVHSInstalled",
            "DailySignalDay",
            "DailyDialogueDay",
            "SudokuSolvedCount"
        };

        foreach (string suffix in scalarSuffixes)
        {
            string newKey = UniqueId + "/" + suffix;
            if (player.modData.ContainsKey(newKey))
                continue;

            foreach (string legacyPrefix in LegacyPrefixes)
            {
                string oldKey = legacyPrefix + "/" + suffix;
                if (player.modData.TryGetValue(oldKey, out string? value))
                {
                    player.modData[newKey] = value;
                    migrated++;
                    break;
                }
            }
        }

        foreach (string suffix in DailySudokuSuffixes)
        {
            string newKey = DailySudokuPrefix + suffix;
            if (player.modData.ContainsKey(newKey))
                continue;

            foreach (string legacyPrefix in LegacyPrefixes)
            {
                string oldKey = legacyPrefix + "/DailySudoku/" + suffix;
                if (player.modData.TryGetValue(oldKey, out string? value))
                {
                    player.modData[newKey] = value;
                    migrated++;
                    break;
                }
            }
        }

        return migrated;
    }

    public static bool HasFirstConversationCompleted(Farmer player)
    {
        return player.modData.TryGetValue(FirstConversationCompletedKey, out string? value)
            && value == "true";
    }

    public static bool HasDailyDialogueRunToday(Farmer player)
    {
        int day = Game1.Date.TotalDays;
        return player.modData.TryGetValue(DailyDialogueDayKey, out string? raw)
            && int.TryParse(raw, out int storedDay)
            && storedDay == day;
    }

    public static void MarkDailyDialogueRunToday(Farmer player)
    {
        player.modData[DailyDialogueDayKey] = Game1.Date.TotalDays.ToString();
    }

    public static int GetSudokuSolvedCount(Farmer player)
    {
        return player.modData.TryGetValue(SudokuSolvedCountKey, out string? raw)
            && int.TryParse(raw, out int count)
            && count > 0
                ? count
                : 0;
    }

    public static int IncrementSudokuSolvedCount(Farmer player)
    {
        int next = GetSudokuSolvedCount(player) + 1;
        player.modData[SudokuSolvedCountKey] = next.ToString();
        return next;
    }

    public static bool IsCursedVhsInstalled(Farmer player)
    {
        return player.modData.TryGetValue(CursedVhsInstalledKey, out string? value)
            && value == "true";
    }

    public static bool HasDailySignalRunToday(Farmer player)
    {
        int day = Game1.Date.TotalDays;
        return player.modData.TryGetValue(DailySignalDayKey, out string? raw)
            && int.TryParse(raw, out int storedDay)
            && storedDay == day;
    }

    public static void MarkDailySignalRunToday(Farmer player)
    {
        player.modData[DailySignalDayKey] = Game1.Date.TotalDays.ToString();
    }

    public static void ClearArrivalFlags(Farmer player)
    {
        string[] suffixes =
        {
            "SudokuArrivalSeen",
            "SudokuNpcEnabled",
            "FirstConversationCompleted",
            "CursedVHSGranted",
            "CursedVHSInstalled",
            "DailySignalDay",
            "DailyDialogueDay",
            "SudokuSolvedCount"
        };

        foreach (string suffix in suffixes)
        {
            player.modData.Remove(UniqueId + "/" + suffix);
            foreach (string legacyPrefix in LegacyPrefixes)
                player.modData.Remove(legacyPrefix + "/" + suffix);
        }
    }
}
