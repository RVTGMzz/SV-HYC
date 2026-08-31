using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace HeyYoureCursed;

internal sealed partial class ModEntry
{
    private const int SudokuRescueCooldownDays = 15;
    private const int SudokuRescueDurationTicks = 138;

    private bool sudokuRescueActive;
    private int sudokuRescueTicks;
    private Vector2 sudokuRescueWorldPosition;
    private Texture2D? sudokuRescueTexture;
    private int lastKnownCalicoEggCount;

    private void RegisterSudokuRescueFeatures(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicking += this.OnSudokuRescueUpdateTicking;
        helper.Events.GameLoop.UpdateTicked += this.OnSudokuRescueUpdateTicked;
        helper.Events.Display.RenderedWorld += this.OnSudokuRescueRenderedWorld;

        helper.ConsoleCommands.Add(
            "heyyourecursed_test_rescue",
            "Trigger Sudoku's max-Trust rescue at the current location. Optional: death or exhaustion.",
            this.OnTestRescueCommand
        );
        helper.ConsoleCommands.Add(
            "heyyourecursed_reset_rescue",
            "Clear Sudoku's 15-day rescue cooldown for testing.",
            this.OnResetRescueCommand
        );
    }

    private void OnSudokuRescueUpdateTicking(object? sender, UpdateTickingEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;

        this.lastKnownCalicoEggCount = Game1.player.getItemCount("CalicoEgg");

        if (!this.sudokuRescueActive)
            this.TryBeginSudokuRescue();
    }

    private void OnSudokuRescueUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;

        if (!this.sudokuRescueActive)
        {
            // Fallback for lethal damage applied during the current update tick.
            this.TryBeginSudokuRescue();
            return;
        }

        this.StabilizeRescuedPlayer();
        this.sudokuRescueTicks++;

        if (this.sudokuRescueTicks == 24)
            Game1.playSound("thunder");
        else if (this.sudokuRescueTicks == 72)
            Game1.playSound("smallSelect");

