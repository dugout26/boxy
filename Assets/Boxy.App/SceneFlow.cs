using Boxy.App.Levels;

namespace Boxy.App
{
    // Build Settings에 등록된 씬 이름과 일치 — 정수가 Unity 활성 후 Build Settings에 4개 씬 추가 필수
    public static class BoxySceneNames
    {
        public const string MainMenu = "MainMenu";
        public const string LevelSelect = "LevelSelect";
        public const string Gameplay = "Gameplay";
        public const string Settings = "Settings";
    }

    // 씬 간 데이터 전달 — LevelSelect → Gameplay에 선택한 레벨 정보 넘김.
    // 정적 필드라 단순. v1.1 다중 세션 지원 시 SessionManager로 확장.
    public static class GameSession
    {
        public static LevelData PendingLevel;
        public static int LastClearedStars;
    }
}
