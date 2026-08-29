using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace HeyYoureCursed;

internal sealed class OccultCabinetOption
{
    public string Label { get; init; } = string.Empty;
    public Action Action { get; init; } = () => { };
}

/// <summary>Controller-friendly management UI for the active/sealed haunting slots.</summary>
internal sealed class OccultCabinetMenu : IClickableMenu
{
    private readonly Texture2D? cabinetTexture;
    private readonly string title;
    private readonly string intro;
    private readonly string activeHeader;
    private readonly string activeText;
    private readonly string sealedHeader;
    private readonly string sealedText;
    private readonly OccultCabinetOption[] options;
    private readonly Action? onClosed;
    private int selectedIndex;
    private bool controllerModeSeen;

    public OccultCabinetMenu(Texture2D? cabinetTexture, string title, string intro, string activeHeader, string activeText, string sealedHeader, string sealedText, IEnumerable<OccultCabinetOption> options, Action? onClosed = null)
        : base(Game1.uiViewport.Width / 2 - GetMenuWidth() / 2, Game1.uiViewport.Height / 2 - GetMenuHeight() / 2, GetMenuWidth(), GetMenuHeight(), showUpperRightCloseButton: false)
    {
        this.cabinetTexture = cabinetTexture;
        this.title = title;
        this.intro = intro;
        this.activeHeader = activeHeader;
        this.activeText = activeText;
        this.sealedHeader = sealedHeader;
        this.sealedText = sealedText;
        this.options = options.Where(p => !string.IsNullOrWhiteSpace(p.Label)).Take(5).ToArray();
        this.onClosed = onClosed;
        this.selectedIndex = 0;
        this.controllerModeSeen = SudokuInputState.LastWasController;
    }