        if (this.sudokuRescueTicks >= SudokuRescueDurationTicks)
            this.FinishSudokuRescue();
    }

    private void TryBeginSudokuRescue()
    {
        if (!this.CanSudokuRescueNow())
            return;

        Farmer player = Game1.player;
        bool death = player.health <= 0 || Game1.killScreen;
        bool exhaustion = player.Stamina <= -15f
            && (!player.IsBusyDoingSomething() || player.FarmerSprite.isPassingOut());

        if (!death && !exhaustion)
            return;

        this.BeginSudokuRescue(death ? "death" : "exhaustion");
    }

    private bool CanSudokuRescueNow()
    {
        if (!Context.IsWorldReady
            || this.sudokuRescueActive
            || this.sequenceActive
            || Game1.timeOfDay >= 2600
            || Game1.currentLocation is null
            || Game1.eventUp
            || Game1.currentMinigame is not null
            || this.IsSudokuSealed()
            || !this.IsSudokuActiveHaunting()
            || !ModIdentity.HasFirstConversationCompleted(Game1.player))
        {
            return false;
        }

        return ModIdentity.IsSudokuRescueReady(Game1.player, SudokuRescueCooldownDays);
    }

    private void BeginSudokuRescue(string trigger)
    {
        bool vanillaDeathScreenStarted = Game1.killScreen;

        this.sudokuRescueActive = true;
        this.sudokuRescueTicks = 0;
        this.sudokuRescueWorldPosition = Game1.player.Position;
        ModIdentity.MarkSudokuRescueUsed(Game1.player);

        try
        {
            this.sudokuRescueTexture ??= this.Helper.ModContent.Load<Texture2D>("assets/Characters/Sudoku.png");
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"Couldn't load Sudoku's rescue apparition texture. {ex.Message}", LogLevel.Warn);
        }

        this.RestoreEmergencyDeathLoss(vanillaDeathScreenStarted);
        Game1.player.completelyStopAnimatingOrDoingAction();
        Game1.player.faceDirection(Game1.player.FacingDirection);
        this.StabilizeRescuedPlayer();
        Game1.pauseTime = SudokuRescueDurationTicks * (1000f / 60f);
        Game1.playSound("ghost");

        this.Monitor.Log(
            $"Sudoku protection activated from {trigger} at {Game1.currentLocation.NameOrUniqueName} {Game1.player.Tile}. HP/Energy restored; cooldown set to {SudokuRescueCooldownDays} in-game days.",
            LogLevel.Info
        );
    }

    private void StabilizeRescuedPlayer()
    {
        Farmer player = Game1.player;
        Game1.killScreen = false;
        Game1.screenGlow = false;
        player.freezePause = 0;
        player.jitterStrength = 0f;
        player.health = player.maxHealth;
        player.Stamina = player.MaxStamina;
        player.exhausted.Value = false;
        player.FarmerSprite.PauseForSingleAnimation = false;
        player.Halt();
        player.CanMove = false;
    }

    private void RestoreEmergencyDeathLoss(bool vanillaDeathScreenStarted)
    {
        if (!vanillaDeathScreenStarted)
            return;

        if (Game1.stats.TimesUnconscious > 0)
            Game1.stats.TimesUnconscious--;

        int currentEggs = Game1.player.getItemCount("CalicoEgg");
        int eggsToRestore = Math.Max(0, this.lastKnownCalicoEggCount - currentEggs);
        if (eggsToRestore > 0)
            Game1.player.addItemToInventory(ItemRegistry.Create("(O)CalicoEgg", eggsToRestore));

        Game1.player.itemsLostLastDeath.Clear();
    }

    private void FinishSudokuRescue()
    {
        this.StabilizeRescuedPlayer();
        this.sudokuRescueActive = false;
        this.sudokuRescueTicks = 0;
        Game1.pauseTime = 0f;

        Farmer player = Game1.player;
        player.completelyStopAnimatingOrDoingAction();
        player.faceDirection(player.FacingDirection);
        player.CanMove = true;
        player.temporarilyInvincible = true;
        player.temporaryInvincibilityTimer = 0;
        player.currentTemporaryInvincibilityDuration = 2000;
        Game1.currentLocation?.checkForMusic(Game1.currentGameTime);
        Game1.playSound("yoba");
    }

    private void ResetSudokuRescueRuntime()
    {
        this.sudokuRescueActive = false;
        this.sudokuRescueTicks = 0;
        this.sudokuRescueTexture = null;
    }

    private string GetSudokuProtectionStatus()
    {
        if (ModIdentity.GetSudokuTrust(Game1.player) < 30)
            return T("rescue.status.locked");

        int days = ModIdentity.GetSudokuRescueDaysRemaining(Game1.player, SudokuRescueCooldownDays);
        return days <= 0
            ? T("rescue.status.ready")
            : T("rescue.status.cooldown", new { days });
    }

    private void OnSudokuRescueRenderedWorld(object? sender, RenderedWorldEventArgs e)
    {
        if (!this.sudokuRescueActive)
            return;

        SpriteBatch b = e.SpriteBatch;
        float progress = Math.Clamp(this.sudokuRescueTicks / (float)SudokuRescueDurationTicks, 0f, 1f);
        float pulse = MathF.Sin(progress * MathF.PI);

        b.Draw(
            Game1.staminaRect,
            new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height),
            Color.Black * (0.35f + pulse * 0.35f)
        );

        Random random = new(Game1.Date.TotalDays * 7919 + this.sudokuRescueTicks * 3571);
        for (int i = 0; i < 34; i++)
        {
            int y = random.Next(0, Math.Max(1, Game1.viewport.Height));
            int x = random.Next(0, Math.Max(1, Game1.viewport.Width));
            int width = random.Next(12, Math.Max(13, Game1.viewport.Width / 4));
            int height = random.Next(1, 6);
            Color tint = i % 3 == 0 ? new Color(178, 217, 234) : new Color(92, 116, 139);
            b.Draw(Game1.staminaRect, new Rectangle(x, y, width, height), tint * (0.08f + pulse * 0.14f));
        }

        if (this.sudokuRescueTexture is not null && this.sudokuRescueTicks is >= 18 and <= 116)
        {
            Vector2 screen = Game1.GlobalToLocal(Game1.viewport, this.sudokuRescueWorldPosition);
            Rectangle source = new(0, 0, 16, 32);
            Vector2 origin = new(8f, 32f);
            float alpha = Math.Clamp(pulse * 1.2f, 0f, 0.92f);
            Vector2 anchor = screen + new Vector2(32f, -16f);

            b.Draw(this.sudokuRescueTexture, anchor + new Vector2(-14f, 2f), source, new Color(135, 190, 220) * (alpha * 0.22f), 0f, origin, 4.2f, SpriteEffects.None, 1f);
            b.Draw(this.sudokuRescueTexture, anchor + new Vector2(14f, -2f), source, new Color(188, 222, 238) * (alpha * 0.18f), 0f, origin, 4.2f, SpriteEffects.None, 1f);
            b.Draw(this.sudokuRescueTexture, anchor, source, Color.White * alpha, 0f, origin, 4.2f, SpriteEffects.None, 1f);
        }

        if (this.sudokuRescueTicks >= 48)
        {
            string line = T("rescue.line");
            Vector2 size = Game1.dialogueFont.MeasureString(line);
            Vector2 textPosition = new(
                (Game1.viewport.Width - size.X) / 2f,
                Math.Min(Game1.viewport.Height - size.Y - 56f, Game1.viewport.Height * 0.72f)
            );
            Rectangle backdrop = new(
                (int)textPosition.X - 18,
                (int)textPosition.Y - 10,
                (int)size.X + 36,
                (int)size.Y + 20
            );

            b.Draw(Game1.staminaRect, backdrop, Color.Black * 0.72f);
            b.DrawString(Game1.dialogueFont, line, textPosition + new Vector2(2f, 2f), Color.Black * 0.9f);
            b.DrawString(Game1.dialogueFont, line, textPosition, new Color(215, 232, 241));
        }
    }

    private void OnTestRescueCommand(string command, string[] args)
    {
        if (!this.TryRequireWorld(command))
            return;

        if (!this.CanSudokuRescueNow())
        {
            this.Monitor.Log(
                $"Sudoku rescue isn't ready: trust={ModIdentity.GetSudokuTrust(Game1.player)}/30, sealed={this.IsSudokuSealed()}, activeHaunting={this.GetActiveHauntingId()}, time={Game1.timeOfDay}, cooldown={ModIdentity.GetSudokuRescueDaysRemaining(Game1.player, SudokuRescueCooldownDays)} day(s).",
                LogLevel.Warn
            );
            return;
        }

        string mode = args.Length > 0 ? args[0].Trim().ToLowerInvariant() : "death";
        if (mode is not ("death" or "exhaustion"))
        {
            this.Monitor.Log("Usage: heyyourecursed_test_rescue [death|exhaustion]", LogLevel.Info);
            return;
        }

        if (mode == "exhaustion")
            Game1.player.Stamina = -15f;
        else
            Game1.player.health = 0;

        this.TryBeginSudokuRescue();
    }

    private void OnResetRescueCommand(string command, string[] args)
    {
        if (!this.TryRequireWorld(command))
            return;

        Game1.player.modData.Remove(ModIdentity.SudokuRescueDayKey);
        this.Monitor.Log("Sudoku's rescue cooldown was reset. Protection is ready whenever Trust is 30/30.", LogLevel.Info);
    }
}
