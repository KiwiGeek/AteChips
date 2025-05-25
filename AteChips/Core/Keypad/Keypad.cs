using System;
using AteChips.Core.Shared.Interfaces;
using AteChips.Core.Shared.Timing;
using AteChips.Shared.Runtime;

namespace AteChips.Core;

public class Keypad : IHardware, IResettable, IKeypad
{
    public string Name => GetType().Name;
    public double FrequencyHz => 120;
    public byte UpdatePriority => UpdatePriorities.Keypad;

    public enum KeyState
    {
        Up,
        Pressed,
        Down,
        Released
    }

    private readonly bool[] _lastKeyStates = new bool[16];
    public KeyState[] KeypadButtons { get; } = new KeyState[16];
    public byte? FirstKeyPressedThisFrame { get; private set; }
    private readonly bool[] _keyPressLatch = new bool[16];
    // Latch counters for each key (frames remaining latched)
    private readonly int[] _latchCounters = new int[16];
    // Configurable latch duration (frames)
    public int LatchDurationFrames { get; set; } = 12;

    public bool Update(double delta)
    {
        FirstKeyPressedThisFrame = null;

        for (int i = 0; i < KeypadButtons.Length; i++)
        {
            KeyState state = KeypadButtons[i];

            // Decrement latch counter if > 0
            if (_latchCounters[i] > 0)
                _latchCounters[i]--;

            // Check if this key was pressed at any point since last frame
            if (_keyPressLatch[i] && FirstKeyPressedThisFrame == null)
            {
                FirstKeyPressedThisFrame = (byte)i;
            }

            // Transition transient states to stable ones
            KeypadButtons[i] = state switch
            {
                KeyState.Pressed => KeyState.Down,
                KeyState.Released => KeyState.Up,
                _ => state
            };

            // Clear latch
            _keyPressLatch[i] = false;
        }

        return false;
    }

    public void Reset()
    {
        for (int i = 0; i < KeypadButtons.Length; i++)
        {
            KeypadButtons[i] = KeyState.Up;
            _latchCounters[i] = 0;
        }

        FirstKeyPressedThisFrame = null;
    }

    // Returns true if the key is considered latched down (for non-blocking checks)
    public bool IsLatchedDown(int key) => _latchCounters[key] > 0;

    public void FeedInput(Func<int, bool> isKeyDown)
    {
        FirstKeyPressedThisFrame = null;

        for (int i = 0; i < 16; i++)
        {
            bool isDown = isKeyDown(i);
            bool wasDown = _lastKeyStates[i];

            KeypadButtons[i] = (isDown, wasDown) switch
            {
                (true, false) => KeyState.Pressed,
                (true, true) => KeyState.Down,
                (false, true) => KeyState.Released,
                (false, false) => KeyState.Up,
            };

            if (isDown && !wasDown)
            {
                _keyPressLatch[i] = true;
                _latchCounters[i] = LatchDurationFrames; // Set latch counter on press
            }
            else if (!isDown && wasDown)
            {
                _latchCounters[i] = 0; // Clear latch on release
            }

            _lastKeyStates[i] = isDown;
        }
    }

    private IHostBridge? _hostBridge;

    public virtual void SetHostBridge(IHostBridge hostBridge)
    {
        _hostBridge = hostBridge;
    }
}
