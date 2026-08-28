namespace CursedSignal;

internal sealed class ModConfig
{
    /// <summary>Enable the temporary 7:00 AM development trigger.</summary>
    public bool EnableArrivalTest { get; set; } = true;

    /// <summary>In-game time at which the prototype arrival may begin.</summary>
    public int TestArrivalTime { get; set; } = 700;

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

    /// <summary>Fallback gold reward if the Easy item reward pool fails.</summary>
    public int DailyRewardEasy { get; set; } = 250;

    /// <summary>Fallback gold reward if the Normal item reward pool fails.</summary>
    public int DailyRewardNormal { get; set; } = 500;

    /// <summary>Fallback gold reward if the Hard item reward pool fails.</summary>
    public int DailyRewardHard { get; set; } = 900;
}
