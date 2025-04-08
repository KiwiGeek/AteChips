using AteChips.Video.ImGui;
using ImGuiNET;
using System.Numerics;

namespace AteChips;
public partial class Display
{

    public override void RenderVisual()
    {
        ImGui.Begin("Visual Settings##Main", ImGuiWindowFlags.NoDocking);

        ImGuiHelpers.Checkbox("Maintain Aspect Ratio", () => Settings.MaintainAspectRatio,
            value => Settings.MaintainAspectRatio = value);

        //PhosphorColor();

        //ImGui.SeparatorText("Visual Effects");

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


    //private void PhosphorColor()
    //{
    //    ImGui.SeparatorText("Phosphor Color");

    //    // Preset dropdown
    //    string[] colorOptions = ["White", "Amber", "Green"];

    //    ImGui.Checkbox("Use Custom Color", ref _useCustomColor);

    //    if (_useCustomColor)
    //    {
    //        ImGui.SliderFloat("Red", ref _customColor.X, 0.0f, 1.0f);
    //        ImGui.SliderFloat("Green", ref _customColor.Y, 0.0f, 1.0f);
    //        ImGui.SliderFloat("Blue", ref _customColor.Z, 0.0f, 1.0f);
    //        _postProcessor.PhosphorColor = _customColor;
    //    }
    //    else
    //    {
    //        if (ImGui.Combo("Preset", ref _selectedPhosphorIndex, colorOptions, colorOptions.Length))
    //        {
    //            _customColor = _selectedPhosphorIndex switch
    //            {
    //                0 => new Vector3(1.0f, 1.0f, 1.0f), // White
    //                1 => new Vector3(1.0f, 0.64f, 0.1f), // Amber
    //                2 => new Vector3(0.1f, 1.0f, 0.1f), // Green
    //                _ => new Vector3(1.0f, 1.0f, 1.0f)
    //            };
    //            _postProcessor.PhosphorColor = _customColor;
    //        }
    //    }
    //}
}
