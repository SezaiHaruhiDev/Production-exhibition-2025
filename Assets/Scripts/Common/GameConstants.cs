using UnityEngine;

namespace Common
{
    /// <summary>
    /// ゲーム全体で使用する定数定義
    /// </summary>
    public static class GameConstants
    {
        public static class Scenes
        {
            public const string Formation = "FormationScene";
            public const string Battle = "BattleScene";
        }

        public static class Prefs
        {
            public const string PlayerParty = "PlayerParty";
        }

        public static class Resources
        {
            public const string ScenarioDir = "texts/scenario";
            public const string SpritesDir = "sprites/";
        }

        public static class NovelCommands
        {
            public const string Zoom = "zoom";
            public const string Reset = "reset";
            public const string Offset = "offset";
            public const string Value = "value";
            public const string Name = "name";
            public const string Size = "size";
            public const string Pos = "pos";
            public const string Col = "col";
            public const string Sprite = "sprite";
            public const string Show = "show";
        }

        public static class Colors
        {
            public const string ChatBlue = "#00AFFF";
            public const string ChatRed = "#FF5555";
        }
    }
}
