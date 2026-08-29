using StardewValley;

namespace HeyYoureCursed;

internal sealed class SudokuRoommateDialogue
{
    public string[] Lines { get; init; } = Array.Empty<string>();
    public int PortraitIndex { get; init; }
}

/// <summary>
/// Natural roommate dialogue, deliberately separate from puzzle progression.
/// Stage clears represent Puzzle Bond; Trust represents ordinary time spent together.
/// </summary>
internal static class SudokuRoommateLibrary
{
    private static readonly SudokuRoommateDialogue[][] TalkPools =
    {
        new[]
        {
            D(6, "roommate.talk.0.0.a", "roommate.talk.0.0.b"),
            D(1, "roommate.talk.0.1.a", "roommate.talk.0.1.b"),
            D(0, "roommate.talk.0.2.a", "roommate.talk.0.2.b"),
            D(6, "roommate.talk.0.3.a", "roommate.talk.0.3.b")
        },
        new[]
        {
            D(0, "roommate.talk.1.0.a", "roommate.talk.1.0.b"),
            D(2, "roommate.talk.1.1.a", "roommate.talk.1.1.b"),
            D(5, "roommate.talk.1.2.a", "roommate.talk.1.2.b"),
            D(0, "roommate.talk.1.3.a", "roommate.talk.1.3.b")
        },
        new[]
        {
            D(4, "roommate.talk.2.0.a", "roommate.talk.2.0.b"),
            D(5, "roommate.talk.2.1.a", "roommate.talk.2.1.b"),
            D(4, "roommate.talk.2.2.a", "roommate.talk.2.2.b"),
            D(3, "roommate.talk.2.3.a", "roommate.talk.2.3.b")
        },
        new[]
        {
            D(3, "roommate.talk.3.0.a", "roommate.talk.3.0.b"),
            D(4, "roommate.talk.3.1.a", "roommate.talk.3.1.b"),
            D(3, "roommate.talk.3.2.a", "roommate.talk.3.2.b"),
            D(4, "roommate.talk.3.3.a", "roommate.talk.3.3.b")
        },
        new[]
        {
            D(4, "roommate.talk.4.0.a", "roommate.talk.4.0.b"),
            D(3, "roommate.talk.4.1.a", "roommate.talk.4.1.b"),
            D(4, "roommate.talk.4.2.a", "roommate.talk.4.2.b"),
            D(3, "roommate.talk.4.3.a", "roommate.talk.4.3.b")
        }
    };

    private static readonly Dictionary<SudokuActivityKind, SudokuRoommateDialogue[]> ActivityPools = new()
    {
        [SudokuActivityKind.TvWatch] = new[]
        {
            D(1, "roommate.activity.tv.0.a", "roommate.activity.tv.0.b"),
            D(0, "roommate.activity.tv.1.a", "roommate.activity.tv.1.b"),
            D(4, "roommate.activity.tv.2.a", "roommate.activity.tv.2.b")
        },
        [SudokuActivityKind.FurnitureWatch] = new[]
        {
            D(0, "roommate.activity.furniture.0.a", "roommate.activity.furniture.0.b"),
            D(5, "roommate.activity.furniture.1.a", "roommate.activity.furniture.1.b"),
            D(4, "roommate.activity.furniture.2.a", "roommate.activity.furniture.2.b")
        },
        [SudokuActivityKind.PetWatch] = new[]
        {
            D(2, "roommate.activity.pet.0.a", "roommate.activity.pet.0.b"),
            D(4, "roommate.activity.pet.1.a", "roommate.activity.pet.1.b"),
            D(3, "roommate.activity.pet.2.a", "roommate.activity.pet.2.b")
        },
        [SudokuActivityKind.HouseListening] = new[]
        {
            D(6, "roommate.activity.listen.0.a", "roommate.activity.listen.0.b"),
            D(0, "roommate.activity.listen.1.a", "roommate.activity.listen.1.b"),
            D(4, "roommate.activity.listen.2.a", "roommate.activity.listen.2.b")
        },
        [SudokuActivityKind.DoorWatch] = new[]
        {
            D(5, "roommate.activity.door.0.a", "roommate.activity.door.0.b"),
            D(0, "roommate.activity.door.1.a", "roommate.activity.door.1.b"),
            D(3, "roommate.activity.door.2.a", "roommate.activity.door.2.b")
        },
        [SudokuActivityKind.QuietCorner] = new[]
        {
            D(6, "roommate.activity.corner.0.a", "roommate.activity.corner.0.b"),
            D(1, "roommate.activity.corner.1.a", "roommate.activity.corner.1.b"),
            D(0, "roommate.activity.corner.2.a", "roommate.activity.corner.2.b")
        },
        [SudokuActivityKind.WaitingForPlayer] = new[]
        {
            D(4, "roommate.activity.wait.0.a", "roommate.activity.wait.0.b"),
            D(3, "roommate.activity.wait.1.a", "roommate.activity.wait.1.b"),
            D(4, "roommate.activity.wait.2.a", "roommate.activity.wait.2.b")
        }
    };

