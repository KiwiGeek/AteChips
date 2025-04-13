using AteChips.Core;
using AteChips.Core.Shared.Interfaces;
using AteChips.Core.Shared.Timing;
using AteChips.Host.Video;
using AteChips.Shared.Settings;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace AteChips.Host.Input;

public class Keyboard : IUpdatable
{
    private readonly Display _display;
    private readonly Keypad _keypad;
    private KeyboardState? _lastSnapshot;
    private readonly Keys[] _chipToOpenTk =
    [
        Keys.X, Keys.D1, Keys.D2, Keys.D3,
        Keys.Q, Keys.W, Keys.E, Keys.A,
        Keys.S, Keys.D, Keys.Z, Keys.C,
        Keys.D4, Keys.R, Keys.F, Keys.V
    ];

    public Keyboard(Display display, Keypad keypad)
    {
        _display = display;
        _keypad = keypad;
    }

    public double FrequencyHz => 120;
    public byte UpdatePriority => UpdatePriorities.Keyboard;

    public bool Update(double delta)
    {
        KeyboardState? keyboard = _display.Window.KeyboardState;
        KeyboardState? snapshot = _lastSnapshot ?? keyboard.GetSnapshot(); // fallback for first frame

        bool alt = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
        if (alt && keyboard.IsKeyDown(Keys.Enter) && !snapshot.IsKeyDown(Keys.Enter))
        {
            _display.ToggleFullScreen();
        }

        if (keyboard.IsKeyDown(Keys.GraveAccent) && !snapshot.IsKeyDown(Keys.GraveAccent))
        {
            Settings.ShowImGui ^= true;
        }

        if (keyboard.IsKeyDown(Keys.Escape))
        {
            return true;
        }

        _lastSnapshot = keyboard.GetSnapshot(); // store for next frame

        // Convert physical key state to emulated key state
        _keypad.FeedInput(key =>
        {
            Keys physicalKey = _chipToOpenTk[key];
            return keyboard.IsKeyDown(physicalKey);
        });

        return false;
    }
}
