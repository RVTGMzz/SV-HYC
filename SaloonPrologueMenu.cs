using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace HeyYoureCursed;

/// <summary>
/// Lightweight story-event UI for the persistent Stardrop Saloon prologue.
/// The farmer must actually attend before the cursed VHS story can begin.
/// </summary>
internal sealed class SaloonPrologueMenu : IClickableMenu
{
    private const int PortraitFrameSize = 64;
    private const int ChoiceBeatIndex = 3;

    private readonly Action<string> onChoice;
    private readonly Action onComplete;
    private readonly Beat[] beats;

    private int beatIndex;
    private int selectedChoice;
    private bool controllerModeSeen;
    private Texture2D? portrait;
    private string? loadedPortraitAsset;

    public SaloonPrologueMenu(Action<string> onChoice, Action onComplete)
        : base(
            Game1.uiViewport.Width / 2 - GetMenuWidth() / 2,
            Game1.uiViewport.Height / 2 - GetMenuHeight() / 2,
            GetMenuWidth(),
            GetMenuHeight(),
            showUpperRightCloseButton: false
        )
    {
        this.onChoice = onChoice;
        this.onComplete = onComplete;
        this.controllerModeSeen = SudokuInputState.LastWasController;

        this.beats = new[]
        {
            new Beat("Gus", "Portraits/Gus", "story.prologue.saloon.gus.1"),
            new Beat("Abigail", "Portraits/Abigail", "story.prologue.saloon.abigail.1"),
            new Beat("Pam", "Portraits/Pam", "story.prologue.saloon.pam.1"),
            new Beat(Game1.player.Name, null, "story.prologue.saloon.farmer.prompt", isChoice: true),
            new Beat("Gus", "Portraits/Gus", "story.prologue.saloon.gus.warning"),
            new Beat("Abigail", "Portraits/Abigail", "story.prologue.saloon.abigail.warning"),
            new Beat(string.Empty, null, "story.prologue.saloon.narration.wizard"),
            new Beat(ModEntry.T("story.prologue.speaker.wizard"), "Portraits/Wizard", "story.prologue.saloon.wizard.1"),
            new Beat(string.Empty, null, "story.prologue.saloon.narration.end")
        };
    }

    private static int GetMenuWidth() => Math.Clamp(Game1.uiViewport.Width - 96, 640, 820);
    private static int GetMenuHeight() => Math.Clamp(Game1.uiViewport.Height - 150, 420, 540);

    private bool IsChoiceBeat => this.beatIndex == ChoiceBeatIndex;

    private Rectangle ContinueButton => new(
        this.xPositionOnScreen + this.width / 2 - 95,
        this.yPositionOnScreen + this.height - 82,
        190,
        50
    );

    private Rectangle ChoiceLeft => new(
        this.xPositionOnScreen + 42,
        this.yPositionOnScreen + this.height - 100,
        this.width / 2 - 54,
        68
    );

    private Rectangle ChoiceRight => new(
        this.xPositionOnScreen + this.width / 2 + 12,
        this.yPositionOnScreen + this.height - 100,
        this.width / 2 - 54,
        68
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
                    this.AdvanceChoice();
                    return;
            }

