using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace HeyYoureCursed;

internal sealed partial class SudokuMenu : IClickableMenu
{
    private readonly int cellSize;

    private readonly DailySudokuService service;
    private readonly SudokuPuzzle puzzle;
    private readonly SudokuPlayMode mode;
    private readonly int stageIndex;
    private readonly Action? returnToStageSelect;

    private int selectedRow;
    private int selectedColumn;
    private bool controllerModeSeen;
    private bool numberPickerOpen;
    private bool helpOpen;
    private int numberPickerValue = 1;
    private string statusText = string.Empty;

    public SudokuMenu(
        DailySudokuService service,
        SudokuPuzzle puzzle,
        SudokuPlayMode mode = SudokuPlayMode.DailyChallenge,
        int stageIndex = -1,
        Action? returnToStageSelect = null
    )
        : base(
            Game1.uiViewport.Width / 2 - GetMenuWidth() / 2,
            Game1.uiViewport.Height / 2 - GetMenuHeight() / 2,
            GetMenuWidth(),
            GetMenuHeight(),
            showUpperRightCloseButton: true
        )
    {
        this.service = service;
        this.puzzle = puzzle;
        this.mode = mode;
        this.stageIndex = stageIndex;
        this.returnToStageSelect = returnToStageSelect;
        this.cellSize = Math.Clamp((this.height - 330) / 9, 34, 48);
        this.controllerModeSeen = SudokuInputState.LastWasController;
        this.statusText = ModEntry.T("sudoku.status.initial");
        this.SelectFirstEditableCell();
    }

    private static int GetMenuWidth() => Math.Clamp(Game1.uiViewport.Width - 64, 600, 700);
    private static int GetMenuHeight() => Math.Clamp(Game1.uiViewport.Height - 48, 650, 820);

    private int GridSize => this.cellSize * 9;
    private int GridX => this.xPositionOnScreen + (this.width - this.GridSize) / 2;
    private int GridY => this.yPositionOnScreen + 94;
    private int NumberButtonSize => Math.Clamp(this.cellSize - 4, 30, 42);
    private int NumberGap => 6;
    private int NumberTotalWidth => 9 * this.NumberButtonSize + 8 * this.NumberGap;
    private int NumberStartX => this.xPositionOnScreen + (this.width - this.NumberTotalWidth) / 2;
    private int NumberY => this.GridY + this.GridSize + 14;
    private int ActionY => this.NumberY + this.NumberButtonSize + 12;
    private Rectangle ClearRect => new(this.xPositionOnScreen + 68, this.ActionY, 150, 46);
    private Rectangle CheckRect => new(this.xPositionOnScreen + this.width - 218, this.ActionY, 150, 46);
    private Rectangle HelpRect => new(this.xPositionOnScreen + 22, this.yPositionOnScreen + 19, 128, 38);
    private Rectangle HelpPanelRect => new(this.xPositionOnScreen + 28, this.yPositionOnScreen + 58, this.width - 56, this.height - 92);
    private Rectangle HelpCloseRect => new(this.HelpPanelRect.Center.X - 90, this.HelpPanelRect.Bottom - 54, 180, 38);
    private int FooterY => this.ActionY + 60;

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (this.helpOpen)
        {
            if (this.HelpCloseRect.Contains(x, y))
                this.CloseHelp();
            return;
        }

        if (this.HelpRect.Contains(x, y))
        {
            this.OpenHelp();
            return;
        }

        this.controllerModeSeen = false;
        SudokuInputState.LastWasController = false;
        this.numberPickerOpen = false;

        base.receiveLeftClick(x, y, playSound);
        if (Game1.activeClickableMenu != this)
            return;

        if (x >= this.GridX && x < this.GridX + GridSize && y >= this.GridY && y < this.GridY + GridSize)
        {
            int col = (x - this.GridX) / this.cellSize;
            int row = (y - this.GridY) / this.cellSize;
            this.selectedRow = Math.Clamp(row, 0, 8);
            this.selectedColumn = Math.Clamp(col, 0, 8);
            int index = this.selectedRow * 9 + this.selectedColumn;
            this.statusText = this.puzzle.Puzzle[index] == '0'
                ? ModEntry.T("sudoku.status.select-empty")
                : ModEntry.T("sudoku.status.given");
            Game1.playSound("shiny4");
            return;
        }

        for (int n = 1; n <= 9; n++)
        {
            Rectangle rect = new(
                this.NumberStartX + (n - 1) * (this.NumberButtonSize + this.NumberGap),
                this.NumberY,
                this.NumberButtonSize,
                this.NumberButtonSize
            );
            if (rect.Contains(x, y))
            {
                this.EnterNumber(n);
                return;
            }
        }

        if (this.ClearRect.Contains(x, y))
        {
            this.EnterNumber(0);
            return;
        }

