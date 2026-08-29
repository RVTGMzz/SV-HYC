using StardewModdingAPI;
using StardewValley;

namespace HeyYoureCursed;

internal sealed class DailySudokuService
{
    private const string DayKey = ModIdentity.DailySudokuPrefix + "Day";
    private const string PuzzleIdKey = ModIdentity.DailySudokuPrefix + "PuzzleId";
    private const string BoardKey = ModIdentity.DailySudokuPrefix + "Board";
    private const string ClaimedDayKey = ModIdentity.DailySudokuPrefix + "ClaimedDay";

    private readonly IMonitor monitor;
    private readonly ModConfig config;
    private readonly List<SudokuPuzzle> puzzles;
    private readonly List<SudokuPuzzle> stages;
    private readonly List<SudokuRewardEntry> rewards;

    public DailySudokuService(IModHelper helper, IMonitor monitor, ModConfig config)
    {
        this.monitor = monitor;
        this.config = config;

        this.puzzles = helper.Data.ReadJsonFile<List<SudokuPuzzle>>(
            "assets/Data/Sudoku.puzzles.json"
        ) ?? new List<SudokuPuzzle>();

        this.puzzles = this.puzzles
            .Where(p => p.Puzzle.Length == 81 && p.Solution.Length == 81)
            .ToList();

        this.stages = this.puzzles
            .OrderBy(p => DifficultyRank(p.Difficulty))
            .ThenBy(p => p.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();

        this.rewards = helper.Data.ReadJsonFile<List<SudokuRewardEntry>>(
            "assets/Data/Sudoku.rewards.json"
        ) ?? new List<SudokuRewardEntry>();

        this.rewards = this.rewards
            .Where(p =>
                !string.IsNullOrWhiteSpace(p.QualifiedItemId)
                && p.MinStack > 0
                && p.MaxStack >= p.MinStack
                && p.Weight > 0
            )
            .ToList();

        if (this.puzzles.Count == 0)
            this.monitor.Log("Daily Sudoku puzzle bank is empty or invalid.", LogLevel.Error);

        if (this.rewards.Count == 0)
            this.monitor.Log("Daily Sudoku item reward pool is empty or invalid; gold fallback will be used.", LogLevel.Warn);
    }

    public int GetStageCount() => this.stages.Count;

    public SudokuPuzzle? GetStage(int stageIndex)
    {
        this.EnsureStageProgressMigration();

        return stageIndex >= 0 && stageIndex < this.stages.Count
            ? this.stages[stageIndex]
            : null;
    }

    public IReadOnlyList<SudokuPuzzle> GetStages()
    {
        this.EnsureStageProgressMigration();
        return this.stages;
    }

    public bool IsStageUnlocked(int stageIndex)
    {
        this.EnsureStageProgressMigration();

        if (stageIndex < 0 || stageIndex >= this.stages.Count)
            return false;

        return stageIndex == 0 || this.IsStageCleared(stageIndex - 1);
    }

    public bool IsStageCleared(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= this.stages.Count || !Context.IsWorldReady)
            return false;

        return IsTrue(Game1.player, ModIdentity.StageClearedPrefix + this.stages[stageIndex].Id);
    }

    public bool IsStageCleared(string puzzleId)
    {
        if (!Context.IsWorldReady)
            return false;

        return IsTrue(Game1.player, ModIdentity.StageClearedPrefix + puzzleId);
    }

    public void PrepareStageForPlay(int stageIndex)
    {
        if (!Context.IsWorldReady || stageIndex < 0 || stageIndex >= this.stages.Count)
            return;

        SudokuPuzzle puzzle = this.stages[stageIndex];
        string boardKey = ModIdentity.StageBoardPrefix + puzzle.Id;

        // Unfinished Stages resume from their saved board. Cleared Stages replay from a fresh
        // copy so "play again" is an actual puzzle, not an already-completed grid.
        if (this.IsStageCleared(stageIndex))
        {
            Game1.player.modData[boardKey] = puzzle.Puzzle;
            return;
        }

        if (!Game1.player.modData.TryGetValue(boardKey, out string? board) || board.Length != 81)
            Game1.player.modData[boardKey] = puzzle.Puzzle;
    }

    public int GetSolvedCount()
    {
        this.EnsureStageProgressMigration();

        if (!Context.IsWorldReady)
            return 0;

        return this.stages.Count(p => IsTrue(Game1.player, ModIdentity.StageClearedPrefix + p.Id));
    }

    public bool IsEndlessPracticeUnlocked()
    {
        return this.stages.Count > 0 && this.GetSolvedCount() >= this.stages.Count;
    }

