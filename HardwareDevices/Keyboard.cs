using System;
using AteChips.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AteChips;
public class Keyboard : Hardware, IResettable, IKeyboard
{

    private KeyboardState _prevKeyboardState;

    public enum KeyState
    {
        Up,
        Pressed,   // transitioned to down *this* frame
        Down,
        Released   // transitioned to up *this* frame
    }

    private bool[] _lastKeyStates = new bool[16];
    public KeyState[] Keypad { get; } = new KeyState[16];


    public void Update(double gameTime)
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

        Keys[] _chipToMonokey = [
            Keys.X, Keys.D1, Keys.D2, Keys.D3, 
            Keys.Q, Keys.W, Keys.E, Keys.A, 
            Keys.S, Keys.D, Keys.Z, Keys.C,
            Keys.D4, Keys.R, Keys.F, Keys.V
        ];

        for (int i = 0; i < 16; i++)
        {
            var key = _chipToMonokey[i]; // your mapping: 0..F → Keys.D1, Keys.D2, ...
            bool isDown = keyboard.IsKeyDown(key);
            bool wasDown = _lastKeyStates[i];

            Keypad[i] = (isDown, wasDown) switch
            {
                (true, false) => KeyState.Pressed,
                (true, true) => KeyState.Down,
                (false, true) => KeyState.Released,
                (false, false) => KeyState.Up,
            };

            _lastKeyStates[i] = isDown;
        }

        // Save current state for next frame
        _prevKeyboardState = keyboard;

        // todo: split _keyboard_ and _chip8_keyboard_ into two classes so that the ui isn't blocked when the cpuisn't running
    }

    public void Reset()
    {
        for (int i = 0; i < Keypad.Length; i++)
        {
            Keypad[i] = KeyState.Up;
        }
    }


    public byte UpdatePriority => 0;
}
