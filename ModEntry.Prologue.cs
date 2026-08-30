using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace HeyYoureCursed;

internal sealed partial class ModEntry
{
    private const int SaloonPrologueRuntimeEventId = 970070034;

    private bool pendingSaloonInviteDialogue;
    private bool saloonPrologueActive;
    private bool saloonPrologueEventWasRunning;

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
        this.saloonPrologueEventWasRunning = false;
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
        this.saloonPrologueEventWasRunning = false;
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
        this.TryFinishSaloonPrologueEvent();

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
        this.TryFinishSaloonPrologueEvent();

        if (Game1.timeOfDay >= 1800
            && Game1.activeClickableMenu is null
            && !Game1.eventUp)
        {
            this.TryStartSaloonPrologue();
        }
    }

    private void HandleSaloonPrologueTimeChanged(TimeChangedEventArgs e)
    {
        this.TryFinishSaloonPrologueEvent();

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
        this.TryFinishSaloonPrologueEvent();

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

        try
        {
            this.saloonPrologueActive = true;
            this.saloonPrologueEventWasRunning = false;
            Game1.player.Halt();

            // The exact answer isn't story-gating; both options establish the same skeptical canon.
            // Keep a neutral marker for future callbacks without lying about which native event answer was picked.
            Game1.player.modData[ModIdentity.SaloonPrologueChoiceKey] = "skeptical";

            string script = this.BuildNativeSaloonPrologueEvent();
            StardewValley.Event saloonEvent = new(script, SaloonPrologueRuntimeEventId);
            Game1.currentLocation.currentEvent = saloonEvent;
            Game1.currentLocation.startEvent(saloonEvent);
            this.saloonPrologueEventWasRunning = true;

            this.Monitor.Log("Persistent Stardrop Saloon prologue started as a native in-world event.", LogLevel.Info);
        }
        catch (Exception ex)
        {
            this.saloonPrologueActive = false;
            this.saloonPrologueEventWasRunning = false;
            this.Monitor.Log($"Couldn't start the native Stardrop Saloon prologue. {ex}", LogLevel.Error);
        }
    }

    private string BuildNativeSaloonPrologueEvent()
    {
        string gusIntro = QuoteEventText(T("story.prologue.saloon.gus.1"));
        string abigailStory = QuoteEventText(T("story.prologue.saloon.abigail.1"));
        string pamStory = QuoteEventText(T("story.prologue.saloon.pam.1"));
        string farmerPrompt = QuoteEventText(T("story.prologue.saloon.farmer.prompt"));
        string choiceNever = QuoteEventText(T("story.prologue.saloon.choice.never"));
        string choiceDoubt = QuoteEventText(T("story.prologue.saloon.choice.doubt"));
        string gusWarning = QuoteEventText(T("story.prologue.saloon.gus.warning"));
        string abigailWarning = QuoteEventText(T("story.prologue.saloon.abigail.warning"));
        string hiddenVoice = QuoteEventText(T("story.prologue.saloon.wizard.1"));

        // Keep the room visibly occupied instead of presenting dialogue over an empty Saloon.
        // The final voice deliberately has no Wizard actor, portrait, name, purple-cloak narration, or other identity tell.
        return string.Join("/", new[]
        {
            "Saloon1",
            "14 20",
            "farmer 14 24 0 Gus 10 18 2 Abigail 16 20 3 Pam 11 20 1 Emily 14 17 2 Shane 20 20 3",
            "skippable",
            "pause 400",
            "playSound doorClose",
            "move farmer 0 -2 0",
            "pause 350",
            $"speak Gus \"{gusIntro}\"",
            "pause 250",
            "faceDirection Abigail 3",
            $"speak Abigail \"{abigailStory}\"",
            "pause 250",
            "faceDirection Pam 1",
            $"speak Pam \"{pamStory}\"",
            "pause 350",
            $"question null \"{farmerPrompt}#{choiceNever}#{choiceDoubt}\"",
            "pause 300",
            "faceDirection Gus 2",
            $"speak Gus \"{gusWarning}\"",
            "pause 250",
            "faceDirection Abigail 3",
            $"speak Abigail \"{abigailWarning}\"",
            "pause 550",
            "fade",
            "pause 750",
            $"message \"{hiddenVoice}\"",
            "pause 900",
            "end warpOut"
        });
    }

    private static string QuoteEventText(string text)
    {
        return text
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private void TryFinishSaloonPrologueEvent()
    {
        if (!this.saloonPrologueActive || !this.saloonPrologueEventWasRunning || Game1.eventUp)
            return;

        this.saloonPrologueEventWasRunning = false;
        this.CompleteSaloonPrologue();
    }

    private void CompleteSaloonPrologue()
    {
        if (this.HasSaloonPrologueSeen())
        {
            this.saloonPrologueActive = false;
            return;
        }

        this.saloonPrologueActive = false;
        Game1.player.modData[ModIdentity.SaloonPrologueSeenKey] = "true";
        Game1.player.modData[ModIdentity.SaloonPrologueCompletedDayKey] = Game1.Date.TotalDays.ToString();

        // The intended 18:00 gathering consumes the evening and releases the farmer outside around 20:00.
        // Never rewind time for a player who intentionally arrived later than that.
        if (Game1.timeOfDay < 2000)
            Game1.timeOfDay = 2000;

        Game1.showGlobalMessage(T("story.prologue.saloon.after"));

        this.Monitor.Log(
            "Stardrop Saloon prologue completed. The hidden observer remains unidentified; the cursed VHS unlocks the following morning.",
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
        this.saloonPrologueEventWasRunning = false;
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
