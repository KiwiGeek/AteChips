using ImGuiNET;
using Microsoft.Xna.Framework.Audio;
using System;

namespace AteChips;
public partial class Buzzer
{

    private float[] _waveformPreview = [];

    public override void RenderVisual()
    {

        if (_waveformPreview.Length == 0)
        {
            GeneratePreviewBuffer();
        }

        ImGui.Begin("Buzzer", ImGuiWindowFlags.AlwaysAutoResize);

        if (ImGui.Checkbox("Mute", ref _isMuted))
        {
            GenerateSoundWave();
        }

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

        if (Waveform == WaveformType.Pulse)
        {
            float duty = PulseDutyCycle * 100f; // Show as %
            if (ImGui.SliderFloat("Pulse Width (%)", ref duty, 1f, 99f))
            {
                PulseDutyCycle = duty / 100f;
                GenerateSoundWave();
            }
        }

        if (Waveform == WaveformType.RoundedSquare)
        {
            ImGui.SliderFloat("Sharpness", ref _roundedSharpness, 1f, 40f);
            GenerateSoundWave();
        }

        if (Waveform == WaveformType.Staircase)
        {
            ImGui.SliderInt("Steps", ref _stairSteps, 2, 32);
            GenerateSoundWave();
        }

        float pitchHz = Pitch;
        if (ImGui.SliderFloat("Pitch (Hz)", ref pitchHz, 50f, 2000f))
        {
            Pitch = pitchHz;
            GenerateSoundWave();
        }

        float volume = Volume;
        if (ImGui.SliderFloat("Volume", ref volume, 0f, 1f))
        {
            Volume = volume;
            GenerateSoundWave();
        }

        if (_buzzerInstance.State == SoundState.Playing)
        {
            if (ImGui.Button("Stop"))
            {
                _buzzerInstance.Stop();
            }
        }
        else
        {
            if (ImGui.Button("Play"))
            {
                _buzzerInstance.Play();
            }
        }

        if (_waveformPreview.Length > 0)
        {
            System.Numerics.Vector2 cursor = ImGui.GetCursorScreenPos();
            System.Numerics.Vector2 size = new(400, 100);
            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRect(cursor, cursor + size, ImGui.GetColorU32(ImGuiCol.FrameBg));

            for (int i = 0; i < _waveformPreview.Length - 1; i++)
            {
                float x1 = cursor.X + (i / (float)_waveformPreview.Length) * size.X;
                float y1 = cursor.Y + (1f - (_waveformPreview[i] + 1f) / 2f) * size.Y;
                float x2 = cursor.X + ((i + 1) / (float)_waveformPreview.Length) * size.X;
                float y2 = cursor.Y + (1f - (_waveformPreview[i + 1] + 1f) / 2f) * size.Y;

                uint color = ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0.1f, 1.0f, 0.1f, 1.0f)); // bright green
                drawList.AddLine(new System.Numerics.Vector2(x1, y1), new System.Numerics.Vector2(x2, y2), color, 1.5f);
            }

            // Midline
            float midY = cursor.Y + size.Y / 2f;
            drawList.AddLine(new System.Numerics.Vector2(cursor.X, midY), new System.Numerics.Vector2(cursor.X + size.X, midY),
                ImGui.GetColorU32(new System.Numerics.Vector4(0f, 1f, 0f, 0.2f)));

            // Scanlines
            for (int y = 0; y < size.Y; y += 8)
            {
                drawList.AddLine(
                    new System.Numerics.Vector2(cursor.X, cursor.Y + y),
                    new System.Numerics.Vector2(cursor.X + size.X, cursor.Y + y),
                    ImGui.GetColorU32(new System.Numerics.Vector4(0f, 1f, 0f, 0.05f))
                );
            }

            ImGui.Dummy(size); // Reserve the space
        }

        ImGui.End();
    }
    private void GeneratePreviewBuffer()
    {
        int previewSampleCount = (int)(SampleRate * PreviewSeconds);
        int samplesPerCycle = (int)(SampleRate / Pitch);

        _waveformPreview = new float[previewSampleCount];

        for (int i = 0; i < previewSampleCount; i++)
        {
            float t = i / (float)SampleRate;
            float phase = (i % samplesPerCycle) / (float)samplesPerCycle;

            _waveformPreview[i] = GetWaveformSample(t, phase) * Volume;
        }
    }

}
