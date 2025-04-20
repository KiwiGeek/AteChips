using System;
using ImGuiNET;
using System.Collections.Generic;
using System.Diagnostics;
using AteChips.Host.Audio;
using System.Linq;
using AteChips.Host.UI.ImGui;
using AteChips.Core.Shared.Interfaces;

// ReSharper disable once CheckNamespace
namespace AteChips.Core;

public partial class Buzzer
{

    private int? _selectedDeviceIndex;
    private List<(int Index, string Name)>? _deviceList;
    private bool _deviceListInitialized;

    const float TIME_WINDOW_SECONDS = 0.016f;
    private int _sampleCount;
    private float[] _waveform = null!;

    private WaveformSettings _lastVisualizedSettings;

    private record struct WaveformSettings(
        float Pitch,
        float Volume,
        WaveformType Waveform,
        float DutyCycle,
        float Sharpness,
        int Steps
    );

    private void VisualizerInit()
    {
        // calculate duration and initialize the visualization preview waveform
        _sampleCount = (int)(SampleRate * TIME_WINDOW_SECONDS);
        _waveform = new float[_sampleCount];
    }

    public void Visualize()
    {
        ImGui.SetNextWindowSizeConstraints(
            new System.Numerics.Vector2(400, 300), // Min size
            new System.Numerics.Vector2(float.MaxValue, float.MaxValue) // Max size (infinite)
        );
        ImGui.Begin("Buzzer");

        // Get a list of all available sound devices from the Speakers
        StereoSpeakers speakers = IVisualizable.HostBridge.Get<StereoSpeakers>()!;

        // on first call, initialize the device list
        if (_deviceList is null)
        {
            _deviceList = [.. StereoSpeakers.GetHardwareDevices()];
            _selectedDeviceIndex = 0; // todo: get this from the speaker.
        }

        Debug.Assert(_selectedDeviceIndex != null);
        Debug.Assert(_deviceList != null);

        // Show the combo with the current selection label
        string previewValue = _deviceList!.ElementAt(_selectedDeviceIndex.Value).Name;

        if (ImGui.BeginCombo("Output Device", previewValue))
        {
            // Only populate devices when the combo is opened
            if (!_deviceListInitialized)
            {
                _deviceList = [.. StereoSpeakers.GetHardwareDevices()];
                _deviceListInitialized = true;
            }

            for (int i = 0; i < _deviceList.Count; i++)
            {
                bool isSelected = (i == _selectedDeviceIndex);
                if (ImGui.Selectable($"{_deviceList[i].Name}##{_deviceList[i].Index}", isSelected))
                {
                    _selectedDeviceIndex = i;
                    speakers.ConnectToSoundDevice(_deviceList[i].Index);
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }

            ImGui.EndCombo();
        }
        else
        {
            // Reset so next open will re-enumerate
            _deviceListInitialized = false;
        }

        ImGuiWidgets.Checkbox("Mute", () => IsMuted, v => IsMuted = v);
        ImGuiWidgets.SliderFloat("Pitch (Hz)", () => Pitch, v => Pitch = v, 50f, 2000f);
        ImGuiWidgets.SliderFloat("Volume", () => Volume, v => Volume = v);

        string[] waveforms = Enum.GetNames<WaveformType>();
        string selectedName = Waveform.ToString();

        if (ImGui.BeginCombo("Waveform", selectedName))
        {
            foreach (string wave in waveforms)
            {
                bool isSelected = wave == selectedName;
                if (ImGui.Selectable(wave, isSelected))
                {
                    Waveform = Enum.Parse<WaveformType>(wave);
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }

            ImGui.EndCombo();
        }

        if (Waveform is WaveformType.Pulse or WaveformType.StaticBuzz or WaveformType.RetroLaser
            or WaveformType.MorphPulse)
        {
            ImGuiWidgets.SliderFloat("Phase Duty Cycle (%)", 
                () => PulseDutyCycle / TAU * 100f,
                (v) => PulseDutyCycle = (float)(v / 100.0) * TAU, 
                0f, 100f);
        }

        if (Waveform == WaveformType.RoundedSquare)
        {
            ImGuiWidgets.SliderFloat("Sharpness", () => RoundedSharpness, (v) => RoundedSharpness = v, 1f, 40f);
        }

        if (Waveform == WaveformType.Staircase)
        {
            ImGuiWidgets.SliderInt("Steps", () => StairSteps, (v) => StairSteps = v, 2, 32);
        }


        if (!TestTone)
        {
            if (ImGui.Button("Test"))
            {
                TestTone = true;
            }
        }
        else
        {
            if (ImGui.Button("Stop"))
            {
                TestTone = false;
            }
        }


        ImGui.Text("Waveform Preview");

        if (NeedsToGenerateWaveform)
        {
            GenerateWaveformPreview();
        }

        System.Numerics.Vector2 cursor = ImGui.GetCursorScreenPos();
        System.Numerics.Vector2 size = ImGui.GetContentRegionAvail();

        ImGui.PlotLines("##waveform", ref _waveform[0], _sampleCount, 0, null, -1f, 1f, size);

        // Midline
        float midY = cursor.Y + (size.Y / 2f);
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        drawList.AddLine(
            cursor with { Y = midY },
            new System.Numerics.Vector2(cursor.X + size.X, midY),
            ImGui.GetColorU32(new System.Numerics.Vector4(0f, 1f, 0f, 0.2f))
        );

        // Scan lines
        for (int y = 0; y < size.Y; y += 8)
        {
            drawList.AddLine(
                cursor with { Y = cursor.Y + y },
                new System.Numerics.Vector2(cursor.X + size.X, cursor.Y + y),
                ImGui.GetColorU32(new System.Numerics.Vector4(0f, 1f, 0f, 0.05f))
            );
        }

        ImGui.End();
    }

    private bool NeedsToGenerateWaveform => _lastVisualizedSettings != CurrentSettings;
    private WaveformSettings CurrentSettings =>
        new(Pitch, Volume, Waveform, PulseDutyCycle, RoundedSharpness, StairSteps);

    private void GenerateWaveformPreview()
    {
        float phase = 0.0f;
        float phaseIncrement = TAU * Pitch / SampleRate;

        for (int i = 0; i < _sampleCount; i++)
        {
            float t = i / (float)SampleRate;

            _waveform[i] = GetWaveformSample(phase, t) * Volume;
            phase += phaseIncrement;
            if (phase >= TAU)
            {
                phase -= TAU;
            }
        }

        _lastVisualizedSettings = CurrentSettings;
    }
}
