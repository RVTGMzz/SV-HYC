using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Objects;
using StardewValley.Locations;

namespace CursedSignal;

internal sealed partial class ModEntry
{
    private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
    {
        if (!this.sequenceActive || this.tvArrivalSheet is null)
            return;

        int staticEnd = Math.Max(1, this.Config.StaticTicks);
        int wellEnd = staticEnd + Math.Max(1, this.Config.WellTicks);
        int glitchEnd = wellEnd + Math.Max(1, this.Config.GlitchTicks);

        this.DrawDarkBackdrop(e.SpriteBatch);

        if (this.elapsedTicks < staticEnd)
        {
            this.DrawStatic(e.SpriteBatch, 1f);
            return;
        }

        if (this.elapsedTicks < wellEnd)
        {
            this.DrawWellGlimpse(e.SpriteBatch);
            if ((this.elapsedTicks / 5) % 2 == 0)
                this.DrawStatic(e.SpriteBatch, 0.22f);
            return;
        }

        if (this.elapsedTicks < glitchEnd)
        {
            this.DrawWellGlimpse(e.SpriteBatch);
            this.DrawGlitch(e.SpriteBatch);
            return;
        }

        this.DrawEmergenceFrame(e.SpriteBatch);
    }

    private void DrawDarkBackdrop(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            Game1.staminaRect,
            new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height),
            Color.Black * 0.68f
        );
    }

    private void DrawStatic(SpriteBatch spriteBatch, float strength)
    {
        Random random = new(this.elapsedTicks * 7919 + 17);
        int width = Game1.viewport.Width;
        int height = Game1.viewport.Height;

        for (int i = 0; i < 90; i++)
        {
            int y = random.Next(0, Math.Max(1, height));
            int x = random.Next(0, Math.Max(1, width));
            int lineWidth = random.Next(8, Math.Max(9, width / 5));
            int lineHeight = random.Next(1, 5);
            Color tint = random.NextDouble() > 0.5
                ? new Color(170, 205, 225)
                : new Color(80, 105, 130);

            spriteBatch.Draw(
                Game1.staminaRect,
                new Rectangle(x, y, lineWidth, lineHeight),
                tint * (0.16f * strength)
            );
        }
    }

    private void DrawWellGlimpse(SpriteBatch spriteBatch)
    {
        int cx = Game1.viewport.Width / 2;
        int cy = Game1.viewport.Height / 2;

        spriteBatch.Draw(
            Game1.staminaRect,
            new Rectangle(cx - 150, cy - 110, 300, 220),
            new Color(25, 37, 48) * 0.96f
        );
        spriteBatch.Draw(
            Game1.staminaRect,
            new Rectangle(cx - 132, cy - 92, 264, 184),
            new Color(90, 125, 145) * 0.35f
        );

        for (int i = 0; i < 11; i++)
        {
            int width = 132 - (i * 7);
            int y = cy + 28 + (i * 3);
            spriteBatch.Draw(
                Game1.staminaRect,
                new Rectangle(cx - width / 2, y, width, 4),
                new Color(105, 110, 118) * (0.85f - i * 0.035f)
            );
        }

        spriteBatch.Draw(
            Game1.staminaRect,
            new Rectangle(cx - 45, cy + 39, 90, 23),
            Color.Black * 0.92f
        );
        spriteBatch.Draw(
            Game1.staminaRect,
            new Rectangle(cx - 8, cy - 50, 16, 76),
            new Color(7, 10, 14) * 0.92f
        );
        spriteBatch.Draw(
            Game1.staminaRect,
            new Rectangle(cx - 24, cy - 58, 48, 34),
            new Color(7, 10, 14) * 0.92f
        );
    }

}
