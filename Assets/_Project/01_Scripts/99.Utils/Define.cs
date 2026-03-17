public class Define
{
    // 씬 정용
    public enum Scene
    {
        Unknown,
        Boot,
        Title,
        MainGame
    }

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount
    }

    public enum UIEvent
    {
        Click
    }

    public enum EnenyType
    {
        Normal,
        Epic,
        Boss
    }

    //문자열(String) 상수화
    // 태그 사용시 오타 방지를 위해
    public const string TAG_PLAYER = "Player";
    public const string TAG_ENEMY = "Enemy";
    public const string TAG_BOSS = "Boss";


}
