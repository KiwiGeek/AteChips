using ImGuiNET;
using System.Numerics;

namespace AteChips;
public partial class Ram
{

    private bool firstOpen = true;

    public override void Visualize()
    {

        if (firstOpen)
        {
            ImGuiViewportPtr viewport = ImGui.GetMainViewport();
            Vector2 workPos = viewport.WorkPos;
            Vector2 workSize = viewport.WorkSize;

            Vector2 windowSize = new (550, 540);
            Vector2 topRightPos = new (workPos.X + workSize.X - windowSize.X - 7, workPos.Y + 45 + 10 + 446);

            ImGui.SetNextWindowPos(topRightPos, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.FirstUseEver);
        }

        _cpu ??= Chip8Machine.Instance.Get<Cpu>();

        ImGui.Begin("Ram", ImGuiWindowFlags.NoSavedSettings);

        const int BYTES_PER_ROW = 16;

        for (int address = 0; address < Memory.Length; address += BYTES_PER_ROW)
        {
            // Print address
            ImGui.Text($"0x{address:X4}: ");

            ImGui.SameLine();

            // Print hex values
            for (int i = 0; i < BYTES_PER_ROW; i++)
            {
                if (address + i < Memory.Length)
                {
                    ImGui.SameLine();
                    Vector4 color = new(0.8f, 0.8f, 0.8f, 1f);

                    if (address + i >= _cpu.ProgramCounter && address + i < _cpu.ProgramCounter + 2)
                    {
                        color = new Vector4(1f, 1f, 0f, 1f);
                    }

                    if (address + i == PROGRAM_COUNTER_UPPER_ADDR ||
                        address + i == PROGRAM_COUNTER_LOWER_ADDR)
                    {
                        color = new Vector4(0.3f, 0.3f, 1f, 1f);
                    }

                    if (address + i >= _cpu.IndexRegister && address + i < _cpu.IndexRegister + 2)
                    {
                        color = new Vector4(.6f, 0f, 0.6f, 1f);
                    }

                    if (address + i == INDEX_REGISTER_UPPER_ADDR ||
                        address + i == INDEX_REGISTER_LOWER_ADDR)
                    {
                        color = new Vector4(1f, 0f, 1f, 1f);
                    }

                    if (address + i == STACK_POINTER_ADDR)
                    {
                        color = new Vector4(0f, 1f, 1f, 1f);
                    }

                    if (address + i >= _cpu.StackPointer && address + i < _cpu.StackPointer + 2)
                    {
                        color = new Vector4(0f, .6f, .6f, 1f);
                    }

                    if (address + i == DELAY_TIMER_ADDR)
                    {
                        color = new Vector4(0f, 1f, 0f, 1f);
                    }

                    if (address + i == SOUND_TIMER_ADDR)
                    {
                        color = new Vector4(1f, 0f, 0f, 1f);
                    }

                    ImGui.TextColored(color,
                        $"{Memory[address + i]:X2} ");
                    ImGui.SameLine();
                }
            }

            ImGui.NewLine();
        }

        if (firstOpen)
        {
            firstOpen ^= true;
            ImGui.SetScrollY(500);
        }

        ImGui.End();
    }

}
