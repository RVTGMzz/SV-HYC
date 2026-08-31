using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace HeyYoureCursed;

/// <summary>The one-time emotional capstone after all 18 Stages and Trust 30/30.</summary>
internal sealed class SudokuCapstoneMenu : IClickableMenu
{
    private const int PortraitFrameSize = 64;
    private const int ChoiceBeatIndex = 4;

    private readonly Texture2D? portraits;
    private readonly Action<string> onChoice;
    private readonly Action onComplete;
    private readonly Beat[] beats;

    private int beatIndex;
    private int selectedChoice;
    private bool controllerModeSeen;

    public SudokuCapstoneMenu(Texture2D? portraits, Action<string> onChoice, Action onComplete)
        : base(
            Game1.uiViewport.Width / 2 - GetMenuWidth() / 2,
            Game1.uiViewport.Height / 2 - GetMenuHeight() / 2,
            GetMenuWidth(),
            GetMenuHeight(),
            showUpperRightCloseButton: false
        )
    {
        this.portraits = portraits;
        this.onChoice = onChoice;
        this.onComplete = onComplete;
        this.controllerModeSeen = SudokuInputState.LastWasController;
        this.beats = new[]
        {
            new Beat("capstone.1", 4),
            new Beat("capstone.2", 6),
            new Beat("capstone.3", 3),
            new Beat("capstone.4", 4),
            new Beat("capstone.5", 0, true),
            new Beat("capstone.6", 3),
            new Beat("capstone.7", 4)
        };
    }

    private static int GetMenuWidth() => Math.Clamp(Game1.uiViewport.Width - 96, 640, 820);
    private static int GetMenuHeight() => Math.Clamp(Game1.uiViewport.Height - 150, 430, 560);
    private bool IsChoiceBeat => this.beatIndex == ChoiceBeatIndex;

    private Rectangle ContinueButton => new(
        this.xPositionOnScreen + this.width / 2 - 95,
        this.yPositionOnScreen + this.height - 82,
        190,
        50
    );

    private Rectangle ChoiceLeft => new(
        this.xPositionOnScreen + 34,
        this.yPositionOnScreen + this.height - 104,
        this.width / 2 - 50,
        70
    );

    private Rectangle ChoiceRight => new(
        this.xPositionOnScreen + this.width / 2 + 16,
        this.yPositionOnScreen + this.height - 104,
        this.width / 2 - 50,
        70
    );

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        this.controllerModeSeen = false;
        SudokuInputState.LastWasController = false;

        if (this.IsChoiceBeat)
        {
            if (this.ChoiceLeft.Contains(x, y))
            {
                this.selectedChoice = 0;
                this.AdvanceChoice();
            }
            else if (this.ChoiceRight.Contains(x, y))
            {
                this.selectedChoice = 1;
                this.AdvanceChoice();
            }

            return;
        }

