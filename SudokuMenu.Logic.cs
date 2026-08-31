using StardewModdingAPI;
using StardewValley;

namespace HeyYoureCursed;

internal sealed partial class SudokuMenu
{
    private void OpenHelp()
    {
        this.helpOpen = true;
        this.numberPickerOpen = false;
        Game1.playSound("smallSelect");
    }

    private void CloseHelp()
    {
        this.helpOpen = false;
        Game1.playSound("cancel");
    }

    private void EnterNumber(int value)
    {
        int index = this.selectedRow * 9 + this.selectedColumn;
        if (this.puzzle.Puzzle[index] != '0')
        {
            this.statusText = ModEntry.T("sudoku.status.given-edit");
            Game1.playSound("cancel");
            return;
        }

        this.service.SetCell(this.puzzle, this.mode, this.selectedRow, this.selectedColumn, value);
        this.statusText = value == 0
            ? ModEntry.T("sudoku.status.erased")
            : ModEntry.T("sudoku.status.entered", new { value });
        Game1.playSound("smallSelect");
    }

    private void CheckBoard()
    {
        if (!this.service.IsSolved(this.puzzle, this.mode))
        {
            this.statusText = ModEntry.T("sudoku.status.wrong");
            Game1.playSound("cancel");
            return;
        }

        if (this.mode == SudokuPlayMode.Stage)
        {
            bool firstClear = this.service.CompleteStage(this.puzzle);
            int total = this.service.GetStageCount();
            if (firstClear)
            {
                int cleared = this.service.GetSolvedCount();
                string unlockText = this.stageIndex + 1 < total
                    ? ModEntry.T("sudoku.status.stage-next", new { number = this.stageIndex + 2 })
                    : ModEntry.T("sudoku.status.stage-endless");
                this.statusText = ModEntry.T("sudoku.status.stage-cleared", new
                {
                    number = this.stageIndex + 1,
                    unlock = unlockText,
                    cleared,
                    total
                });
                Game1.playSound("purchase");
            }
            else
            {
                this.statusText = ModEntry.T("sudoku.status.stage-replay", new { number = this.stageIndex + 1 });
                Game1.playSound("coin");
            }
            return;
        }

        if (this.mode == SudokuPlayMode.Practice)
        {
            this.statusText = ModEntry.T("sudoku.status.practice-solved");
            Game1.playSound("coin");
            return;
        }

        string? reward = this.service.ClaimDailyReward(this.puzzle);
        if (!string.IsNullOrWhiteSpace(reward))
        {
            this.statusText = ModEntry.T("sudoku.status.daily-reward", new { reward });
            Game1.playSound("purchase");
        }
        else
        {
            this.statusText = ModEntry.T("sudoku.status.daily-claimed");
            Game1.playSound("coin");
        }
    }

    private void CloseOrReturn()
    {
        Game1.playSound("cancel");
        Game1.activeClickableMenu = null;
        if (this.returnToStageSelect is not null)
            this.returnToStageSelect();
    }

    private void OpenNumberPicker()
    {
        int index = this.selectedRow * 9 + this.selectedColumn;
        if (this.puzzle.Puzzle[index] != '0')
        {
            this.statusText = ModEntry.T("sudoku.status.given");
            Game1.playSound("cancel");
            return;
        }

        string board = this.service.GetBoard(this.puzzle, this.mode);
        char current = board[index];
        this.numberPickerValue = current is >= '1' and <= '9' ? current - '0' : 1;
        this.numberPickerOpen = true;
        this.statusText = ModEntry.T("sudoku.status.picker");
        Game1.playSound("smallSelect");
    }

    private bool HandleNumberPickerInput(SButton button)
    {
        switch (button)
        {
            case SButton.DPadLeft:
            case SButton.LeftThumbstickLeft:
            case SButton.LeftShoulder:
            case SButton.LeftTrigger:
                this.ChangePicker(-1); return true;
            case SButton.DPadRight:
            case SButton.LeftThumbstickRight:
            case SButton.RightShoulder:
            case SButton.RightTrigger:
                this.ChangePicker(1); return true;
            case SButton.ControllerA:
                this.EnterNumber(this.numberPickerValue); this.numberPickerOpen = false; return true;
            case SButton.ControllerX:
                this.EnterNumber(0); this.numberPickerOpen = false; return true;
            case SButton.ControllerB:
                this.numberPickerOpen = false; this.statusText = ModEntry.T("sudoku.status.picker-cancelled"); Game1.playSound("cancel"); return true;
            case SButton.DPadUp:
            case SButton.LeftThumbstickUp:
                this.ChangePicker(-1); return true;
            case SButton.DPadDown:
            case SButton.LeftThumbstickDown:
                this.ChangePicker(1); return true;
        }
        return true;
    }

    private void ChangePicker(int delta)
    {
        this.numberPickerValue += delta;
        if (this.numberPickerValue > 9) this.numberPickerValue = 1;
        else if (this.numberPickerValue < 1) this.numberPickerValue = 9;
        Game1.playSound("shiny4");
    }

    private void MoveSelection(int rowDelta, int colDelta)
    {
        this.numberPickerOpen = false;
        this.selectedRow = (this.selectedRow + rowDelta + 9) % 9;
        this.selectedColumn = (this.selectedColumn + colDelta + 9) % 9;
        this.statusText = ModEntry.T("sudoku.status.select");
        Game1.playSound("shiny4");
    }

    private void MoveToNextEditable(int delta)
    {
        int start = this.selectedRow * 9 + this.selectedColumn;
        for (int step = 1; step <= 81; step++)
        {
            int index = (start + delta * step) % 81;
            if (index < 0) index += 81;
            if (this.puzzle.Puzzle[index] == '0')
            {
                this.selectedRow = index / 9;
                this.selectedColumn = index % 9;
                this.numberPickerOpen = false;
                this.statusText = ModEntry.T("sudoku.status.next-empty");
                Game1.playSound("shiny4");
                return;
            }
        }
    }

    private void SelectFirstEditableCell()
    {
        for (int i = 0; i < 81; i++)
        {
            if (this.puzzle.Puzzle[i] == '0')
            {
                this.selectedRow = i / 9;
                this.selectedColumn = i % 9;
                return;
            }
        }
    }
}
