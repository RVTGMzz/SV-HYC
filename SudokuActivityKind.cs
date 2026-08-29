namespace HeyYoureCursed;

/// <summary>
/// A small set of readable roommate states. The movement system chooses one state per time block,
/// then resolves a safe farmhouse tile for it at runtime so farmhouse upgrades/furniture layouts
/// don't need hard-coded coordinates.
/// </summary>
internal enum SudokuActivityKind
{
    TvWatch,
    FurnitureWatch,
    PetWatch,
    HouseListening,
    DoorWatch,
    QuietCorner,
    WaitingForPlayer
}