        if (this.ContinueButton.Contains(x, y))
            this.Advance();
    }

    public override void performHoverAction(int x, int y)
    {
        if (this.controllerModeSeen || !this.IsChoiceBeat)
            return;

        if (this.ChoiceLeft.Contains(x, y))
            this.selectedChoice = 0;
        else if (this.ChoiceRight.Contains(x, y))
            this.selectedChoice = 1;
    }

    public override void receiveKeyPress(Keys key)
    {
        this.controllerModeSeen = false;
        SudokuInputState.LastWasController = false;

        if (this.IsChoiceBeat)
        {
            if (key is Keys.Left or Keys.Up)
            {
                this.ChangeChoice(-1);
                return;
            }

            if (key is Keys.Right or Keys.Down)
            {
                this.ChangeChoice(1);
                return;
            }

            if (key is Keys.Enter or Keys.Space)
            {
                this.AdvanceChoice();
                return;
            }

            if (key == Keys.Escape)
            {
                Game1.activeClickableMenu = null;
                return;
            }

            return;
        }

        if (key is Keys.Enter or Keys.Space)
        {
            this.Advance();
            return;
        }

        if (key == Keys.Escape)
        {
            Game1.activeClickableMenu = null;
            return;
        }
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
        SudokuInputState.LastWasController = true;

        if (this.IsChoiceBeat)
        {
            if (button is SButton.DPadLeft or SButton.DPadUp or SButton.LeftThumbstickLeft or SButton.LeftThumbstickUp)
            {
                this.ChangeChoice(-1);
                return true;
            }

            if (button is SButton.DPadRight or SButton.DPadDown or SButton.LeftThumbstickRight or SButton.LeftThumbstickDown)
            {
                this.ChangeChoice(1);
                return true;
            }

            if (button == SButton.ControllerA)
            {
                this.AdvanceChoice();
                return true;
            }

            if (button == SButton.ControllerB)
            {
                Game1.activeClickableMenu = null;
                return true;
            }

            return false;
        }

        if (button == SButton.ControllerA)
        {
            this.Advance();
            return true;
        }

        if (button == SButton.ControllerB)
        {
            Game1.activeClickableMenu = null;
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

    private void ChangeChoice(int delta)
    {
        int old = this.selectedChoice;
        this.selectedChoice = (this.selectedChoice + delta + 2) % 2;
        if (old != this.selectedChoice)
            Game1.playSound("shiny4");
    }

    private void AdvanceChoice()
    {
        this.onChoice(this.selectedChoice == 0 ? "stay" : "home");
        Game1.playSound("smallSelect");
        this.beatIndex++;
    }

    private void Advance()
    {
        Game1.playSound("smallSelect");
        this.beatIndex++;
        if (this.beatIndex < this.beats.Length)
            return;

        Game1.activeClickableMenu = null;
        this.onComplete();
    }

    public override void draw(SpriteBatch b)
    {
        Rectangle viewport = new(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height);
        b.Draw(Game1.staminaRect, viewport, Color.Black * 0.66f);

        Rectangle panel = new(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);
        SudokuUiStyle.DrawRoundedPanel(b, panel, new Color(18, 24, 34) * 0.98f, new Color(135, 102, 157), 4, 16);

        Beat beat = this.beats[Math.Clamp(this.beatIndex, 0, this.beats.Length - 1)];
        Rectangle portraitBox = new(this.xPositionOnScreen + 34, this.yPositionOnScreen + 80, 190, 190);
        SudokuUiStyle.DrawRoundedPanel(b, portraitBox, new Color(9, 13, 20), new Color(82, 110, 132), 2, 10);
        if (this.portraits is not null)
        {
            Rectangle source = new(
                (beat.PortraitIndex % 4) * PortraitFrameSize,
                (beat.PortraitIndex / 4) * PortraitFrameSize,
                PortraitFrameSize,
                PortraitFrameSize
            );
            b.Draw(this.portraits, portraitBox, source, Color.White);
        }

        string title = ModEntry.T("capstone.title");
        b.DrawString(Game1.dialogueFont, title, new Vector2(this.xPositionOnScreen + 32, this.yPositionOnScreen + 24), new Color(226, 215, 236));

        int textX = portraitBox.Right + 30;
        int textWidth = this.xPositionOnScreen + this.width - 38 - textX;
        string wrapped = WrapText(Game1.dialogueFont, ModEntry.T(beat.TextKey), textWidth);
        b.DrawString(Game1.dialogueFont, wrapped, new Vector2(textX, this.yPositionOnScreen + 92), new Color(235, 230, 240));

        if (this.IsChoiceBeat)
        {
            DrawChoice(b, this.ChoiceLeft, ModEntry.T("capstone.choice.stay"), this.selectedChoice == 0);
            DrawChoice(b, this.ChoiceRight, ModEntry.T("capstone.choice.home"), this.selectedChoice == 1);
        }
        else
        {
            DrawChoice(b, this.ContinueButton, ModEntry.T("ui.common.continue"), true);
        }

        string hint = this.controllerModeSeen
            ? ModEntry.T(this.IsChoiceBeat ? "capstone.hint.controller.choice" : "capstone.hint.controller")
            : ModEntry.T(this.IsChoiceBeat ? "capstone.hint.keyboard.choice" : "capstone.hint.keyboard");
        Vector2 hintSize = Game1.smallFont.MeasureString(hint);
        b.DrawString(Game1.smallFont, hint, new Vector2((viewport.Width - hintSize.X) / 2f, viewport.Height - 28), new Color(145, 128, 160));
        this.drawMouse(b);
    }

    private static void DrawChoice(SpriteBatch b, Rectangle rect, string text, bool selected)
    {
        Color fill = selected ? new Color(77, 105, 127) : new Color(40, 55, 69);
        Color border = selected ? new Color(226, 236, 242) : new Color(103, 133, 154);
        SudokuUiStyle.DrawRoundedPanel(b, rect, fill, border, selected ? 4 : 2, 10);
        string wrapped = WrapText(Game1.smallFont, text, rect.Width - 22);
        Vector2 size = Game1.smallFont.MeasureString(wrapped);
        b.DrawString(Game1.smallFont, wrapped, new Vector2(rect.Center.X - size.X / 2f, rect.Center.Y - size.Y / 2f), Color.White);
    }

    private static string WrapText(SpriteFont font, string text, float maxWidth)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        List<string> lines = new();
        foreach (string paragraph in text.Split('\n'))
        {
            string current = string.Empty;
            foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                string candidate = string.IsNullOrEmpty(current) ? word : current + " " + word;
                if (font.MeasureString(candidate).X <= maxWidth || string.IsNullOrEmpty(current))
                    current = candidate;
                else
                {
                    lines.Add(current);
                    current = word;
                }
            }

            if (!string.IsNullOrEmpty(current))
                lines.Add(current);
        }

        return string.Join("\n", lines);
    }

    private sealed record Beat(string TextKey, int PortraitIndex, bool IsChoice = false);
}
