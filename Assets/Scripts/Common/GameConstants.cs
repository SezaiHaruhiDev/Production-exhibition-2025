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
            public const string SEDir = "se/";
        }

        public static class NovelCommands
        {
            public static class Types
            {
                public const string Background = "bg";
                public const string NewCharacter = "ncr";
                public const string ChangeCharacterSize = "crs";
                public const string ChangeCharacter = "cr";
                public const string Choice = "ch";
                public const string BGM = "bgm";
                public const string SE = "se";
                public const string Affiliation = "af";
                public const string UI = "ui";
                public const string Outline = "out";
                public const string FocusCamera = "fc";
                public const string Wait = "wait";
                public const string Speed = "speed";
                public const string Font = "font";
                public const string Color = "color";
                public const string ToggleBackground = "tb";
                public const string Click = "cl";
                public const string Shake = "shake";
                public const string Skip = "skip";
                public const string Title = "title";
            }

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

        public static class ScenarioLabels
        {
            public const string Start = "start";
            public const string End = "end";
        }

        public static class UI
        {
            public const float DefaultFadeDuration = 0.5f;
            public const float TitleStayDuration = 3f;
        }

        public static class Colors
        {
            public const string ChatBlue = "#00AFFF";
            public const string ChatRed = "#FF5555";
        }
    }
}
