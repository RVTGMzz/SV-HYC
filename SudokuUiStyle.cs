using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace HeyYoureCursed;

/// <summary>
/// Shared pixel-soft styling for Sudoku's custom menus. The corners are stepped rather than
/// anti-aliased so the panels stay at home in Stardew's pixel-art UI while feeling less boxy.
/// </summary>
internal static class SudokuUiStyle
{
    public static void DrawRoundedPanel(
        SpriteBatch b,
        Rectangle rect,
        Color fill,
        Color border,
        int borderThickness = 2,
        int radius = 10
    )
    {
        if (rect.Width <= 0 || rect.Height <= 0)
            return;

        borderThickness = Math.Clamp(borderThickness, 1, Math.Max(1, Math.Min(rect.Width, rect.Height) / 4));
        radius = Math.Clamp(radius, 4, Math.Max(4, Math.Min(rect.Width, rect.Height) / 3));

        DrawRoundedFill(b, rect, border, radius);

        Rectangle inner = new(
            rect.X + borderThickness,
            rect.Y + borderThickness,
            Math.Max(1, rect.Width - borderThickness * 2),
            Math.Max(1, rect.Height - borderThickness * 2)
        );
        DrawRoundedFill(b, inner, fill, Math.Max(3, radius - borderThickness));
    }

    private static void DrawRoundedFill(SpriteBatch b, Rectangle rect, Color color, int radius)
    {
        if (rect.Width <= 0 || rect.Height <= 0)
            return;

        radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);
        if (radius < 3)
        {
            b.Draw(Game1.staminaRect, rect, color);
            return;
        }

        // Three stepped bands on the top and bottom approximate a rounded corner while keeping
        // every edge pixel-crisp. The wide center band fills the rest of the panel.
        int band1 = Math.Max(1, radius / 3);
        int band2 = Math.Max(1, radius / 3);
        int band3 = Math.Max(1, radius - band1 - band2);

        DrawBand(b, rect, rect.Y, band1, radius, color);
        DrawBand(b, rect, rect.Y + band1, band2, Math.Max(2, radius / 2), color);
        DrawBand(b, rect, rect.Y + band1 + band2, band3, Math.Max(1, radius / 4), color);

        int middleY = rect.Y + radius;
        int middleHeight = Math.Max(0, rect.Height - radius * 2);
        if (middleHeight > 0)
            b.Draw(Game1.staminaRect, new Rectangle(rect.X, middleY, rect.Width, middleHeight), color);

        int bottomStart = rect.Bottom - radius;
        DrawBand(b, rect, bottomStart, band3, Math.Max(1, radius / 4), color);
        DrawBand(b, rect, bottomStart + band3, band2, Math.Max(2, radius / 2), color);
        DrawBand(b, rect, bottomStart + band3 + band2, band1, radius, color);
    }

    private static void DrawBand(SpriteBatch b, Rectangle bounds, int y, int height, int inset, Color color)
    {
        if (height <= 0)
            return;

        inset = Math.Clamp(inset, 0, Math.Max(0, bounds.Width / 2 - 1));
        int width = bounds.Width - inset * 2;
        if (width <= 0)
            return;

        b.Draw(Game1.staminaRect, new Rectangle(bounds.X + inset, y, width, height), color);
    }
}
