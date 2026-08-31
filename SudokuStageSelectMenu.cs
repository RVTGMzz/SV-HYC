using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace HeyYoureCursed;

/// <summary>
/// Hub for the repeatable Daily Challenge and the fixed 18-Stage progression.
/// Stage clears unlock sequentially and never grant the repeatable daily reward.
/// </summary>
internal sealed class SudokuStageSelectMenu : IClickableMenu
{
    private readonly DailySudokuService service;
    private readonly Action onDaily;
    private readonly Action<int> onStage;

    private int selectedIndex;
    private bool controllerModeSeen;

    public SudokuStageSelectMenu(
        DailySudokuService service,
        Action onDaily,
        Action<int> onStage
    )
        : base(
            Game1.viewport.Width / 2 - GetMenuWidth() / 2,
            Game1.viewport.Height / 2 - GetMenuHeight() / 2,
            GetMenuWidth(),
            GetMenuHeight(),
            showUpperRightCloseButton: true
        )
    {
        this.service = service;
        this.onDaily = onDaily;
        this.onStage = onStage;
        this.selectedIndex = 0;
    }

    private static int GetMenuWidth() => Math.Clamp(Game1.viewport.Width - 48, 640, 860);
    private static int GetMenuHeight() => Math.Clamp(Game1.viewport.Height - 48, 580, 760);

    private Rectangle DailyRect => new(
        this.xPositionOnScreen + 60,
        this.yPositionOnScreen + 96,
        this.width - 120,
        68
    );

    private int StageAreaTop => this.yPositionOnScreen + 190;
    private int StageGap => 8;
    private int StageButtonWidth => (this.width - 96 - this.StageGap * 5) / 6;
    private int StageButtonHeight => 52;
    private int StageRowStride => Math.Clamp((this.height - 300) / 3, 88, 112);
    private int StageStartX => this.xPositionOnScreen + 48;

    private Rectangle GetStageRect(int stageIndex)
    {
        int row = stageIndex / 6;
        int col = stageIndex % 6;
        int rowY = this.StageAreaTop + row * this.StageRowStride + 24;

        return new Rectangle(
            this.StageStartX + col * (this.StageButtonWidth + this.StageGap),
            rowY,
            this.StageButtonWidth,
            this.StageButtonHeight
        );
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        this.controllerModeSeen = false;

        base.receiveLeftClick(x, y, playSound);
        if (Game1.activeClickableMenu != this)
            return;

        if (this.DailyRect.Contains(x, y))
        {
            this.selectedIndex = 0;
            this.ActivateSelection();
            return;
        }

        int stageCount = this.service.GetStageCount();
        for (int i = 0; i < stageCount; i++)
        {
            if (!this.GetStageRect(i).Contains(x, y))
                continue;

            this.selectedIndex = i + 1;
            this.ActivateSelection();
            return;
        }
    }

    public override void performHoverAction(int x, int y)
    {
        if (this.DailyRect.Contains(x, y))
        {
            this.selectedIndex = 0;
            return;
        }

        int stageCount = this.service.GetStageCount();
        for (int i = 0; i < stageCount; i++)
        {
            if (this.GetStageRect(i).Contains(x, y))
            {
                this.selectedIndex = i + 1;
                return;
            }
        }
    }

    public override void receiveKeyPress(Keys key)
    {
        this.controllerModeSeen = false;

        switch (key)
        {
            case Keys.Left:
                this.MoveSelectionLeft();
                return;
            case Keys.Right:
                this.MoveSelectionRight();
                return;
            case Keys.Up:
                this.MoveSelectionUp();
                return;
            case Keys.Down:
                this.MoveSelectionDown();
                return;
            case Keys.Enter:
            case Keys.Space:
                this.ActivateSelection();
                return;
            case Keys.Escape:
                this.exitThisMenu();
                return;
        }

        base.receiveKeyPress(key);
    }

