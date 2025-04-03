using AteChips;

using Chip8 game = new (Machine.Instance);
MonoGameManager.Initialize(game);

game.Run();