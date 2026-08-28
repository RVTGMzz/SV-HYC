using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace HeyYoureCursed;

internal sealed class SudokuMenu : IClickableMenu
{
    private const int CellSize = 48;
    private const int GridSize = CellSize * 9;

    private readonly DailySudokuService service;
    private readonly SudokuPuzzle puzzle;

    private int selectedRow;
    private int selectedColumn;
    private bool controllerModeSeen;
    private bool numberPickerOpen;
    private int numberPickerValue = 1;
    private string statusText = "Chọn một ô trống, rồi điền số 1–9.";

    public SudokuMenu(DailySudokuService service, SudokuPuzzle puzzle)
        : base(
            Game1.viewport.Width / 2 - 325,
            Game1.viewport.Height / 2 - 360,
            650,
            720,
            showUpperRightCloseButton: true
        )
    {
        this.service = service;
        this.puzzle = puzzle;
        this.SelectFirstEditableCell();
    }

    private int GridX => this.xPositionOnScreen + (this.width - GridSize) / 2;
    private int GridY => this.yPositionOnScreen + 112;

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        this.controllerModeSeen = false;
        this.numberPickerOpen = false;

        base.receiveLeftClick(x, y, playSound);

        if (Game1.activeClickableMenu != this)
            return;

        if (x >= this.GridX && x < this.GridX + GridSize && y >= this.GridY && y < this.GridY + GridSize)
        {
            int col = (x - this.GridX) / CellSize;
            int row = (y - this.GridY) / CellSize;
            this.selectedRow = Math.Clamp(row, 0, 8);
            this.selectedColumn = Math.Clamp(col, 0, 8);
            int index = this.selectedRow * 9 + this.selectedColumn;
            this.statusText = this.puzzle.Puzzle[index] == '0'
                ? "Đã chọn ô trống. Chọn số 1–9 bên dưới."
                : "Ô này là đề bài. Hãy chọn một ô trống.";
            Game1.playSound("shiny4");
            return;
        }

        int numberY = this.GridY + GridSize + 22;
        int buttonSize = 42;
        int gap = 6;
        int totalWidth = 9 * buttonSize + 8 * gap;
        int startX = this.xPositionOnScreen + (this.width - totalWidth) / 2;

        for (int n = 1; n <= 9; n++)
        {
            Rectangle rect = new(startX + (n - 1) * (buttonSize + gap), numberY, buttonSize, buttonSize);
            if (rect.Contains(x, y))
            {
                this.EnterNumber(n);
                return;
            }
        }

        Rectangle clearRect = new(this.xPositionOnScreen + 72, numberY + 58, 145, 46);
        Rectangle checkRect = new(this.xPositionOnScreen + this.width - 217, numberY + 58, 145, 46);

        if (clearRect.Contains(x, y))
        {
            this.EnterNumber(0);
            return;
        }

