using DungeonSlime.UI;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Managers;
using Gum.Wireframe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using SaintGameLibrary;
using SaintGameLibrary.Graphics;
using SaintGameLibrary.Scenes;
using System;

namespace DungeonSlime.Scenes;

public class GameScene : Scene
{
    private const float MOVEMENT_SPEED = 5.0f;

    private AnimatedSprite _slime;
    private AnimatedSprite _bat;

    private Vector2 _slimePosition;
    private Vector2 _batPosition;
    private Vector2 _batVelocity;

    private Tilemap _tilemap;
    private Rectangle _roomBounds;

    private SoundEffect _bounceSoundEffect;
    private SoundEffect _collectSoundEffect;

    private SpriteFont _font;
    private int _score;
    private Vector2 _scoreTextPosition;
    private Vector2 _scoreTextOrigin;

    private Panel _pausePanel;
    private AnimatedButton _resumeButton;
    private SoundEffect _uiSoundEffect;
    private TextureAtlas _atlas;

    public override void Initialize()
    {
        base.Initialize();

        Core.ExitOnEscape = false;
        var screenBounds = Core.GraphicsDevice.PresentationParameters.Bounds;
        _roomBounds = new Rectangle(
            (int)_tilemap.TileWidth, (int)_tilemap.TileHeight,
            screenBounds.Width - (int)_tilemap.TileWidth * 2,
            screenBounds.Height - (int)_tilemap.TileHeight * 2);

        var centerRow = _tilemap.Rows / 2;
        var centerColumn = _tilemap.Columns / 2;

        _slimePosition = new Vector2(centerColumn * _tilemap.TileWidth, centerRow * _tilemap.TileHeight);
        _batPosition = new Vector2(_roomBounds.Left, _roomBounds.Top);

        _scoreTextPosition = new Vector2(_roomBounds.Left, _tilemap.TileHeight * .5f);
        var scoreTextYOrigin = _font.MeasureString("Score").Y * .5f;
        _scoreTextOrigin = new Vector2(0, scoreTextYOrigin);

        AssignRandomBatVelocity();
        InitializeUI();
    }

    public override void LoadContent()
    {
        _atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");
        _slime = _atlas.CreateAnimatedSprite("slime-animation");
        _slime.Scale = new Vector2(4.0f, 4.0f);

        _bat = _atlas.CreateAnimatedSprite("bat-animation");
        _bat.Scale = new Vector2(4.0f, 4.0f);

        _tilemap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");
        _tilemap.Scale = new Vector2(4.0f, 4.0f);

        _bounceSoundEffect = Content.Load<SoundEffect>("audio/bounce");
        _collectSoundEffect = Content.Load<SoundEffect>("audio/collect");
        
        _uiSoundEffect = Core.Content.Load<SoundEffect>("audio/ui");
        _font = Core.Content.Load<SpriteFont>("fonts/04B_30");
    }

