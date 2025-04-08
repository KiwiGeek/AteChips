using AteChips.Shared.Interfaces;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace AteChips.Core.Keypad;

public class Keyboard : IHardware, IResettable, IKeyboard
{
    public string Name => GetType().Name;

    private const double ClockRateHz = 60;
    private const double SecondsPerCycle = 1.0 / ClockRateHz;
    private double _cycleAccumulator = 0;

    public enum KeyState
    {
        Up,
        Pressed,
        Down,
        Released
    }

    private readonly bool[] _lastKeyStates = new bool[16];
    public KeyState[] Keypad { get; } = new KeyState[16];
    private KeyboardState _keyboardState;

    public byte? FirstKeyPressedThisFrame { get; private set; }

    public bool Update(double delta)
    {
        _cycleAccumulator += delta;

        //if (_cycleAccumulator >= SecondsPerCycle)
        //{
        //    GameWindow window = Chip8Machine.Instance.Get<Display>().Window;
        //    KeyboardState keyboard = window.KeyboardState;

        //    bool alt = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
        //    bool enterPressed = keyboard.IsKeyDown(Keys.Enter) && !_keyboardState.IsKeyDown(Keys.Enter);
        //    if (alt && enterPressed)
        //        Chip8Machine.Instance.Get<Display>().ToggleFullScreen();

        //    if (keyboard.IsKeyDown(Keys.GraveAccent) && !_keyboardState.IsKeyDown(Keys.GraveAccent))
        //        Settings.ShowImGui ^= true;

        //    if (keyboard.IsKeyDown(Keys.Escape))
        //        return true;

        //    Keys[] _chipToOpenTK = [
        //        Keys.X, Keys.D1, Keys.D2, Keys.D3,
        //        Keys.Q, Keys.W, Keys.E, Keys.A,
        //        Keys.S, Keys.D, Keys.Z, Keys.C,
        //        Keys.D4, Keys.R, Keys.F, Keys.V
        //    ];

        //    FirstKeyPressedThisFrame = null;

        //    for (int i = 0; i < 16; i++)
        //    {
        //        Keys key = _chipToOpenTK[i];
        //        bool isDown = keyboard.IsKeyDown(key);
        //        bool wasDown = _lastKeyStates[i];

        //        Keypad[i] = (isDown, wasDown) switch
        //        {
        //            (true, false) => KeyState.Pressed,
        //            (true, true) => KeyState.Down,
        //            (false, true) => KeyState.Released,
        //            (false, false) => KeyState.Up,
        //        };

        //        if (Keypad[i] == KeyState.Pressed && FirstKeyPressedThisFrame == null)
        //        {
        //            FirstKeyPressedThisFrame = (byte)i;
        //        }

        //        _lastKeyStates[i] = isDown;
        //    }

        //    _keyboardState = keyboard.GetSnapshot();
            _cycleAccumulator = 0;
        //}

        return false;
    }

    public void Reset()
    {
        for (int i = 0; i < Keypad.Length; i++)
        {
            Keypad[i] = KeyState.Up;
        }

        FirstKeyPressedThisFrame = null;
    }

    public byte UpdatePriority => 0;
}
