using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using SaintGameLibrary;
using SaintGameLibrary.Graphics;
using System;

namespace DungeonSlime.GameObjects;

/// <summary>
/// Creates a new Bat using the specified animated sprite and sound effect.
/// </summary>
/// <param name="sprite">The AnimatedSprite ot use when drawing the bat.</param>
/// <param name="bounceSoundEffect">The sound effect to play when the bat bounces off a wall.</param>
public class Bat(AnimatedSprite sprite, SoundEffect bounceSoundEffect)
{
    private const float MOVEMENT_SPEED = 5.0f;
    private Vector2 _velocity;
    private AnimatedSprite _sprite = sprite;
    private SoundEffect _bounceSoundEffect = bounceSoundEffect;

    /// <summary>
    /// Gets or Sets the position of the bat.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Randomizes the velocity of the bat.
    /// </summary>
    public void RandomizeVelocity()
    {
        var angle = (float)(Random.Shared.NextDouble() * MathHelper.TwoPi);
        var x = (float)Math.Cos(angle);
        var y = (float)Math.Sin(angle);
        var direction = new Vector2(x, y);
        _velocity = direction * MOVEMENT_SPEED;
    }

    /// <summary>
    /// Handles a bounce event when the bat collides with a wall or boundary.
    /// </summary>
    /// <param name="normal">The normal vector of the surface the bat is bouncing against.</param>
    public void Bounce(Vector2 normal)
    {
        var newPosition = Position;
        // Adjust the position based on the normal to prevent sticking to walls.
        if (normal.X != 0)
        {
            // We are bouncing off a vertical wall (left/right).
            // Move slightly away from the wall in the direction of the normal.
            newPosition.X += normal.X * (_sprite.Width * 0.1f);
        }

        if (normal.Y != 0)
        {
            // We are bouncing off a horizontal wall (top/bottom).
            // Move slightly way from the wall in the direction of the normal.
            newPosition.Y += normal.Y * (_sprite.Height * 0.1f);
        }

        // Apply the new position
        Position = newPosition;

        // Apply reflection based on the normal.
        _velocity = Vector2.Reflect(_velocity, normal);

        // Play the bounce sound effect.
        Core.Audio.PlaySoundEffect(_bounceSoundEffect);
    }

    /// <summary>
    /// Returns a Circle value that represents collision bounds of the bat.
    /// </summary>
    /// <returns>A Circle value.</returns>
    public Circle GetBounds()
    {
        var x = (int)(Position.X + _sprite.Width * 0.5f);
        var y = (int)(Position.Y + _sprite.Height * 0.5f);
        var radius = (int)(_sprite.Width * 0.25f);
        return new Circle(x, y, radius);
    }

    /// <summary>
    /// Updates the bat.
    /// </summary>
    /// <param name="gameTime">A snapshot of the timing values for the current update cycle.</param>
    public void Update(GameTime gameTime)
    {
        _sprite.Update(gameTime);
        Position += _velocity;
    }

    /// <summary>
    /// Draws the bat.
    /// </summary>
    public void Draw()
    {
        _sprite.Draw(Core.SpriteBatch, Position);
    }
}
