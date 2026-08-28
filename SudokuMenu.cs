using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

    public override void receiveGamePadButton(Buttons button)
    {
        switch (button)
        {
            case Buttons.DPadLeft:
                this.MoveSelection(0, -1);
                return;
            case Buttons.DPadRight:
                this.MoveSelection(0, 1);
                return;
            case Buttons.DPadUp:
                this.MoveSelection(-1, 0);
                return;
            case Buttons.DPadDown:
                this.MoveSelection(1, 0);
                return;
            case Buttons.A:
                this.CycleSelectedCell(1);
                return;
            case Buttons.X:
                this.EnterNumber(0);
                return;
            case Buttons.Y:
                this.CheckBoard();
                return;
            case Buttons.B:
                this.exitThisMenu();
                return;
            case Buttons.LeftShoulder:
                this.CycleSelectedCell(-1);
                return;
            case Buttons.RightShoulder:
                this.CycleSelectedCell(1);
                return;
        }

        base.receiveGamePadButton(button);
    }

    public override void draw(SpriteBatch b)
    {
        b.Draw(
            Game1.staminaRect,
            new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height),
            new Color(18, 24, 34) * 0.96f
        );

        this.DrawBorder(b, new Rectangle(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height), 4, new Color(95, 125, 148));

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

        Vector2 statusSize = Game1.smallFont.MeasureString(this.statusText);
        b.DrawString(
            Game1.smallFont,
            this.statusText,
            new Vector2(this.xPositionOnScreen + (this.width - statusSize.X) / 2, this.yPositionOnScreen + this.height - 42),
            new Color(205, 215, 225)
        );

        this.upperRightCloseButton?.draw(b);
        this.drawMouse(b);
    }

    private void DrawGrid(SpriteBatch b)
    {
        string board = this.service.GetBoard(this.puzzle);

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                int index = row * 9 + col;
                bool given = this.puzzle.Puzzle[index] != '0';
                bool selected = row == this.selectedRow && col == this.selectedColumn;

                Rectangle cell = new(
                    this.GridX + col * CellSize,
                    this.GridY + row * CellSize,
                    CellSize,
                    CellSize
                );

                Color fill = given
                    ? new Color(45, 57, 70)
                    : new Color(28, 38, 50);

                if (selected)
                    fill = new Color(78, 102, 122);

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

            b.Draw(Game1.staminaRect, new Rectangle(this.GridX + i * CellSize - thickness / 2, this.GridY, thickness, GridSize), line);
            b.Draw(Game1.staminaRect, new Rectangle(this.GridX, this.GridY + i * CellSize - thickness / 2, GridSize, thickness), line);
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
            b.Draw(Game1.staminaRect, rect, new Color(49, 67, 82));
            this.DrawBorder(b, rect, 2, new Color(105, 135, 158));

            string text = n.ToString();
            Vector2 size = Game1.smallFont.MeasureString(text);
            b.DrawString(Game1.smallFont, text, new Vector2(rect.Center.X - size.X / 2, rect.Center.Y - size.Y / 2), Color.White);
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
        b.DrawString(Game1.smallFont, text, new Vector2(rect.Center.X - size.X / 2, rect.Center.Y - size.Y / 2), Color.White);
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

    private void MoveSelection(int rowDelta, int colDelta)
    {
        this.selectedRow = (this.selectedRow + rowDelta + 9) % 9;
        this.selectedColumn = (this.selectedColumn + colDelta + 9) % 9;
        Game1.playSound("shiny4");
    }

    private void CycleSelectedCell(int delta)
    {
        int index = this.selectedRow * 9 + this.selectedColumn;
        if (this.puzzle.Puzzle[index] != '0')
        {
            Game1.playSound("cancel");
            return;
        }

        string board = this.service.GetBoard(this.puzzle);
        int current = board[index] == '0' ? 0 : board[index] - '0';
        int next = current + delta;
        if (next > 9) next = 0;
        if (next < 0) next = 9;
        this.EnterNumber(next);
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
