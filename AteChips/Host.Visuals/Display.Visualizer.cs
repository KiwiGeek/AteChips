using AteChips.Host.UI.ImGui;
using AteChips.Shared.Settings;
using ImGuiNET;
using Shared.Settings;

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

        //ImGui.Checkbox("CRT Scanlines", ref _postProcessor.EnableScanlines);
        //if (_postProcessor.EnableScanlines &&
        //    ImGui.CollapsingHeader("Scanline Settings", ImGuiTreeNodeFlags.None))
        //{
        //    ImGui.SliderFloat("Scanline Intensity", ref _scanlines.ScanlineIntensity, 0.0f, 1.0f);
        //    ImGui.SliderFloat("Scanline Sharpness", ref _scanlines.ScanlineSharpness, 0.1f, 10.0f);
        //    ImGui.SliderFloat("Bleed Amount", ref _scanlines.BleedAmount, 0.0f, 1.0f);
        //    ImGui.SliderFloat("Flicker Strength", ref _scanlines.FlickerStrength, 0.0f, 0.2f);
        //    ImGui.SliderFloat("RGB Mask Strength", ref _scanlines.MaskStrength, 0.0f, 1.0f);
        //    ImGui.SliderFloat("Slot Sharpness", ref _scanlines.SlotSharpness, 1.0f, 20.0f);
        //}

        //ImGui.Checkbox("Bloom", ref _postProcessor.EnableBloom);

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

        //PhosphorDecay();

        ImGuiWindowSettingsManager.Update(settings);
        ImGui.End();
    }

    //private void PhosphorDecay()
    //{
    //    ImGui.Checkbox("Phosphor Decay", ref _postProcessor.EnablePhosphor);

    //    if (_postProcessor.EnablePhosphor)
    //    {
    //        ImGui.SliderFloat("Decay Amount", ref _postProcessor.PhosphorSettings.Decay, 0.90f, 1.0f, "%.3f");
    //    }
    //}


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
}
