using System.Diagnostics;

namespace AteChips;
class Chip8
{

    private readonly Machine _machine;
    private Cpu _cpu;
    private bool done = false;
    private Display _display;

    private double _renderAccumulator = 0;
    private const double RenderHz = 60;

    public Chip8(Machine machine)
    {
        _machine = machine;
    }

    public void Start()
    {
        _cpu = _machine.Get<Cpu>();
        _display = _machine.Get<Display>();

        Stopwatch stopwatch = Stopwatch.StartNew();
        double previousTime = stopwatch.Elapsed.TotalSeconds;

        while (!done)
        {
            double currentTime = stopwatch.Elapsed.TotalSeconds;
            double delta = currentTime - previousTime;
            previousTime = currentTime;

            Update(delta);
            Render(delta);

            _cpu.Step();
            //Thread.Sleep(1);
        }
    }

    private void Update(double delta)
    {
        _cpu.Update(delta);
    }

    private void Render(double delta)
    {
        _renderAccumulator += delta;
        double renderInterval = 1.0 / RenderHz;

        while (_renderAccumulator >= renderInterval)
        {

            _display.Draw(renderInterval);
            _renderAccumulator -= renderInterval;
        }
    }

}
