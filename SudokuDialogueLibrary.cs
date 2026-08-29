using StardewValley;

namespace HeyYoureCursed;

internal sealed class SudokuDailyDialogue
{
    public string[] Lines { get; init; } = Array.Empty<string>();
    public string Question { get; init; } = string.Empty;
    public int PortraitIndex { get; init; }
}

internal static class SudokuDialogueLibrary
{
    private static readonly SudokuDailyDialogue[][] FreshPools =
    {
        new[]
        {
            D(6, "daily.0.0.q", "daily.0.0.a", "daily.0.0.b"),
            D(1, "daily.0.1.q", "daily.0.1.a", "daily.0.1.b"),
            D(0, "daily.0.2.q", "daily.0.2.a", "daily.0.2.b"),
            D(6, "daily.0.3.q", "daily.0.3.a", "daily.0.3.b"),
            D(1, "daily.0.4.q", "daily.0.4.a", "daily.0.4.b")
        },
        new[]
        {
            D(0, "daily.1.0.q", "daily.1.0.a", "daily.1.0.b"),
            D(2, "daily.1.1.q", "daily.1.1.a", "daily.1.1.b"),
            D(5, "daily.1.2.q", "daily.1.2.a", "daily.1.2.b"),
            D(0, "daily.1.3.q", "daily.1.3.a", "daily.1.3.b"),
            D(2, "daily.1.4.q", "daily.1.4.a", "daily.1.4.b")
        },
        new[]
        {
            D(4, "daily.2.0.q", "daily.2.0.a", "daily.2.0.b"),
            D(5, "daily.2.1.q", "daily.2.1.a", "daily.2.1.b"),
            D(4, "daily.2.2.q", "daily.2.2.a", "daily.2.2.b"),
            D(3, "daily.2.3.q", "daily.2.3.a", "daily.2.3.b"),
            D(5, "daily.2.4.q", "daily.2.4.a", "daily.2.4.b")
        },
        new[]
        {
            D(3, "daily.3.0.q", "daily.3.0.a", "daily.3.0.b"),
            D(4, "daily.3.1.q", "daily.3.1.a", "daily.3.1.b"),
            D(3, "daily.3.2.q", "daily.3.2.a", "daily.3.2.b"),
            D(4, "daily.3.3.q", "daily.3.3.a", "daily.3.3.b"),
            D(3, "daily.3.4.q", "daily.3.4.a", "daily.3.4.b")
        },
        new[]
        {
            D(4, "daily.4.0.q", "daily.4.0.a", "daily.4.0.b"),
            D(3, "daily.4.1.q", "daily.4.1.a", "daily.4.1.b"),
            D(4, "daily.4.2.q", "daily.4.2.a", "daily.4.2.b")
        }
    };

    private static readonly SudokuDailyDialogue[] RepeatPool =
    {
        D(6, "daily.repeat.0.q", "daily.repeat.0.a"),
        D(0, "daily.repeat.1.q", "daily.repeat.1.a"),
        D(4, "daily.repeat.2.q", "daily.repeat.2.a"),
        D(3, "daily.repeat.3.q", "daily.repeat.3.a"),
        D(4, "daily.repeat.4.q", "daily.repeat.4.a")
    };

    public static SudokuDailyDialogue GetForToday(int solvedCount, bool repeatTalk, bool solvedToday)
    {
        int stage = solvedCount switch
        {
            <= 2 => 0,
            <= 6 => 1,
            <= 12 => 2,
            <= 17 => 3,
            _ => 4
        };

        SudokuDailyDialogue[] pool = repeatTalk
            ? new[] { RepeatPool[stage] }
            : FreshPools[stage];

        long raw = unchecked((long)Game1.uniqueIDForThisGame ^ ((long)Game1.Date.TotalDays * 104729L) ^ ((long)solvedCount * 7919L));
        int index = (int)((raw & long.MaxValue) % pool.Length);
        SudokuDailyDialogue result = pool[index];

        if (!solvedToday)
            return result;

        return new SudokuDailyDialogue
        {
            Lines = result.Lines,
            PortraitIndex = result.PortraitIndex,
            Question = ModEntry.T(stage >= 2 ? "daily.solved.question.close" : "daily.solved.question.early")
        };
    }

    private static SudokuDailyDialogue D(int portrait, string questionKey, params string[] lineKeys)
    {
        return new SudokuDailyDialogue
        {
            PortraitIndex = portrait,
            Lines = lineKeys.Select(key => ModEntry.T(key)).ToArray(),
            Question = ModEntry.T(questionKey)
        };
    }
}
