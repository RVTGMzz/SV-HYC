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
    private void DrawGlitch(SpriteBatch spriteBatch)
    {
        Random random = new(this.elapsedTicks * 3571 + 91);

        for (int i = 0; i < 18; i++)
        {
            int y = random.Next(0, Math.Max(1, Game1.viewport.Height));
            int h = random.Next(4, 22);
            Color tint = (i % 3) switch
            {
                0 => new Color(170, 210, 230),
                1 => new Color(70, 95, 120),
                _ => Color.White
            };

            spriteBatch.Draw(
                Game1.staminaRect,
                new Rectangle(0, y, Game1.viewport.Width, h),
                tint * 0.12f
            );
        }

        this.DrawStatic(spriteBatch, 0.8f);
    }

    private void DrawEmergenceFrame(SpriteBatch spriteBatch)
    {
        if (this.tvArrivalSheet is null)
            return;

        int sourceColumn = this.frameIndex % 3;
        int sourceRow = this.frameIndex / 3;
        Rectangle source = new(
            sourceColumn * FrameWidth,
            sourceRow * FrameHeight,
            FrameWidth,
            FrameHeight
        );

        float scale = Math.Max(0.5f, this.Config.EventScale);
        int drawWidth = (int)(FrameWidth * scale);
        int drawHeight = (int)(FrameHeight * scale);
        Rectangle destination = new(
            (Game1.viewport.Width - drawWidth) / 2,
            (Game1.viewport.Height - drawHeight) / 2,
            drawWidth,
            drawHeight
        );

        spriteBatch.Draw(this.tvArrivalSheet, destination, source, Color.White);

        if ((this.elapsedTicks / 4) % 3 == 0)
            this.DrawStatic(spriteBatch, 0.15f);
    }

    private void TryStartArrival(bool force)
    {
        if (!Context.IsWorldReady || this.sequenceActive || Game1.currentLocation is not FarmHouse)
            return;

        if (!force && Game1.activeClickableMenu is not null)
            return;

        if (!force)
        {
            if (!this.Config.EnableDailySignal)
                return;
            if (!ModIdentity.IsCursedVhsInstalled(Game1.player))
                return;
            if (Game1.timeOfDay < this.Config.DailySignalTime)
                return;
            if (ModIdentity.HasDailySignalRunToday(Game1.player))
                return;
        }

        this.sequenceActive = true;
        this.elapsedTicks = 0;
        this.frameIndex = 0;
        this.Monitor.Log(
            force
                ? "Cursed Signal arrival sequence started by debug command."
                : "The installed Cursed VHS activated today's 8:00 AM signal.",
            LogLevel.Info
        );
    }
}
