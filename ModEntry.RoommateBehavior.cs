using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace HeyYoureCursed;

internal sealed partial class ModEntry
{
    private const int RoommateGlideTicksPerTile = 18;

    private readonly Queue<Vector2> roommatePath = new();
    private SudokuActivityKind? roommateCurrentActivity;
    private Vector2? roommateActivityTargetTile;
    private Vector2? roommateActivityFocusTile;
    private int roommateActivityDay = -1;
    private int roommateActivitySlot = -1;
    private int roommateBehaviorCooldownTicks;
    private Vector2? roommateGlideTargetTile;
    private Vector2 roommateGlideStartPosition;
    private Vector2 roommateGlideTargetPosition;
    private int roommateGlideTicks;

    private void ClearSudokuRoommateActivityRuntime()
    {
        this.roommatePath.Clear();
        this.roommateCurrentActivity = null;
        this.roommateActivityTargetTile = null;
        this.roommateActivityFocusTile = null;
        this.roommateActivityDay = -1;
        this.roommateActivitySlot = -1;
        this.roommateBehaviorCooldownTicks = 0;
        this.roommateGlideTargetTile = null;
        this.roommateGlideTicks = 0;
    }

    private void ResetSudokuRoommateBehavior()
    {
        this.ClearSudokuRoommateActivityRuntime();
        this.sudokuRoommateYieldedToTeamUp = false;
    }

    private void BeginSudokuRoommateBehaviorAfterArrival()
    {
        // Let the arrival/reunion moment breathe before Sudoku starts drifting to her own spot.
        this.roommateBehaviorCooldownTicks = 240;
        this.roommateActivityDay = -1;
        this.roommateActivitySlot = -1;
        this.roommatePath.Clear();
        this.roommateGlideTargetTile = null;
        this.RefreshSudokuRoommateActivity(force: true);
    }

    private void HandleSudokuRoommateWarp(GameLocation oldLocation, GameLocation newLocation)
    {
        if (!Context.IsWorldReady)
            return;

        if (oldLocation is FarmHouse && newLocation is not FarmHouse)
        {
            this.roommatePath.Clear();
            this.roommateGlideTargetTile = null;
            this.roommateGlideTicks = 0;
            return;
        }

        if (newLocation is not FarmHouse)
            return;

        this.roommateBehaviorCooldownTicks = Math.Max(this.roommateBehaviorCooldownTicks, 90);

        if (!this.CanSudokuUseRoommateBehavior())
            return;

        NPC? sudoku = this.FindSudoku(currentLocationOnly: true);
        if (sudoku is null || sudoku.IsInvisible)
            return;

        if (this.ShouldYieldSudokuRoommateControl(sudoku))
            return;

        int trust = ModIdentity.GetSudokuTrust(Game1.player);
        bool cameFromOutside = oldLocation is not FarmHouse;
        if (cameFromOutside
            && trust >= 3
            && Game1.timeOfDay >= 1100
            && !ModIdentity.HasWelcomeHomeRunToday(Game1.player))
        {
            ModIdentity.MarkWelcomeHomeRunToday(Game1.player);
            Game1.showGlobalMessage(SudokuRoommateLibrary.GetWelcomeHomeLine(trust));
        }

        this.RefreshSudokuRoommateActivity(force: true);
    }

