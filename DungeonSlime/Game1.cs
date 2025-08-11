using DungeonSlime.Scenes;
using Microsoft.Xna.Framework.Media;
using SaintGameLibrary;

namespace DungeonSlime;

public class Game1 : Core
{
    private const string GameName = "Dungeon Slime";
    private Song _themeSong;

    public Game1() : base(GameName, 1280, 720, false)
    {
    }

    protected override void Initialize()
    {
        base.Initialize();
        Audio.PlaySong(_themeSong);
        ChangeScene(new TitleScene());
    }

    protected override void LoadContent()
    {
        _themeSong = Content.Load<Song>("audio/theme");
    }
}
