using StardewValley;

namespace HeyYoureCursed;

internal sealed partial class SudokuMenu
{
    /// <summary>Developer/test helper: fill every editable cell with the solution and run normal completion logic.</summary>
    internal void SolveInstantlyForTesting()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int column = 0; column < 9; column++)
            {
                int index = row * 9 + column;
                if (this.puzzle.Puzzle[index] != '0')
                    continue;

                int value = this.puzzle.Solution[index] - '0';
                if (value is >= 1 and <= 9)
                    this.service.SetCell(this.puzzle, this.mode, row, column, value);
            }
        }

        this.CheckBoard();
        Game1.playSound("discoverMineral");
    }
}