    private void RefreshSudokuRoommateActivity(bool force)
    {
        if (!this.CanSudokuUseRoommateBehavior() || Game1.currentLocation is not FarmHouse farmHouse)
            return;

        NPC? sudoku = this.FindSudoku(currentLocationOnly: true);
        if (sudoku is null || sudoku.IsInvisible)
            return;

        if (this.ShouldYieldSudokuRoommateControl(sudoku))
            return;

        sudoku.ignoreScheduleToday = true;
        sudoku.Halt();

        int day = Game1.Date.TotalDays;
        int slot = GetRoommateActivitySlot(Game1.timeOfDay);
        if (!force
            && this.roommateActivityDay == day
            && this.roommateActivitySlot == slot
            && this.roommateCurrentActivity.HasValue
            && this.roommateActivityTargetTile.HasValue)
        {
            return;
        }

        int trust = ModIdentity.GetSudokuTrust(Game1.player);
        long seed = unchecked((long)Game1.uniqueIDForThisGame ^ ((long)day * 486187739L) ^ ((long)slot * 104729L));
        SudokuActivityKind requested = ChooseRoommateActivityKind(trust, seed);

        if (!this.TryResolveRoommateActivity(
                farmHouse,
                requested,
                seed,
                out SudokuActivityKind resolved,
                out Vector2 target,
                out Vector2? focus))
        {
            this.Monitor.Log("Couldn't resolve a safe farmhouse activity tile for Sudoku.", StardewModdingAPI.LogLevel.Trace);
            return;
        }

        this.roommateActivityDay = day;
        this.roommateActivitySlot = slot;
        this.roommateCurrentActivity = resolved;
        this.roommateActivityTargetTile = target;
        this.roommateActivityFocusTile = focus;

        this.roommatePath.Clear();
        this.roommateGlideTargetTile = null;
        this.roommateGlideTicks = 0;

        List<Vector2> path = this.FindRoommatePath(farmHouse, sudoku.Tile, target);
        foreach (Vector2 tile in path)
            this.roommatePath.Enqueue(tile);

        if (path.Count == 0 && Vector2.Distance(sudoku.Tile, target) > 0.75f)
        {
            // A remodeled farmhouse or unusual furniture layout can make a normal path
            // impossible. Sudoku is a ghost, so an off-camera-ish blink is preferable to
            // getting permanently stuck in a doorway. Never blink while the player is close.
            if (Vector2.Distance(Game1.player.Tile, sudoku.Tile) > 5f
                && Vector2.Distance(Game1.player.Tile, target) > 4f)
            {
                Game1.warpCharacter(sudoku, farmHouse, target);
            }
        }

        this.FaceSudokuTowardFocus(sudoku);
        this.Monitor.Log(
            $"Sudoku roommate activity: {resolved}, slot={slot}, target={target}, pathTiles={path.Count}.",
            StardewModdingAPI.LogLevel.Trace
        );
    }

    private void UpdateSudokuRoommateBehavior()
    {
        if (!this.CanSudokuUseRoommateBehavior() || Game1.currentLocation is not FarmHouse farmHouse)
            return;

        NPC? sudoku = this.FindSudoku(currentLocationOnly: true);
        if (sudoku is null || sudoku.IsInvisible)
            return;

        if (this.ShouldYieldSudokuRoommateControl(sudoku))
            return;

        int currentSlot = GetRoommateActivitySlot(Game1.timeOfDay);
        if (this.roommateActivityDay != Game1.Date.TotalDays || this.roommateActivitySlot != currentSlot)
            this.RefreshSudokuRoommateActivity(force: true);

        if (this.roommateBehaviorCooldownTicks > 0)
        {
            this.roommateBehaviorCooldownTicks--;
            return;
        }

        if (Game1.activeClickableMenu is not null || Vector2.Distance(Game1.player.Tile, sudoku.Tile) <= 2.25f)
        {
            sudoku.Halt();
            return;
        }

        if (this.roommateGlideTargetTile.HasValue)
        {
            // If the player walks into the next tile, Sudoku simply waits rather than clipping
            // through them. The glide resumes when the space clears.
            if (Vector2.Distance(Game1.player.Tile, this.roommateGlideTargetTile.Value) <= 1.1f)
                return;

            this.roommateGlideTicks++;
            float raw = Math.Clamp(this.roommateGlideTicks / (float)RoommateGlideTicksPerTile, 0f, 1f);
            float eased = raw * raw * (3f - 2f * raw);
            sudoku.Position = Vector2.Lerp(this.roommateGlideStartPosition, this.roommateGlideTargetPosition, eased);
            sudoku.Halt();

            if (raw >= 1f)
            {
                sudoku.Position = this.roommateGlideTargetPosition;
                this.roommateGlideTargetTile = null;
                this.roommateGlideTicks = 0;
                this.FaceSudokuTowardFocus(sudoku);
            }

            return;
        }

        if (this.roommatePath.Count == 0)
        {
            if (this.roommateActivityTargetTile.HasValue
                && Vector2.Distance(sudoku.Tile, this.roommateActivityTargetTile.Value) > 0.75f)
            {
                this.roommateBehaviorCooldownTicks = 90;
                this.RefreshSudokuRoommateActivity(force: true);
                return;
            }

            this.FaceSudokuTowardFocus(sudoku);
            return;
        }

        Vector2 nextTile = this.roommatePath.Peek();
        if (!farmHouse.CanSpawnCharacterHere(nextTile)
            || Vector2.Distance(Game1.player.Tile, nextTile) <= 1.1f)
        {
            // Furniture/player movement can invalidate a path after it was planned.
            this.RefreshSudokuRoommateActivity(force: true);
            return;
        }

        this.roommatePath.Dequeue();
        this.roommateGlideTargetTile = nextTile;
        this.roommateGlideStartPosition = sudoku.Position;
        this.roommateGlideTargetPosition = nextTile * 64f;
        this.roommateGlideTicks = 0;

        Vector2 direction = nextTile - sudoku.Tile;
        if (Math.Abs(direction.X) > Math.Abs(direction.Y))
            sudoku.faceDirection(direction.X > 0 ? 1 : 3);
        else if (Math.Abs(direction.Y) > 0.01f)
            sudoku.faceDirection(direction.Y > 0 ? 2 : 0);
    }

