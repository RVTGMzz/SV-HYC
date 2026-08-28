namespace ChuyenTamLinhKoDuaDuocDau;

internal sealed class ModConfig
{
    /// <summary>Enable the temporary 7:00 AM arrival trigger used during development.</summary>
    public bool EnableArrivalTest { get; set; } = true;

    /// <summary>In-game time at which the prototype arrival may begin.</summary>
    public int TestArrivalTime { get; set; } = 700;

    /// <summary>How long each TV-emergence frame stays on screen, in game update ticks.</summary>
    public int FrameDurationTicks { get; set; } = 18;

    /// <summary>How large the 64x64 event frame is drawn on screen.</summary>
    public float EventScale { get; set; } = 4f;
}
