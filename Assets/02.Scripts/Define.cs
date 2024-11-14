public enum ETeam
{
    Red,
    Blue,
    None
}

public enum ERole
{
    Gunner,
    Runner
}

public enum EEntity
{
    Bullet,
    Point,
    Bomb
}

public class Define
{
    public const float START_POSITION_OFFSET = 5f;
    public const float START_DISTANCE_OFFSET = 10f;
    public const float FIRE_COOL_TIME = 0.3f;
    public const float GAME_GUNNER_POSITION_OFFSET = 70f;
    public const float GAME_RUNNER_POSITION_OFFSET = 10f;
    public const float READY_TIME = 1.5f;

    // public static float START_POSITION_OFFSET;
    // public static float START_DISTANCE_OFFSET;
    // public static float FIRE_COOL_TIME;
    // public static float GAME_GUNNER_POSITION_OFFSET;
    // public static float GAME_RUNNER_POSITION_OFFSET;
    // public static float READY_TIME;
}