namespace HeyYoureCursed;

internal sealed class ModConfig
{
    public bool EnableDailySignal { get; set; } = true;
    public int DailySignalTime { get; set; } = 800;

    public int StaticTicks { get; set; } = 40;
    public int GlitchTicks { get; set; } = 24;
    public int FrameDurationTicks { get; set; } = 16;
    public float EventScale { get; set; } = 4f;

    public int RepeatStaticTicks { get; set; } = 22;
    public int RepeatGlitchTicks { get; set; } = 14;

    public bool EnableDailySudoku { get; set; } = true;
    public int DailyRewardEasy { get; set; } = 250;
    public int DailyRewardNormal { get; set; } = 500;
    public int DailyRewardHard { get; set; } = 900;
}
