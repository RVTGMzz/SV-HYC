using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace HeyYoureCursed;

/// <summary>Seven-day story beat where the Wizard admits his part and unlocks the Occult Cabinet.</summary>
internal sealed class WizardRevealMenu : IClickableMenu
{
    private const int PortraitFrameSize = 64;
    private const int ChoiceBeatIndex = 4;
    private readonly Action<string> onChoice;
    private readonly Action onComplete;
    private readonly Beat[] beats;
    private int beatIndex;
    private int selectedChoice;
    private bool controllerModeSeen;
    private Texture2D? wizardPortrait;

    public WizardRevealMenu(Action<string> onChoice, Action onComplete)
        : base(Game1.uiViewport.Width / 2 - GetMenuWidth() / 2, Game1.uiViewport.Height / 2 - GetMenuHeight() / 2, GetMenuWidth(), GetMenuHeight(), showUpperRightCloseButton: false)
    {
        this.onChoice = onChoice;
        this.onComplete = onComplete;
        this.controllerModeSeen = SudokuInputState.LastWasController;
        this.beats = new[]
        {
            new Beat(string.Empty, null, "story.wizard7.narration.1"),
            new Beat(ModEntry.T("story.prologue.speaker.wizard"), "Portraits/Wizard", "story.wizard7.line.1"),
            new Beat(ModEntry.T("story.prologue.speaker.wizard"), "Portraits/Wizard", "story.wizard7.line.2"),
            new Beat(ModEntry.T("story.prologue.speaker.wizard"), "Portraits/Wizard", "story.wizard7.line.3"),
            new Beat(Game1.player.Name, null, "story.wizard7.choice.prompt", true),
            new Beat(ModEntry.T("story.prologue.speaker.wizard"), "Portraits/Wizard", "story.wizard7.line.4"),
            new Beat(ModEntry.T("story.prologue.speaker.wizard"), "Portraits/Wizard", "story.wizard7.line.5"),
            new Beat(ModEntry.T("story.prologue.speaker.wizard"), "Portraits/Wizard", "story.wizard7.line.6"),
            new Beat(string.Empty, null, "story.wizard7.narration.cabinet"),
            new Beat(ModEntry.T("story.prologue.speaker.wizard"), "Portraits/Wizard", "story.wizard7.line.final")
        };
    }

