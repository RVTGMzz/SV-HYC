using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace HeyYoureCursed;

internal sealed class SudokuChoiceOption
{
    public string Label { get; init; } = string.Empty;
    public Action Action { get; init; } = () => { };
}

/// <summary>A portrait-first vertical choice menu for Sudoku's roommate interactions.</summary>
internal sealed class SudokuChoiceMenu : IClickableMenu
{
    private const int PortraitFrameSize = 64;

    private readonly Texture2D? portraits;
    private readonly int portraitIndex;
    private readonly string[] lines;
    private readonly string question;
    private readonly string progressText;
    private readonly SudokuChoiceOption[] options;

    private int selectedIndex;
    private bool controllerModeSeen;

    public SudokuChoiceMenu(
        Texture2D? portraits,
        int portraitIndex,
        IEnumerable<string> lines,
        string question,
        string progressText,
        IEnumerable<SudokuChoiceOption> options
    )
        : base(
            Game1.uiViewport.Width / 2 - GetMenuWidth() / 2,
            Game1.uiViewport.Height / 2 - GetMenuHeight() / 2,
            GetMenuWidth(),
            GetMenuHeight(),
            showUpperRightCloseButton: false
        )
    {
        this.portraits = portraits;
        this.portraitIndex = Math.Clamp(portraitIndex, 0, 7);
        this.lines = lines.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
        this.question = question;
        this.progressText = progressText;
        this.options = options.Where(p => !string.IsNullOrWhiteSpace(p.Label)).Take(6).ToArray();
    }

    private static int GetMenuWidth() => Math.Clamp(Game1.uiViewport.Width - 96, 680, 880);
    private static int GetMenuHeight() => Math.Clamp(Game1.uiViewport.Height - 100, 500, 650);

    private int OptionsX => this.xPositionOnScreen + Math.Min(230, this.width / 3) + 52;
    private int OptionsWidth => this.xPositionOnScreen + this.width - 34 - this.OptionsX;
    private int OptionsBottom => this.yPositionOnScreen + this.height - 54;
    private int OptionHeight => 46;
    private int OptionGap => 8;
    private int OptionsTop => this.OptionsBottom - this.options.Length * this.OptionHeight - Math.Max(0, this.options.Length - 1) * this.OptionGap;

    private Rectangle GetOptionRect(int index)
    {
        return new Rectangle(
            this.OptionsX,
            this.OptionsTop + index * (this.OptionHeight + this.OptionGap),
            this.OptionsWidth,
            this.OptionHeight
        );
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        this.controllerModeSeen = false;

        for (int i = 0; i < this.options.Length; i++)
        {
            if (!this.GetOptionRect(i).Contains(x, y))
                continue;

            this.selectedIndex = i;
            this.Activate();
            return;
        }
    }

    public override void performHoverAction(int x, int y)
    {
        if (this.controllerModeSeen)
            return;

        for (int i = 0; i < this.options.Length; i++)
        {
            if (this.GetOptionRect(i).Contains(x, y))
            {
                this.selectedIndex = i;
                return;
            }
        }
    }

