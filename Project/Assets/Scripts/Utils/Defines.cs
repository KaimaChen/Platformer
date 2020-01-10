/// <summary>
/// 存放各种全局的定义
/// </summary>
public static class Defines
{
    /// <summary>
    /// 重力加速度
    /// </summary>
    public const float c_gravity = -50f;

    /// <summary>
    /// 贴墙下滑时的速度
    /// </summary>
    public const float c_slideSpeed = -3;

    public const int c_left = -1;
    public const int c_right = 1;
    public const int c_bottom = -1;
    public const int c_top = 1;

    #region Tags
    public const string c_tagOneWayPlatform = "OneWayPlatform";
    public const string c_tagPlayer = "Player";
    #endregion

    #region Layers
    public const string c_layerLadder = "Ladder";
    #endregion
}

public enum FaceDir
{
    Left,
    Right,
}

public enum PlayerState
{
    Normal, //走路、跳跃等
    SlideWall, //滑墙
    Dash, //冲刺
    GrabLedge, //抓住墙角
    ClimbLadder, //爬楼梯
}

public enum KeyState
{
    None,
    Down,
    Pressing,
    Up,
}