    private static int GetMenuWidth() => Math.Clamp(Game1.uiViewport.Width - 96, 640, 820);
    private static int GetMenuHeight() => Math.Clamp(Game1.uiViewport.Height - 150, 420, 540);
    private bool IsChoiceBeat => this.beatIndex == ChoiceBeatIndex;
    private Rectangle ContinueButton => new(this.xPositionOnScreen + this.width / 2 - 95, this.yPositionOnScreen + this.height - 82, 190, 50);
    private Rectangle ChoiceLeft => new(this.xPositionOnScreen + 30, this.yPositionOnScreen + this.height - 108, this.width / 3 - 38, 76);
    private Rectangle ChoiceMiddle => new(this.xPositionOnScreen + this.width / 3 + 8, this.yPositionOnScreen + this.height - 108, this.width / 3 - 16, 76);
    private Rectangle ChoiceRight => new(this.xPositionOnScreen + this.width * 2 / 3 + 8, this.yPositionOnScreen + this.height - 108, this.width / 3 - 38, 76);

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        this.controllerModeSeen = false; SudokuInputState.LastWasController = false;
        if (this.IsChoiceBeat)
        {
            if (this.ChoiceLeft.Contains(x, y)) { this.selectedChoice = 0; this.AdvanceChoice(); }
            else if (this.ChoiceMiddle.Contains(x, y)) { this.selectedChoice = 1; this.AdvanceChoice(); }
            else if (this.ChoiceRight.Contains(x, y)) { this.selectedChoice = 2; this.AdvanceChoice(); }
            return;
        }
        if (this.ContinueButton.Contains(x, y)) this.Advance();
    }

    public override void performHoverAction(int x, int y)
    {
        if (this.controllerModeSeen || !this.IsChoiceBeat) return;
        if (this.ChoiceLeft.Contains(x, y)) this.selectedChoice = 0;
        else if (this.ChoiceMiddle.Contains(x, y)) this.selectedChoice = 1;
        else if (this.ChoiceRight.Contains(x, y)) this.selectedChoice = 2;
    }

    public override void receiveKeyPress(Keys key)
    {
        this.controllerModeSeen = false; SudokuInputState.LastWasController = false;
        if (this.IsChoiceBeat)
        {
            if (key is Keys.Left or Keys.Up) { this.ChangeChoice(-1); return; }
            if (key is Keys.Right or Keys.Down) { this.ChangeChoice(1); return; }
            if (key is Keys.Enter or Keys.Space) { this.AdvanceChoice(); return; }
            return;
        }
        if (key is Keys.Enter or Keys.Space) this.Advance();
    }

    internal bool HandleSmapiInput(SButton button)
    {
        bool controller = button is SButton.ControllerA or SButton.ControllerB or SButton.DPadLeft or SButton.DPadRight or SButton.DPadUp or SButton.DPadDown or SButton.LeftThumbstickLeft or SButton.LeftThumbstickRight or SButton.LeftThumbstickUp or SButton.LeftThumbstickDown;
        if (!controller) return false;
        this.controllerModeSeen = true; SudokuInputState.LastWasController = true;
        if (this.IsChoiceBeat)
        {
            if (button is SButton.DPadLeft or SButton.DPadUp or SButton.LeftThumbstickLeft or SButton.LeftThumbstickUp) { this.ChangeChoice(-1); return true; }
            if (button is SButton.DPadRight or SButton.DPadDown or SButton.LeftThumbstickRight or SButton.LeftThumbstickDown) { this.ChangeChoice(1); return true; }
            if (button == SButton.ControllerA) { this.AdvanceChoice(); return true; }
            if (button == SButton.ControllerB) return true;
            return false;
        }
        if (button == SButton.ControllerA) { this.Advance(); return true; }
        if (button == SButton.ControllerB) return true;
        return false;
    }

    public override void receiveGamePadButton(Buttons button)
    {
        SButton? mapped = button switch { Buttons.DPadLeft => SButton.DPadLeft, Buttons.DPadRight => SButton.DPadRight, Buttons.DPadUp => SButton.DPadUp, Buttons.DPadDown => SButton.DPadDown, Buttons.A => SButton.ControllerA, Buttons.B => SButton.ControllerB, _ => null };
        if (mapped.HasValue && this.HandleSmapiInput(mapped.Value)) return;
        base.receiveGamePadButton(button);
    }

    private void ChangeChoice(int delta) { int old = this.selectedChoice; this.selectedChoice = (this.selectedChoice + delta + 3) % 3; if (old != this.selectedChoice) Game1.playSound("shiny4"); }
    private void AdvanceChoice() { string choice = this.selectedChoice switch { 0 => "learned", 1 => "accuse", _ => "attached" }; this.onChoice(choice); Game1.playSound("smallSelect"); this.beatIndex++; }
    private void Advance() { Game1.playSound("smallSelect"); this.beatIndex++; if (this.beatIndex < this.beats.Length) return; Game1.activeClickableMenu = null; this.onComplete(); }

    public override void draw(SpriteBatch b)
    {
        b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.60f);
        Rectangle panel = new(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);
        SudokuUiStyle.DrawRoundedPanel(b, panel, new Color(24, 19, 31) * 0.98f, new Color(112, 76, 137), 4, 16);
        Beat beat = this.beats[Math.Clamp(this.beatIndex, 0, this.beats.Length - 1)]; this.EnsurePortrait(beat.PortraitAsset);
        string header = string.IsNullOrWhiteSpace(beat.Speaker) ? ModEntry.T("story.wizard7.header") : beat.Speaker;
        b.DrawString(Game1.dialogueFont, header, new Vector2(this.xPositionOnScreen + 32, this.yPositionOnScreen + 24), new Color(231, 218, 241));
        int bodyX = this.xPositionOnScreen + 38; int bodyWidth = this.width - 76; int bodyY = this.yPositionOnScreen + 92;
        if (this.wizardPortrait is not null)
        {
            Rectangle portraitBox = new(this.xPositionOnScreen + 36, bodyY, 176, 176);
            SudokuUiStyle.DrawRoundedPanel(b, portraitBox, new Color(17, 13, 22), new Color(85, 58, 104), 2, 10);
            b.Draw(this.wizardPortrait, portraitBox, new Rectangle(0, 0, PortraitFrameSize, PortraitFrameSize), Color.White);
            bodyX = portraitBox.Right + 28; bodyWidth = this.xPositionOnScreen + this.width - 38 - bodyX;
        }
        string wrapped = WrapText(Game1.dialogueFont, ModEntry.T(beat.TextKey), bodyWidth);
        b.DrawString(Game1.dialogueFont, wrapped, new Vector2(bodyX, bodyY + 6), new Color(236, 228, 240));
        if (this.IsChoiceBeat)
        {
            this.DrawChoice(b, this.ChoiceLeft, ModEntry.T("story.wizard7.choice.learned"), this.selectedChoice == 0);
            this.DrawChoice(b, this.ChoiceMiddle, ModEntry.T("story.wizard7.choice.accuse"), this.selectedChoice == 1);
            this.DrawChoice(b, this.ChoiceRight, ModEntry.T("story.wizard7.choice.attached"), this.selectedChoice == 2);
        }
        else this.DrawChoice(b, this.ContinueButton, ModEntry.T("ui.common.continue"), true);
        string hint = this.controllerModeSeen ? (this.IsChoiceBeat ? ModEntry.T("story.wizard7.hint.controller.choice") : ModEntry.T("story.wizard7.hint.controller")) : (this.IsChoiceBeat ? ModEntry.T("story.wizard7.hint.keyboard.choice") : ModEntry.T("story.wizard7.hint.keyboard"));
        Vector2 hintSize = Game1.smallFont.MeasureString(hint);
        b.DrawString(Game1.smallFont, hint, new Vector2(this.xPositionOnScreen + (this.width - hintSize.X) / 2, this.yPositionOnScreen + this.height - 25), new Color(149, 132, 163));
        this.drawMouse(b);
    }

    private void DrawChoice(SpriteBatch b, Rectangle rect, string text, bool selected)
    {
        SudokuUiStyle.DrawRoundedPanel(b, rect, selected ? new Color(83, 60, 99) : new Color(42, 33, 51), selected ? new Color(223, 207, 234) : new Color(94, 75, 107), selected ? 4 : 2, 10);
        string wrapped = WrapText(Game1.smallFont, text, rect.Width - 20); Vector2 size = Game1.smallFont.MeasureString(wrapped);
        b.DrawString(Game1.smallFont, wrapped, new Vector2(rect.Center.X - size.X / 2, rect.Center.Y - size.Y / 2), Color.White);
    }

    private void EnsurePortrait(string? assetName)
    {
        if (assetName is null) { this.wizardPortrait = null; return; }
        if (this.wizardPortrait is not null) return;
        try { this.wizardPortrait = Game1.content.Load<Texture2D>(assetName); } catch { this.wizardPortrait = null; }
    }

    private static string WrapText(SpriteFont font, string text, float maxWidth)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        List<string> lines = new();
        foreach (string paragraph in text.Split('\n'))
        {
            string line = string.Empty;
            foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                string candidate = string.IsNullOrEmpty(line) ? word : line + " " + word;
                if (font.MeasureString(candidate).X <= maxWidth || string.IsNullOrEmpty(line)) line = candidate;
                else { lines.Add(line); line = word; }
            }
            if (!string.IsNullOrEmpty(line)) lines.Add(line); else if (string.IsNullOrEmpty(paragraph)) lines.Add(string.Empty);
        }
        return string.Join("\n", lines);
    }

    private sealed record Beat(string Speaker, string? PortraitAsset, string TextKey, bool IsChoice = false);
}
