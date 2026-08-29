using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace HeyYoureCursed;

internal sealed partial class ModEntry
{
    private bool pendingSaloonInviteDialogue;
    private bool saloonPrologueActive;

    private bool HasSaloonPrologueSeen()
    {
        return Game1.player.modData.TryGetValue(ModIdentity.SaloonPrologueSeenKey, out string? raw)
            && raw == "true";
    }

    private bool HasLegacyStoryProgressBeforePrologue()
    {
        if (ModIdentity.HasArrivalBeenSeen(Game1.player)
            || ModIdentity.IsCursedVhsInstalled(Game1.player)
            || ModIdentity.HasFirstConversationCompleted(Game1.player))
        {
            return true;
        }

        return Game1.player.modData.TryGetValue(ModIdentity.CursedVhsGrantedKey, out string? vhsGranted)
            && vhsGranted == "true";
    }

    private void PrepareSaloonPrologueOnSaveLoaded()
    {
        this.saloonPrologueActive = false;
        this.pendingSaloonInviteDialogue = false;

        if (!this.HasSaloonPrologueSeen() && this.HasLegacyStoryProgressBeforePrologue())
        {
            Game1.player.modData[ModIdentity.SaloonPrologueSeenKey] = "true";
            Game1.player.modData[ModIdentity.SaloonInvitationReceivedKey] = "true";
            Game1.player.modData[ModIdentity.SaloonPrologueCompletedDayKey] = Math.Max(0, Game1.Date.TotalDays - 1).ToString();
            this.Monitor.Log("Existing pre-alpha.3.2 story progress detected; Saloon prologue marked complete for compatibility.", LogLevel.Info);
        }

        if (this.HasSaloonPrologueSeen())
            this.TryGrantPostPrologueVhs();
        else
            this.QueueSaloonInvitationIfNeeded();
    }

    private void PrepareSaloonPrologueOnDayStarted()
    {
        this.saloonPrologueActive = false;
        this.pendingSaloonInviteDialogue = false;

        if (this.HasSaloonPrologueSeen())
        {
            this.TryGrantPostPrologueVhs();
            return;
        }

        this.QueueSaloonInvitationIfNeeded();
    }

    private void QueueSaloonInvitationIfNeeded()
    {
        if (this.HasSaloonPrologueSeen())
            return;

        int today = Game1.Date.TotalDays;
        bool shownToday = Game1.player.modData.TryGetValue(ModIdentity.SaloonInviteShownDayKey, out string? raw)
            && int.TryParse(raw, out int shownDay)
            && shownDay == today;

        if (!shownToday)
            this.pendingSaloonInviteDialogue = true;
    }

    private void UpdateSaloonPrologueUi()
    {
        if (!this.pendingSaloonInviteDialogue
            || this.saloonPrologueActive
            || this.sequenceActive
            || Game1.activeClickableMenu is not null
            || Game1.eventUp)
        {
            return;
        }

        this.pendingSaloonInviteDialogue = false;
        bool firstInvite = !Game1.player.modData.TryGetValue(ModIdentity.SaloonInvitationReceivedKey, out string? raw)
            || raw != "true";

        Game1.player.modData[ModIdentity.SaloonInvitationReceivedKey] = "true";
        Game1.player.modData[ModIdentity.SaloonInviteShownDayKey] = Game1.Date.TotalDays.ToString();
        Game1.drawObjectDialogue(T(firstInvite
            ? "story.prologue.invitation.first"
            : "story.prologue.invitation.reminder"));
    }

    private void PollSaloonPrologueStart()
    {
        if (Game1.timeOfDay >= 1800
            && Game1.activeClickableMenu is null
            && !Game1.eventUp)
        {
            this.TryStartSaloonPrologue();
        }
    }

