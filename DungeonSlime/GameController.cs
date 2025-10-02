using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using SaintGameLibrary.Input;
using SaintGameLibrary;

namespace DungeonSlime;

public static class GameController
{
    private static KeyboardInfo s_keyboard => Core.Input.Keyboard;
    private static GamePadInfo s_gamePad => Core.Input.GamePads[(int)PlayerIndex.One];

    /// <summary>
    /// Returns true if the player has triggered the "move up" action.
    /// </summary>
    public static bool MoveUp() => 
        s_keyboard.WasKeyJustPressed(Keys.Up) ||
        s_keyboard.WasKeyJustPressed(Keys.W) ||
        s_gamePad.WasButtonJustPressed(Buttons.DPadUp) ||
        s_gamePad.WasButtonJustPressed(Buttons.LeftThumbstickUp);

    /// <summary>
    /// Returns true if the player has triggered the "move down" action.
    /// </summary>
    public static bool MoveDown() => 
        s_keyboard.WasKeyJustPressed(Keys.Down) ||
        s_keyboard.WasKeyJustPressed(Keys.S) ||
        s_gamePad.WasButtonJustPressed(Buttons.DPadDown) ||
        s_gamePad.WasButtonJustPressed(Buttons.LeftThumbstickDown);

    /// <summary>
    /// Returns true if the player has triggered the "move left" action.
    /// </summary>
    public static bool MoveLeft() => 
        s_keyboard.WasKeyJustPressed(Keys.Left) ||
        s_keyboard.WasKeyJustPressed(Keys.A) ||
        s_gamePad.WasButtonJustPressed(Buttons.DPadLeft) ||
        s_gamePad.WasButtonJustPressed(Buttons.LeftThumbstickLeft);

    /// <summary>
    /// Returns true if the player has triggered the "move right" action.
    /// </summary>
    public static bool MoveRight() =>
        s_keyboard.WasKeyJustPressed(Keys.Right) ||
        s_keyboard.WasKeyJustPressed(Keys.D) ||
        s_gamePad.WasButtonJustPressed(Buttons.DPadRight) ||
        s_gamePad.WasButtonJustPressed(Buttons.LeftThumbstickRight);

    /// <summary>
    /// Returns true if the player has triggered the "pause" action.
    /// </summary>
    public static bool Pause()
    {
        return s_keyboard.WasKeyJustPressed(Keys.Escape) ||
               s_gamePad.WasButtonJustPressed(Buttons.Start);
    }

    /// <summary>
    /// Returns true if the player has triggered the "action" button,
    /// typically used for menu confirmation.
    /// </summary>
    public static bool Action()
    {
        return s_keyboard.WasKeyJustPressed(Keys.Enter) ||
               s_gamePad.WasButtonJustPressed(Buttons.A);
    }
}