    public SudokuPuzzle? PrepareNextPracticePuzzle()
    {
        if (!Context.IsWorldReady || this.stages.Count == 0 || !this.IsEndlessPracticeUnlocked())
            return null;

        int cursor = 0;
        if (Game1.player.modData.TryGetValue(ModIdentity.PracticeCursorKey, out string? raw))
            int.TryParse(raw, out cursor);

        long seed = unchecked((long)Game1.uniqueIDForThisGame ^ 0x5EEDBEEFL);
        int offset = (int)((seed & long.MaxValue) % this.stages.Count);
        int index = (offset + Math.Abs(cursor)) % this.stages.Count;
        SudokuPuzzle puzzle = this.stages[index];

        Game1.player.modData[ModIdentity.PracticeCursorKey] = (cursor + 1).ToString();
        Game1.player.modData[ModIdentity.PracticePuzzleIdKey] = puzzle.Id;
        Game1.player.modData[ModIdentity.PracticeBoardKey] = puzzle.Puzzle;
        return puzzle;
    }

    public SudokuPuzzle? EnsureToday()
    {
        if (!Context.IsWorldReady || this.puzzles.Count == 0)
            return null;

        ModIdentity.MigrateLegacyPlayerData(Game1.player);
        this.EnsureStageProgressMigration();

        int day = Game1.Date.TotalDays;
        bool sameDay =
            Game1.player.modData.TryGetValue(DayKey, out string? dayRaw)
            && int.TryParse(dayRaw, out int storedDay)
            && storedDay == day;

        // Once today's challenge has been chosen, keep that exact puzzle for the entire day.
        // Clearing a Stage can raise the player's difficulty tier, but it must not replace an
        // in-progress Daily Challenge until tomorrow.
        SudokuPuzzle? storedPuzzle = null;
        if (sameDay && Game1.player.modData.TryGetValue(PuzzleIdKey, out string? storedId))
        {
            storedPuzzle = this.puzzles.FirstOrDefault(p =>
                p.Id.Equals(storedId, StringComparison.OrdinalIgnoreCase)
            );
        }

        SudokuPuzzle puzzle = storedPuzzle ?? this.SelectPuzzleForToday(day);

        if (!sameDay || storedPuzzle is null)
        {
            Game1.player.modData[DayKey] = day.ToString();
            Game1.player.modData[PuzzleIdKey] = puzzle.Id;
            Game1.player.modData[BoardKey] = puzzle.Puzzle;
        }
        else if (!Game1.player.modData.TryGetValue(BoardKey, out string? board) || board.Length != 81)
        {
            Game1.player.modData[BoardKey] = puzzle.Puzzle;
        }

        return puzzle;
    }

    public string GetBoard(SudokuPuzzle puzzle, SudokuPlayMode mode)
    {
        if (!Context.IsWorldReady)
            return puzzle.Puzzle;

        if (mode == SudokuPlayMode.Stage)
        {
            string key = ModIdentity.StageBoardPrefix + puzzle.Id;
            if (Game1.player.modData.TryGetValue(key, out string? board) && board.Length == 81)
                return board;

            Game1.player.modData[key] = puzzle.Puzzle;
            return puzzle.Puzzle;
        }

        if (mode == SudokuPlayMode.Practice)
        {
            bool samePuzzle = Game1.player.modData.TryGetValue(ModIdentity.PracticePuzzleIdKey, out string? practiceId)
                && practiceId.Equals(puzzle.Id, StringComparison.OrdinalIgnoreCase);

            if (samePuzzle
                && Game1.player.modData.TryGetValue(ModIdentity.PracticeBoardKey, out string? practiceBoard)
                && practiceBoard.Length == 81)
            {
                return practiceBoard;
            }

            Game1.player.modData[ModIdentity.PracticePuzzleIdKey] = puzzle.Id;
            Game1.player.modData[ModIdentity.PracticeBoardKey] = puzzle.Puzzle;
            return puzzle.Puzzle;
        }

        this.EnsureToday();

        if (Game1.player.modData.TryGetValue(BoardKey, out string? dailyBoard) && dailyBoard.Length == 81)
            return dailyBoard;

        return puzzle.Puzzle;
    }

    public void SetCell(SudokuPuzzle puzzle, SudokuPlayMode mode, int row, int column, int value)
    {
        if (!Context.IsWorldReady || row < 0 || row > 8 || column < 0 || column > 8 || value < 0 || value > 9)
            return;

        int index = row * 9 + column;
        if (puzzle.Puzzle[index] != '0')
            return;

        char[] board = this.GetBoard(puzzle, mode).ToCharArray();
        board[index] = value == 0 ? '0' : (char)('0' + value);

        string key = mode switch
        {
            SudokuPlayMode.Stage => ModIdentity.StageBoardPrefix + puzzle.Id,
            SudokuPlayMode.Practice => ModIdentity.PracticeBoardKey,
            _ => BoardKey
        };

        if (mode == SudokuPlayMode.Practice)
            Game1.player.modData[ModIdentity.PracticePuzzleIdKey] = puzzle.Id;

        Game1.player.modData[key] = new string(board);
    }

