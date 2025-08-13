using Microsoft.Xna.Framework;
using SaintGameLibrary;
using SaintGameLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonSlime.GameObjects;

/// <summary>
/// Creates a new Slime using the specified animated sprite.
/// </summary>
/// <param name="sprite">The AnimatedSprite to use when drawing the slime.</param>
public class Slime(AnimatedSprite sprite)
{
    // A constant value that represents the amount of time to wait between
    // movement updates.
    private static readonly TimeSpan s_movementTime = TimeSpan.FromMilliseconds(200);

    // The amount of time that has elapsed since the last movement update.
    private TimeSpan _movementTimer;

    // Normalized value (0-1) representing progress between movement ticks for visual interpolation
    private float _movementProgress;

    // The next direction to apply to the head of the slime chain during the
    // next movement update.
    private Vector2 _nextDirection;

    // The number of pixels to move the head segment during the movement cycle.
    private float _stride;

    // Tracks the segments of the slime chain.
    private List<SlimeSegment> _segments;

    // The AnimatedSprite used when drawing each slime segment
    private AnimatedSprite _sprite = sprite;

    private Queue<Vector2> _inputBuffer;
    private const int MAX_BUFFER_SIZE = 2;

    /// <summary>
    /// Event that is raised if it is detected that the head segment of the slime
    /// has collided with a body segment.
    /// </summary>
    public event EventHandler BodyCollision;

    /// <summary>
    /// Initializes the slime, can be used to reset it back to an initial state.
    /// </summary>
    /// <param name="startingPosition">The position the slime should start at.</param>
    /// <param name="stride">The total number of pixels to move the head segment during each movement cycle.</param>
    public void Initialize(Vector2 startingPosition, float stride)
    {
        _segments = [];
        _stride = stride;

        var head = new SlimeSegment
        {
            At = startingPosition,
            To = startingPosition + new Vector2(_stride, 0),
            Direction = Vector2.UnitX
        };

        _segments.Add(head);
        _nextDirection = head.Direction;
        _movementTimer = TimeSpan.Zero;

        _inputBuffer = new Queue<Vector2>(MAX_BUFFER_SIZE);
    }

    private void HandleInput()
    {
        var potentialNextDirection = Vector2.Zero;
        if (GameController.MoveUp())
        {
            potentialNextDirection = -Vector2.UnitY;
        }
        else if (GameController.MoveDown())
        {
            potentialNextDirection = Vector2.UnitY;
        }
        else if (GameController.MoveLeft())
        {
            potentialNextDirection = -Vector2.UnitX;
        }
        else if (GameController.MoveRight())
        {
            potentialNextDirection = Vector2.UnitX;
        }

        // If a new direction was input, consider adding it to the buffer
        if (potentialNextDirection != Vector2.Zero && _inputBuffer.Count < MAX_BUFFER_SIZE)
        {
            // If the buffer is empty, validate against the current direction;
            // otherwise, validate against the last buffered direction
            var validateAgainst = _inputBuffer.Count > 0
                ? _inputBuffer.Last()
                : _segments[0].Direction;

            // Only allow direction change if it is not reversing the current
            // direction.  This prevents th slime from backing into itself
            var dot = Vector2.Dot(potentialNextDirection, validateAgainst);
            if (dot >= 0)
            {
                _inputBuffer.Enqueue(potentialNextDirection);
            }
        }
    }

    private void Move()
    {
        if (_inputBuffer.Count > 0)
        {
            _nextDirection = _inputBuffer.Dequeue();
        }

        var head = _segments[0];
        head.Direction = _nextDirection;
        head.At = head.To;
        head.To = head.At + head.Direction * _stride;

        // Insert the new adjusted value for the head at the front of the
        // segments and remove the tail segment. This effectively moves
        // the entire chain forward without needing to loop through every
        // segment and update its "at" and "to" positions.
        _segments.Insert(0, head);
        _segments.RemoveAt(_segments.Count - 1);

        // Iterate through all of the segments except the head and check
        // if they are at the same position as the head. If they are, then
        // the head is colliding with a body segment and a body collision
        // has occurred.
        for (var i = 1; i < _segments.Count; i++)
        {
            var segment = _segments[i];
            if (head.At == segment.At)
            {
                BodyCollision?.Invoke(this, EventArgs.Empty);
                return;
            }
        }
    }

    /// <summary>
    /// Informs the slime to grow by one segment.
    /// </summary>
    public void Grow()
    {
        var tail = _segments[_segments.Count - 1];

        // Create a new tail segment that is positioned a grid cell in the
        // reverse direction from the tail moving to the tail.
        var newTail = new SlimeSegment();
        newTail.At = tail.To + tail.ReverseDirection * _stride;
        newTail.To = tail.At;
        newTail.Direction = Vector2.Normalize(tail.At - newTail.At);

        _segments.Add(newTail);
    }

    /// <summary>
    /// Updates the slime.
    /// </summary>
    /// <param name="gameTime">A snapshot of the timing values for the current update cycle.</param>
    public void Update(GameTime gameTime)
    {
        _sprite.Update(gameTime);
        HandleInput();
        _movementTimer += gameTime.ElapsedGameTime;

        // If the movement timer has accumulated enough time to be greater than
        // the movement time threshold, then perform a full movement.
        if (_movementTimer >= s_movementTime)
        {
            _movementTimer -= s_movementTime;
            Move();
        }

        // Update the movement lerp offset amount
        _movementProgress = (float)(_movementTimer.TotalSeconds / s_movementTime.TotalSeconds);
    }

    /// <summary>
    /// Draws the slime.
    /// </summary>
    public void Draw()
    {
        // Iterate through each segment and draw it
        foreach (var segment in _segments)
        {
            // Calculate the visual position of the segment at the moment by
            // lerping between its "at" and "to" position by the movement
            // offset lerp amount
            var pos = Vector2.Lerp(segment.At, segment.To, _movementProgress);

            // Draw the slime sprite at the calculated visual position of this
            // segment
            _sprite.Draw(Core.SpriteBatch, pos);
        }
    }

    /// <summary>
    /// Returns a Circle value that represents collision bounds of the slime.
    /// </summary>
    /// <returns>A Circle value.</returns>
    public Circle GetBounds()
    {
        var head = _segments[0];

        // Calculate the visual position of the head at the moment of this
        // method call by lerping between the "at" and "to" position by the
        // movement offset lerp amount
        var pos = Vector2.Lerp(head.At, head.To, _movementProgress);

        // Create the bounds using the calculated visual position of the head.
        var bounds = new Circle(
            (int)(pos.X + (_sprite.Width * 0.5f)),
            (int)(pos.Y + (_sprite.Height * 0.5f)),
            (int)(_sprite.Width * 0.5f)
        );

        return bounds;
    }
}