    public override void receiveKeyPress(Keys key)
    {
        this.controllerModeSeen = false;

        switch (key)
        {
            case Keys.Up:
            case Keys.Left:
                this.Move(-1);
                return;
            case Keys.Down:
            case Keys.Right:
                this.Move(1);
                return;
            case Keys.Enter:
            case Keys.Space:
                this.Activate();
                return;
            case Keys.Escape:
                this.Close();
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
            case SButton.DPadUp:
            case SButton.DPadLeft:
            case SButton.LeftThumbstickUp:
            case SButton.LeftThumbstickLeft:
                this.Move(-1);
                return true;
            case SButton.DPadDown:
            case SButton.DPadRight:
            case SButton.LeftThumbstickDown:
            case SButton.LeftThumbstickRight:
                this.Move(1);
                return true;
            case SButton.ControllerA:
                this.Activate();
                return true;
            case SButton.ControllerB:
                this.Close();
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
        DrawBorder(b, panel, 4, new Color(100, 132, 155));

        b.DrawString(
            Game1.dialogueFont,
            "Sudoku",
            new Vector2(this.xPositionOnScreen + 30, this.yPositionOnScreen + 22),
            new Color(222, 232, 239)
        );

        int portraitSize = Math.Min(184, this.height - 230);
        Rectangle portraitBox = new(
            this.xPositionOnScreen + 30,
            this.yPositionOnScreen + 78,
            portraitSize,
            portraitSize
        );
        b.Draw(Game1.staminaRect, portraitBox, new Color(10, 14, 20));
        DrawBorder(b, portraitBox, 2, new Color(78, 104, 124));

        if (this.portraits is not null)
        {
            Rectangle source = new(
                (this.portraitIndex % 4) * PortraitFrameSize,
                (this.portraitIndex / 4) * PortraitFrameSize,
                PortraitFrameSize,
                PortraitFrameSize
            );
            b.Draw(this.portraits, portraitBox, source, Color.White);
        }

        if (!string.IsNullOrWhiteSpace(this.progressText))
        {
            string wrappedProgress = WrapText(Game1.smallFont, this.progressText, portraitBox.Width);
            b.DrawString(
                Game1.smallFont,
                wrappedProgress,
                new Vector2(portraitBox.X, portraitBox.Bottom + 14),
                new Color(135, 158, 176)
            );
        }

        int textX = this.OptionsX;
        int textWidth = this.OptionsWidth;
        float y = this.yPositionOnScreen + 78;
        foreach (string line in this.lines)
        {
            string wrapped = WrapText(Game1.smallFont, line, textWidth);
            b.DrawString(Game1.smallFont, wrapped, new Vector2(textX, y), new Color(215, 223, 230));
            y += Game1.smallFont.MeasureString(wrapped).Y + 10;
        }

        string wrappedQuestion = WrapText(Game1.dialogueFont, this.question, textWidth);
        b.DrawString(
            Game1.dialogueFont,
            wrappedQuestion,
            new Vector2(textX, Math.Min(y + 8, this.OptionsTop - 72)),
            new Color(238, 241, 244)
        );

        for (int i = 0; i < this.options.Length; i++)
            DrawChoiceButton(b, this.GetOptionRect(i), this.options[i].Label, i == this.selectedIndex);

        string hint = this.controllerModeSeen
            ? "D-pad/LS: chọn   •   A: xác nhận   •   B: đóng"
            : "↑/↓: chọn   •   Enter: xác nhận   •   Esc: đóng";
        Vector2 hintSize = Game1.smallFont.MeasureString(hint);
        b.DrawString(
            Game1.smallFont,
            hint,
            new Vector2(this.xPositionOnScreen + (this.width - hintSize.X) / 2, this.yPositionOnScreen + this.height - 28),
            new Color(125, 150, 168)
        );

        this.drawMouse(b);
    }

    private void Move(int delta)
    {
        if (this.options.Length == 0)
            return;

        this.selectedIndex = (this.selectedIndex + delta + this.options.Length) % this.options.Length;
        Game1.playSound("shiny4");
    }

    private void Activate()
    {
        if (this.options.Length == 0)
        {
            this.Close();
            return;
        }

        Action action = this.options[Math.Clamp(this.selectedIndex, 0, this.options.Length - 1)].Action;
        Game1.playSound("smallSelect");
        Game1.activeClickableMenu = null;
        action();
    }

    private void Close()
    {
        Game1.playSound("cancel");
        Game1.activeClickableMenu = null;
    }

    private static void DrawChoiceButton(SpriteBatch b, Rectangle rect, string text, bool selected)
    {
        Color fill = selected ? new Color(82, 110, 133) : new Color(43, 59, 73);
        Color border = selected ? new Color(225, 235, 242) : new Color(105, 135, 158);
        b.Draw(Game1.staminaRect, rect, fill);
        DrawBorder(b, rect, selected ? 4 : 2, border);

        string wrapped = WrapText(Game1.smallFont, text, rect.Width - 18);
        Vector2 size = Game1.smallFont.MeasureString(wrapped);
        b.DrawString(
            Game1.smallFont,
            wrapped,
            new Vector2(rect.Center.X - size.X / 2, rect.Center.Y - size.Y / 2),
            Color.White
        );
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
        }

        return string.Join("\n", output);
    }
}
