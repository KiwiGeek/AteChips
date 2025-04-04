using AteChips;

using Chip8 chip8 = new (Machine.Instance);
MonoGameManager.Initialize(chip8);

chip8.Run();