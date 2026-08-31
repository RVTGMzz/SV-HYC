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

        string title = this.mode switch
        {
            SudokuPlayMode.Stage => ModEntry.T("sudoku.title.stage", new { number = this.stageIndex + 1 }),
            SudokuPlayMode.Practice => ModEntry.T("sudoku.title.practice"),
            _ => ModEntry.T("sudoku.title.daily")
        };
        Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
        b.DrawString(
            Game1.dialogueFont,
            title,
            new Vector2(this.xPositionOnScreen + (this.width - titleSize.X) / 2, this.yPositionOnScreen + 20),
            new Color(215, 230, 238)
        );

        string difficulty = ModEntry.LocalizeDifficulty(this.puzzle.Difficulty);
        string sub = this.mode switch
        {
            SudokuPlayMode.Stage => ModEntry.T("sudoku.sub.stage", new { difficulty, current = this.stageIndex + 1, total = this.service.GetStageCount() }),
            SudokuPlayMode.Practice => ModEntry.T("sudoku.sub.practice", new { difficulty }),
            _ => ModEntry.T("sudoku.sub.daily", new { difficulty, puzzle = this.puzzle.Id })
        };
        Vector2 subSize = Game1.smallFont.MeasureString(sub);
        b.DrawString(
            Game1.smallFont,
            sub,
            new Vector2(this.xPositionOnScreen + (this.width - subSize.X) / 2, this.yPositionOnScreen + 66),
            new Color(145, 170, 188)
        );

        this.DrawButton(b, this.HelpRect, "? " + ModEntry.T("sudoku.help.button"));

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

        string backLabel = this.returnToStageSelect is null
            ? ModEntry.T("sudoku.action.close")
            : ModEntry.T("sudoku.action.list");
        string hint = this.controllerModeSeen
            ? this.numberPickerOpen
                ? ModEntry.T("sudoku.hint.controller-picker")
                : ModEntry.T("sudoku.hint.controller", new { back = backLabel })
            : ModEntry.T("sudoku.hint.keyboard", new { back = backLabel });

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

        if (this.helpOpen)
            this.DrawHelpOverlay(b);

        this.drawMouse(b);
    }

    private void DrawHelpOverlay(SpriteBatch b)
    {
        b.Draw(
            Game1.staminaRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.62f
        );

        Rectangle panel = this.HelpPanelRect;
        b.Draw(Game1.staminaRect, panel, new Color(18, 24, 34) * 0.99f);
        this.DrawBorder(b, panel, 4, new Color(140, 170, 192));

        string title = ModEntry.T("sudoku.help.title");
        Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
        b.DrawString(
            Game1.dialogueFont,
            title,
            new Vector2(panel.Center.X - titleSize.X / 2, panel.Y + 18),
            new Color(225, 235, 242)
        );

        int contentTop = panel.Y + 68;
        int left = panel.X + 24;
        int right = panel.Right - 24;
        int exampleSize = Math.Clamp(panel.Width / 4, 108, 132);
        Rectangle example = new(left, contentTop + 30, exampleSize, exampleSize);

        string goal = WrapText(Game1.smallFont, ModEntry.T("sudoku.help.goal"), panel.Width - 48);
        b.DrawString(Game1.smallFont, goal, new Vector2(left, contentTop), new Color(205, 220, 230));

        this.DrawHelpExample(b, example);

        int rulesX = example.Right + 26;
        int rulesWidth = Math.Max(180, right - rulesX);
        string rules = string.Join("\n", new[]
        {
            ModEntry.T("sudoku.help.rule.row"),
            ModEntry.T("sudoku.help.rule.column"),
            ModEntry.T("sudoku.help.rule.box")
        });
        b.DrawString(
            Game1.smallFont,
            WrapText(Game1.smallFont, rules, rulesWidth),
            new Vector2(rulesX, example.Y + 4),
            new Color(215, 226, 233)
        );

        string exampleText = WrapText(Game1.smallFont, ModEntry.T("sudoku.help.example"), rulesWidth);
        b.DrawString(
            Game1.smallFont,
            exampleText,
            new Vector2(rulesX, example.Bottom - Game1.smallFont.MeasureString(exampleText).Y),
            new Color(151, 207, 230)
        );

        int textY = example.Bottom + 18;
        string tip = WrapText(Game1.smallFont, ModEntry.T("sudoku.help.tip"), panel.Width - 48);
        b.DrawString(Game1.smallFont, tip, new Vector2(left, textY), new Color(224, 210, 158));
        textY += (int)Game1.smallFont.MeasureString(tip).Y + 12;

        string legend = WrapText(Game1.smallFont, ModEntry.T("sudoku.help.legend"), panel.Width - 48);
        b.DrawString(Game1.smallFont, legend, new Vector2(left, textY), new Color(180, 205, 220));
        textY += (int)Game1.smallFont.MeasureString(legend).Y + 10;

        string controls = WrapText(Game1.smallFont, ModEntry.T("sudoku.help.controls"), panel.Width - 48);
        b.DrawString(Game1.smallFont, controls, new Vector2(left, textY), new Color(190, 210, 222));

        this.DrawButton(b, this.HelpCloseRect, ModEntry.T("sudoku.help.close"));

        string closeHint = this.controllerModeSeen
            ? ModEntry.T("sudoku.help.close.controller")
            : ModEntry.T("sudoku.help.close.keyboard");
        Vector2 hintSize = Game1.tinyFont.MeasureString(closeHint);
        b.DrawString(
            Game1.tinyFont,
            closeHint,
            new Vector2(panel.Center.X - hintSize.X / 2, this.HelpCloseRect.Y - 22),
            new Color(125, 150, 168)
        );
    }

    private void DrawHelpExample(SpriteBatch b, Rectangle rect)
    {
        int cell = rect.Width / 3;
        int[,] values =
        {
            { 1, 2, 3 },
            { 4, 0, 6 },
            { 7, 8, 9 }
        };

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                Rectangle cellRect = new(rect.X + col * cell, rect.Y + row * cell, cell, cell);
                bool answer = row == 1 && col == 1;
                b.Draw(Game1.staminaRect, cellRect, answer ? new Color(70, 102, 124) : new Color(39, 52, 65));
                this.DrawBorder(b, cellRect, 1, new Color(130, 154, 170));

                string text = answer ? "5" : values[row, col].ToString();
                Vector2 size = Game1.smallFont.MeasureString(text);
                b.DrawString(
                    Game1.smallFont,
                    text,
                    new Vector2(cellRect.Center.X - size.X / 2, cellRect.Center.Y - size.Y / 2),
                    answer ? new Color(145, 205, 230) : new Color(230, 235, 238)
                );
            }
        }

        this.DrawBorder(b, new Rectangle(rect.X, rect.Y, cell * 3, cell * 3), 3, new Color(205, 220, 230));
    }

    private void DrawGrid(SpriteBatch b)
    {
        string board = this.service.GetBoard(this.puzzle, this.mode);
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

        this.DrawButton(b, this.ClearRect, ModEntry.T("sudoku.action.erase"));
        this.DrawButton(b, this.CheckRect, ModEntry.T("sudoku.action.check"));
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
