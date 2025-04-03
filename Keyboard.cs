using AteChips.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AteChips;
public class Keyboard : Hardware, IResettable, IKeyboard
{

    private KeyboardState _prevKeyboardState;

    public void Update()
    {
        KeyboardState keyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        // Check for a fresh Alt+Enter press
        bool alt = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
        bool enter = keyboard.IsKeyDown(Keys.Enter);
        bool prevEnter = _prevKeyboardState.IsKeyDown(Keys.Enter);
        if (alt && enter && !prevEnter)
        {
            MonoGameManager.ToggleFullScreen();
        }

        bool currentBacktick = keyboard.IsKeyDown(Keys.OemTilde);
        bool prevBacktick = _prevKeyboardState.IsKeyDown(Keys.OemTilde);

        if (currentBacktick && !prevBacktick)
        {
            Settings.ShowImGui = !Settings.ShowImGui;
        }

        // Save current state for next frame
        _prevKeyboardState = keyboard;

    }

    public void Reset()
    {
        
    }
}
