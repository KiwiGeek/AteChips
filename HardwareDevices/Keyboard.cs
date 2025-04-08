using System;
using System.Diagnostics;
using AteChips.Interfaces;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace AteChips;

public class Keyboard : Hardware, IResettable, IKeyboard
{

    private double _cycleAccumulator = 0;

    private const double ClockRateHz = 60;
    private const double SecondsPerCycle = 1.0 / ClockRateHz;

    public enum KeyState
    {
        Up,
        Pressed,   // transitioned to down *this* frame
        Down,
        Released   // transitioned to up *this* frame
    }

    private readonly bool[] _lastKeyStates = new bool[16];
    public KeyState[] Keypad { get; } = new KeyState[16];
    private KeyboardState _keyboardState;


    public bool Update(double delta)
    {

        _cycleAccumulator += delta;

        if (_cycleAccumulator >= SecondsPerCycle)
        {

            GameWindow window = Machine.Instance.Get<Display>().Window;
            KeyboardState? keyboard = window.KeyboardState;


            bool alt = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
            bool enterPressed = keyboard.IsKeyDown(Keys.Enter) && !_keyboardState.IsKeyDown(Keys.Enter);

            if (alt && enterPressed)
            {
                // Toggle fullscreen here if needed
                Machine.Instance.Get<Display>().ToggleFullScreen();
            }

            if (keyboard.IsKeyDown(Keys.GraveAccent) && !keyboard.WasKeyDown(Keys.GraveAccent))
            {
                Settings.ShowImGui = !Settings.ShowImGui;
            }

            if (keyboard.IsKeyDown(Keys.Escape))
            {
                return true;
            }

            Keys[] _chipToOpenTK = [
                Keys.X, Keys.D1, Keys.D2, Keys.D3,
                Keys.Q, Keys.W, Keys.E, Keys.A,
                Keys.S, Keys.D, Keys.Z, Keys.C,
                Keys.D4, Keys.R, Keys.F, Keys.V
            ];

            for (int i = 0; i < 16; i++)
            {
                var key = _chipToOpenTK[i];
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

            _cycleAccumulator = 0;

            _keyboardState = keyboard.GetSnapshot();
        }

        // todo: split chip8 keypad and UI keyboard inputs.

        return false;
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
