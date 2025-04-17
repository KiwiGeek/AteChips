using System;
using ImGuiNET;
using System.Collections.Generic;
using System.Diagnostics;
using AteChips.Host.Audio;
using System.Linq;
using AteChips.Host.UI.ImGui;
using System.Runtime.InteropServices;
using AteChips.Core.Shared.Base;

// ReSharper disable once CheckNamespace
namespace AteChips.Core;

public partial class Buzzer : VisualizableHardware
{

    private int? _selectedDeviceIndex;
    private List<(int Index, string Name)>? _deviceList;
    private bool _deviceListInitialized;

    public override void Visualize()
    {

        ImGui.Begin("Buzzer", ImGuiWindowFlags.AlwaysAutoResize);

        // Get a list of all available sound devices from the Speakers
        StereoSpeakers speakers = HostBridge?.Get<StereoSpeakers>()!;

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


        string[] waveforms = Enum.GetNames<WaveformTypes>();
        string selectedName = Waveform.ToString();

        if (ImGui.BeginCombo("Waveform", selectedName))
        {
            foreach (string wave in waveforms)
            {
                bool isSelected = wave == selectedName;
                if (ImGui.Selectable(wave, isSelected))
                {
                    Waveform = Enum.Parse<WaveformTypes>(wave);
                }

                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }

            ImGui.EndCombo();
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

        // === Preview Parameters ===
        const float TIME_WINDOW_SECONDS = 0.05f; // 50ms
        int sampleCount = (int)(SampleRate * TIME_WINDOW_SECONDS);
        Span<float> waveform = stackalloc float[sampleCount];

        double phase = 0.0;
        double phaseIncrement = TAU * Pitch / SampleRate;

        for (int i = 0; i < sampleCount; i++)
        {
            waveform[i] = GetWaveformSample(phase) * Volume;
            phase += phaseIncrement;
            if (phase >= TAU)
            {
                phase -= TAU;
            }
        }

        // === Plot Waveform ===
        System.Numerics.Vector2 size = new(400, 120); // Plot size
        System.Numerics.Vector2 cursor = ImGui.GetCursorScreenPos();
        ImGui.PlotLines("##waveform", ref MemoryMarshal.GetReference(waveform), sampleCount, 0, null, -1f, 1f, size);

        // === Overlay Drawing ===
        ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        // Midline (horizontal center)
        float midY = cursor.Y + (size.Y / 2f);
        drawList.AddLine(
            cursor with { Y = midY },
            new System.Numerics.Vector2(cursor.X + size.X, midY),
            ImGui.GetColorU32(new System.Numerics.Vector4(0f, 1f, 0f, 0.2f))
        );

        // Scan lines every 8 pixels
        for (int y = 0; y < size.Y; y += 8)
        {
            drawList.AddLine(
                cursor with { Y = cursor.Y + y },
                new System.Numerics.Vector2(cursor.X + size.X, cursor.Y + y),
                ImGui.GetColorU32(new System.Numerics.Vector4(0f, 1f, 0f, 0.05f))
            );
        }


        ImGui.End();


        //if (Waveform == WaveformType.Pulse)
        //{
        //    float duty = PulseDutyCycle * 100f; // Show as %
        //    if (ImGui.SliderFloat("Pulse Width (%)", ref duty, 1f, 99f))
        //    {
        //        PulseDutyCycle = duty / 100f;
        //       // GenerateSoundWave();
        //    }
        //}

        //if (Waveform == WaveformType.RoundedSquare)
        //{
        //    ImGui.SliderFloat("Sharpness", ref _roundedSharpness, 1f, 40f);
        //    //GenerateSoundWave();
        //}

        //if (Waveform == WaveformType.Staircase)
        //{
        //    ImGui.SliderInt("Steps", ref _stairSteps, 2, 32);
        //   // GenerateSoundWave();
        //}
    }
}
