using UnityEngine;

/// <summary>
/// 爬墙能力
/// </summary>
public class ClimbWallAbility : BaseAbility
{
    /// <summary>
    /// 不按方向键时的贴墙跳跃速度
    /// </summary>
    Vector2 m_wallJumpVelocity = new Vector2(8, 8);

    /// <summary>
    /// 按下朝墙壁的方向键时的贴墙跳跃速度
    /// </summary>
    Vector2 m_towardWallJumpVelocity = new Vector2(8, 16);

    /// <summary>
    /// 按下远离墙壁的方向键时的贴墙跳跃速度
    /// </summary>
    Vector2 m_offWallJumpVelocity = new Vector2(16, 16);

    /// <summary>
    /// 贴墙下滑时的最小速度
    /// </summary>
    float m_slideMinSpeed = -3;

    bool m_isSliding;
    int m_wallRelativeDir;
    
    public override void Update(PlayerController controller, Vector2 input)
    {
        Vector2 v = controller.Velocity;

        m_wallRelativeDir = (controller.CollisionInfo.m_left ? Defines.c_left : Defines.c_right);
        m_isSliding = IsSliding(controller.CollisionInfo, v);

        if(m_isSliding)
            v.y = Mathf.Max(v.y, m_slideMinSpeed);

        if(m_isSliding && Input.GetKeyDown(KeyCode.Space))
        {
            if (m_wallRelativeDir == input.x)
                v = m_towardWallJumpVelocity;
            else if (input.x == 0)
                v = m_wallJumpVelocity;
            else
                v = m_offWallJumpVelocity;

            v.x *= -m_wallRelativeDir;
        }

        controller.Velocity = v;
    }

    bool IsSliding(CollisionInfo info, Vector2 velocity)
    {
        bool touchWall = info.m_left || info.m_right;
        return (touchWall && !info.m_below && velocity.y < 0);
    }
}