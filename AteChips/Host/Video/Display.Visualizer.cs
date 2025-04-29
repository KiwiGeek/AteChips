using AteChips.Host.UI.ImGui;
using AteChips.Shared.Settings;
using ImGuiNET;
using Shared.Settings;
using AteChips.Host.Video.EffectSettings;

namespace AteChips.Host.Video;

public partial class Display
{
    public bool VisualShown { get; set; } = false;

    public void Visualize()
    {
        ImGuiSettings settings = SettingsManager.Current.Display.VisualizerLayout;
        ImGuiWindowSettingsManager.Begin("Visual Settings##Main", settings);

        ImGuiWidgets.Checkbox("Maintain Aspect Ratio", 
            () => SettingsManager.Current.Display.VideoSettings.MaintainAspectRatio,
            value =>
            {
                SettingsManager.Current.Display.VideoSettings.MaintainAspectRatio = value;
                SettingsChanged?.Invoke();
            });

        ImGuiWidgets.Checkbox("Fullscreen", 
            () => SettingsManager.Current.Display.VideoSettings.FullScreen,
            _ => ToggleFullScreen()
            );

        PhosphorColor();

        ImGui.SeparatorText("Visual Effects");

        PhosphorDecay();
        CrtScanLines();
        Bloom();
        
        //ImGui.Checkbox("Curvature", ref _postProcessor.EnableCurvature);
        //if (_postProcessor.EnableCurvature &&
        //    ImGui.CollapsingHeader("Curvature Settings", ImGuiTreeNodeFlags.None))
        //{
        //    ImGui.SliderFloat("Curvature Amount", ref _curvature.CurvatureAmount, 0.0f, 1.5f);
        //    ImGui.SliderFloat("Edge Fade Strength", ref _curvature.EdgeFadeStrength, 0.1f, 5.0f);
        //    ImGui.SliderFloat("Flicker Strength", ref _curvature.FlickerStrength, 0.0f, 0.1f);
        //    ImGui.SliderFloat("Flicker Speed", ref _curvature.FlickerSpeed, 1.0f, 120.0f);
        //    ImGui.SliderFloat("RGB Warp Amount", ref _curvature.WarpAmount, 0.0f, 0.01f);
        //}

        //ImGui.Checkbox("Vignette", ref _postProcessor.EnableVignette);
        //ImGui.Checkbox("Chromatic Aberration", ref _postProcessor.EnableChromatic);

        ImGuiWindowSettingsManager.Update(settings);
        ImGui.End();
    }

    private void PhosphorColor()
    {
        ImGui.SeparatorText("Phosphor Color");

        ImGuiWidgets.Checkbox("Use Custom Color",
            () => _videoSettings.CustomPhosphorColor is not null,
            value =>
            {
                _videoSettings.CustomPhosphorColor = value
                    ? new VideoSettings.PhosphorColor()
                    : null;
                _videoSettings.PhosphorColorType = value
                    ? null
                    : VideoSettings.PresetPhosphorColor.Amber;
                SettingsChanged?.Invoke();
            });

        if (_videoSettings.CustomPhosphorColor is not null)
        {
            ImGuiWidgets.SliderFloat("Red",
                () => _videoSettings.CustomPhosphorColor.Red,
                value =>
                {
                    _videoSettings.CustomPhosphorColor.Red = value;
                    SettingsChanged?.Invoke();
                });
            ImGuiWidgets.SliderFloat("Green",
                () => _videoSettings.CustomPhosphorColor.Green,
                value =>
                {
                    _videoSettings.CustomPhosphorColor.Green = value;
                    SettingsChanged?.Invoke();
                });
            ImGuiWidgets.SliderFloat("Blue",
                () => _videoSettings.CustomPhosphorColor.Blue,
                value =>
                {
                    _videoSettings.CustomPhosphorColor.Blue = value;
                    SettingsChanged?.Invoke();
                });
        }
        else
        {
            ImGuiWidgets.ComboEnum("Preset",
                () => (VideoSettings.PresetPhosphorColor)_videoSettings.PhosphorColorType!,
                value =>
                {
                    _videoSettings.PhosphorColorType = value;
                    SettingsChanged?.Invoke();
                });
        }
    }

    private void PhosphorDecay()
    {
        ImGuiWidgets.Checkbox("Phosphor Decay",
            () => SettingsManager.Current.Display.VideoSettings.PhosphorDecaySettings.IsEnabled,
            value =>
            {
                SettingsManager.Current.Display.VideoSettings.PhosphorDecaySettings.IsEnabled = value;
                SettingsChanged?.Invoke();
            });

        if (_videoSettings.PhosphorDecaySettings.IsEnabled)
        {
            ImGuiWidgets.SliderFloat("Decay Rate",
                () => _videoSettings.PhosphorDecaySettings.DecayRate,
                value =>
                {
                    _videoSettings.PhosphorDecaySettings.DecayRate = value;
                    SettingsChanged?.Invoke();
                }, 0.70f, 0.9999f);
        }
    }

