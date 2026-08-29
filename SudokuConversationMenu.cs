using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace HeyYoureCursed;

/// <summary>
/// Portrait-first conversation menu used for Sudoku's intro and daily puzzle invitation.
/// Keeping this custom avoids Stardew's fragile question-dialogue -> custom-menu handoff.
/// </summary>
internal sealed class SudokuConversationMenu : IClickableMenu
{
    private const int PortraitFrameSize = 64;
    private const float BodyDialogueScale = 1.22f;

    private readonly Texture2D? portraits;
    private readonly int portraitIndex;
    private readonly string[] lines;
    private readonly string question;
    private readonly string primaryLabel;
    private readonly string secondaryLabel;
    private readonly string progressText;
    private readonly Action onPrimary;
    private readonly Action onSecondary;

    private int selectedChoice;
    private bool controllerModeSeen;

    public SudokuConversationMenu(
        Texture2D? portraits,
        int portraitIndex,
        IEnumerable<string> lines,
        string question,
        string primaryLabel,
        string secondaryLabel,
        string progressText,
        Action onPrimary,
        Action onSecondary
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
        this.primaryLabel = primaryLabel;
        this.secondaryLabel = secondaryLabel;
        this.progressText = progressText;
        this.onPrimary = onPrimary;
        this.onSecondary = onSecondary;
    }

    private static int GetMenuWidth() => Math.Clamp(Game1.uiViewport.Width - 96, 620, 800);
    private static int GetMenuHeight() => Math.Clamp(Game1.uiViewport.Height - 120, 430, 540);

    private Rectangle PrimaryButton => new(
        this.xPositionOnScreen + this.width / 2 - 196,
        this.yPositionOnScreen + this.height - 88,
        180,
        54
    );

    private Rectangle SecondaryButton => new(
        this.xPositionOnScreen + this.width / 2 + 16,
        this.yPositionOnScreen + this.height - 88,
        180,
        54
    );

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        this.controllerModeSeen = false;

        if (this.PrimaryButton.Contains(x, y))
        {
            this.selectedChoice = 0;
            this.ChoosePrimary();
            return;
        }

