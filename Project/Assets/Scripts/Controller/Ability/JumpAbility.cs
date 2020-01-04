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

    public JumpAbility(PlayerController controller, int maxJumpCount = 1)
    {
        controller.m_belowCollisionCB += ResetJumpCount;

        m_maxJumpCount = maxJumpCount;
        m_remainJumpCount = maxJumpCount;
    }

    public override void Update(PlayerController controller, Vector2 input)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnJumpKeyDown(controller, input);
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            OnJumpKeyUp(controller);
        }
    }

    private void OnJumpKeyDown(PlayerController controller, Vector2 input)
    {
        if (controller.CollisionInfo.m_below)
        {
            if(input.y < 0)
                controller.FallThrough();
            else
                Jump(controller);
        }
        else
        {
            if(m_remainJumpCount > 0)
                Jump(controller);
        }
    }

    private void OnJumpKeyUp(PlayerController controller)
    {
        Vector2 v = controller.Velocity;

        if (v.y > m_minJumpVelocity)
        {
            v.y = m_minJumpVelocity;
            controller.Velocity = v;
        }
    }

    void Jump(PlayerController controller)
    {
        m_remainJumpCount--;

        Vector2 v = controller.Velocity;
        v.y = m_maxJumpVelocity;
        controller.Velocity = v;
    }

    void ResetJumpCount()
    {
        m_remainJumpCount = m_maxJumpCount;
    }
}