    public bool IsSolved(SudokuPuzzle puzzle, SudokuPlayMode mode)
    {
        return this.GetBoard(puzzle, mode) == puzzle.Solution;
    }

    public bool IsRewardClaimedToday()
    {
        if (!Context.IsWorldReady)
            return false;

        ModIdentity.MigrateLegacyPlayerData(Game1.player);

        int day = Game1.Date.TotalDays;
        return Game1.player.modData.TryGetValue(ClaimedDayKey, out string? raw)
            && int.TryParse(raw, out int claimedDay)
            && claimedDay == day;
    }

    /// <summary>Claims the reward for today's Daily Challenge. Stage clears never grant this reward.</summary>
    public string? ClaimDailyReward(SudokuPuzzle puzzle)
    {
        if (!this.IsSolved(puzzle, SudokuPlayMode.DailyChallenge) || this.IsRewardClaimedToday())
            return null;

        string rewardDescription;

        try
        {
            SudokuRewardEntry? selected = this.SelectRewardForToday(puzzle);
            if (selected is null)
            {
                rewardDescription = this.GrantFallbackGold(puzzle);
            }
            else
            {
                int day = Game1.Date.TotalDays;
                long seed64 = unchecked((long)Game1.uniqueIDForThisGame ^ ((long)day * 486187739L) ^ 0x5A17BEEFL);
                int seed = unchecked((int)seed64);
                Random random = new(seed);
                int stack = random.Next(selected.MinStack, selected.MaxStack + 1);

                Item reward = ItemRegistry.Create(selected.QualifiedItemId, stack);
                Game1.player.addItemByMenuIfNecessary(reward);
                rewardDescription = stack > 1
                    ? $"{stack}× {reward.DisplayName}"
                    : reward.DisplayName;
            }
        }
        catch (Exception ex)
        {
            this.monitor.Log(
                $"Couldn't create today's Daily Sudoku item reward; using gold fallback instead. {ex}",
                LogLevel.Warn
            );
            rewardDescription = this.GrantFallbackGold(puzzle);
        }

        Game1.player.modData[ClaimedDayKey] = Game1.Date.TotalDays.ToString();

        this.monitor.Log(
            $"Daily Sudoku solved: {puzzle.Id} ({puzzle.Difficulty}), reward={rewardDescription}. Stage bond progress is unchanged.",
            LogLevel.Info
        );

        return rewardDescription;
    }

    /// <summary>
    /// Marks a Stage as cleared. Returns true only on the first clear of that unique Stage.
    /// </summary>
    public bool CompleteStage(SudokuPuzzle puzzle)
    {
        if (!Context.IsWorldReady || !this.IsSolved(puzzle, SudokuPlayMode.Stage))
            return false;

        int stageIndex = this.stages.FindIndex(p => p.Id.Equals(puzzle.Id, StringComparison.OrdinalIgnoreCase));
        if (stageIndex < 0)
            return false;

        string key = ModIdentity.StageClearedPrefix + puzzle.Id;
        bool firstClear = !IsTrue(Game1.player, key);

        if (firstClear)
        {
            Game1.player.modData[key] = "true";
            int uniqueClears = this.GetSolvedCount();

            Game1.player.modData[ModIdentity.SudokuSolvedCountKey] = uniqueClears.ToString();

            this.monitor.Log(
                $"Sudoku Stage cleared: index={stageIndex + 1}/{this.stages.Count}, puzzle={puzzle.Id}, uniqueClears={uniqueClears}.",
                LogLevel.Info
            );
        }

        return firstClear;
    }

    public void ResetTodayForTesting()
    {
        if (!Context.IsWorldReady)
            return;

        string[] suffixes = { "Day", "PuzzleId", "Board", "ClaimedDay" };

        foreach (string suffix in suffixes)
            Game1.player.modData.Remove(ModIdentity.DailySudokuPrefix + suffix);

        Game1.player.modData.Remove(ModIdentity.DailyDialogueDayKey);
        Game1.player.modData.Remove(ModIdentity.SocialInteractionDayKey);
    }