    private bool CanSudokuUseRoommateBehavior()
    {
        if (!Context.IsWorldReady
            || this.sequenceActive
            || Game1.eventUp
            || Game1.currentLocation is not FarmHouse
            || !ModIdentity.HasArrivalBeenSeen(Game1.player))
        {
            return false;
        }

        if (ModIdentity.IsCursedVhsInstalled(Game1.player)
            && !ModIdentity.HasDailySignalRunToday(Game1.player))
        {
            return false;
        }

        return true;
    }

    private SudokuActivityKind GetCurrentSudokuActivityKind()
    {
        if (!this.roommateCurrentActivity.HasValue)
            this.RefreshSudokuRoommateActivity(force: true);

        return this.roommateCurrentActivity ?? SudokuActivityKind.HouseListening;
    }

    private static int GetRoommateActivitySlot(int time)
    {
        return time switch
        {
            < 1000 => 0,
            < 1300 => 1,
            < 1600 => 2,
            < 1900 => 3,
            < 2200 => 4,
            _ => 5
        };
    }

    private static SudokuActivityKind ChooseRoommateActivityKind(int trust, long seed)
    {
        SudokuActivityKind[] pool = trust switch
        {
            <= 2 => new[]
            {
                SudokuActivityKind.TvWatch,
                SudokuActivityKind.QuietCorner,
                SudokuActivityKind.HouseListening,
                SudokuActivityKind.TvWatch
            },
            <= 6 => new[]
            {
                SudokuActivityKind.TvWatch,
                SudokuActivityKind.HouseListening,
                SudokuActivityKind.FurnitureWatch,
                SudokuActivityKind.QuietCorner,
                SudokuActivityKind.DoorWatch
            },
            <= 12 => new[]
            {
                SudokuActivityKind.TvWatch,
                SudokuActivityKind.FurnitureWatch,
                SudokuActivityKind.PetWatch,
                SudokuActivityKind.HouseListening,
                SudokuActivityKind.DoorWatch
            },
            <= 20 => new[]
            {
                SudokuActivityKind.FurnitureWatch,
                SudokuActivityKind.PetWatch,
                SudokuActivityKind.DoorWatch,
                SudokuActivityKind.TvWatch,
                SudokuActivityKind.WaitingForPlayer
            },
            _ => new[]
            {
                SudokuActivityKind.WaitingForPlayer,
                SudokuActivityKind.PetWatch,
                SudokuActivityKind.TvWatch,
                SudokuActivityKind.FurnitureWatch,
                SudokuActivityKind.HouseListening
            }
        };

        int index = (int)((seed & long.MaxValue) % pool.Length);
        return pool[index];
    }

