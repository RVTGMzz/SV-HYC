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
        if (!this.sequenceActive)
            return;

        this.DrawDarkBackdrop(e.SpriteBatch);

        if (!this.sequenceIsFirstArrival)
        {
            int repeatStaticEnd = Math.Max(1, this.Config.RepeatStaticTicks);

            if (this.elapsedTicks < repeatStaticEnd)
                this.DrawStatic(e.SpriteBatch, 0.85f);
            else
                this.DrawGlitch(e.SpriteBatch);

            return;
        }

        int staticEnd = Math.Max(1, this.Config.StaticTicks);
        int wellEnd = staticEnd + Math.Max(1, this.Config.WellTicks);
        int glitchEnd = wellEnd + Math.Max(1, this.Config.GlitchTicks);

        if (this.elapsedTicks < staticEnd)
        {
            this.DrawStatic(e.SpriteBatch, 1f);
            return;
        }

        if (this.elapsedTicks < wellEnd)
        {
            this.DrawWellGlimpse(e.SpriteBatch);
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
            Color.Black * (this.sequenceIsFirstArrival ? 0.68f : 0.38f)
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
        if (this.wellBroadcastTexture is null)
            return;

        int availableWidth = Math.Max(192, Game1.viewport.Width - 120);
        int availableHeight = Math.Max(128, Game1.viewport.Height - 180);

        int drawWidth = Math.Min(576, availableWidth);
        int drawHeight = drawWidth * 128 / 192;

        if (drawHeight > availableHeight)
        {
            drawHeight = availableHeight;
            drawWidth = drawHeight * 192 / 128;
        }

        int jitterX = (this.elapsedTicks / 4) % 9 == 0 ? 3 : 0;
        int jitterY = (this.elapsedTicks / 7) % 11 == 0 ? -2 : 0;

        Rectangle destination = new(
            (Game1.viewport.Width - drawWidth) / 2 + jitterX,
            (Game1.viewport.Height - drawHeight) / 2 + jitterY,
            drawWidth,
            drawHeight
        );

        Rectangle frame = new(
            destination.X - 12,
            destination.Y - 12,
            destination.Width + 24,
            destination.Height + 24
        );

        spriteBatch.Draw(Game1.staminaRect, frame, new Color(5, 8, 11) * 0.98f);
        spriteBatch.Draw(this.wellBroadcastTexture, destination, Color.White);

        for (int y = destination.Y + 8; y < destination.Bottom; y += 16)
        {
            spriteBatch.Draw(
                Game1.staminaRect,
                new Rectangle(destination.X, y, destination.Width, 2),
                Color.Black * 0.20f
            );
        }
    }
}