    private void HandleSaloonPrologueTimeChanged(TimeChangedEventArgs e)
    {
        if (this.HasSaloonPrologueSeen())
            return;

        bool invited = Game1.player.modData.TryGetValue(ModIdentity.SaloonInvitationReceivedKey, out string? raw)
            && raw == "true";
        if (!invited)
            return;

        if (e.NewTime == 1730)
            Game1.showGlobalMessage(T("story.prologue.hud.1730"));
        else if (e.NewTime == 1800)
            Game1.showGlobalMessage(T("story.prologue.hud.1800"));

        if (e.NewTime >= 1800)
            this.TryStartSaloonPrologue();
    }

    private void HandleSaloonPrologueWarped(WarpedEventArgs e)
    {
        if (this.HasSaloonPrologueSeen() || Game1.timeOfDay < 1800)
            return;

        if (string.Equals(e.NewLocation.NameOrUniqueName, "Saloon", StringComparison.OrdinalIgnoreCase))
            this.TryStartSaloonPrologue();
    }

    private void TryStartSaloonPrologue()
    {
        if (!Context.IsWorldReady
            || this.saloonPrologueActive
            || this.sequenceActive
            || this.HasSaloonPrologueSeen()
            || Game1.timeOfDay < 1800
            || Game1.timeOfDay >= 2600
            || Game1.activeClickableMenu is not null
            || Game1.eventUp
            || Game1.currentLocation is null
            || !string.Equals(Game1.currentLocation.NameOrUniqueName, "Saloon", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        bool invited = Game1.player.modData.TryGetValue(ModIdentity.SaloonInvitationReceivedKey, out string? raw)
            && raw == "true";
        if (!invited)
            return;

        this.saloonPrologueActive = true;
        Game1.player.Halt();
        Game1.playSound("doorClose");
        Game1.activeClickableMenu = new SaloonPrologueMenu(
            onChoice: choice => Game1.player.modData[ModIdentity.SaloonPrologueChoiceKey] = choice,
            onComplete: this.CompleteSaloonPrologue
        );

        this.Monitor.Log("Persistent Stardrop Saloon prologue started.", LogLevel.Info);
    }

    private void CompleteSaloonPrologue()
    {
        this.saloonPrologueActive = false;
        Game1.player.modData[ModIdentity.SaloonPrologueSeenKey] = "true";
        Game1.player.modData[ModIdentity.SaloonPrologueCompletedDayKey] = Game1.Date.TotalDays.ToString();
        Game1.showGlobalMessage(T("story.prologue.saloon.after"));

        this.Monitor.Log(
            "Stardrop Saloon prologue completed. The cursed VHS is now story-unlocked and will appear the following morning.",
            LogLevel.Info
        );
    }

    private void TryGrantPostPrologueVhs()
    {
        if (!this.HasSaloonPrologueSeen() || ModIdentity.IsCursedVhsInstalled(Game1.player))
            return;

        if (!Game1.player.modData.TryGetValue(ModIdentity.SaloonPrologueCompletedDayKey, out string? raw)
            || !int.TryParse(raw, out int completedDay))
        {
            completedDay = Math.Max(0, Game1.Date.TotalDays - 1);
        }

        if (Game1.Date.TotalDays <= completedDay)
            return;

        this.EnsureCursedVhsGranted(showDialogue: false);
    }

    private void ResetSaloonPrologueRuntime()
    {
        this.saloonPrologueActive = false;
        this.pendingSaloonInviteDialogue = false;
    }

    private void OnTestPrologueCommand(string command, string[] args)
    {
        if (!Context.IsWorldReady)
        {
            this.Monitor.Log("Load a save before using heyyourecursed_test_prologue.", LogLevel.Warn);
            return;
        }

        if (!string.Equals(Game1.currentLocation?.NameOrUniqueName, "Saloon", StringComparison.OrdinalIgnoreCase))
        {
            this.Monitor.Log("Enter the Stardrop Saloon, then use heyyourecursed_test_prologue.", LogLevel.Warn);
            return;
        }

        Game1.player.modData.Remove(ModIdentity.SaloonPrologueSeenKey);
        Game1.player.modData[ModIdentity.SaloonInvitationReceivedKey] = "true";
        if (Game1.timeOfDay < 1800)
            Game1.timeOfDay = 1800;

        this.TryStartSaloonPrologue();
    }
}
