namespace CursedSignal;

internal sealed class ModConfig
{
    /// <summary>Enable the daily haunted-TV signal after the Cursed VHS is installed.</summary>
    public bool EnableDailySignal { get; set; } = true;

    /// <summary>In-game time when the installed VHS activates each day.</summary>
    public int DailySignalTime { get; set; } = 800;

    /// <summary>How long the first TV static intro stays on-screen, in update ticks.</summary>
    public int StaticTicks { get; set; } = 40;

    /// <summary>How long the first well glimpse stays on-screen, in update ticks.</summary>
    public int WellTicks { get; set; } = 45;

    /// <summary>How long the first glitch transition stays on-screen, in update ticks.</summary>
    public int GlitchTicks { get; set; } = 24;

    /// <summary>How long each 64x64 emergence frame stays on-screen during the first arrival.</summary>
    public int FrameDurationTicks { get; set; } = 16;

    /// <summary>How large the 64x64 first-arrival event frame is drawn on-screen.</summary>
    public float EventScale { get; set; } = 4f;

    /// <summary>Short static phase used on repeat mornings after Sudoku has already arrived once.</summary>
    public int RepeatStaticTicks { get; set; } = 22;

    /// <summary>Short glitch phase used on repeat mornings after Sudoku has already arrived once.</summary>
    public int RepeatGlitchTicks { get; set; } = 14;

    /// <summary>Enable the daily Sudoku prototype.</summary>
    public bool EnableDailySudoku { get; set; } = true;

    public int DailyRewardEasy { get; set; } = 250;
    public int DailyRewardNormal { get; set; } = 500;
    public int DailyRewardHard { get; set; } = 900;
}
