namespace HeyYoureCursed;

/// <summary>
/// Carries the player's last deliberate input mode across Sudoku menu transitions. Stardew can
/// keep sending hover callbacks for a stationary mouse after a controller-created submenu opens;
/// remembering controller mode prevents that stale cursor from stealing focus on the new menu.
/// </summary>
internal static class SudokuInputState
{
    public static bool LastWasController { get; set; }
}
