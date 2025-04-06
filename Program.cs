using AteChips;

Machine machine = Machine.Instance;
machine.Reset();

using Chip8 chip8 = new (machine);
MonoGameManager.Initialize(chip8);

chip8.Run();