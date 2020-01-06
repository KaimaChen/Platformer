using UnityEngine;

/// <summary>
/// 爬墙能力
/// </summary>
public class ClimbWallAbility : BaseAbility
{
    /// <summary>
    /// 在不按朝着墙壁的方向键时，能在墙壁上坚持多久
    /// 如果不加个时间，那么在按反向跳跃时很容易脱离墙壁，导致被当成普通跳
    /// </summary>
    const float c_stickTime = 0.15f;

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
    float m_stickTimer;

    public ClimbWallAbility(PlayerController owner) : base(owner) { }

    protected override bool CanUpdate()
    {
        return m_owner.State != PlayerState.Dash;
    }

    protected override void UpdateImpl(Vector2 input)
    {
        Vector2 v = m_owner.Velocity;

        m_wallRelativeDir = (m_owner.CollisionInfo.m_left ? Defines.c_left : Defines.c_right);
        m_isSliding = IsSliding(m_owner.CollisionInfo, v);

        if(m_isSliding)
        {
            v.y = Mathf.Max(v.y, m_slideMinSpeed);
            m_owner.State = PlayerState.ClimbWall;

            float dirX = Mathf.Sign(input.x);
            if (dirX == m_wallRelativeDir)
            {
                m_stickTimer = 0;
            }
            else
            {
                m_stickTimer += Time.deltaTime;
                if (m_stickTimer < c_stickTime)
                    v.x = m_wallRelativeDir;
            }
        }
        else
        {
            if (m_owner.State == PlayerState.ClimbWall)
                m_owner.State = PlayerState.Normal;
        }

        if(m_isSliding && InputBuffer.Instance.JumpDown)
        {
            if (m_wallRelativeDir == input.x)
                v = m_towardWallJumpVelocity;
            else if (input.x == 0)
                v = m_wallJumpVelocity;
            else
                v = m_offWallJumpVelocity;

            v.x *= -m_wallRelativeDir;

            InputBuffer.Instance.ResetJump();
        }

        m_owner.Velocity = v;
    }

    bool IsSliding(CollisionInfo info, Vector2 velocity)
    {
        bool touchWall = info.m_left || info.m_right;
        return (touchWall && !info.m_below && velocity.y < 0);
    }
}