    private static readonly SudokuRoommateDialogue[] StrangePool =
    {
        D(6, "roommate.strange.0.a", "roommate.strange.0.b"),
        D(1, "roommate.strange.1.a", "roommate.strange.1.b"),
        D(0, "roommate.strange.2.a", "roommate.strange.2.b"),
        D(5, "roommate.strange.3.a", "roommate.strange.3.b"),
        D(2, "roommate.strange.4.a", "roommate.strange.4.b")
    };

    public static SudokuRoommateDialogue GetNaturalTalk(int trust, int stageClears)
    {
        int level = GetTrustLevel(trust);
        SudokuRoommateDialogue[] pool = TalkPools[level];
        return Pick(pool, 0x51A7L + stageClears * 17L + trust * 101L);
    }

    public static SudokuRoommateDialogue GetActivity(SudokuActivityKind activity, int trust)
    {
        SudokuRoommateDialogue[] pool = ActivityPools.TryGetValue(activity, out SudokuRoommateDialogue[]? found)
            ? found
            : ActivityPools[SudokuActivityKind.HouseListening];

        return Pick(pool, 0xA671L + trust * 37L + (int)activity * 997L + Game1.timeOfDay);
    }

    public static SudokuRoommateDialogue GetStrangeThing(int trust)
    {
        return Pick(StrangePool, 0x6A057L + trust * 53L);
    }

    public static string GetWelcomeHomeLine(int trust)
    {
        string[] lines = trust switch
        {
            <= 6 => new[]
            {
                ModEntry.T("roommate.welcome.0.0"),
                ModEntry.T("roommate.welcome.0.1")
            },
            <= 12 => new[]
            {
                ModEntry.T("roommate.welcome.1.0"),
                ModEntry.T("roommate.welcome.1.1")
            },
            <= 20 => new[]
            {
                ModEntry.T("roommate.welcome.2.0"),
                ModEntry.T("roommate.welcome.2.1")
            },
            _ => new[]
            {
                ModEntry.T("roommate.welcome.3.0"),
                ModEntry.T("roommate.welcome.3.1")
            }
        };

        long raw = unchecked((long)Game1.uniqueIDForThisGame ^ ((long)Game1.Date.TotalDays * 65537L) ^ trust * 313L);
        int index = (int)((raw & long.MaxValue) % lines.Length);
        return lines[index];
    }

    public static string GetActivityLabel(SudokuActivityKind activity)
    {
        string key = activity switch
        {
            SudokuActivityKind.TvWatch => "roommate.activity-label.tv",
            SudokuActivityKind.FurnitureWatch => "roommate.activity-label.furniture",
            SudokuActivityKind.PetWatch => "roommate.activity-label.pet",
            SudokuActivityKind.HouseListening => "roommate.activity-label.listen",
            SudokuActivityKind.DoorWatch => "roommate.activity-label.door",
            SudokuActivityKind.QuietCorner => "roommate.activity-label.corner",
            SudokuActivityKind.WaitingForPlayer => "roommate.activity-label.wait",
            _ => "roommate.activity-label.home"
        };
        return ModEntry.T(key);
    }

    public static string GetTrustLabel(int trust)
    {
        string key = trust switch
        {
            <= 2 => "roommate.trust.stranger",
            <= 6 => "roommate.trust.warming",
            <= 12 => "roommate.trust.familiar",
            <= 20 => "roommate.trust.trusted",
            _ => "roommate.trust.family"
        };
        return ModEntry.T(key);
    }

    private static int GetTrustLevel(int trust)
    {
        return trust switch
        {
            <= 2 => 0,
            <= 6 => 1,
            <= 12 => 2,
            <= 20 => 3,
            _ => 4
        };
    }

    private static SudokuRoommateDialogue Pick(IReadOnlyList<SudokuRoommateDialogue> pool, long salt)
    {
        long raw = unchecked((long)Game1.uniqueIDForThisGame ^ ((long)Game1.Date.TotalDays * 104729L) ^ salt);
        int index = (int)((raw & long.MaxValue) % pool.Count);
        return pool[index];
    }

    private static SudokuRoommateDialogue D(int portrait, params string[] keys)
    {
        return new SudokuRoommateDialogue
        {
            PortraitIndex = portrait,
            Lines = keys.Select(key => ModEntry.T(key)).ToArray()
        };
    }
}
