namespace CursedSignal;

internal sealed class SudokuRewardEntry
{
    public string Id { get; set; } = "";
    public string Difficulty { get; set; } = "Easy";
    public string QualifiedItemId { get; set; } = "";
    public int MinStack { get; set; } = 1;
    public int MaxStack { get; set; } = 1;
    public int Weight { get; set; } = 1;
}
