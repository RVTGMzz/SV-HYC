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

        if (Game1.timeOfDay >= this.Config.DailySignalTime)
            this.TryStartArrival(force: false);
    }


    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;

        if (this.pendingSudokuMenuOpen && !this.sequenceActive)
        {
            if (this.pendingSudokuMenuDelayTicks > 0)
            {
                this.pendingSudokuMenuDelayTicks--;
            }
            else
            {
                this.pendingSudokuMenuWaitTicks++;

                // The answer callback runs while Stardew still owns the DialogueBox.
                // Wait a short moment so its click/transition completes, then dismiss only
                // that stale dialogue. Open Sudoku on the NEXT update tick, never the same
                // tick we clear the dialogue, so Stardew can't overwrite our custom menu.
                if (Game1.activeClickableMenu is DialogueBox)
                {
                    this.Monitor.Log("Closing completed Sudoku dialogue before custom-menu handoff.", LogLevel.Trace);
                    Game1.activeClickableMenu = null;
                    this.pendingSudokuMenuWaitTicks = 0;
                    return;
                }

                if (Game1.activeClickableMenu is null)
                {
                    this.pendingSudokuMenuOpen = false;
                    this.pendingSudokuMenuWaitTicks = 0;
                    this.OpenDailySudoku(force: true);
                }
                else if (this.pendingSudokuMenuWaitTicks >= 300)
                {
                    this.Monitor.Log(
                        $"Daily Sudoku menu was queued but another menu ({Game1.activeClickableMenu.GetType().Name}) blocked the handoff for too long. Cancelling the pending open.",
                        LogLevel.Warn
                    );
                    this.pendingSudokuMenuOpen = false;
                    this.pendingSudokuMenuWaitTicks = 0;
                }
            }
        }

        if (!this.sequenceActive)
            return;

        if (Game1.currentLocation is not FarmHouse)
        {
            this.CancelSequence();
            return;
        }

        // Keep the player from drifting away from the TV while the lightweight custom
        // sequence is running. This avoids accidental warps/collision weirdness without
        // depending on event-script state.
        Game1.player.Halt();

        this.elapsedTicks++;

        if (this.elapsedTicks == 1)
            Game1.playSound("ghost");

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
        int glitchEnd = staticEnd + Math.Max(1, this.Config.GlitchTicks);

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