    private void CrtScanLines()
    {

        ImGuiWidgets.Checkbox("CRT Scan lines",
            () => SettingsManager.Current.Display.VideoSettings.ScanlineSettings.IsEnabled,
            value =>
            {
                SettingsManager.Current.Display.VideoSettings.ScanlineSettings.IsEnabled = value;
                SettingsChanged?.Invoke();
            });
        // Scanline Shader Parameters
        if (_videoSettings.ScanlineSettings.IsEnabled)
        {
            ImGuiWidgets.SliderFloat("Scanline Intensity",
                () => _videoSettings.ScanlineSettings.Intensity,
                value =>
                {
                    _videoSettings.ScanlineSettings.Intensity = value;
                    SettingsChanged?.Invoke();
                }, 0.0f, 2.0f);

            ImGuiWidgets.SliderFloat("Scanline Sharpness",
                () => _videoSettings.ScanlineSettings.Sharpness,
                value =>
                {
                    _videoSettings.ScanlineSettings.Sharpness = value;
                    SettingsChanged?.Invoke();
                }, 0.0f, 2.0f);

            ImGuiWidgets.SliderFloat("Bleed Amount",
                () => _videoSettings.ScanlineSettings.BleedAmount,
                value =>
                {
                    _videoSettings.ScanlineSettings.BleedAmount = value;
                    SettingsChanged?.Invoke();
                });

            ImGuiWidgets.SliderFloat("Flicker Strength",
                () => _videoSettings.ScanlineSettings.FlickerStrength,
                value =>
                {
                    _videoSettings.ScanlineSettings.FlickerStrength = value;
                    SettingsChanged?.Invoke();
                });

            ImGuiWidgets.SliderFloat("Mask Strength",
                () => _videoSettings.ScanlineSettings.MaskStrength,
                value =>
                {
                    _videoSettings.ScanlineSettings.MaskStrength = value;
                    SettingsChanged?.Invoke();
                });

            ImGuiWidgets.SliderFloat("Slot Sharpness",
                () => _videoSettings.ScanlineSettings.SlotSharpness,
                value =>
                {
                    _videoSettings.ScanlineSettings.SlotSharpness = value;
                    SettingsChanged?.Invoke();
                }, 0.0f, 2.0f);

            if (ImGui.Button("Preset: 1980s Arcade CRT"))
            {
                ScanlineSettings s = _videoSettings.ScanlineSettings;
                s.Intensity = 0.6f;
                s.Sharpness = 1.4f;
                s.BleedAmount = 0.2f;
                s.FlickerStrength = 0.04f;
                s.MaskStrength = 0.15f;
                s.SlotSharpness = 1.0f;
                SettingsChanged?.Invoke();
            }
            ImGui.SameLine();
            if (ImGui.Button("Preset: 1990s PC Monitor CRT"))
            {
                ScanlineSettings s = _videoSettings.ScanlineSettings;
                s.Intensity = 0.4f;
                s.Sharpness = 1.6f;
                s.BleedAmount = 0.08f;
                s.FlickerStrength = 0.01f;
                s.MaskStrength = 0.0f;
                s.SlotSharpness = 1.0f;
                SettingsChanged?.Invoke();
            }
        }
    }

    private void Bloom()
    {
        ImGuiWidgets.Checkbox("Bloom Glow",
            () => SettingsManager.Current.Display.VideoSettings.BloomSettings.IsEnabled,
            value =>
            {
                SettingsManager.Current.Display.VideoSettings.BloomSettings.IsEnabled = value;
                SettingsChanged?.Invoke();
            });

        if (_videoSettings.BloomSettings.IsEnabled)
        {
            ImGuiWidgets.SliderFloat("Bloom Threshold",
                () => _videoSettings.BloomSettings.Threshold,
                value =>
                {
                    _videoSettings.BloomSettings.Threshold = value;
                    SettingsChanged?.Invoke();
                }, 0.0f, 2.0f);

            ImGuiWidgets.SliderFloat("Bloom Intensity",
                () => _videoSettings.BloomSettings.Intensity,
                value =>
                {
                    _videoSettings.BloomSettings.Intensity = value;
                    SettingsChanged?.Invoke();
                }, 0.0f, 3.0f);
        }

    }



}