    internal bool HandleSmapiInput(SButton button)
    {
        bool controller = button is
            SButton.ControllerA or SButton.ControllerB
            or SButton.DPadLeft or SButton.DPadRight or SButton.DPadUp or SButton.DPadDown
            or SButton.LeftThumbstickLeft or SButton.LeftThumbstickRight
            or SButton.LeftThumbstickUp or SButton.LeftThumbstickDown;

        if (!controller)
            return false;

        this.controllerModeSeen = true;

        switch (button)
        {
            case SButton.DPadLeft:
            case SButton.LeftThumbstickLeft:
                this.MoveSelectionLeft();
                return true;
            case SButton.DPadRight:
            case SButton.LeftThumbstickRight:
                this.MoveSelectionRight();
                return true;
            case SButton.DPadUp:
            case SButton.LeftThumbstickUp:
                this.MoveSelectionUp();
                return true;
            case SButton.DPadDown:
            case SButton.LeftThumbstickDown:
                this.MoveSelectionDown();
                return true;
            case SButton.ControllerA:
                this.ActivateSelection();
                return true;
            case SButton.ControllerB:
                this.exitThisMenu();
                return true;
        }

        return false;
    }

    public override void receiveGamePadButton(Buttons button)
    {
        SButton? mapped = button switch
        {
            Buttons.DPadLeft => SButton.DPadLeft,
            Buttons.DPadRight => SButton.DPadRight,
            Buttons.DPadUp => SButton.DPadUp,
            Buttons.DPadDown => SButton.DPadDown,
            Buttons.A => SButton.ControllerA,
            Buttons.B => SButton.ControllerB,
            _ => null
        };

        if (mapped.HasValue && this.HandleSmapiInput(mapped.Value))
            return;

        base.receiveGamePadButton(button);
    }

    public override void draw(SpriteBatch b)
    {
        Rectangle panel = new(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);
        b.Draw(Game1.staminaRect, panel, new Color(18, 24, 34) * 0.97f);
        DrawBorder(b, panel, 4, new Color(95, 125, 148));

        string title = ModEntry.T("stage.title");
        Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
        b.DrawString(
            Game1.dialogueFont,
            title,
            new Vector2(this.xPositionOnScreen + (this.width - titleSize.X) / 2, this.yPositionOnScreen + 24),
            new Color(220, 232, 240)
        );

        int cleared = this.service.GetSolvedCount();
        int total = this.service.GetStageCount();
        string progress = ModEntry.T("stage.progress", new { cleared, total });
        Vector2 progressSize = Game1.smallFont.MeasureString(progress);
        b.DrawString(
            Game1.smallFont,
            progress,
            new Vector2(this.xPositionOnScreen + (this.width - progressSize.X) / 2, this.yPositionOnScreen + 72),
            new Color(145, 170, 188)
        );

        this.DrawDailyCard(b);

        string[] rowNames =
        {
            ModEntry.LocalizeDifficulty("easy").ToUpperInvariant(),
            ModEntry.LocalizeDifficulty("normal").ToUpperInvariant(),
            ModEntry.LocalizeDifficulty("hard").ToUpperInvariant()
        };
        for (int row = 0; row < 3; row++)
        {
            int labelY = this.StageAreaTop + row * this.StageRowStride;
            b.DrawString(
                Game1.smallFont,
                rowNames[row],
                new Vector2(this.StageStartX, labelY),
                new Color(170, 194, 210)
            );

            for (int col = 0; col < 6; col++)
            {
                int stageIndex = row * 6 + col;
                if (stageIndex >= total)
                    break;

                this.DrawStageButton(b, stageIndex);
            }
        }

        string detail = this.GetSelectedDetail();
        string wrappedDetail = WrapText(Game1.smallFont, detail, this.width - 100);
        Vector2 detailSize = Game1.smallFont.MeasureString(wrappedDetail);
        b.DrawString(
            Game1.smallFont,
            wrappedDetail,
            new Vector2(
                this.xPositionOnScreen + (this.width - detailSize.X) / 2,
                this.yPositionOnScreen + this.height - 68
            ),
            new Color(180, 200, 214)
        );

        string hint = this.controllerModeSeen
            ? ModEntry.T("stage.hint.controller")
            : ModEntry.T("stage.hint.keyboard");
        Vector2 hintSize = Game1.smallFont.MeasureString(hint);
        b.DrawString(
            Game1.smallFont,
            hint,
            new Vector2(
                this.xPositionOnScreen + (this.width - hintSize.X) / 2,
                this.yPositionOnScreen + this.height - 30
            ),
            new Color(120, 145, 164)
        );

        this.upperRightCloseButton?.draw(b);
        this.drawMouse(b);
    }

