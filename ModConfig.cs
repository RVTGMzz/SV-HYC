namespace CursedSignal;

internal sealed class ModConfig
{
    /// <summary>Enable the daily haunted-TV signal after the Cursed VHS is installed.</summary>
    public bool EnableDailySignal { get; set; } = true;

    /// <summary>In-game time when the installed VHS activates each day.</summary>
    public int DailySignalTime { get; set; } = 800;

    /// <summary>How long the TV static intro stays on-screen, in update ticks.</summary>
    public int StaticTicks { get; set; } = 40;

    /// <summary>How long the well glimpse stays on-screen, in update ticks.</summary>
    public int WellTicks { get; set; } = 45;

    /// <summary>How long the glitch transition stays on-screen, in update ticks.</summary>
    public int GlitchTicks { get; set; } = 24;

    /// <summary>How long each 64x64 emergence frame stays on-screen, in update ticks.</summary>
    public int FrameDurationTicks { get; set; } = 16;

    /// <summary>How large the 64x64 event frame is drawn on-screen.</summary>
    public float EventScale { get; set; } = 4f;

    /// <summary>Enable the daily Sudoku prototype.</summary>
    public bool EnableDailySudoku { get; set; } = true;

    public int DailyRewardEasy { get; set; } = 250;
    public int DailyRewardNormal { get; set; } = 500;
    public int DailyRewardHard { get; set; } = 900;
}
