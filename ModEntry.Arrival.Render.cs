using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Objects;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace HeyYoureCursed;

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
        int glitchEnd = staticEnd + Math.Max(1, this.Config.GlitchTicks);

        if (this.elapsedTicks < staticEnd)
        {
            this.DrawStatic(e.SpriteBatch, 1f);
            return;
        }

        if (this.elapsedTicks < glitchEnd)
        {
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
}