    private void DrawDailyCard(SpriteBatch b)
    {
        bool selected = this.selectedIndex == 0;
        bool claimed = this.service.IsRewardClaimedToday();
        SudokuPuzzle? daily = this.service.EnsureToday();

        Color fill = selected ? new Color(76, 103, 126) : new Color(40, 56, 70);
        Color border = selected ? new Color(226, 237, 244) : new Color(102, 133, 156);
        b.Draw(Game1.staminaRect, this.DailyRect, fill);
        DrawBorder(b, this.DailyRect, selected ? 4 : 2, border);

        string title = ModEntry.T("stage.daily.title");
        string info = daily is null
            ? ModEntry.T("stage.daily.invalid")
            : ModEntry.T(claimed ? "stage.daily.claimed" : "stage.daily.available", new { difficulty = ModEntry.LocalizeDifficulty(daily.Difficulty) });

        b.DrawString(
            Game1.smallFont,
            title,
            new Vector2(this.DailyRect.X + 18, this.DailyRect.Y + 10),
            Color.White
        );
        b.DrawString(
            Game1.smallFont,
            info,
            new Vector2(this.DailyRect.X + 18, this.DailyRect.Y + 38),
            new Color(188, 207, 220)
        );
    }

    private void DrawStageButton(SpriteBatch b, int stageIndex)
    {
        Rectangle rect = this.GetStageRect(stageIndex);
        bool selected = this.selectedIndex == stageIndex + 1;
        bool unlocked = this.service.IsStageUnlocked(stageIndex);
        bool cleared = this.service.IsStageCleared(stageIndex);

        Color fill;
        Color border;
        if (!unlocked)
        {
            fill = new Color(28, 34, 42);
            border = new Color(67, 79, 90);
        }
        else if (cleared)
        {
            fill = selected ? new Color(73, 110, 105) : new Color(48, 79, 76);
            border = selected ? new Color(228, 240, 236) : new Color(112, 154, 146);
        }
        else
        {
            fill = selected ? new Color(82, 110, 133) : new Color(44, 61, 76);
            border = selected ? new Color(229, 238, 244) : new Color(104, 135, 159);
        }

        b.Draw(Game1.staminaRect, rect, fill);
        DrawBorder(b, rect, selected ? 4 : 2, border);

        string label = $"{stageIndex + 1:00}";
        Vector2 size = Game1.smallFont.MeasureString(label);
        b.DrawString(
            Game1.smallFont,
            label,
            new Vector2(rect.Center.X - size.X / 2, rect.Y + 7),
            unlocked ? Color.White : new Color(105, 115, 124)
        );

        string state = !unlocked
            ? ModEntry.T("stage.state.locked")
            : cleared
                ? "✓"
                : ModEntry.T("stage.state.open");
        Vector2 stateSize = Game1.smallFont.MeasureString(state);
        b.DrawString(
            Game1.smallFont,
            state,
            new Vector2(rect.Center.X - stateSize.X / 2, rect.Bottom - 24),
            !unlocked ? new Color(92, 102, 112) : new Color(185, 213, 223)
        );
    }

