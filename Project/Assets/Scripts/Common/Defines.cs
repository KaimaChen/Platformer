/// <summary>
/// 存放各种全局的定义
/// </summary>
public static class Defines
{
    /// <summary>
    /// 重力加速度
    /// </summary>
    public const float c_gravity = -50f;

    public const int c_left = -1;
    public const int c_right = 1;
    public const int c_bottom = -1;
    public const int c_top = 1;

    #region Tags
    public const string c_tagOneWayPlatform = "OneWayPlatform";
    public const string c_tagPlayer = "Player";
    #endregion
}

public enum FaceDir
{
    Left,
    Right,
}

public enum PlayerState
{
    Normal,
    SlideWall,
    Dash,
}

public enum KeyState
{
    None,
    Down,
    Pressing,
    Up,
}