            return;
        }

        if (key is Keys.Enter or Keys.Space)
        {
            this.Advance();
            return;
        }
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
        SudokuInputState.LastWasController = true;

        if (this.IsChoiceBeat)
        {
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
                    this.AdvanceChoice();
                    return true;
                case SButton.ControllerB:
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
            return true;

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
        string choice = this.selectedChoice == 0 ? "never" : "doubt";
        this.onChoice(choice);
        Game1.playSound("smallSelect");
        this.beatIndex++;
        this.loadedPortraitAsset = null;
        this.portrait = null;
    }

    private void Advance()
    {
        Game1.playSound("smallSelect");
        this.beatIndex++;
        this.loadedPortraitAsset = null;
        this.portrait = null;

        if (this.beatIndex < this.beats.Length)
            return;

        Game1.activeClickableMenu = null;
        this.onComplete();
    }

    public override void draw(SpriteBatch b)
    {
        b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.55f);

        Rectangle panel = new(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);
        SudokuUiStyle.DrawRoundedPanel(
            b,
            panel,
            new Color(35, 25, 24) * 0.98f,
            new Color(164, 111, 72),
            borderThickness: 4,
            radius: 16
        );

        Beat beat = this.beats[Math.Clamp(this.beatIndex, 0, this.beats.Length - 1)];
        this.EnsurePortrait(beat.PortraitAsset);

        string header = string.IsNullOrWhiteSpace(beat.Speaker)
            ? ModEntry.T("story.prologue.saloon.header")
            : beat.Speaker;

        b.DrawString(
            Game1.dialogueFont,
            header,
            new Vector2(this.xPositionOnScreen + 32, this.yPositionOnScreen + 24),
            new Color(246, 226, 198)
        );

        int bodyX = this.xPositionOnScreen + 38;
        int bodyWidth = this.width - 76;
        int bodyY = this.yPositionOnScreen + 92;

        if (this.portrait is not null)
        {
            Rectangle portraitBox = new(this.xPositionOnScreen + 36, bodyY, 176, 176);
            SudokuUiStyle.DrawRoundedPanel(
                b,
                portraitBox,
                new Color(20, 15, 15),
                new Color(115, 79, 58),
                borderThickness: 2,
                radius: 10
            );

            Rectangle source = new(0, 0, PortraitFrameSize, PortraitFrameSize);
            b.Draw(this.portrait, portraitBox, source, Color.White);
            bodyX = portraitBox.Right + 28;
            bodyWidth = this.xPositionOnScreen + this.width - 38 - bodyX;
        }

        string text = ModEntry.T(beat.TextKey);
        string wrapped = WrapText(Game1.dialogueFont, text, bodyWidth);
        b.DrawString(
            Game1.dialogueFont,
            wrapped,
            new Vector2(bodyX, bodyY + 6),
            new Color(237, 225, 213)
        );

        if (this.IsChoiceBeat)
        {
            this.DrawChoiceButton(b, this.ChoiceLeft, ModEntry.T("story.prologue.saloon.choice.never"), this.selectedChoice == 0);
            this.DrawChoiceButton(b, this.ChoiceRight, ModEntry.T("story.prologue.saloon.choice.doubt"), this.selectedChoice == 1);
        }
        else
        {
            SudokuUiStyle.DrawRoundedPanel(
                b,
                this.ContinueButton,
                new Color(117, 78, 58),
                new Color(246, 221, 190),
                borderThickness: 3,
                radius: 10
            );

            string label = this.beatIndex == this.beats.Length - 1
                ? ModEntry.T("story.prologue.saloon.finish")
                : ModEntry.T("story.prologue.saloon.continue");
            DrawCenteredText(b, this.ContinueButton, label, Color.White);
        }

        string hint = this.controllerModeSeen
            ? ModEntry.T("story.prologue.saloon.hint.controller")
            : ModEntry.T("story.prologue.saloon.hint.keyboard");
        Vector2 hintSize = Game1.smallFont.MeasureString(hint);
        b.DrawString(
            Game1.smallFont,
            hint,
            new Vector2(this.xPositionOnScreen + (this.width - hintSize.X) / 2, this.yPositionOnScreen + this.height - 24),
            new Color(180, 151, 128)
        );

        this.drawMouse(b);
    }

    private void DrawChoiceButton(SpriteBatch b, Rectangle rect, string text, bool selected)
    {
        Color fill = selected ? new Color(116, 77, 58) : new Color(70, 50, 43);
        Color border = selected ? new Color(245, 221, 192) : new Color(137, 96, 72);
        SudokuUiStyle.DrawRoundedPanel(
            b,
            rect,
            fill,
            border,
            borderThickness: selected ? 3 : 2,
            radius: 10
        );

        string wrapped = WrapText(Game1.smallFont, text, rect.Width - 24);
        Vector2 size = Game1.smallFont.MeasureString(wrapped);
        b.DrawString(
            Game1.smallFont,
            wrapped,
            new Vector2(rect.Center.X - size.X / 2, rect.Center.Y - size.Y / 2),
            Color.White
        );
    }

    private void EnsurePortrait(string? asset)
    {
        if (asset == this.loadedPortraitAsset)
            return;

        this.loadedPortraitAsset = asset;
        this.portrait = null;

        if (string.IsNullOrWhiteSpace(asset))
            return;

        try
        {
            this.portrait = Game1.content.Load<Texture2D>(asset);
        }
        catch
        {
            this.portrait = null;
        }
    }

    private static void DrawCenteredText(SpriteBatch b, Rectangle rect, string text, Color color)
    {
        Vector2 size = Game1.smallFont.MeasureString(text);
        b.DrawString(
            Game1.smallFont,
            text,
            new Vector2(rect.Center.X - size.X / 2, rect.Center.Y - size.Y / 2),
            color
        );
    }

    private static string WrapText(SpriteFont font, string text, float maxWidth)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        List<string> output = new();
        foreach (string paragraph in text.Split('^'))
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

    private sealed class Beat
    {
        public string Speaker { get; }
        public string? PortraitAsset { get; }
        public string TextKey { get; }
        public bool IsChoice { get; }

        public Beat(string speaker, string? portraitAsset, string textKey, bool isChoice = false)
        {
            this.Speaker = speaker;
            this.PortraitAsset = portraitAsset;
            this.TextKey = textKey;
            this.IsChoice = isChoice;
        }
    }
}