    private bool TryResolveRoommateActivity(
        FarmHouse farmHouse,
        SudokuActivityKind requested,
        long seed,
        out SudokuActivityKind resolved,
        out Vector2 target,
        out Vector2? focus)
    {
        List<Vector2> safeTiles = this.GetRoommateSafeTiles(farmHouse);
        resolved = requested;
        target = Vector2.Zero;
        focus = null;

        if (safeTiles.Count == 0)
            return false;

        TV? tv = farmHouse.furniture.OfType<TV>().FirstOrDefault();
        List<NPC> possiblePets = farmHouse.characters
            .Where(p => !ModIdentity.IsSudokuNpcId(p.Name)
                && p.GetType().Name.Contains("Pet", StringComparison.OrdinalIgnoreCase))
            .ToList();

        switch (requested)
        {
            case SudokuActivityKind.TvWatch:
                if (tv is not null && TryPickTileNear(safeTiles, tv.TileLocation, out target))
                {
                    focus = tv.TileLocation;
                    return true;
                }
                resolved = SudokuActivityKind.HouseListening;
                break;

            case SudokuActivityKind.PetWatch:
                if (possiblePets.Count > 0)
                {
                    int petIndex = (int)((seed & long.MaxValue) % possiblePets.Count);
                    NPC pet = possiblePets[petIndex];
                    if (TryPickTileNear(safeTiles, pet.Tile, out target))
                    {
                        focus = pet.Tile;
                        return true;
                    }
                }
                resolved = SudokuActivityKind.FurnitureWatch;
                break;

            case SudokuActivityKind.FurnitureWatch:
            {
                var furniture = farmHouse.furniture.Where(p => p is not TV).ToList();
                if (furniture.Count > 0)
                {
                    int furnitureIndex = (int)((seed & long.MaxValue) % furniture.Count);
                    Vector2 furnitureTile = furniture[furnitureIndex].TileLocation;
                    if (TryPickTileNear(safeTiles, furnitureTile, out target))
                    {
                        focus = furnitureTile;
                        return true;
                    }
                }
                resolved = SudokuActivityKind.HouseListening;
                break;
            }

            case SudokuActivityKind.DoorWatch:
            {
                int mapWidth = farmHouse.Map.Layers[0].LayerWidth;
                int mapHeight = farmHouse.Map.Layers[0].LayerHeight;
                Vector2 ideal = new(mapWidth / 2f, Math.Max(1f, mapHeight - 5f));
                target = safeTiles.OrderBy(p => Vector2.Distance(p, ideal)).First();
                focus = new Vector2(target.X, target.Y + 2f);
                return true;
            }

            case SudokuActivityKind.QuietCorner:
            {
                int mapWidth = farmHouse.Map.Layers[0].LayerWidth;
                int mapHeight = farmHouse.Map.Layers[0].LayerHeight;
                Vector2 center = new(mapWidth / 2f, mapHeight / 2f);
                target = safeTiles
                    .OrderByDescending(p => Vector2.Distance(p, center) + Vector2.Distance(p, Game1.player.Tile) * 0.25f)
                    .First();
                focus = center;
                return true;
            }

            case SudokuActivityKind.WaitingForPlayer:
            {
                Vector2 ideal = tv?.TileLocation ?? new Vector2(
                    farmHouse.Map.Layers[0].LayerWidth / 2f,
                    farmHouse.Map.Layers[0].LayerHeight / 2f
                );
                target = safeTiles.OrderBy(p => Vector2.Distance(p, ideal)).First();
                focus = Game1.player.Tile;
                return true;
            }
        }

        // HouseListening is the universal fallback. Favor the upper-middle of the current map,
        // which feels like Sudoku is listening to the walls instead of standing at the exit.
        int width = farmHouse.Map.Layers[0].LayerWidth;
        int height = farmHouse.Map.Layers[0].LayerHeight;
        Vector2 listeningIdeal = new(width / 2f, Math.Max(3f, height * 0.32f));
        target = safeTiles.OrderBy(p => Vector2.Distance(p, listeningIdeal)).First();
        focus = new Vector2(target.X, Math.Max(0f, target.Y - 2f));
        resolved = SudokuActivityKind.HouseListening;
        return true;
    }