        if (checkRect.Contains(x, y))
        {
            this.CheckBoard();
            return;
        }
    }

    public override void receiveKeyPress(Keys key)
    {
        this.controllerModeSeen = false;
        this.numberPickerOpen = false;

        if (key == Keys.Escape)
        {
            this.exitThisMenu();
            return;
        }

        int keyCode = (int)key;
        if (keyCode >= (int)Keys.D1 && keyCode <= (int)Keys.D9)
        {
            this.EnterNumber(keyCode - (int)Keys.D0);
            return;
        }

        if (keyCode >= (int)Keys.NumPad1 && keyCode <= (int)Keys.NumPad9)
        {
            this.EnterNumber(keyCode - (int)Keys.NumPad0);
            return;
        }

        switch (key)
        {
            case Keys.Back:
            case Keys.Delete:
            case Keys.D0:
            case Keys.NumPad0:
                this.EnterNumber(0);
                return;
            case Keys.Enter:
                this.CheckBoard();
                return;
            case Keys.Left:
                this.MoveSelection(0, -1);
                return;
            case Keys.Right:
                this.MoveSelection(0, 1);
                return;
            case Keys.Up:
                this.MoveSelection(-1, 0);
                return;
            case Keys.Down:
                this.MoveSelection(1, 0);
                return;
        }

        base.receiveKeyPress(key);
    }

    /// <summary>
    /// Handle controller input through SMAPI. This is the primary controller path because it
    /// works even when Stardew doesn't forward a particular gamepad button to receiveGamePadButton.
    /// </summary>
    internal bool HandleSmapiInput(SButton button)
    {
        bool isControllerInput = button is
            SButton.ControllerA or SButton.ControllerB or SButton.ControllerX or SButton.ControllerY
            or SButton.DPadLeft or SButton.DPadRight or SButton.DPadUp or SButton.DPadDown
            or SButton.LeftThumbstickLeft or SButton.LeftThumbstickRight or SButton.LeftThumbstickUp or SButton.LeftThumbstickDown
            or SButton.LeftShoulder or SButton.RightShoulder
            or SButton.LeftTrigger or SButton.RightTrigger
            or SButton.ControllerStart;

        if (!isControllerInput)
            return false;

        this.controllerModeSeen = true;

        if (this.numberPickerOpen)
            return this.HandleNumberPickerInput(button);

        switch (button)
        {
            case SButton.DPadLeft:
            case SButton.LeftThumbstickLeft:
                this.MoveSelection(0, -1);
                return true;
            case SButton.DPadRight:
            case SButton.LeftThumbstickRight:
                this.MoveSelection(0, 1);
                return true;
            case SButton.DPadUp:
            case SButton.LeftThumbstickUp:
                this.MoveSelection(-1, 0);
                return true;
            case SButton.DPadDown:
            case SButton.LeftThumbstickDown:
                this.MoveSelection(1, 0);
                return true;
            case SButton.ControllerA:
                this.OpenNumberPicker();
                return true;
            case SButton.ControllerX:
                this.EnterNumber(0);
                return true;
            case SButton.ControllerY:
            case SButton.ControllerStart:
                this.CheckBoard();
                return true;
            case SButton.LeftShoulder:
            case SButton.LeftTrigger:
                this.MoveToNextEditable(-1);
                return true;
            case SButton.RightShoulder:
            case SButton.RightTrigger:
                this.MoveToNextEditable(1);
                return true;
            case SButton.ControllerB:
                this.exitThisMenu();
                return true;
        }

        return false;
    }

    public override void receiveGamePadButton(Buttons button)
    {
        // Fallback for gamepad paths that bypass SMAPI's ButtonPressed event.
        SButton? mapped = button switch
        {
            Buttons.DPadLeft => SButton.DPadLeft,
            Buttons.DPadRight => SButton.DPadRight,
            Buttons.DPadUp => SButton.DPadUp,
            Buttons.DPadDown => SButton.DPadDown,
            Buttons.A => SButton.ControllerA,
            Buttons.B => SButton.ControllerB,
            Buttons.X => SButton.ControllerX,
            Buttons.Y => SButton.ControllerY,
            Buttons.LeftShoulder => SButton.LeftShoulder,
            Buttons.RightShoulder => SButton.RightShoulder,
            Buttons.Start => SButton.ControllerStart,
            _ => null
        };

        if (mapped.HasValue && this.HandleSmapiInput(mapped.Value))
            return;

        base.receiveGamePadButton(button);
    }

    public override void draw(SpriteBatch b)
    {
        b.Draw(
            Game1.staminaRect,
            new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height),
            new Color(18, 24, 34) * 0.96f
        );

        this.DrawBorder(
            b,
            new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height),
            4,
            new Color(95, 125, 148)
        );

        string title = "SUDOKU — BẢNG MỖI NGÀY";
        Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
        b.DrawString(
            Game1.dialogueFont,
            title,
            new Vector2(this.xPositionOnScreen + (this.width - titleSize.X) / 2, this.yPositionOnScreen + 28),
            new Color(215, 230, 238)
        );

        string sub = $"{this.puzzle.Difficulty}  •  {this.puzzle.Id}";
        Vector2 subSize = Game1.smallFont.MeasureString(sub);
        b.DrawString(
            Game1.smallFont,
            sub,
            new Vector2(this.xPositionOnScreen + (this.width - subSize.X) / 2, this.yPositionOnScreen + 78),
            new Color(145, 170, 188)
        );

        this.DrawGrid(b);
        this.DrawNumberButtons(b);

        string hint = this.controllerModeSeen
            ? this.numberPickerOpen
                ? "←/→: chọn số   A: điền   X: xóa   B: hủy"
                : "D-pad/LS: di chuyển   A: chọn số   X: xóa   Y: kiểm tra   B: đóng"
            : "Chuột: chọn ô/số   1–9: điền   Delete: xóa   Enter: kiểm tra";

        Vector2 hintSize = Game1.smallFont.MeasureString(hint);
        b.DrawString(
            Game1.smallFont,
            hint,
            new Vector2(this.xPositionOnScreen + (this.width - hintSize.X) / 2, this.yPositionOnScreen + this.height - 66),
            new Color(145, 170, 188)
        );

        Vector2 statusSize = Game1.smallFont.MeasureString(this.statusText);
        b.DrawString(
            Game1.smallFont,
            this.statusText,
            new Vector2(this.xPositionOnScreen + (this.width - statusSize.X) / 2, this.yPositionOnScreen + this.height - 38),
            new Color(205, 215, 225)
        );

        this.upperRightCloseButton?.draw(b);
        this.drawMouse(b);
    }

    private void DrawGrid(SpriteBatch b)
    {
        string board = this.service.GetBoard(this.puzzle);
        char selectedValue = board[this.selectedRow * 9 + this.selectedColumn];

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                int index = row * 9 + col;
                bool given = this.puzzle.Puzzle[index] != '0';
                bool selected = row == this.selectedRow && col == this.selectedColumn;
                bool sameBox = row / 3 == this.selectedRow / 3 && col / 3 == this.selectedColumn / 3;
                bool peer = row == this.selectedRow || col == this.selectedColumn || sameBox;
                bool sameValue = selectedValue != '0' && board[index] == selectedValue;

                Rectangle cell = new(
                    this.GridX + col * CellSize,
                    this.GridY + row * CellSize,
                    CellSize,
                    CellSize
                );

                Color fill = given
                    ? new Color(45, 57, 70)
                    : new Color(28, 38, 50);

                if (peer)
                    fill = given ? new Color(53, 67, 80) : new Color(34, 47, 60);

                if (sameValue)
                    fill = given ? new Color(64, 82, 96) : new Color(48, 67, 82);

                if (selected)
                    fill = new Color(88, 118, 142);

                b.Draw(Game1.staminaRect, cell, fill);

                char value = board[index];
                if (value != '0')
                {
                    string text = value.ToString();
                    Vector2 size = Game1.dialogueFont.MeasureString(text);
                    Color color = given
                        ? new Color(225, 232, 236)
                        : new Color(145, 205, 230);

                    b.DrawString(
                        Game1.dialogueFont,
                        text,
                        new Vector2(cell.Center.X - size.X / 2, cell.Center.Y - size.Y / 2 - 2),
                        color
                    );
                }
            }
        }

        for (int i = 0; i <= 9; i++)
        {
            int thickness = i % 3 == 0 ? 4 : 1;
            Color line = i % 3 == 0 ? new Color(185, 205, 218) : new Color(92, 112, 128);

            b.Draw(
                Game1.staminaRect,
                new Rectangle(this.GridX + i * CellSize - thickness / 2, this.GridY, thickness, GridSize),
                line
            );
            b.Draw(
                Game1.staminaRect,
                new Rectangle(this.GridX, this.GridY + i * CellSize - thickness / 2, GridSize, thickness),
                line
            );
        }
    }

    private void DrawNumberButtons(SpriteBatch b)
    {
        int numberY = this.GridY + GridSize + 22;
        int buttonSize = 42;
        int gap = 6;
        int totalWidth = 9 * buttonSize + 8 * gap;
        int startX = this.xPositionOnScreen + (this.width - totalWidth) / 2;

        for (int n = 1; n <= 9; n++)
        {
            Rectangle rect = new(startX + (n - 1) * (buttonSize + gap), numberY, buttonSize, buttonSize);
            bool pickerSelected = this.numberPickerOpen && n == this.numberPickerValue;
            Color fill = pickerSelected ? new Color(86, 118, 143) : new Color(49, 67, 82);
            Color border = pickerSelected ? new Color(225, 235, 242) : new Color(105, 135, 158);

            b.Draw(Game1.staminaRect, rect, fill);
            this.DrawBorder(b, rect, pickerSelected ? 4 : 2, border);

            string text = n.ToString();
            Vector2 size = Game1.smallFont.MeasureString(text);
            b.DrawString(
                Game1.smallFont,
                text,
                new Vector2(rect.Center.X - size.X / 2, rect.Center.Y - size.Y / 2),
                Color.White
            );
        }

        Rectangle clearRect = new(this.xPositionOnScreen + 72, numberY + 58, 145, 46);
        Rectangle checkRect = new(this.xPositionOnScreen + this.width - 217, numberY + 58, 145, 46);
        this.DrawButton(b, clearRect, "XÓA");
        this.DrawButton(b, checkRect, "KIỂM TRA");
    }

    private void DrawButton(SpriteBatch b, Rectangle rect, string text)
    {
        b.Draw(Game1.staminaRect, rect, new Color(49, 67, 82));
        this.DrawBorder(b, rect, 2, new Color(120, 150, 170));
        Vector2 size = Game1.smallFont.MeasureString(text);
        b.DrawString(
            Game1.smallFont,
            text,
            new Vector2(rect.Center.X - size.X / 2, rect.Center.Y - size.Y / 2),
            Color.White
        );
    }

    private void DrawBorder(SpriteBatch b, Rectangle rect, int thickness, Color color)
    {
        b.Draw(Game1.staminaRect, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        b.Draw(Game1.staminaRect, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        b.Draw(Game1.staminaRect, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        b.Draw(Game1.staminaRect, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }

    private void EnterNumber(int value)
    {
        int index = this.selectedRow * 9 + this.selectedColumn;
        if (this.puzzle.Puzzle[index] != '0')
        {
            this.statusText = "Ô đó là đề bài. Đừng sửa.";
            Game1.playSound("cancel");
            return;
        }

        this.service.SetCell(this.puzzle, this.selectedRow, this.selectedColumn, value);
        this.statusText = value == 0 ? "Đã xóa ô." : $"Đã điền {value}.";
        Game1.playSound("smallSelect");
    }

    private void CheckBoard()
    {
        if (!this.service.IsSolved(this.puzzle))
        {
            this.statusText = "......Sai. Nhìn lại hàng, cột và ô 3×3.";
            Game1.playSound("cancel");
            return;
        }

        string? reward = this.service.ClaimReward(this.puzzle);
        if (!string.IsNullOrWhiteSpace(reward))
        {
            this.statusText = $"Đúng. Sudoku đẩy sang cho bạn {reward}. 'Đừng hiểu lầm. Không phải quà.'";
            Game1.playSound("purchase");
        }
        else
        {
            this.statusText = "Đúng rồi. Nhưng phần thưởng hôm nay ngươi đã lấy rồi.";
            Game1.playSound("coin");
        }
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

        string board = this.service.GetBoard(this.puzzle);
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
