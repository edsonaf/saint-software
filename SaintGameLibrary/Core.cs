using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SaintGameLibrary.Audio;
using SaintGameLibrary.Input;
using System;

namespace SaintGameLibrary;

public class Core : Game
{
    internal static Core s_instance;

    public static Core Instance => s_instance;

    public static GraphicsDeviceManager Graphics { get; private set; }

    public static new GraphicsDevice GraphicsDevice { get; private set; }

    public static SpriteBatch SpriteBatch { get; private set; }

    public static new ContentManager Content { get; private set; }

    public static InputManager Input { get; private set; }

    public static bool ExitOnEscape { get; set; }

    public static AudioController Audio { get; private set; }

    /// <summary>
    /// Creates a new Core instance.
    /// </summary>
    /// <param name="title">The title to display in the title bar of the game window.</param>
    /// <param name="width">The initial width, in pixels, of the game window.</param>
    /// <param name="height">The initial height, in pixels, of the game window.</param>
    /// <param name="fullScreen">Indicates if the game should start in fullscreen mode.</param>
    public Core(string title, int width, int height, bool fullScreen)
    {
        if (s_instance != null)
        {
            throw new InvalidOperationException($"Only a single Core instance can be created");
        }

        s_instance = this;
        Graphics = new GraphicsDeviceManager(this);
        Graphics.PreferredBackBufferWidth = width;
        Graphics.PreferredBackBufferHeight = height;
        Graphics.IsFullScreen = fullScreen;
        Graphics.ApplyChanges();
        Window.Title = title;
        Content = base.Content;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        ExitOnEscape = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
        GraphicsDevice = base.GraphicsDevice;
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        Input = new InputManager();
        Audio = new AudioController();
    }

    protected override void UnloadContent()
    {
        Audio.Dispose();
        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        Input.Update(gameTime);

        if (ExitOnEscape && Input.Keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        base.Update(gameTime);
    }
}