    private static int GetMenuWidth() => Math.Clamp(Game1.uiViewport.Width - 96, 680, 840);
    private static int GetMenuHeight() => Math.Clamp(Game1.uiViewport.Height - 110, 500, 620);
    private int OptionsX => this.xPositionOnScreen + 290;
    private int OptionsWidth => this.width - 330;
    private int OptionHeight => 48;
    private int OptionGap => 10;
    private int OptionsBottom => this.yPositionOnScreen + this.height - 48;
    private int OptionsTop => this.OptionsBottom - this.options.Length * this.OptionHeight - Math.Max(0, this.options.Length - 1) * this.OptionGap;
    private Rectangle GetOptionRect(int index) => new(this.OptionsX, this.OptionsTop + index * (this.OptionHeight + this.OptionGap), this.OptionsWidth, this.OptionHeight);

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        this.controllerModeSeen = false; SudokuInputState.LastWasController = false;
        for (int i = 0; i < this.options.Length; i++)
        {
            if (!this.GetOptionRect(i).Contains(x, y)) continue;
            this.selectedIndex = i; this.Activate(); return;
        }
    }

    public override void performHoverAction(int x, int y)
    {
        if (this.controllerModeSeen) return;
        for (int i = 0; i < this.options.Length; i++) if (this.GetOptionRect(i).Contains(x, y)) { this.selectedIndex = i; return; }
    }

    public override void receiveKeyPress(Keys key)
    {
        this.controllerModeSeen = false; SudokuInputState.LastWasController = false;
        switch (key)
        {
            case Keys.Up:
            case Keys.Left: this.Move(-1); return;
            case Keys.Down:
            case Keys.Right: this.Move(1); return;
            case Keys.Enter:
            case Keys.Space: this.Activate(); return;
            case Keys.Escape: this.Close(); return;
        }
        base.receiveKeyPress(key);
    }

    internal bool HandleSmapiInput(SButton button)
    {
        bool controller = button is SButton.ControllerA or SButton.ControllerB or SButton.DPadLeft or SButton.DPadRight or SButton.DPadUp or SButton.DPadDown or SButton.LeftThumbstickLeft or SButton.LeftThumbstickRight or SButton.LeftThumbstickUp or SButton.LeftThumbstickDown;
        if (!controller) return false;
        this.controllerModeSeen = true; SudokuInputState.LastWasController = true;
        switch (button)
        {
            case SButton.DPadUp:
            case SButton.DPadLeft:
            case SButton.LeftThumbstickUp:
            case SButton.LeftThumbstickLeft: this.Move(-1); return true;
            case SButton.DPadDown:
            case SButton.DPadRight:
            case SButton.LeftThumbstickDown:
            case SButton.LeftThumbstickRight: this.Move(1); return true;
            case SButton.ControllerA: this.Activate(); return true;
            case SButton.ControllerB: this.Close(); return true;
        }
        return false;
    }

    public override void receiveGamePadButton(Buttons button)
    {
        SButton? mapped = button switch { Buttons.DPadLeft => SButton.DPadLeft, Buttons.DPadRight => SButton.DPadRight, Buttons.DPadUp => SButton.DPadUp, Buttons.DPadDown => SButton.DPadDown, Buttons.A => SButton.ControllerA, Buttons.B => SButton.ControllerB, _ => null };
        if (mapped.HasValue && this.HandleSmapiInput(mapped.Value)) return;
        base.receiveGamePadButton(button);
    }

    public override void draw(SpriteBatch b)
    {
        Rectangle panel = new(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height);
        SudokuUiStyle.DrawRoundedPanel(b, panel, new Color(18, 24, 34) * 0.98f, new Color(108, 86, 130), 4, 16);
        b.DrawString(Game1.dialogueFont, this.title, new Vector2(this.xPositionOnScreen + 30, this.yPositionOnScreen + 22), new Color(232, 222, 240));
        Rectangle cabinetBox = new(this.xPositionOnScreen + 34, this.yPositionOnScreen + 88, 210, 330);
        SudokuUiStyle.DrawRoundedPanel(b, cabinetBox, new Color(11, 12, 18), new Color(75, 63, 90), 2, 12);
        if (this.cabinetTexture is not null)
        {
            Rectangle source = new(0, 0, Math.Min(16, this.cabinetTexture.Width), Math.Min(32, this.cabinetTexture.Height));
            int scale = 6; int drawWidth = source.Width * scale; int drawHeight = source.Height * scale;
            Rectangle destination = new(cabinetBox.Center.X - drawWidth / 2, cabinetBox.Y + 28, drawWidth, drawHeight);
            b.Draw(this.cabinetTexture, destination, source, Color.White);
        }
        string wrappedIntro = WrapText(Game1.smallFont, this.intro, cabinetBox.Width - 28);
        b.DrawString(Game1.smallFont, wrappedIntro, new Vector2(cabinetBox.X + 14, cabinetBox.Bottom - 106), new Color(180, 184, 196));
        int infoX = this.OptionsX; int infoWidth = this.OptionsWidth; int activeY = this.yPositionOnScreen + 92;
        DrawStatusBlock(b, new Rectangle(infoX, activeY, infoWidth, 92), this.activeHeader, this.activeText);
        DrawStatusBlock(b, new Rectangle(infoX, activeY + 106, infoWidth, 92), this.sealedHeader, this.sealedText);
        for (int i = 0; i < this.options.Length; i++)
        {
            Rectangle rect = this.GetOptionRect(i); bool selected = i == this.selectedIndex;
            SudokuUiStyle.DrawRoundedPanel(b, rect, selected ? new Color(82, 68, 100) : new Color(42, 48, 60), selected ? new Color(232, 220, 242) : new Color(102, 105, 122), selected ? 3 : 2, 10);
            Vector2 size = Game1.smallFont.MeasureString(this.options[i].Label);
            b.DrawString(Game1.smallFont, this.options[i].Label, new Vector2(rect.X + 18, rect.Center.Y - size.Y / 2), selected ? Color.White : new Color(205, 207, 216));
        }
        this.drawMouse(b);
    }

    private static void DrawStatusBlock(SpriteBatch b, Rectangle rect, string header, string text)
    {
        SudokuUiStyle.DrawRoundedPanel(b, rect, new Color(25, 29, 39), new Color(72, 78, 94), 2, 10);
        b.DrawString(Game1.smallFont, header, new Vector2(rect.X + 14, rect.Y + 10), new Color(154, 139, 177));
        string wrapped = WrapText(Game1.smallFont, text, rect.Width - 28);
        b.DrawString(Game1.smallFont, wrapped, new Vector2(rect.X + 14, rect.Y + 38), new Color(220, 222, 229));
    }

    private void Move(int direction) { if (this.options.Length == 0) return; this.selectedIndex = (this.selectedIndex + direction + this.options.Length) % this.options.Length; Game1.playSound("shiny4"); }
    private void Activate() { if (this.options.Length == 0) return; OccultCabinetOption option = this.options[this.selectedIndex]; Game1.playSound("smallSelect"); Game1.activeClickableMenu = null; option.Action(); }
    private void Close() { Game1.playSound("bigDeSelect"); Game1.activeClickableMenu = null; this.onClosed?.Invoke(); }

    private static string WrapText(SpriteFont font, string text, float maxWidth)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        List<string> output = new();
        foreach (string paragraph in text.Split('\n'))
        {
            string current = string.Empty;
            foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                string candidate = string.IsNullOrEmpty(current) ? word : current + " " + word;
                if (font.MeasureString(candidate).X <= maxWidth || string.IsNullOrEmpty(current)) { current = candidate; continue; }
                output.Add(current); current = word;
            }
            if (!string.IsNullOrEmpty(current)) output.Add(current); else if (string.IsNullOrEmpty(paragraph)) output.Add(string.Empty);
        }
        return string.Join("\n", output);
    }
}
