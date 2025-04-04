using System;
using AteChips.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AteChips;
public class Keyboard : Hardware, IResettable, IKeyboard
{

    private KeyboardState _prevKeyboardState;

    public void Update(GameTime gameTime)
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

        if (keyboard.IsKeyDown(Keys.Escape))
        {
            Environment.Exit(0);
        }

        // Save current state for next frame
        _prevKeyboardState = keyboard;

        // todo: Actually implement the Chip8 keybpad
        // todo: split _keyboard_ and _chip8_keyboard_ into two classes

    }

    public void Reset()
    {
        
    }


    public byte UpdatePriority => 0;
}
