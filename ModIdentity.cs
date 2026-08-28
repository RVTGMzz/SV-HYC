using StardewValley;

namespace CursedSignal;

/// <summary>Central identity constants plus one-way compatibility helpers for pre-Cursed Signal prototype saves.</summary>
internal static class ModIdentity
{
    public const string UniqueId = "ronvotri.CursedSignal";
    public const string LegacyUniqueId = "ronvotri.chuyentamlinhkoduaduocdau";

    public const string SudokuNpcId = UniqueId + "_Sudoku";
    public const string LegacySudokuNpcId = LegacyUniqueId + "_Sudoku";

    public const string ArrivalSeenKey = UniqueId + "/SudokuArrivalSeen";
    public const string LegacyArrivalSeenKey = LegacyUniqueId + "/SudokuArrivalSeen";
    public const string SudokuNpcEnabledKey = UniqueId + "/SudokuNpcEnabled";

    public const string CursedVhsItemId = UniqueId + "_CursedVHS";
    public const string CursedVhsQualifiedItemId = "(O)" + CursedVhsItemId;
    public const string CursedVhsGrantedKey = UniqueId + "/CursedVHSGranted";
    public const string ItemTextureAsset = "Mods/" + UniqueId + "/Items";

    public const string DailySudokuPrefix = UniqueId + "/DailySudoku/";
    public const string LegacyDailySudokuPrefix = LegacyUniqueId + "/DailySudoku/";

    private static readonly string[] DailySudokuSuffixes =
    {
        "Day",
        "PuzzleId",
        "Board",
        "ClaimedDay"
    };

    public static bool IsSudokuNpcId(string? name)
    {
        return name == SudokuNpcId || name == LegacySudokuNpcId;
    }

    public static bool HasArrivalBeenSeen(Farmer player)
    {
        if (player.modData.TryGetValue(ArrivalSeenKey, out string? current) && current == "true")
            return true;

        if (player.modData.TryGetValue(LegacyArrivalSeenKey, out string? legacy) && legacy == "true")
        {
            player.modData[ArrivalSeenKey] = "true";
            return true;
        }

        return false;
    }

    /// <summary>
    /// Copies legacy prototype state into the Cursed Signal keyspace without deleting the old keys.
    /// Keeping the old keys makes rollback to an older dev branch safer while the mod is still untested.
    /// </summary>
    public static int MigrateLegacyPlayerData(Farmer player)
    {
        int migrated = 0;

        if (!player.modData.ContainsKey(ArrivalSeenKey)
            && player.modData.TryGetValue(LegacyArrivalSeenKey, out string? arrival))
        {
            player.modData[ArrivalSeenKey] = arrival;
            migrated++;
        }

        foreach (string suffix in DailySudokuSuffixes)
        {
            string newKey = DailySudokuPrefix + suffix;
            string oldKey = LegacyDailySudokuPrefix + suffix;

            if (!player.modData.ContainsKey(newKey)
                && player.modData.TryGetValue(oldKey, out string? value))
            {
                player.modData[newKey] = value;
                migrated++;
            }
        }

        return migrated;
    }

    public static void ClearArrivalFlags(Farmer player)
    {
        player.modData.Remove(ArrivalSeenKey);
        player.modData.Remove(LegacyArrivalSeenKey);
        player.modData.Remove(SudokuNpcEnabledKey);
        player.modData.Remove(CursedVhsGrantedKey);
    }
}
