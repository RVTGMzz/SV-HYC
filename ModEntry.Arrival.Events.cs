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
    private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
    {
        if (this.Config.EnableDailySignal && e.NewTime >= this.Config.DailySignalTime)
            this.TryStartArrival(force: false);
    }

    private void OnWarped(object? sender, WarpedEventArgs e)
    {
        if (this.sequenceActive && e.OldLocation is FarmHouse && e.NewLocation is not FarmHouse)
        {
            this.CancelSequence();
            return;
        }

        if (!this.Config.EnableDailySignal || e.NewLocation is not FarmHouse)
            return;

        // If the player wasn't home at 8:00, fire today's signal the first time they return.
        if (Game1.timeOfDay >= this.Config.DailySignalTime)
            this.TryStartArrival(force: false);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!this.sequenceActive || !Context.IsWorldReady)
            return;

        if (Game1.currentLocation is not FarmHouse)
        {
            this.CancelSequence();
            return;
        }

        this.elapsedTicks++;

        if (this.elapsedTicks == 1)
            Game1.playSound("ghost");

        // After the first arrival, the VHS only needs a short burst of static/glitch each morning.
        // Replaying the full well + crawl scene every day would become intrusive very quickly.
        if (!this.sequenceIsFirstArrival)
        {
            int repeatStaticEnd = Math.Max(1, this.Config.RepeatStaticTicks);
            int repeatEnd = repeatStaticEnd + Math.Max(1, this.Config.RepeatGlitchTicks);

            if (this.elapsedTicks == repeatStaticEnd)
                Game1.playSound("smallSelect");

            if (this.elapsedTicks >= repeatEnd)
                this.FinishSequence();

            return;
        }

        int staticEnd = Math.Max(1, this.Config.StaticTicks);
        int wellEnd = staticEnd + Math.Max(1, this.Config.WellTicks);
        int glitchEnd = wellEnd + Math.Max(1, this.Config.GlitchTicks);

        if (this.elapsedTicks == staticEnd)
            Game1.playSound("thunder");

        if (this.elapsedTicks < glitchEnd)
            return;

        int safeFrameDuration = Math.Max(1, this.Config.FrameDurationTicks);
        int rawFrame = (this.elapsedTicks - glitchEnd) / safeFrameDuration;

        if (rawFrame >= FrameCount)
        {
            this.FinishSequence();
            return;
        }

        this.frameIndex = Math.Clamp(rawFrame, 0, FrameCount - 1);
    }
}
