using System;
using ImGuiNET;
using System.Collections.Generic;
using System.Diagnostics;
using AteChips.Host.Audio;
using System.Linq;
using AteChips.Host.UI.ImGui;

// ReSharper disable once CheckNamespace
namespace AteChips.Core;

public partial class Buzzer
{

    private int? _selectedDeviceIndex;
    private List<(int Index, string Name)>? _deviceList;
    private bool _deviceListInitialized = false;

    public override void Visualize()
    {

        ImGui.Begin("Buzzer", ImGuiWindowFlags.AlwaysAutoResize);

        // Get a list of all available sound devices from the Speakers
        StereoSpeakers speakers = HostBridge?.Get<StereoSpeakers>()!;

        // on first call, initialize the device list
        if (_deviceList is null)
        {
            _deviceList = speakers.GetHardwareDevices().ToList();
            _selectedDeviceIndex = 0;       // todo, get this from the speaker.
        }

        Debug.Assert(_selectedDeviceIndex != null);
        Debug.Assert(_deviceList != null);

        // Show the combo with the current selection label
        string previewValue = _deviceList!.ElementAt(_selectedDeviceIndex.Value).Name;

        if (ImGui.BeginCombo("Output Device", previewValue))
        {
            // Only populate devices when the combo is opened
            if (!_deviceListInitialized && speakers != null)
            {
                _deviceList = speakers.GetHardwareDevices().ToList();
                _deviceListInitialized = true;
            }

            for (int i = 0; i < _deviceList.Count; i++)
            {
                bool isSelected = (i == _selectedDeviceIndex);
                if (ImGui.Selectable($"{_deviceList[i].Item2}##{_deviceList[i].Item1}", isSelected))
                {
                    _selectedDeviceIndex = i;
                    speakers!.ConnectToSoundDevice(_deviceList[i].Item1);
                }

                if (isSelected)
                    ImGui.SetItemDefaultFocus();
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

        ImGui.End();


        //if (_waveformPreview.Length == 0)
        //{
        //    GeneratePreviewBuffer();
        //}

    //private float[] _waveformPreview = [];



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

        //if (_waveformPreview.Length > 0)
        //{
        //    System.Numerics.Vector2 cursor = ImGui.GetCursorScreenPos();
        //    System.Numerics.Vector2 size = new(400, 100);
        //    ImDrawListPtr drawList = ImGui.GetWindowDrawList();

        //    drawList.AddRect(cursor, cursor + size, ImGui.GetColorU32(ImGuiCol.FrameBg));

        //    for (int i = 0; i < _waveformPreview.Length - 1; i++)
        //    {
        //        float x1 = cursor.X + (i / (float)_waveformPreview.Length) * size.X;
        //        float y1 = cursor.Y + (1f - (_waveformPreview[i] + 1f) / 2f) * size.Y;
        //        float x2 = cursor.X + ((i + 1) / (float)_waveformPreview.Length) * size.X;
        //        float y2 = cursor.Y + (1f - (_waveformPreview[i + 1] + 1f) / 2f) * size.Y;

        //        uint color = ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0.1f, 1.0f, 0.1f, 1.0f)); // bright green
        //        drawList.AddLine(new System.Numerics.Vector2(x1, y1), new System.Numerics.Vector2(x2, y2), color, 1.5f);
        //    }

        //    // Midline
        //    float midY = cursor.Y + size.Y / 2f;
        //    drawList.AddLine(new System.Numerics.Vector2(cursor.X, midY), new System.Numerics.Vector2(cursor.X + size.X, midY),
        //        ImGui.GetColorU32(new System.Numerics.Vector4(0f, 1f, 0f, 0.2f)));

        //    // Scanlines
        //    for (int y = 0; y < size.Y; y += 8)
        //    {
        //        drawList.AddLine(
        //            new System.Numerics.Vector2(cursor.X, cursor.Y + y),
        //            new System.Numerics.Vector2(cursor.X + size.X, cursor.Y + y),
        //            ImGui.GetColorU32(new System.Numerics.Vector4(0f, 1f, 0f, 0.05f))
        //        );
        //    }

        //    ImGui.Dummy(size); // Reserve the space
        //}

        //ImGui.End();
    }
    //private void GeneratePreviewBuffer()
    //{
    //    int previewSampleCount = (int)(SampleRate * PreviewSeconds);
    //    int samplesPerCycle = (int)(SampleRate / Pitch);

    //    _waveformPreview = new float[previewSampleCount];

    //    for (int i = 0; i < previewSampleCount; i++)
    //    {
    //        float t = i / (float)SampleRate;
    //        float phase = (i % samplesPerCycle) / (float)samplesPerCycle;

    //        _waveformPreview[i] = GetWaveformSample(t, phase) * Volume;
    //    }
    //}

}