    private List<Vector2> GetRoommateSafeTiles(FarmHouse farmHouse)
    {
        List<Vector2> result = new();
        int width = farmHouse.Map.Layers[0].LayerWidth;
        int height = farmHouse.Map.Layers[0].LayerHeight;

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                Vector2 tile = new(x, y);
                if (Vector2.Distance(tile, Game1.player.Tile) < 1.75f)
                    continue;

                try
                {
                    if (farmHouse.CanSpawnCharacterHere(tile))
                        result.Add(tile);
                }
                catch
                {
                    // Custom farmhouse maps can have unusual layer bounds/properties. A single
                    // bad tile should not disable the whole roommate system.
                }
            }
        }

        return result;
    }

    private static bool TryPickTileNear(IReadOnlyList<Vector2> safeTiles, Vector2 focus, out Vector2 target)
    {
        Vector2[] preferredOffsets =
        {
            new(0f, 1f),
            new(1f, 0f),
            new(-1f, 0f),
            new(0f, -1f),
            new(1f, 1f),
            new(-1f, 1f),
            new(2f, 0f),
            new(-2f, 0f)
        };

        foreach (Vector2 offset in preferredOffsets)
        {
            Vector2 candidate = focus + offset;
            if (safeTiles.Any(p => Vector2.Distance(p, candidate) < 0.1f))
            {
                target = candidate;
                return true;
            }
        }

        if (safeTiles.Count > 0)
        {
            target = safeTiles.OrderBy(p => Vector2.Distance(p, focus)).First();
            return true;
        }

        target = Vector2.Zero;
        return false;
    }

    private List<Vector2> FindRoommatePath(FarmHouse farmHouse, Vector2 startTile, Vector2 targetTile)
    {
        Point start = new((int)Math.Round(startTile.X), (int)Math.Round(startTile.Y));
        Point goal = new((int)Math.Round(targetTile.X), (int)Math.Round(targetTile.Y));
        if (start == goal)
            return new List<Vector2>();

        int width = farmHouse.Map.Layers[0].LayerWidth;
        int height = farmHouse.Map.Layers[0].LayerHeight;
        Queue<Point> frontier = new();
        Dictionary<Point, Point> previous = new();
        HashSet<Point> seen = new();

        frontier.Enqueue(start);
        seen.Add(start);

        Point[] directions =
        {
            new(1, 0),
            new(-1, 0),
            new(0, 1),
            new(0, -1)
        };

        bool found = false;
        int inspected = 0;
        while (frontier.Count > 0 && inspected < 4096)
        {
            Point current = frontier.Dequeue();
            inspected++;

            foreach (Point direction in directions)
            {
                Point next = new(current.X + direction.X, current.Y + direction.Y);
                if (next.X < 1 || next.Y < 1 || next.X >= width - 1 || next.Y >= height - 1 || seen.Contains(next))
                    continue;

                Vector2 nextTile = new(next.X, next.Y);
                if (next != goal && Vector2.Distance(nextTile, Game1.player.Tile) <= 1.1f)
                    continue;

                bool passable;
                try
                {
                    passable = farmHouse.CanSpawnCharacterHere(nextTile);
                }
                catch
                {
                    passable = false;
                }

                if (!passable && next != goal)
                    continue;

                seen.Add(next);
                previous[next] = current;

                if (next == goal)
                {
                    found = true;
                    frontier.Clear();
                    break;
                }

                frontier.Enqueue(next);
            }
        }

        if (!found)
            return new List<Vector2>();

        List<Vector2> reversed = new();
        Point cursor = goal;
        while (cursor != start)
        {
            reversed.Add(new Vector2(cursor.X, cursor.Y));
            if (!previous.TryGetValue(cursor, out cursor))
                return new List<Vector2>();
        }

        reversed.Reverse();
        return reversed;
    }

    private void FaceSudokuTowardFocus(NPC sudoku)
    {
        if (!this.roommateActivityFocusTile.HasValue)
            return;

        Vector2 delta = this.roommateActivityFocusTile.Value - sudoku.Tile;
        if (Math.Abs(delta.X) > Math.Abs(delta.Y))
            sudoku.faceDirection(delta.X > 0 ? 1 : 3);
        else if (Math.Abs(delta.Y) > 0.01f)
            sudoku.faceDirection(delta.Y > 0 ? 2 : 0);
    }
}
