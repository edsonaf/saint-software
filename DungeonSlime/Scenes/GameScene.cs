using System;
using DungeonSlime.GameObjects;
using DungeonSlime.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using SaintGameLibrary;
using SaintGameLibrary.Graphics;
using SaintGameLibrary.Scenes;

namespace DungeonSlime.Scenes;

public class GameScene : Scene
{
    private enum GameState
    {
        Playing,
        Paused,
        GameOver
    }

    private Slime _slime;
    private Bat _bat;
    private Tilemap _tilemap;
    private Rectangle _roomBounds;
    private SoundEffect _collectSoundEffect;
    private int _score;
    private GameSceneUI _ui;
    private GameState _state;

    public override void Initialize()
    {
        base.Initialize();
        Core.ExitOnEscape = false;

        // Create the room bounds by getting the bounds of the screen then
        // using the Inflate method to "Deflate" the bounds by the width and
        // height of a tile so that the bounds only covers the inside room of
        // the dungeon tilemap.
        _roomBounds = Core.GraphicsDevice.PresentationParameters.Bounds;
        _roomBounds.Inflate(-_tilemap.TileWidth, -_tilemap.TileHeight);

        // Subscribe to the slime's BodyCollision event so that a game over
        // can be triggered when this event is raised.
        _slime.BodyCollision += OnSlimeBodyCollision;

        // Clear any UI elements from the root element created in previous scenes.
        GumService.Default.Root.Children.Clear();

        // Initialize the user interface for the game scene.
        InitializeUI();

        // Initialize a new game to be played.
        InitializeNewGame();
    }

    private void InitializeUI()
    {
        GumService.Default.Root.Children.Clear();

        _ui = new GameSceneUI();

        // Subscribe to the events from the game scene ui.
        _ui.ResumeButtonClick += OnResumeButtonClicked;
        _ui.RetryButtonClick += OnRetryButtonClicked;
        _ui.QuitButtonClick += OnQuitButtonClicked;
    }

    private void OnResumeButtonClicked(object sender, EventArgs args) =>
        _state = GameState.Playing;

    private void OnRetryButtonClicked(object sender, EventArgs args) =>
        InitializeNewGame();

    private void OnQuitButtonClicked(object sender, EventArgs args) =>
        Core.ChangeScene(new TitleScene());

    private void InitializeNewGame()
    {
        // Calculate the position for the slime, which will be at the center
        // tile of the tile map.
        var slimePos = new Vector2();
        slimePos.X = (_tilemap.Columns / 2) * _tilemap.TileWidth;
        slimePos.Y = (_tilemap.Rows / 2) * _tilemap.TileHeight;

        // Initialize the slime.
        _slime.Initialize(slimePos, _tilemap.TileWidth);

        // Initialize the bat.
        _bat.RandomizeVelocity();
        PositionBatAwayFromSlime();

        _score = 0;
        _state = GameState.Playing;
    }

    public override void LoadContent()
    {
        var atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");
        _tilemap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");
        _tilemap.Scale = new Vector2(4.0f, 4.0f);

        var slimeAnimation = atlas.CreateAnimatedSprite("slime-animation");
        slimeAnimation.Scale = new Vector2(4.0f, 4.0f);
        _slime = new Slime(slimeAnimation);

        var batAnimation = atlas.CreateAnimatedSprite("bat-animation");
        batAnimation.Scale = new Vector2(4.0f, 4.0f);
        var bounceSoundEffect = Content.Load<SoundEffect>("audio/bounce");
        _bat = new Bat(batAnimation, bounceSoundEffect);
        _collectSoundEffect = Content.Load<SoundEffect>("audio/collect");
    }

    public override void Update(GameTime gameTime)
    {
        _ui.Update(gameTime);

        if (_state == GameState.GameOver)
        {
            return;
        }

        if (GameController.Pause())
        {
            TogglePause();
        }

        if (_state == GameState.Paused)
        {
            return;
        }

        _slime.Update(gameTime);

        _bat.Update(gameTime);

        CollisionChecks();
    }

