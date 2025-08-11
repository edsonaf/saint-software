using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SaintGameLibrary;
using SaintGameLibrary.Graphics;
using System;

namespace DungeonSlime;

public class Game1 : Core
{
    private const string GameName = "Dungeon Slime";
    private const float MOVEMENT_SPEED = 5.0f;
    
    private AnimatedSprite _slime;
    private AnimatedSprite _bat;

    private Vector2 _slimePosition;
    private Vector2 _batPosition;
    private Vector2 _batVelocity;
    
    // Track the state of keyboard input during the previous frame.
    private KeyboardState _previousKeyboardState;
    
    public Game1() : base(GameName, 1280, 720, false)
    {
    }

    protected override void Initialize()
    {
        base.Initialize();
        _batPosition = new Vector2(_slime.Width + 10, 0);
        AssignRandomBatVelocity();
    }

    protected override void LoadContent()
    {
        var atlas = TextureAtlas.FromFile(Content, "images/atlas-definition.xml");
        _slime = atlas.CreateAnimatedSprite("slime-animation");
        _slime.Scale = new Vector2(4.0f, 4.0f);

        _bat = atlas.CreateAnimatedSprite("bat-animation");
        _bat.Scale = new Vector2(4.0f, 4.0f);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _slime.Update(gameTime);
        _bat.Update(gameTime);

        CheckKeyboardInput();

        CheckGamePadInput();

        // Create a bounding rectangle for the screen.
        var screenBounds = new Rectangle(
            0,
            0,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight
        );

        // Creating a bounding circle for the slime
        var slimeBounds = new Circle(
            (int)(_slimePosition.X + (_slime.Width * 0.5f)),
            (int)(_slimePosition.Y + (_slime.Height * 0.5f)),
            (int)(_slime.Width * 0.5f)
        );

        // Use distance based checks to determine if the slime is within the
        // bounds of the game screen, and if it is outside that screen edge,
        // move it back inside.
        if (slimeBounds.Left < screenBounds.Left)
        {
            _slimePosition.X = screenBounds.Left;
        }
        else if (slimeBounds.Right > screenBounds.Right)
        {
            _slimePosition.X = screenBounds.Right - _slime.Width;
        }

        if (slimeBounds.Top < screenBounds.Top)
        {
            _slimePosition.Y = screenBounds.Top;
        }
        else if (slimeBounds.Bottom > screenBounds.Bottom)
        {
            _slimePosition.Y = screenBounds.Bottom - _slime.Height;
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

        // Use distance based checks to determine if the bat is within the
        // bounds of the game screen, and if it is outside that screen edge,
        // reflect it about the screen edge normal.
        if (batBounds.Left < screenBounds.Left)
        {
            normal.X = Vector2.UnitX.X;
            newBatPosition.X = screenBounds.Left;
        }
        else if (batBounds.Right > screenBounds.Right)
        {
            normal.X = -Vector2.UnitX.X;
            newBatPosition.X = screenBounds.Right - _bat.Width;
        }

        if (batBounds.Top < screenBounds.Top)
        {
            normal.Y = Vector2.UnitY.Y;
            newBatPosition.Y = screenBounds.Top;
        }
        else if (batBounds.Bottom > screenBounds.Bottom)
        {
            normal.Y = -Vector2.UnitY.Y;
            newBatPosition.Y = screenBounds.Bottom - _bat.Height;
        }

        // If the normal is anything but Vector2.Zero, this means the bat had
        // moved outside the screen edge so we should reflect it about the
        // normal.
        if (normal != Vector2.Zero)
        {
            _batVelocity = Vector2.Reflect(_batVelocity, normal);
        }

        _batPosition = newBatPosition;

        if (slimeBounds.Intersects(batBounds))
        {
            // Divide the width  and height of the screen into equal columns and
            // rows based on the width and height of the bat.
            var totalColumns = GraphicsDevice.PresentationParameters.BackBufferWidth / (int)_bat.Width;
            var totalRows = GraphicsDevice.PresentationParameters.BackBufferHeight / (int)_bat.Height;

            // Choose a random row and column based on the total number of each
            var column = Random.Shared.Next(0, totalColumns);
            var row = Random.Shared.Next(0, totalRows);

            // Change the bat position by setting the x and y values equal to
            // the column and row multiplied by the width and height.
            _batPosition = new Vector2(column * _bat.Width, row * _bat.Height);

            // Assign a new random velocity to the bat
            AssignRandomBatVelocity();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _slime.Draw(SpriteBatch, _slimePosition);
        _bat.Draw(SpriteBatch, _batPosition);
        SpriteBatch.End();

        base.Draw(gameTime);
    }

    private void CheckKeyboardInput()
    {
        // If the space key is held down, the movement speed increases by 1.5       
        var speed = MOVEMENT_SPEED;
        if (Input.Keyboard.IsKeyDown(Keys.Space))
        {
            speed *= 1.5f;
        }

        // If the W or Up keys are down, move the slime up on the screen.
        if (Input.Keyboard.IsKeyDown(Keys.W) || Input.Keyboard.IsKeyDown(Keys.Up))
        {
            _slimePosition.Y -= speed;
        }

        // if the S or Down keys are down, move the slime down on the screen.
        if (Input.Keyboard.IsKeyDown(Keys.S) || Input.Keyboard.IsKeyDown(Keys.Down))
        {
            _slimePosition.Y += speed;
        }

        // If the A or Left keys are down, move the slime left on the screen.
        if (Input.Keyboard.IsKeyDown(Keys.A) || Input.Keyboard.IsKeyDown(Keys.Left))
        {
            _slimePosition.X -= speed;
        }

        // If the D or Right keys are down, move the slime right on the screen.
        if (Input.Keyboard.IsKeyDown(Keys.D) || Input.Keyboard.IsKeyDown(Keys.Right))
        {
            _slimePosition.X += speed;
        }
    }

    private void CheckGamePadInput()
    {
        var gamePadOne = Input.GamePads[(int)PlayerIndex.One];

        // If the A button is held down, the movement speed increases by 1.5
        // and the gamepad vibrates as feedback to the player.
        float speed = MOVEMENT_SPEED;
        if (gamePadOne.IsButtonDown(Buttons.A))
        {
            speed *= 1.5f;
            gamePadOne.SetVibration(1.0f, TimeSpan.FromSeconds(1));
        }
        else
        {
            gamePadOne.StopVibration();
        }

        // Check thumbstick first since it has priority over which gamepad input
        // is movement. It has priority since the thumbstick values provide a
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

    private void AssignRandomBatVelocity()
    {
        // Generate a random angle.
        var angle = (float)(Random.Shared.NextDouble() * Math.PI * 2);

        // Convert angle to a direction vector.
        var x = (float)Math.Cos(angle);
        var y = (float)Math.Sin(angle);
        var direction = new Vector2(x, y);

        // Multiply the direction vector by the movement speed.
        _batVelocity = direction * MOVEMENT_SPEED;
    }
}
