using System.Collections.Generic;
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
    public static float START_POSITION_OFFSET = 5f;
    public static float START_DISTANCE_OFFSET = 10f;
    public static float FIRE_COOL_TIME = 0.3f;
    public static float GAME_GUNNER_POSITION_OFFSET = 70f;
    public static float GAME_RUNNER_POSITION_OFFSET = 10f;
    public static float READY_TIME = 1.5f;

    // PlayerBasic 구조체 정의
    public struct PlayerBasicData
    {
        public float StartPosX;
        public float StartPosY;
        public float StartPosZ;
        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;
        public float MoveSpeed;
        public float ReadyPressTime;
        public bool ReadyStats;
        public bool TeamStats;
        public bool RoleStats;
    }

    // Gunner 데이터 구조체
    public struct GunnerData
    {
        public float StartPosX;
        public float StartPosY;
        public float StartPosZ;
        public float PosX;
        public float PosY;
        public float PosZ;
        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;
        public float MoveSpeed;
        public float DashSpeed;
        public float FiringCoolTime;
        public int GetPoint;
        public bool IsClicked;
        public bool TeamStats;
    }

    // Runner 데이터 구조체
    public struct RunnerData
    {
        public float StartPosX;
        public float StartPosY;
        public float StartPosZ;
        public float PosX;
        public float PosY;
        public float PosZ;
        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;
        public float MoveSpeed;
        public float DashForce;
        public float DashCoolTime;
        public int GetPoint;
        public bool IsClicked;
        public bool TeamStats;
    }


    // WinCheck 데이터 구조체
    public struct WinCheckData
    {
        public float StartPosX;
        public float StartPosY;
        public float StartPosZ;
        public bool TeamStats;
        public float EndPoint;
        public float BoxPosY;
        public float PlayerPosY;
    }

    // Item 데이터 구조체
    public struct ItemData
    {
        public float PosX;
        public float PosY;
        public float PosZ;
        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;
        public float CreateCoolTime;
        public int GetPoint;
        public int MaxVal;
    }

    // Bullet 데이터 구조체
    public struct BulletData
    {
        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;
        public float BulletSpeed;
        public int GetPoint;
    }

    // 시트 데이터를 저장할 딕셔너리
    public static Dictionary<string, PlayerBasicData> Players = new Dictionary<string, PlayerBasicData>();
    public static Dictionary<string, GunnerData> Gunners = new Dictionary<string, GunnerData>();
    public static Dictionary<string, RunnerData> Runners = new Dictionary<string, RunnerData>();
    public static Dictionary<string, WinCheckData> WinChecks = new Dictionary<string, WinCheckData>();
    public static Dictionary<string, ItemData> Items = new Dictionary<string, ItemData>();
    public static Dictionary<string, BulletData> Bullets = new Dictionary<string, BulletData>();





    // public static float START_POSITION_OFFSET;
    // public static float START_DISTANCE_OFFSET;
    // public static float FIRE_COOL_TIME;
    // public static float GAME_GUNNER_POSITION_OFFSET;
    // public static float GAME_RUNNER_POSITION_OFFSET;
    // public static float READY_TIME;
}