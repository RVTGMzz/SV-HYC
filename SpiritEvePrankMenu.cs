using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace HeyYoureCursed;

/// <summary>A short visible Spirit's Eve payoff for the prank plan chosen with Sudoku.</summary>
internal sealed class SpiritEvePrankMenu : IClickableMenu
{
    private const int PortraitFrameSize = 64;
    private const int BeatDurationTicks = 210;

    private readonly Texture2D? sudokuPortraits;
    private readonly Action onComplete;
    private readonly Beat[] beats;
    private readonly string mode;

    private Texture2D? lewisPortrait;
    private int beatIndex;
    private int beatTicks;
    private bool controllerModeSeen;
    private bool completed;

    public SpiritEvePrankMenu(Texture2D? sudokuPortraits, string mode, Action onComplete)
        : base(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height, showUpperRightCloseButton: false)
    {
        this.sudokuPortraits = sudokuPortraits;
        this.mode = mode is "free" or "lewis" ? mode : "gentle";
        this.onComplete = onComplete;
        this.controllerModeSeen = SudokuInputState.LastWasController;
        this.beats = BuildBeats(this.mode);

        if (this.mode == "lewis")
        {
            try
            {
                this.lewisPortrait = Game1.content.Load<Texture2D>("Portraits/Lewis");
            }
            catch
            {
                this.lewisPortrait = null;
            }
        }
    }

    public override void update(GameTime time)
    {
        base.update(time);
        if (this.completed)
            return;

        this.beatTicks++;
        if (this.beatTicks >= BeatDurationTicks)
            this.Advance();
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        this.controllerModeSeen = false;
        SudokuInputState.LastWasController = false;
        this.Advance();
    }

    public override void receiveKeyPress(Keys key)
    {
        this.controllerModeSeen = false;
        SudokuInputState.LastWasController = false;

        if (key is Keys.Escape)
        {
            this.Finish();
            return;
        }

        if (key is Keys.Enter or Keys.Space)
        {
            this.Advance();
            return;
        }

        base.receiveKeyPress(key);
    }

    internal bool HandleSmapiInput(SButton button)
    {
        if (button is not (SButton.ControllerA or SButton.ControllerB))
            return false;

        this.controllerModeSeen = true;
        SudokuInputState.LastWasController = true;

        if (button == SButton.ControllerB)
            this.Finish();
        else
            this.Advance();

        return true;
    }

    public override void receiveGamePadButton(Buttons button)
    {
        SButton? mapped = button switch
        {
            Buttons.A => SButton.ControllerA,
            Buttons.B => SButton.ControllerB,
            _ => null
        };

        if (mapped.HasValue && this.HandleSmapiInput(mapped.Value))
            return;

        base.receiveGamePadButton(button);
    }

    private void Advance()
    {
        if (this.completed)
            return;

        Game1.playSound(this.beatIndex is 1 or 4 ? "ghost" : "smallSelect");
        this.beatIndex++;
        this.beatTicks = 0;
        if (this.beatIndex >= this.beats.Length)
            this.Finish();
    }

    private void Finish()
    {
        if (this.completed)
            return;

        this.completed = true;
        Game1.activeClickableMenu = null;
        this.onComplete();
    }

    public override void draw(SpriteBatch b)
    {
        Rectangle viewport = new(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height);
        b.Draw(Game1.staminaRect, viewport, new Color(8, 5, 12) * 0.96f);

        Beat beat = this.beats[Math.Clamp(this.beatIndex, 0, this.beats.Length - 1)];
        float beatProgress = Math.Clamp(this.beatTicks / (float)BeatDurationTicks, 0f, 1f);
        float pulse = 0.45f + MathF.Sin(beatProgress * MathF.PI) * 0.55f;

        this.DrawStatic(b, viewport, beat.StaticStrength * pulse);
        this.DrawMoonAndLanterns(b, viewport, pulse);
        this.DrawCharacters(b, viewport, beat, pulse);

        Rectangle textPanel = new(
            Math.Max(28, viewport.Width / 2 - Math.Min(430, viewport.Width / 2 - 34)),
            viewport.Height - Math.Min(190, viewport.Height / 3),
            Math.Min(860, viewport.Width - 56),
            Math.Min(132, viewport.Height / 4)
        );
        SudokuUiStyle.DrawRoundedPanel(
            b,
            textPanel,
            new Color(14, 11, 20) * 0.94f,
            new Color(125, 89, 145),
            borderThickness: 3,
            radius: 12
        );

        string title = ModEntry.T("spiriteve.scene.title");
        b.DrawString(
            Game1.dialogueFont,
            title,
            new Vector2(textPanel.X + 24, textPanel.Y + 14),
            new Color(231, 217, 240)
        );

        string wrapped = WrapText(Game1.smallFont, ModEntry.T(beat.TextKey), textPanel.Width - 48);
        b.DrawString(
            Game1.smallFont,
            wrapped,
            new Vector2(textPanel.X + 24, textPanel.Y + 58),
            new Color(226, 225, 232)
        );

        string hint = this.controllerModeSeen
            ? ModEntry.T("spiriteve.scene.hint.controller")
            : ModEntry.T("spiriteve.scene.hint.keyboard");
        Vector2 hintSize = Game1.smallFont.MeasureString(hint);
        b.DrawString(
            Game1.smallFont,
            hint,
            new Vector2((viewport.Width - hintSize.X) / 2f, viewport.Height - 30),
            new Color(150, 133, 163)
        );

        string progress = string.Join("  ", Enumerable.Range(0, this.beats.Length).Select(i => i == this.beatIndex ? "●" : "○"));
        Vector2 progressSize = Game1.smallFont.MeasureString(progress);
        b.DrawString(
            Game1.smallFont,
            progress,
            new Vector2((viewport.Width - progressSize.X) / 2f, textPanel.Y - 28),
            new Color(188, 163, 204)
        );

        this.drawMouse(b);
    }