    public override void Update(GameTime gameTime)
    {
        GumService.Default.Update(gameTime);
        if (_pausePanel.IsVisible)
        {
            return;
        }

        _slime.Update(gameTime);
        _bat.Update(gameTime);

        CheckKeyboardInput();

        CheckGamePadInput();

        // Creating a bounding circle for the slime
        var slimeBounds = new Circle(
            (int)(_slimePosition.X + (_slime.Width * 0.5f)),
            (int)(_slimePosition.Y + (_slime.Height * 0.5f)),
            (int)(_slime.Width * 0.5f)
        );

        if (slimeBounds.Left < _roomBounds.Left)
        {
            _slimePosition.X = _roomBounds.Left;
        }
        else if (slimeBounds.Right > _roomBounds.Right)
        {
            _slimePosition.X = _roomBounds.Right - _slime.Width;
        }

        if (slimeBounds.Top < _roomBounds.Top)
        {
            _slimePosition.Y = _roomBounds.Top;
        }
        else if (slimeBounds.Bottom > _roomBounds.Bottom)
        {
            _slimePosition.Y = _roomBounds.Bottom - _slime.Height;
        }

        // Calculate the new position of the bat based on the velocity.
        var newBatPosition = _batPosition + _batVelocity;

        // Create a bounding circle for the bat.
        var batBounds = new Circle(
            (int)(newBatPosition.X + (_bat.Width * 0.5f)),
            (int)(newBatPosition.Y + (_bat.Height * 0.5f)),
            (int)(_bat.Width * 0.5f)
        );

        var normal = Vector2.Zero;

        if (batBounds.Left < _roomBounds.Left)
        {
            normal.X = Vector2.UnitX.X;
            newBatPosition.X = _roomBounds.Left;
        }
        else if (batBounds.Right > _roomBounds.Right)
        {
            normal.X = -Vector2.UnitX.X;
            newBatPosition.X = _roomBounds.Right - _bat.Width;
        }

        if (batBounds.Top < _roomBounds.Top)
        {
            normal.Y = Vector2.UnitY.Y;
            newBatPosition.Y = _roomBounds.Top;
        }
        else if (batBounds.Bottom > _roomBounds.Bottom)
        {
            normal.Y = -Vector2.UnitY.Y;
            newBatPosition.Y = _roomBounds.Bottom - _bat.Height;
        }

        if (normal != Vector2.Zero)
        {
            _batVelocity = Vector2.Reflect(_batVelocity, normal);
            Core.Audio.PlaySoundEffect(_bounceSoundEffect);
        }

        _batPosition = newBatPosition;

        if (slimeBounds.Intersects(batBounds))
        {
            var column = Random.Shared.Next(0, _tilemap.Columns - 1);
            var row = Random.Shared.Next(0, _tilemap.Rows - 1);
            _batPosition = new Vector2(column * _bat.Width, row * _bat.Height);
            Core.Audio.PlaySoundEffect(_collectSoundEffect);

            AssignRandomBatVelocity();

            _score += 100;
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(Color.CornflowerBlue);

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        _tilemap.Draw(Core.SpriteBatch);
        _slime.Draw(Core.SpriteBatch, _slimePosition);
        _bat.Draw(Core.SpriteBatch, _batPosition);

        Core.SpriteBatch.DrawString(
            _font,              // spriteFont
            $"Score: {_score}", // text
            _scoreTextPosition, // position
            Color.White,        // color
            0.0f,               // rotation
            _scoreTextOrigin,   // origin
            1.0f,               // scale
            SpriteEffects.None, // effects
            0.0f                // layerDepth
        );

        Core.SpriteBatch.End();

        GumService.Default.Draw();
    }

    private void AssignRandomBatVelocity()
    {
        var angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);
        var x = (float)Math.Cos(angle);
        var y = (float)Math.Sin(angle);
        var direction = new Vector2(x, y);
        _batVelocity = direction * MOVEMENT_SPEED;
    }

    private void CheckKeyboardInput()
    {
        var keyboard = Core.Input.Keyboard;

        // If the escape key is pressed, return to the title screen.
        if (Core.Input.Keyboard.WasKeyJustPressed(Keys.Escape))
        {
            PauseGame();
        }

        // If the space key is held down, the movement speed increases by 1.5
        float speed = MOVEMENT_SPEED;
        if (keyboard.IsKeyDown(Keys.Space))
        {
            speed *= 1.5f;
        }

        // If the W or Up keys are down, move the slime up on the screen.
        if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
        {
            _slimePosition.Y -= speed;
        }

        // if the S or Down keys are down, move the slime down on the screen.
        if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
        {
            _slimePosition.Y += speed;
        }

        // If the A or Left keys are down, move the slime left on the screen.
        if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
        {
            _slimePosition.X -= speed;
        }

        // If the D or Right keys are down, move the slime right on the screen.
        if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
        {
            _slimePosition.X += speed;
        }

        // If the M key is pressed, toggle mute state for audio.
        if (keyboard.WasKeyJustPressed(Keys.M))
        {
            Core.Audio.ToggleMute();
        }

        // If the + button is pressed, increase the volume.
        if (keyboard.WasKeyJustPressed(Keys.OemPlus))
        {
            Core.Audio.SongVolume += 0.1f;
            Core.Audio.SoundEffectVolume += 0.1f;
        }

        // If the - button was pressed, decrease the volume.
        if (keyboard.WasKeyJustPressed(Keys.OemMinus))
        {
            Core.Audio.SongVolume -= 0.1f;
            Core.Audio.SoundEffectVolume -= 0.1f;
        }
    }