    private void CollisionChecks()
    {
        // Capture the current bounds of the slime and bat.
        var slimeBounds = _slime.GetBounds();
        var batBounds = _bat.GetBounds();

        // FIrst perform a collision check to see if the slime is colliding with
        // the bat, which means the slime eats the bat.
        if (slimeBounds.Intersects(batBounds))
        {
            // Move the bat to a new position away from the slime.
            PositionBatAwayFromSlime();

            // Randomize the velocity of the bat.
            _bat.RandomizeVelocity();

            // Tell the slime to grow.
            _slime.Grow();

            // Increment the score.
            _score += 100;

            // Update the score display on the UI.
            _ui.UpdateScoreText(_score);

            // Play the collect sound effect.
            Core.Audio.PlaySoundEffect(_collectSoundEffect);
        }

        // Next check if the slime is colliding with the wall by validating if
        // it is within the bounds of the room.  If it is outside the room
        // bounds, then it collided with a wall which triggers a game over.
        if (slimeBounds.Top < _roomBounds.Top ||
           slimeBounds.Bottom > _roomBounds.Bottom ||
           slimeBounds.Left < _roomBounds.Left ||
           slimeBounds.Right > _roomBounds.Right)
        {
            GameOver();
            return;
        }

        // Finally, check if the bat is colliding with a wall by validating if
        // it is within the bounds of the room.  If it is outside the room
        // bounds, then it collided with a wall, and the bat should bounce
        // off of that wall.
        if (batBounds.Top < _roomBounds.Top)
        {
            _bat.Bounce(Vector2.UnitY);
        }
        else if (batBounds.Bottom > _roomBounds.Bottom)
        {
            _bat.Bounce(-Vector2.UnitY);
        }

        if (batBounds.Left < _roomBounds.Left)
        {
            _bat.Bounce(Vector2.UnitX);
        }
        else if (batBounds.Right > _roomBounds.Right)
        {
            _bat.Bounce(-Vector2.UnitX);
        }
    }

    private void PositionBatAwayFromSlime()
    {
        // Calculate the position that is in the center of the bounds
        // of the room.
        var roomCenterX = _roomBounds.X + _roomBounds.Width * 0.5f;
        var roomCenterY = _roomBounds.Y + _roomBounds.Height * 0.5f;
        var roomCenter = new Vector2(roomCenterX, roomCenterY);

        var slimeBounds = _slime.GetBounds();
        var slimeCenter = new Vector2(slimeBounds.X, slimeBounds.Y);
        var centerToSlime = slimeCenter - roomCenter;

        var batBounds = _bat.GetBounds();
        var padding = batBounds.Radius * 2;
        var newBatPosition = Vector2.Zero;
        if (Math.Abs(centerToSlime.X) > Math.Abs(centerToSlime.Y))
        {
            // The slime is closer to either the left or right wall, so the Y
            // position will be a random position between the top and bottom
            // walls.
            newBatPosition.Y = Random.Shared.Next(
                _roomBounds.Top + padding,
                _roomBounds.Bottom - padding
            );

            if (centerToSlime.X > 0)
            {
                // The slime is closer to the right side wall, so place the
                // bat on the left side wall.
                newBatPosition.X = _roomBounds.Left + padding;
            }
            else
            {
                // The slime is closer ot the left side wall, so place the
                // bat on the right side wall.
                newBatPosition.X = _roomBounds.Right - padding * 2;
            }
        }
        else
        {
            // The slime is closer to either the top or bottom wall, so the X
            // position will be a random position between the left and right
            // walls.
            newBatPosition.X = Random.Shared.Next(
                _roomBounds.Left + padding,
                _roomBounds.Right - padding
            );

            if (centerToSlime.Y > 0)
            {
                // The slime is closer to the top wall, so place the bat on the
                // bottom wall.
                newBatPosition.Y = _roomBounds.Top + padding;
            }
            else
            {
                // The slime is closer to the bottom wall, so place the bat on
                // the top wall.
                newBatPosition.Y = _roomBounds.Bottom - padding * 2;
            }
        }

        // Assign the new bat position.
        _bat.Position = newBatPosition;
    }

    private void OnSlimeBodyCollision(object sender, EventArgs args)
    {
        GameOver();
    }

    private void TogglePause()
    {
        if (_state == GameState.Paused)
        {
            _ui.HidePausePanel();
            _state = GameState.Playing;
        }
        else
        {
            _ui.ShowPausePanel();
            _state = GameState.Paused;
        }
    }

    private void GameOver()
    {
        _ui.ShowGameOverPanel();
        _state = GameState.GameOver;
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(Color.CornflowerBlue);

        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _tilemap.Draw(Core.SpriteBatch);
        _slime.Draw();
        _bat.Draw();
        Core.SpriteBatch.End();

        _ui.Draw();
    }

}