        if (this.SecondaryButton.Contains(x, y))
        {
            this.selectedChoice = 1;
            this.ChooseSecondary();
            return;
        }
    }

    public override void performHoverAction(int x, int y)
    {
        if (this.PrimaryButton.Contains(x, y))
            this.selectedChoice = 0;
        else if (this.SecondaryButton.Contains(x, y))
            this.selectedChoice = 1;
    }

    public override void receiveKeyPress(Keys key)
    {
        this.controllerModeSeen = false;

        switch (key)
        {
            case Keys.Left:
            case Keys.Up:
                this.ChangeChoice(-1);
                return;
            case Keys.Right:
            case Keys.Down:
                this.ChangeChoice(1);
                return;
            case Keys.Enter:
            case Keys.Space:
                this.ConfirmChoice();
                return;
            case Keys.Escape:
                this.ChooseSecondary();
                return;
        }

        base.receiveKeyPress(key);
    }

    internal bool HandleSmapiInput(SButton button)
    {
        bool isController = button is
            SButton.ControllerA or SButton.ControllerB
            or SButton.DPadLeft or SButton.DPadRight or SButton.DPadUp or SButton.DPadDown
            or SButton.LeftThumbstickLeft or SButton.LeftThumbstickRight
            or SButton.LeftThumbstickUp or SButton.LeftThumbstickDown;

        if (!isController)
            return false;

        this.controllerModeSeen = true;

        switch (button)
        {
            case SButton.DPadLeft:
            case SButton.DPadUp:
            case SButton.LeftThumbstickLeft:
            case SButton.LeftThumbstickUp:
                this.ChangeChoice(-1);
                return true;
            case SButton.DPadRight:
            case SButton.DPadDown:
            case SButton.LeftThumbstickRight:
            case SButton.LeftThumbstickDown:
                this.ChangeChoice(1);
                return true;
            case SButton.ControllerA:
                this.ConfirmChoice();
                return true;
            case SButton.ControllerB:
                this.ChooseSecondary();
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

        int portraitSize = Math.Min(192, this.height - 190);
        Rectangle portraitBox = new(
            this.xPositionOnScreen + 30,
            this.yPositionOnScreen + 74,
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

        b.DrawString(
            Game1.dialogueFont,
            "Sudoku",
            new Vector2(this.xPositionOnScreen + 30, this.yPositionOnScreen + 22),
            new Color(222, 232, 239)
        );

        int textX = portraitBox.Right + 28;
        int textWidth = this.xPositionOnScreen + this.width - 30 - textX;
        float y = this.yPositionOnScreen + 74;

        foreach (string line in this.lines)
        {
            string wrapped = WrapText(Game1.smallFont, line, textWidth / BodyDialogueScale);
            b.DrawString(
                Game1.smallFont,
                wrapped,
                new Vector2(textX, y),
                new Color(215, 223, 230),
                0f,
                Vector2.Zero,
                BodyDialogueScale,
                SpriteEffects.None,
                0f
            );
            y += Game1.smallFont.MeasureString(wrapped).Y * BodyDialogueScale + 12;
        }

        string wrappedQuestion = WrapText(Game1.dialogueFont, this.question, textWidth);
        float questionY = Math.Max(y + 8, portraitBox.Bottom - Game1.dialogueFont.MeasureString(wrappedQuestion).Y - 8);
        b.DrawString(
            Game1.dialogueFont,
            wrappedQuestion,
            new Vector2(textX, questionY),
            new Color(238, 241, 244)
        );

        if (!string.IsNullOrWhiteSpace(this.progressText))
        {
            b.DrawString(
                Game1.smallFont,
                this.progressText,
                new Vector2(this.xPositionOnScreen + 32, this.yPositionOnScreen + this.height - 122),
                new Color(135, 158, 176)
            );
        }

        DrawChoiceButton(b, this.PrimaryButton, this.primaryLabel, this.selectedChoice == 0);
        DrawChoiceButton(b, this.SecondaryButton, this.secondaryLabel, this.selectedChoice == 1);

        string hint = this.controllerModeSeen
            ? "D-pad/LS: chọn     A: xác nhận     B: để sau"
            : "←/→: chọn     Enter: xác nhận     Esc: để sau";
        Vector2 hintSize = Game1.smallFont.MeasureString(hint);
        b.DrawString(
            Game1.smallFont,
            hint,
            new Vector2(this.xPositionOnScreen + (this.width - hintSize.X) / 2, this.yPositionOnScreen + this.height - 26),
            new Color(125, 150, 168)
        );

        this.drawMouse(b);
    }

    private void ConfirmChoice()
    {
        if (this.selectedChoice == 0)
            this.ChoosePrimary();
        else
            this.ChooseSecondary();
    }

    private void ChangeChoice(int delta)
    {
        this.selectedChoice = (this.selectedChoice + delta + 2) % 2;
        Game1.playSound("shiny4");
    }

    private void ChoosePrimary()
    {
        Game1.playSound("smallSelect");
        Game1.activeClickableMenu = null;
        this.onPrimary();
    }

    private void ChooseSecondary()
    {
        Game1.playSound("cancel");
        Game1.activeClickableMenu = null;
        this.onSecondary();
    }

    private static void DrawChoiceButton(SpriteBatch b, Rectangle rect, string text, bool selected)
    {
        Color fill = selected ? new Color(82, 110, 133) : new Color(43, 59, 73);
        Color border = selected ? new Color(225, 235, 242) : new Color(105, 135, 158);
        b.Draw(Game1.staminaRect, rect, fill);
        DrawBorder(b, rect, selected ? 4 : 2, border);

        Vector2 size = Game1.smallFont.MeasureString(text);
        b.DrawString(
            Game1.smallFont,
            text,
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
