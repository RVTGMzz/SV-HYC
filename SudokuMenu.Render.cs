using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace HeyYoureCursed;

internal sealed partial class SudokuMenu
{
    public override void draw(SpriteBatch b)
    {
        Rectangle panel = new(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);
        b.Draw(Game1.staminaRect, panel, new Color(18, 24, 34) * 0.96f);
        this.DrawBorder(b, panel, 4, new Color(95, 125, 148));

        string title = "SUDOKU — BẢNG MỖI NGÀY";
        Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
        b.DrawString(
            Game1.dialogueFont,
            title,
            new Vector2(this.xPositionOnScreen + (this.width - titleSize.X) / 2, this.yPositionOnScreen + 20),
            new Color(215, 230, 238)
        );

        string sub = $"{this.puzzle.Difficulty}  •  {this.puzzle.Id}";
        Vector2 subSize = Game1.smallFont.MeasureString(sub);
        b.DrawString(
            Game1.smallFont,
            sub,
            new Vector2(this.xPositionOnScreen + (this.width - subSize.X) / 2, this.yPositionOnScreen + 66),
            new Color(145, 170, 188)
        );

        this.DrawGrid(b);
        this.DrawNumberButtons(b);

        Rectangle footer = new(
            this.xPositionOnScreen + 20,
            this.FooterY,
            this.width - 40,
            this.yPositionOnScreen + this.height - this.FooterY - 16
        );
        b.Draw(Game1.staminaRect, footer, new Color(12, 18, 27) * 0.90f);
        this.DrawBorder(b, footer, 1, new Color(65, 88, 106));

        string hint = this.controllerModeSeen
            ? this.numberPickerOpen
                ? "←/→: chọn số   •   A: điền   •   X: xóa   •   B: hủy"
                : "D-pad/LS: di chuyển   •   A: chọn số   •   X: xóa\nLB/RB: ô trống   •   Y/Start: kiểm tra   •   B: đóng"
            : "Chuột/←↑↓→: chọn ô   •   1–9: điền\nDelete: xóa   •   Enter: kiểm tra   •   Esc: đóng";

        string wrappedHint = WrapText(Game1.smallFont, hint, footer.Width - 28);
        b.DrawString(
            Game1.smallFont,
            wrappedHint,
            new Vector2(footer.X + 14, footer.Y + 8),
            new Color(145, 170, 188)
        );

        float hintHeight = Game1.smallFont.MeasureString(wrappedHint).Y;
        string wrappedStatus = WrapText(Game1.smallFont, this.statusText, footer.Width - 28);
        b.DrawString(
            Game1.smallFont,
            wrappedStatus,
            new Vector2(footer.X + 14, footer.Y + 12 + hintHeight),
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
                    this.GridX + col * this.cellSize,
                    this.GridY + row * this.cellSize,
                    this.cellSize,
                    this.cellSize
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
                new Rectangle(this.GridX + i * this.cellSize - thickness / 2, this.GridY, thickness, GridSize),
                line
            );
            b.Draw(
                Game1.staminaRect,
                new Rectangle(this.GridX, this.GridY + i * this.cellSize - thickness / 2, GridSize, thickness),
                line
            );
        }
    }

    private void DrawNumberButtons(SpriteBatch b)
    {
        for (int n = 1; n <= 9; n++)
        {
            Rectangle rect = new(
                this.NumberStartX + (n - 1) * (this.NumberButtonSize + this.NumberGap),
                this.NumberY,
                this.NumberButtonSize,
                this.NumberButtonSize
            );

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

        this.DrawButton(b, this.ClearRect, "XÓA");
        this.DrawButton(b, this.CheckRect, "KIỂM TRA");
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

    private static string WrapText(SpriteFont font, string text, float maxWidth)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        List<string> output = new();
        foreach (string paragraph in text.Split('\n'))
        {
            string current = string.Empty;
            foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                string candidate = string.IsNullOrEmpty(current) ? word : current + " " + word;
                if (font.MeasureString(candidate).X <= maxWidth || string.IsNullOrEmpty(current))
                {
                    current = candidate;
                    continue;
                }

                output.Add(current);
                current = word;
            }

            if (!string.IsNullOrEmpty(current))
                output.Add(current);
            else if (string.IsNullOrEmpty(paragraph))
                output.Add(string.Empty);
        }

        return string.Join("\n", output);
    }

}