        if (this.CheckRect.Contains(x, y))
        {
            this.CheckBoard();
            return;
        }
    }

    public override void receiveKeyPress(Keys key)
    {
        if (this.helpOpen)
        {
            if (key is Keys.Escape or Keys.Enter or Keys.Space)
                this.CloseHelp();
            return;
        }

        this.controllerModeSeen = false;
        SudokuInputState.LastWasController = false;
        this.numberPickerOpen = false;

        if (key is Keys.H or Keys.F1)
        {
            this.OpenHelp();
            return;
        }

        if (key == Keys.Escape)
        {
            this.CloseOrReturn();
            return;
        }

        int keyCode = (int)key;
        if (keyCode >= (int)Keys.D1 && keyCode <= (int)Keys.D9)
        {
            this.EnterNumber(keyCode - (int)Keys.D0);
            return;
        }

        if (keyCode >= (int)Keys.NumPad1 && keyCode <= (int)Keys.NumPad9)
        {
            this.EnterNumber(keyCode - (int)Keys.NumPad0);
            return;
        }

        switch (key)
        {
            case Keys.Back:
            case Keys.Delete:
            case Keys.D0:
            case Keys.NumPad0:
                this.EnterNumber(0); return;
            case Keys.Enter:
                this.CheckBoard(); return;
            case Keys.Left:
                this.MoveSelection(0, -1); return;
            case Keys.Right:
                this.MoveSelection(0, 1); return;
            case Keys.Up:
                this.MoveSelection(-1, 0); return;
            case Keys.Down:
                this.MoveSelection(1, 0); return;
        }

        base.receiveKeyPress(key);
    }

    internal bool HandleSmapiInput(SButton button)
    {
        bool isControllerInput = button is
            SButton.ControllerA or SButton.ControllerB or SButton.ControllerX or SButton.ControllerY or SButton.ControllerBack
            or SButton.DPadLeft or SButton.DPadRight or SButton.DPadUp or SButton.DPadDown
            or SButton.LeftThumbstickLeft or SButton.LeftThumbstickRight or SButton.LeftThumbstickUp or SButton.LeftThumbstickDown
            or SButton.LeftShoulder or SButton.RightShoulder
            or SButton.LeftTrigger or SButton.RightTrigger
            or SButton.ControllerStart;

        if (!isControllerInput)
            return false;

        this.controllerModeSeen = true;
        SudokuInputState.LastWasController = true;

        if (this.helpOpen)
        {
            if (button is SButton.ControllerA or SButton.ControllerB or SButton.ControllerStart)
                this.CloseHelp();
            return true;
        }

        if (this.numberPickerOpen)
            return this.HandleNumberPickerInput(button);

        switch (button)
        {
            case SButton.ControllerBack:
                this.OpenHelp(); return true;
            case SButton.DPadLeft:
            case SButton.LeftThumbstickLeft:
                this.MoveSelection(0, -1); return true;
            case SButton.DPadRight:
            case SButton.LeftThumbstickRight:
                this.MoveSelection(0, 1); return true;
            case SButton.DPadUp:
            case SButton.LeftThumbstickUp:
                this.MoveSelection(-1, 0); return true;
            case SButton.DPadDown:
            case SButton.LeftThumbstickDown:
                this.MoveSelection(1, 0); return true;
            case SButton.ControllerA:
                this.OpenNumberPicker(); return true;
            case SButton.ControllerX:
                this.EnterNumber(0); return true;
            case SButton.ControllerY:
            case SButton.ControllerStart:
                this.CheckBoard(); return true;
            case SButton.LeftShoulder:
            case SButton.LeftTrigger:
                this.MoveToNextEditable(-1); return true;
            case SButton.RightShoulder:
            case SButton.RightTrigger:
                this.MoveToNextEditable(1); return true;
            case SButton.ControllerB:
                this.CloseOrReturn(); return true;
        }

        return false;
    }

    public override void receiveGamePadButton(Buttons button)
    {
        SButton? mapped = button switch
        {
            Buttons.DPadLeft => SButton.DPadLeft,
            Buttons.DPadRight => SButton.DPadRight,
            Buttons.DPadUp => SButton.DPadUp,
            Buttons.DPadDown => SButton.DPadDown,
            Buttons.A => SButton.ControllerA,
            Buttons.B => SButton.ControllerB,
            Buttons.Back => SButton.ControllerBack,
            Buttons.X => SButton.ControllerX,
            Buttons.Y => SButton.ControllerY,
            Buttons.LeftShoulder => SButton.LeftShoulder,
            Buttons.RightShoulder => SButton.RightShoulder,
            Buttons.Start => SButton.ControllerStart,
            _ => null
        };

        if (mapped.HasValue && this.HandleSmapiInput(mapped.Value))
            return;

        base.receiveGamePadButton(button);
    }
}
