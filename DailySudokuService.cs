using StardewModdingAPI;
using StardewValley;

namespace CursedSignal;

internal sealed class DailySudokuService
{
    private const string DayKey = ModIdentity.DailySudokuPrefix + "Day";
    private const string PuzzleIdKey = ModIdentity.DailySudokuPrefix + "PuzzleId";
    private const string BoardKey = ModIdentity.DailySudokuPrefix + "Board";
    private const string ClaimedDayKey = ModIdentity.DailySudokuPrefix + "ClaimedDay";

    private readonly IMonitor monitor;
    private readonly ModConfig config;
    private readonly List<SudokuPuzzle> puzzles;
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

    public SudokuPuzzle? EnsureToday()
    {
        if (!Context.IsWorldReady || this.puzzles.Count == 0)
            return null;

        ModIdentity.MigrateLegacyPlayerData(Game1.player);

        int day = Game1.Date.TotalDays;
        SudokuPuzzle puzzle = this.SelectPuzzleForToday(day);

        bool sameDay =
            Game1.player.modData.TryGetValue(DayKey, out string? dayRaw)
            && int.TryParse(dayRaw, out int storedDay)
            && storedDay == day;

        bool samePuzzle =
            Game1.player.modData.TryGetValue(PuzzleIdKey, out string? id)
            && id == puzzle.Id;

        if (!sameDay || !samePuzzle)
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

    public string GetBoard(SudokuPuzzle puzzle)
    {
        this.EnsureToday();

        if (Game1.player.modData.TryGetValue(BoardKey, out string? board) && board.Length == 81)
            return board;

        return puzzle.Puzzle;
    }

    public void SetCell(SudokuPuzzle puzzle, int row, int column, int value)
    {
        if (row < 0 || row > 8 || column < 0 || column > 8 || value < 0 || value > 9)
            return;

        int index = row * 9 + column;
        if (puzzle.Puzzle[index] != '0')
            return;

        char[] board = this.GetBoard(puzzle).ToCharArray();
        board[index] = value == 0 ? '0' : (char)('0' + value);
        Game1.player.modData[BoardKey] = new string(board);
    }

    public bool IsSolved(SudokuPuzzle puzzle)
    {
        return this.GetBoard(puzzle) == puzzle.Solution;
    }

    public bool IsRewardClaimedToday()
    {
        ModIdentity.MigrateLegacyPlayerData(Game1.player);

        int day = Game1.Date.TotalDays;
        return Game1.player.modData.TryGetValue(ClaimedDayKey, out string? raw)
            && int.TryParse(raw, out int claimedDay)
            && claimedDay == day;
    }

    /// <summary>
    /// Claims today's reward and returns a player-facing description, or null if the reward couldn't be claimed.
    /// </summary>
    public string? ClaimReward(SudokuPuzzle puzzle)
    {
        if (!this.IsSolved(puzzle) || this.IsRewardClaimedToday())
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
            $"Daily Sudoku solved: {puzzle.Id} ({puzzle.Difficulty}), reward={rewardDescription}.",
            LogLevel.Info
        );

        return rewardDescription;
    }

    public void ResetTodayForTesting()
    {
        string[] suffixes = { "Day", "PuzzleId", "Board", "ClaimedDay" };

        foreach (string suffix in suffixes)
        {
            Game1.player.modData.Remove(ModIdentity.DailySudokuPrefix + suffix);
            Game1.player.modData.Remove(ModIdentity.LegacyDailySudokuPrefix + suffix);
        }
    }

    private SudokuPuzzle SelectPuzzleForToday(int day)
    {
        string difficulty = this.GetDifficultyForPlayer();
        List<SudokuPuzzle> pool = this.puzzles
            .Where(p => p.Difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (pool.Count == 0)
            pool = this.puzzles;

        long seed = unchecked((long)Game1.uniqueIDForThisGame + day * 7919L);
        int index = (int)Math.Abs(seed % pool.Count);
        return pool[index];
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

    private string GetDifficultyForPlayer()
    {
        Friendship? friendship = null;

        if (Game1.player.friendshipData.TryGetValue(ModIdentity.SudokuNpcId, out Friendship? current))
            friendship = current;
        else if (Game1.player.friendshipData.TryGetValue(ModIdentity.LegacySudokuNpcId, out Friendship? legacy))
            friendship = legacy;

        if (friendship is null)
            return "Easy";

        if (friendship.Points >= 2000)
            return "Hard";
        if (friendship.Points >= 1000)
            return "Normal";
        return "Easy";
    }
}