    private string GetSelectedDetail()
    {
        if (this.selectedIndex == 0)
        {
            SudokuPuzzle? daily = this.service.EnsureToday();
            if (daily is null)
                return ModEntry.T("stage.detail.daily.invalid");

            return this.service.IsRewardClaimedToday()
                ? ModEntry.T("stage.detail.daily.claimed", new { difficulty = ModEntry.LocalizeDifficulty(daily.Difficulty) })
                : ModEntry.T("stage.detail.daily.reward", new { difficulty = ModEntry.LocalizeDifficulty(daily.Difficulty) });
        }

        int stageIndex = this.selectedIndex - 1;
        SudokuPuzzle? stage = this.service.GetStage(stageIndex);
        if (stage is null)
            return ModEntry.T("stage.detail.missing");

        if (!this.service.IsStageUnlocked(stageIndex))
            return ModEntry.T("stage.detail.previous", new { number = stageIndex + 1, difficulty = ModEntry.LocalizeDifficulty(stage.Difficulty), previous = stageIndex });

        return this.service.IsStageCleared(stageIndex)
            ? ModEntry.T("stage.detail.cleared", new { number = stageIndex + 1, difficulty = ModEntry.LocalizeDifficulty(stage.Difficulty) })
            : ModEntry.T("stage.detail.new", new { number = stageIndex + 1, difficulty = ModEntry.LocalizeDifficulty(stage.Difficulty) });
    }

    private void ActivateSelection()
    {
        if (this.selectedIndex == 0)
        {
            Game1.playSound("smallSelect");
            Game1.activeClickableMenu = null;
            this.onDaily();
            return;
        }

        int stageIndex = this.selectedIndex - 1;
        if (!this.service.IsStageUnlocked(stageIndex))
        {
            Game1.playSound("cancel");
            return;
        }

        Game1.playSound("smallSelect");
        Game1.activeClickableMenu = null;
        this.onStage(stageIndex);
    }

    private void MoveSelectionLeft()
    {
        if (this.selectedIndex == 0)
        {
            Game1.playSound("shiny4");
            return;
        }

        int stageIndex = this.selectedIndex - 1;
        int rowStart = (stageIndex / 6) * 6;
        int col = stageIndex % 6;
        int nextCol = (col + 5) % 6;
        this.selectedIndex = rowStart + nextCol + 1;
        Game1.playSound("shiny4");
    }

    private void MoveSelectionRight()
    {
        if (this.selectedIndex == 0)
        {
            this.selectedIndex = this.GetPreferredStageSelection() + 1;
            Game1.playSound("shiny4");
            return;
        }

        int stageIndex = this.selectedIndex - 1;
        int rowStart = (stageIndex / 6) * 6;
        int col = stageIndex % 6;
        int nextCol = (col + 1) % 6;
        this.selectedIndex = rowStart + nextCol + 1;
        Game1.playSound("shiny4");
    }

    private void MoveSelectionUp()
    {
        if (this.selectedIndex == 0)
        {
            Game1.playSound("shiny4");
            return;
        }

        int stageIndex = this.selectedIndex - 1;
        if (stageIndex < 6)
            this.selectedIndex = 0;
        else
            this.selectedIndex = stageIndex - 6 + 1;

        Game1.playSound("shiny4");
    }

    private void MoveSelectionDown()
    {
        if (this.selectedIndex == 0)
        {
            this.selectedIndex = this.GetPreferredStageSelection() + 1;
            Game1.playSound("shiny4");
            return;
        }

        int stageIndex = this.selectedIndex - 1;
        int candidate = stageIndex + 6;
        if (candidate < this.service.GetStageCount())
            this.selectedIndex = candidate + 1;

        Game1.playSound("shiny4");
    }

    private int GetPreferredStageSelection()
    {
        int stageCount = this.service.GetStageCount();
        if (stageCount <= 0)
            return 0;

        for (int i = 0; i < stageCount; i++)
        {
            if (this.service.IsStageUnlocked(i) && !this.service.IsStageCleared(i))
                return i;
        }

        return stageCount - 1;
    }

    private static void DrawBorder(SpriteBatch b, Rectangle rect, int thickness, Color color)
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

        List<string> lines = new();
        string current = string.Empty;

        foreach (string word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            string candidate = string.IsNullOrEmpty(current) ? word : current + " " + word;
            if (font.MeasureString(candidate).X <= maxWidth || string.IsNullOrEmpty(current))
            {
                current = candidate;
                continue;
            }

            lines.Add(current);
            current = word;
        }

        if (!string.IsNullOrEmpty(current))
            lines.Add(current);

        return string.Join("\n", lines);
    }
}
