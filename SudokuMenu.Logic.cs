using StardewModdingAPI;
using StardewValley;

namespace HeyYoureCursed;

internal sealed partial class SudokuMenu
{
    private void EnterNumber(int value)
    {
        int index = this.selectedRow * 9 + this.selectedColumn;
        if (this.puzzle.Puzzle[index] != '0')
        {
            this.statusText = "Ô đó là đề bài. Đừng sửa.";
            Game1.playSound("cancel");
            return;
        }

        this.service.SetCell(this.puzzle, this.mode, this.selectedRow, this.selectedColumn, value);
        this.statusText = value == 0 ? "Đã xóa ô." : $"Đã điền {value}.";
        Game1.playSound("smallSelect");
    }

    private void CheckBoard()
    {
        if (!this.service.IsSolved(this.puzzle, this.mode))
        {
            this.statusText = "......Sai. Nhìn lại hàng, cột và ô 3×3.";
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
                    ? $" Stage {this.stageIndex + 2:00} đã mở."
                    : " Bạn đã hoàn thành toàn bộ 18 Stage hiện tại.";

                this.statusText = $"Đúng. Stage {this.stageIndex + 1:00} hoàn thành!{unlockText} Tiến độ: {cleared}/{total}.";
                Game1.playSound("purchase");
            }
            else
            {
                this.statusText = $"Đúng. Stage {this.stageIndex + 1:00} đã hoàn thành trước đó; chơi lại không tăng tiến độ.";
                Game1.playSound("coin");
            }

            return;
        }

        string? reward = this.service.ClaimDailyReward(this.puzzle);
        if (!string.IsNullOrWhiteSpace(reward))
        {
            this.statusText = $"Đúng. Sudoku đẩy sang cho bạn {reward}. 'Đừng hiểu lầm. Không phải quà.'";
            Game1.playSound("purchase");
        }
        else
        {
            this.statusText = "Đúng rồi. Nhưng phần thưởng Daily Challenge hôm nay đã nhận rồi.";
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
            this.statusText = "Ô này là đề bài. Hãy chọn một ô trống.";
            Game1.playSound("cancel");
            return;
        }

        string board = this.service.GetBoard(this.puzzle, this.mode);
        char current = board[index];
        this.numberPickerValue = current is >= '1' and <= '9' ? current - '0' : 1;
        this.numberPickerOpen = true;
        this.statusText = "Chọn số bằng trái/phải rồi nhấn A.";
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
                this.ChangePicker(-1);
                return true;
            case SButton.DPadRight:
            case SButton.LeftThumbstickRight:
            case SButton.RightShoulder:
            case SButton.RightTrigger:
                this.ChangePicker(1);
                return true;
            case SButton.ControllerA:
                this.EnterNumber(this.numberPickerValue);
                this.numberPickerOpen = false;
                return true;
            case SButton.ControllerX:
                this.EnterNumber(0);
                this.numberPickerOpen = false;
                return true;
            case SButton.ControllerB:
                this.numberPickerOpen = false;
                this.statusText = "Đã hủy chọn số.";
                Game1.playSound("cancel");
                return true;
            case SButton.DPadUp:
            case SButton.LeftThumbstickUp:
                this.ChangePicker(-1);
                return true;
            case SButton.DPadDown:
            case SButton.LeftThumbstickDown:
                this.ChangePicker(1);
                return true;
        }

        return true;
    }

    private void ChangePicker(int delta)
    {
        this.numberPickerValue += delta;
        if (this.numberPickerValue > 9)
            this.numberPickerValue = 1;
        else if (this.numberPickerValue < 1)
            this.numberPickerValue = 9;

        Game1.playSound("shiny4");
    }

    private void MoveSelection(int rowDelta, int colDelta)
    {
        this.numberPickerOpen = false;
        this.selectedRow = (this.selectedRow + rowDelta + 9) % 9;
        this.selectedColumn = (this.selectedColumn + colDelta + 9) % 9;
        this.statusText = "Chọn ô. Nhấn A để chọn số.";
        Game1.playSound("shiny4");
    }

    private void MoveToNextEditable(int delta)
    {
        int start = this.selectedRow * 9 + this.selectedColumn;
        for (int step = 1; step <= 81; step++)
        {
            int index = (start + delta * step) % 81;
            if (index < 0)
                index += 81;

            if (this.puzzle.Puzzle[index] == '0')
            {
                this.selectedRow = index / 9;
                this.selectedColumn = index % 9;
                this.numberPickerOpen = false;
                this.statusText = "Đã nhảy tới ô trống kế tiếp.";
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