    private void DrawStatic(SpriteBatch b, Rectangle viewport, float strength)
    {
        Random random = new((this.beatIndex + 1) * 7919 + this.beatTicks * 31);
        int bars = 18 + (int)(strength * 36f);
        for (int i = 0; i < bars; i++)
        {
            int x = random.Next(0, Math.Max(1, viewport.Width));
            int y = random.Next(0, Math.Max(1, viewport.Height));
            int width = random.Next(12, Math.Max(13, viewport.Width / 3));
            int height = random.Next(1, 5);
            Color tint = i % 4 == 0 ? new Color(191, 224, 235) : new Color(115, 84, 139);
            b.Draw(Game1.staminaRect, new Rectangle(x, y, width, height), tint * (0.04f + strength * 0.17f));
        }
    }

    private void DrawMoonAndLanterns(SpriteBatch b, Rectangle viewport, float pulse)
    {
        Rectangle moon = new(viewport.Width - 170, 54, 82, 82);
        b.Draw(Game1.staminaRect, moon, new Color(213, 198, 154) * (0.28f + pulse * 0.15f));

        for (int i = 0; i < 5; i++)
        {
            int x = 58 + i * Math.Max(90, (viewport.Width - 120) / 5);
            Rectangle lantern = new(x, 72 + (i % 2) * 12, 22, 30);
            b.Draw(Game1.staminaRect, lantern, new Color(181, 88, 49) * (0.48f + pulse * 0.20f));
        }
    }

    private void DrawCharacters(SpriteBatch b, Rectangle viewport, Beat beat, float pulse)
    {
        int portraitSize = Math.Clamp(Math.Min(viewport.Width, viewport.Height) / 3, 150, 250);
        Rectangle sudokuRect = new(
            viewport.Width / 2 - portraitSize / 2,
            Math.Max(118, viewport.Height / 2 - portraitSize / 2 - 45),
            portraitSize,
            portraitSize
        );

        if (beat.ShowLewis && this.lewisPortrait is not null)
        {
            Rectangle lewisRect = new(sudokuRect.X - portraitSize - 42, sudokuRect.Y + 22, portraitSize, portraitSize);
            b.Draw(this.lewisPortrait, lewisRect, new Rectangle(0, 0, PortraitFrameSize, PortraitFrameSize), Color.White * 0.92f);
        }

        if (this.sudokuPortraits is null)
            return;

        Rectangle source = new(
            (beat.PortraitIndex % 4) * PortraitFrameSize,
            (beat.PortraitIndex / 4) * PortraitFrameSize,
            PortraitFrameSize,
            PortraitFrameSize
        );

        int echoes = beat.EchoCount;
        for (int i = echoes; i >= 1; i--)
        {
            int direction = i % 2 == 0 ? -1 : 1;
            Rectangle echo = new(
                sudokuRect.X + direction * i * 22,
                sudokuRect.Y - i * 3,
                sudokuRect.Width,
                sudokuRect.Height
            );
            b.Draw(this.sudokuPortraits, echo, source, new Color(115, 177, 210) * (0.06f + pulse * 0.08f));
        }

        b.Draw(this.sudokuPortraits, sudokuRect, source, Color.White * (0.76f + pulse * 0.22f));
    }

    private static Beat[] BuildBeats(string mode)
    {
        string prefix = "spiriteve.scene." + mode + ".";
        return mode switch
        {
            "free" => new[]
            {
                new Beat(prefix + "1", 4, false, 1, 0.30f),
                new Beat(prefix + "2", 6, false, 3, 0.72f),
                new Beat(prefix + "3", 3, false, 4, 0.95f),
                new Beat(prefix + "4", 6, false, 5, 1.00f),
                new Beat(prefix + "5", 4, false, 2, 0.54f),
                new Beat(prefix + "6", 3, false, 1, 0.28f)
            },
            "lewis" => new[]
            {
                new Beat(prefix + "1", 4, true, 1, 0.25f),
                new Beat(prefix + "2", 6, true, 2, 0.58f),
                new Beat(prefix + "3", 6, true, 3, 0.86f),
                new Beat(prefix + "4", 3, true, 4, 0.92f),
                new Beat(prefix + "5", 4, true, 2, 0.48f),
                new Beat(prefix + "6", 3, false, 1, 0.24f)
            },
            _ => new[]
            {
                new Beat(prefix + "1", 4, false, 1, 0.20f),
                new Beat(prefix + "2", 0, false, 1, 0.28f),
                new Beat(prefix + "3", 4, false, 2, 0.46f),
                new Beat(prefix + "4", 3, false, 2, 0.55f),
                new Beat(prefix + "5", 4, false, 1, 0.30f),
                new Beat(prefix + "6", 3, false, 1, 0.20f)
            }
        };
    }

    private static string WrapText(SpriteFont font, string text, float maxWidth)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        List<string> lines = new();
        foreach (string paragraph in text.Split('\n'))
        {
            string line = string.Empty;
            foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                string candidate = string.IsNullOrEmpty(line) ? word : line + " " + word;
                if (font.MeasureString(candidate).X <= maxWidth || string.IsNullOrEmpty(line))
                    line = candidate;
                else
                {
                    lines.Add(line);
                    line = word;
                }
            }

            if (!string.IsNullOrEmpty(line))
                lines.Add(line);
        }

        return string.Join("\n", lines);
    }

    private sealed record Beat(string TextKey, int PortraitIndex, bool ShowLewis, int EchoCount, float StaticStrength);
}
