using UnityEngine;

/// <summary>
/// 跳跃能力
/// * 包括多重跳跃
/// 
/// 跳跃次数重置时机：
/// * 底部接触障碍
/// </summary>
public class JumpAbility : BaseAbility
{
    /// <summary>
    /// 起跳时的跳跃速度
    /// </summary>
    float m_maxJumpVelocity = 20;

    /// <summary>
    /// 松开跳跃键时的最大跳跃速度
    /// </summary>
    float m_minJumpVelocity = 10;

    /// <summary>
    /// 最大的多重跳跃次数
    /// </summary>
    int m_maxJumpCount;

    /// <summary>
    /// 剩余的多重跳跃次数
    /// </summary>
    int m_remainJumpCount;

    #region get-set
    public int MaxJumpCount
    {
        set { m_maxJumpCount = value; }
    }
    #endregion

    public JumpAbility(PlayerController owner, int maxJumpCount = 1)
        : base(owner)
    {
        owner.m_belowCollisionCB += ResetJumpCount;

        m_maxJumpCount = maxJumpCount;
        m_remainJumpCount = maxJumpCount;
    }

    public override void Update(Vector2 input)
    {
        if (!m_owner.IsFree)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnJumpKeyDown(input);
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            OnJumpKeyUp();
        }
    }

    private void OnJumpKeyDown(Vector2 input)
    {
        if (m_owner.CollisionInfo.m_below)
        {
            if(input.y < 0)
                m_owner.FallThrough();
            else
                Jump();
        }
        else
        {
            if(m_remainJumpCount > 0)
                Jump();
        }
    }

    private void OnJumpKeyUp()
    {
        Vector2 v = m_owner.Velocity;

        if (v.y > m_minJumpVelocity)
        {
            v.y = m_minJumpVelocity;
            m_owner.Velocity = v;
        }
    }

    void Jump()
    {
        m_remainJumpCount--;

        Vector2 v = m_owner.Velocity;
        v.y = m_maxJumpVelocity;
        m_owner.Velocity = v;
    }

    void ResetJumpCount()
    {
        m_remainJumpCount = m_maxJumpCount;
    }
}