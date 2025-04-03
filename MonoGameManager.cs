using Microsoft.Xna.Framework;

namespace AteChips;
public static class MonoGameManager
{

    private static GraphicsDeviceManager _graphics = null!;

    public static void Initialize(Game game)
    {
        game.Content.RootDirectory = "Content";
        game.IsMouseVisible = true;
        _graphics = new GraphicsDeviceManager(game)
        {
            HardwareModeSwitch = false
        };
        game.Window.AllowUserResizing = true;
        //Machine.Instance.Chip8 = game;
    }

    public static void ToggleFullScreen()
    {
        _graphics.IsFullScreen = !_graphics.IsFullScreen;
        _graphics.ApplyChanges();
    }

}