    public void ResetStageProgressForTesting()
    {
        if (!Context.IsWorldReady)
            return;

        foreach (string key in Game1.player.modData.Keys
                     .Where(p => p.StartsWith(ModIdentity.StageClearedPrefix, StringComparison.Ordinal)
                              || p.StartsWith(ModIdentity.StageBoardPrefix, StringComparison.Ordinal))
                     .ToArray())
        {
            Game1.player.modData.Remove(key);
        }

        Game1.player.modData.Remove(ModIdentity.StageProgressMigratedKey);
        Game1.player.modData.Remove(ModIdentity.PracticePuzzleIdKey);
        Game1.player.modData.Remove(ModIdentity.PracticeBoardKey);
        Game1.player.modData.Remove(ModIdentity.PracticeCursorKey);
        Game1.player.modData[ModIdentity.SudokuSolvedCountKey] = "0";
    }

    private void EnsureStageProgressMigration()
    {
        if (!Context.IsWorldReady)
            return;

        ModIdentity.MigrateLegacyPlayerData(Game1.player);

        if (IsTrue(Game1.player, ModIdentity.StageProgressMigratedKey))
            return;

        bool alreadyHasStageData = this.stages.Any(p =>
            Game1.player.modData.ContainsKey(ModIdentity.StageClearedPrefix + p.Id)
            || Game1.player.modData.ContainsKey(ModIdentity.StageBoardPrefix + p.Id)
        );

        int legacySolvedCount = ModIdentity.GetSudokuSolvedCount(Game1.player);

        if (!alreadyHasStageData && legacySolvedCount > 0)
        {
            int migrated = Math.Min(legacySolvedCount, this.stages.Count);
            for (int i = 0; i < migrated; i++)
                Game1.player.modData[ModIdentity.StageClearedPrefix + this.stages[i].Id] = "true";

            this.monitor.Log(
                $"Migrated alpha.12 Sudoku progress into Stage progression: {migrated}/{this.stages.Count} early Stages marked clear.",
                LogLevel.Info
            );
        }

        int uniqueClears = this.stages.Count(p =>
            IsTrue(Game1.player, ModIdentity.StageClearedPrefix + p.Id)
        );

        Game1.player.modData[ModIdentity.SudokuSolvedCountKey] = uniqueClears.ToString();
        Game1.player.modData[ModIdentity.StageProgressMigratedKey] = "true";
    }

    private SudokuPuzzle SelectPuzzleForToday(int day)
    {
        string difficulty = this.GetDailyDifficultyFromStageProgress();
        List<SudokuPuzzle> pool = this.puzzles
            .Where(p => p.Difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (pool.Count == 0)
            pool = this.puzzles;

        long seed = unchecked((long)Game1.uniqueIDForThisGame + day * 7919L);
        int index = (int)Math.Abs(seed % pool.Count);
        return pool[index];
    }

    private string GetDailyDifficultyFromStageProgress()
    {
        int cleared = this.GetSolvedCount();
        if (cleared >= 12)
            return "Hard";
        if (cleared >= 6)
            return "Normal";
        return "Easy";
    }

    private SudokuRewardEntry? SelectRewardForToday(SudokuPuzzle puzzle)
    {
        List<SudokuRewardEntry> pool = this.rewards
            .Where(p => p.Difficulty.Equals(puzzle.Difficulty, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (pool.Count == 0)
            pool = this.rewards;

        if (pool.Count == 0)
            return null;

        int totalWeight = pool.Sum(p => Math.Max(1, p.Weight));
        int day = Game1.Date.TotalDays;
        long seed64 = unchecked((long)Game1.uniqueIDForThisGame ^ ((long)day * 104729L) ^ 0xC0FFEEL);
        int seed = unchecked((int)seed64);
        Random random = new(seed);
        int roll = random.Next(totalWeight);

        foreach (SudokuRewardEntry entry in pool)
        {
            roll -= Math.Max(1, entry.Weight);
            if (roll < 0)
                return entry;
        }

        return pool[^1];
    }

    private string GrantFallbackGold(SudokuPuzzle puzzle)
    {
        int gold = puzzle.Difficulty.ToLowerInvariant() switch
        {
            "hard" => Math.Max(0, this.config.DailyRewardHard),
            "normal" => Math.Max(0, this.config.DailyRewardNormal),
            _ => Math.Max(0, this.config.DailyRewardEasy)
        };

        Game1.player.Money += gold;
        return $"{gold}g";
    }

    private static int DifficultyRank(string difficulty)
    {
        return difficulty.ToLowerInvariant() switch
        {
            "easy" => 0,
            "normal" => 1,
            "hard" => 2,
            _ => 99
        };
    }

    private static bool IsTrue(Farmer player, string key)
    {
        return player.modData.TryGetValue(key, out string? value)
            && value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}
