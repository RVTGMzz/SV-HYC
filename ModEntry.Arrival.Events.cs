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
        this.HandleSaloonPrologueTimeChanged(e);

        if (this.Config.EnableDailySignal && e.NewTime >= this.Config.DailySignalTime && !this.IsSudokuSealed())
            this.TryStartArrival(force: false);

        if (!this.IsSudokuSealed())
            this.RefreshSudokuRoommateActivity(force: false);
    }

    private void OnWarped(object? sender, WarpedEventArgs e)
    {
        this.HandleSaloonPrologueWarped(e);

        if (this.sequenceActive && e.OldLocation is FarmHouse && e.NewLocation is not FarmHouse)
            this.CancelSequence();

        if (this.Config.EnableDailySignal
            && !this.IsSudokuSealed()
            && e.NewLocation is FarmHouse
            && Game1.timeOfDay >= this.Config.DailySignalTime)
        {
            this.TryStartArrival(force: false);
        }

        if (!this.IsSudokuSealed())
            this.HandleSudokuRoommateWarp(e.OldLocation, e.NewLocation);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;

        this.UpdateSaloonPrologueUi();
        this.PollSaloonPrologueStart();
        this.UpdateOccultCabinetStoryUi();

        if (!this.sequenceActive)
        {
            if (!this.IsSudokuSealed())
                this.UpdateSudokuRoommateBehavior();
            return;
        }

        if (this.IsSudokuSealed())
        {
            this.CancelSequence();
            return;
        }

        if (Game1.currentLocation is not FarmHouse)
        {
            this.CancelSequence();
            return;
        }

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
