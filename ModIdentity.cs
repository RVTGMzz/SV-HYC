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
    public const string StageProgressMigratedKey = UniqueId + "/StageProgressMigrated";
    public const string StageClearedPrefix = UniqueId + "/SudokuStageCleared/";
    public const string StageBoardPrefix = UniqueId + "/SudokuStageBoard/";
    public const string SudokuTrustKey = UniqueId + "/SudokuTrust";
    public const string SocialInteractionDayKey = UniqueId + "/SocialInteractionDay";
    public const string WelcomeHomeDayKey = UniqueId + "/WelcomeHomeDay";
    public const string SpiritEvePlanYearKey = UniqueId + "/SpiritEvePlanYear";
    public const string SpiritEveScareModeKey = UniqueId + "/SpiritEveScareMode";
    public const string SpiritEveAftermathYearKey = UniqueId + "/SpiritEveAftermathYear";
    public const string PracticePuzzleIdKey = UniqueId + "/SudokuPractice/PuzzleId";
    public const string PracticeBoardKey = UniqueId + "/SudokuPractice/Board";
    public const string PracticeCursorKey = UniqueId + "/SudokuPractice/Cursor";
    public const string StageClearDayKey = UniqueId + "/SudokuStage/FirstClearDay";

    public const string CursedVhsItemId = UniqueId + "_CursedVHS";
    public const string CursedVhsQualifiedItemId = "(O)" + CursedVhsItemId;
    public const string CursedVhsGrantedKey = UniqueId + "/CursedVHSGranted";
    public const string CursedVhsInstalledKey = UniqueId + "/CursedVHSInstalled";
    public const string VhsOriginStorySeenKey = UniqueId + "/VhsOriginStorySeen";
    public const string PencilItemId = UniqueId + "_Pencil";
    public const string PencilQualifiedItemId = "(O)" + PencilItemId;
    public const string PencilAcceptedKey = UniqueId + "/PencilAccepted";
    public const string SaloonInvitationReceivedKey = UniqueId + "/SaloonInvitationReceived";
    public const string SaloonInviteShownDayKey = UniqueId + "/SaloonInviteShownDay";
    public const string SaloonPrologueSeenKey = UniqueId + "/SaloonPrologueSeen";
    public const string SaloonPrologueCompletedDayKey = UniqueId + "/SaloonPrologueCompletedDay";
    public const string SaloonPrologueChoiceKey = UniqueId + "/SaloonPrologueChoice";
    public const string SudokuActivatedDayKey = UniqueId + "/SudokuActivatedDay";
    public const string WizardRevealSeenKey = UniqueId + "/WizardRevealSeen";
    public const string WizardRevealChoiceKey = UniqueId + "/WizardRevealChoice";
    public const string OccultCabinetUnlockedKey = UniqueId + "/OccultCabinetUnlocked";
    public const string OccultCabinetGiftedKey = UniqueId + "/OccultCabinetGifted";
    public const string ActiveHauntingIdKey = UniqueId + "/ActiveHauntingId";
    public const string SudokuSealedKey = UniqueId + "/SudokuSealed";
    public const string SudokuSealedDayKey = UniqueId + "/SudokuSealedDay";
    public const string OccultCabinetItemId = UniqueId + "_OccultCabinet";
    public const string OccultCabinetQualifiedItemId = "(BC)" + OccultCabinetItemId;
    public const string DailySignalDayKey = UniqueId + "/DailySignalDay";
    public const string ItemTextureAsset = "Mods/" + UniqueId + "/Items";
    public const string BigCraftableTextureAsset = "Mods/" + UniqueId + "/BigCraftables";

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
            "SudokuArrivalSeen", "SudokuNpcEnabled", "FirstConversationCompleted",
            "CursedVHSGranted", "CursedVHSInstalled", "VhsOriginStorySeen", "PencilAccepted",
            "SaloonInvitationReceived", "SaloonInviteShownDay", "SaloonPrologueSeen",
            "SaloonPrologueCompletedDay", "SaloonPrologueChoice", "SudokuActivatedDay",
            "WizardRevealSeen", "WizardRevealChoice", "OccultCabinetUnlocked", "OccultCabinetGifted",
            "ActiveHauntingId", "SudokuSealed", "SudokuSealedDay", "DailySignalDay", "DailyDialogueDay",
            "SudokuSolvedCount", "SudokuTrust", "SocialInteractionDay", "WelcomeHomeDay",
            "SpiritEvePlanYear", "SpiritEveScareMode", "SpiritEveAftermathYear"
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

    public static bool HasFirstConversationCompleted(Farmer player) => player.modData.TryGetValue(FirstConversationCompletedKey, out string? value) && value == "true";
    public static bool HasDailyDialogueRunToday(Farmer player) => player.modData.TryGetValue(DailyDialogueDayKey, out string? raw) && int.TryParse(raw, out int storedDay) && storedDay == Game1.Date.TotalDays;
    public static void MarkDailyDialogueRunToday(Farmer player) => player.modData[DailyDialogueDayKey] = Game1.Date.TotalDays.ToString();
    public static int GetSudokuSolvedCount(Farmer player) => player.modData.TryGetValue(SudokuSolvedCountKey, out string? raw) && int.TryParse(raw, out int count) && count > 0 ? count : 0;
    public static int GetSudokuTrust(Farmer player) => player.modData.TryGetValue(SudokuTrustKey, out string? raw) && int.TryParse(raw, out int trust) && trust > 0 ? Math.Clamp(trust, 0, 30) : 0;

    public static bool TryGainDailyTrust(Farmer player, int amount = 1)
    {
        int day = Game1.Date.TotalDays;
        bool alreadyGained = player.modData.TryGetValue(SocialInteractionDayKey, out string? raw) && int.TryParse(raw, out int storedDay) && storedDay == day;
        if (alreadyGained) return false;
        int next = Math.Clamp(GetSudokuTrust(player) + Math.Max(1, amount), 0, 30);
        player.modData[SudokuTrustKey] = next.ToString();
        player.modData[SocialInteractionDayKey] = day.ToString();
        return true;
    }

    public static bool HasWelcomeHomeRunToday(Farmer player) => player.modData.TryGetValue(WelcomeHomeDayKey, out string? raw) && int.TryParse(raw, out int storedDay) && storedDay == Game1.Date.TotalDays;
    public static void MarkWelcomeHomeRunToday(Farmer player) => player.modData[WelcomeHomeDayKey] = Game1.Date.TotalDays.ToString();
    public static bool HasSpiritEvePlanForCurrentYear(Farmer player) => player.modData.TryGetValue(SpiritEvePlanYearKey, out string? raw) && int.TryParse(raw, out int storedYear) && storedYear == Game1.year;
    public static string GetSpiritEveScareMode(Farmer player) => HasSpiritEvePlanForCurrentYear(player) && player.modData.TryGetValue(SpiritEveScareModeKey, out string? value) ? value : "gentle";

    public static bool SetSpiritEveScareMode(Farmer player, string mode)
    {
        bool firstPlanThisYear = !HasSpiritEvePlanForCurrentYear(player);
        player.modData[SpiritEvePlanYearKey] = Game1.year.ToString();
        player.modData[SpiritEveScareModeKey] = mode;
        return firstPlanThisYear;
    }

    public static bool HasSpiritEveAftermathSeenForCurrentYear(Farmer player) => player.modData.TryGetValue(SpiritEveAftermathYearKey, out string? raw) && int.TryParse(raw, out int storedYear) && storedYear == Game1.year;
    public static void MarkSpiritEveAftermathSeen(Farmer player) => player.modData[SpiritEveAftermathYearKey] = Game1.year.ToString();
    public static bool IsCursedVhsInstalled(Farmer player) => player.modData.TryGetValue(CursedVhsInstalledKey, out string? value) && value == "true";
    public static bool HasDailySignalRunToday(Farmer player) => player.modData.TryGetValue(DailySignalDayKey, out string? raw) && int.TryParse(raw, out int storedDay) && storedDay == Game1.Date.TotalDays;
    public static void MarkDailySignalRunToday(Farmer player) => player.modData[DailySignalDayKey] = Game1.Date.TotalDays.ToString();

    public static void ClearArrivalFlags(Farmer player)
    {
        string[] suffixes =
        {
            "SudokuArrivalSeen", "SudokuNpcEnabled", "FirstConversationCompleted", "CursedVHSGranted",
            "CursedVHSInstalled", "VhsOriginStorySeen", "PencilAccepted", "SaloonInvitationReceived",
            "SaloonInviteShownDay", "SaloonPrologueSeen", "SaloonPrologueCompletedDay", "SaloonPrologueChoice",
            "SudokuActivatedDay", "WizardRevealSeen", "WizardRevealChoice", "OccultCabinetUnlocked",
            "OccultCabinetGifted", "ActiveHauntingId", "SudokuSealed", "SudokuSealedDay", "DailySignalDay",
            "DailyDialogueDay", "SudokuSolvedCount", "StageProgressMigrated", "SudokuTrust", "SocialInteractionDay",
            "WelcomeHomeDay", "SpiritEvePlanYear", "SpiritEveScareMode", "SpiritEveAftermathYear"
        };

        foreach (string suffix in suffixes)
        {
            player.modData.Remove(UniqueId + "/" + suffix);
            foreach (string legacyPrefix in LegacyPrefixes)
                player.modData.Remove(legacyPrefix + "/" + suffix);
        }

        player.modData.Remove(PracticePuzzleIdKey);
        player.modData.Remove(PracticeBoardKey);
        player.modData.Remove(PracticeCursorKey);
        player.modData.Remove(StageClearDayKey);

        string[] dynamicPrefixes =
        {
            StageClearedPrefix, StageBoardPrefix,
            LegacyUniqueId + "/SudokuStageCleared/", LegacyUniqueId + "/SudokuStageBoard/",
            OlderLegacyUniqueId + "/SudokuStageCleared/", OlderLegacyUniqueId + "/SudokuStageBoard/"
        };

        foreach (string key in player.modData.Keys.Where(k => dynamicPrefixes.Any(prefix => k.StartsWith(prefix, StringComparison.Ordinal))).ToArray())
            player.modData.Remove(key);
    }
}
