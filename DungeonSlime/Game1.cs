using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SaintGameLibrary;
using SaintGameLibrary.Graphics;

namespace DungeonSlime;

public class Game1 : Core
{
    private const string GameName = "Dungeon Slime";
    private AnimatedSprite _slime;
    private AnimatedSprite _bat;

    public Game1() : base(GameName, 1280, 720, false)
    {
    }

    protected override void Initialize()
    {
        base.Initialize();
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

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _slime.Draw(SpriteBatch, Vector2.One);
        _bat.Draw(SpriteBatch, new Vector2(_slime.Width + 10, 0));
        SpriteBatch.End();

        base.Draw(gameTime);
    }
}