    private void CheckGamePadInput()
    {
        // Get the gamepad info for gamepad one.
        var gamePadOne = Core.Input.GamePads[(int)PlayerIndex.One];

        if (gamePadOne.WasButtonJustPressed(Buttons.Start))
        {
            PauseGame();
        }

        // If the A button is held down, the movement speed increases by 1.5
        // and the gamepad vibrates as feedback to the player.
        var speed = MOVEMENT_SPEED;
        if (gamePadOne.IsButtonDown(Buttons.A))
        {
            speed *= 1.5f;
            GamePad.SetVibration(PlayerIndex.One, 1.0f, 1.0f);
        }
        else
        {
            GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
        }

        // Check thumbstick first since it has priority over which gamepad input
        // is movement.  It has priority since the thumbstick values provide a
        // more granular analog value that can be used for movement.
        if (gamePadOne.LeftThumbStick != Vector2.Zero)
        {
            _slimePosition.X += gamePadOne.LeftThumbStick.X * speed;
            _slimePosition.Y -= gamePadOne.LeftThumbStick.Y * speed;
        }
        else
        {
            // If DPadUp is down, move the slime up on the screen.
            if (gamePadOne.IsButtonDown(Buttons.DPadUp))
            {
                _slimePosition.Y -= speed;
            }

            // If DPadDown is down, move the slime down on the screen.
            if (gamePadOne.IsButtonDown(Buttons.DPadDown))
            {
                _slimePosition.Y += speed;
            }

            // If DPapLeft is down, move the slime left on the screen.
            if (gamePadOne.IsButtonDown(Buttons.DPadLeft))
            {
                _slimePosition.X -= speed;
            }

            // If DPadRight is down, move the slime right on the screen.
            if (gamePadOne.IsButtonDown(Buttons.DPadRight))
            {
                _slimePosition.X += speed;
            }
        }
    }

    private void InitializeUI()
    {
        GumService.Default.Root.Children.Clear();
        CreatePausePanel();
    }

    private void PauseGame()
    {
        _pausePanel.IsVisible = true;
        _resumeButton.IsFocused = true;
    }

    private void CreatePausePanel()
    {
        _pausePanel = new Panel();
        _pausePanel.Anchor(Anchor.Center);
        _pausePanel.Visual.WidthUnits = DimensionUnitType.Absolute;
        _pausePanel.Visual.HeightUnits = DimensionUnitType.Absolute;
        _pausePanel.Visual.Height = 70;
        _pausePanel.Visual.Width = 264;
        _pausePanel.IsVisible = false;
        _pausePanel.AddToRoot();

        var backgroundRegion = _atlas.GetRegion("panel-background");
        var background = new NineSliceRuntime();
        background.Dock(Dock.Fill);
        background.Texture = backgroundRegion.Texture;
        background.TextureAddress = TextureAddress.Custom;
        background.TextureHeight = backgroundRegion.Height;
        background.TextureLeft = backgroundRegion.SourceRectangle.Left;
        background.TextureTop = backgroundRegion.SourceRectangle.Top;
        background.TextureWidth = backgroundRegion.Width;
        _pausePanel.AddChild(background);

        var textInstance = new TextRuntime();
        textInstance.Anchor(Anchor.Top);
        textInstance.Text = "PAUSED";
        textInstance.CustomFontFile = @"fonts/04b_30.fnt";
        textInstance.UseCustomFont = true;
        textInstance.FontScale = 0.5f; 
        textInstance.X = 10f;
        textInstance.Y = 10f;
        _pausePanel.AddChild(textInstance);

        _resumeButton = new AnimatedButton(_atlas);
        _resumeButton.Anchor(Anchor.BottomLeft);
        _resumeButton.Text = "RESUME";
        _resumeButton.Visual.X = 9f;
        _resumeButton.Visual.Y = -9f;
        _resumeButton.Visual.Width = 80;
        _resumeButton.Click += HandleResumeButtonClicked;
        _pausePanel.AddChild(_resumeButton);

        var quitButton = new AnimatedButton(_atlas); 
        quitButton.Anchor(Anchor.BottomRight);
        quitButton.Text = "QUIT";
        quitButton.Visual.X = -9f;
        quitButton.Visual.Y = -9f;
        quitButton.Width = 80;
        quitButton.Click += HandleQuitButtonClicked;

        _pausePanel.AddChild(quitButton);
    }

    private void HandleResumeButtonClicked(object sender, EventArgs e)
    {
        Core.Audio.PlaySoundEffect(_uiSoundEffect);
        _pausePanel.IsVisible = false;
    }

    private void HandleQuitButtonClicked(object sender, EventArgs e)
    {
        Core.Audio.PlaySoundEffect(_uiSoundEffect);
        Core.ChangeScene(new TitleScene());
    }

}
