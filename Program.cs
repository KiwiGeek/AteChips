using AteChips;

Machine machine = Machine.Instance;
machine.Reset();

Chip8 chip8 = new(machine);
chip8.Start();