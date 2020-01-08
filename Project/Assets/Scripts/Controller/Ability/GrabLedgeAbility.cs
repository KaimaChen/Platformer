using UnityEngine;

/// <summary>
/// 抓住边缘的能力
/// * 抓住边缘后就不会往下滑了
/// </summary>
public class GrabLedgeAbility : BaseAbility
{
    /// <summary>
    /// 检测边缘的射线距离
    /// </summary>
    const float c_checkOffset = 0.2f;

    /// <summary>
    /// 抓住边缘的水平方向速度
    /// * 要有一个指向墙壁的力，否则就会判定没有和墙壁接触，导致掉下去
    /// * 到达平台地面高度后，这个速度会让你往平台上走
    /// </summary>
    const float c_grabHorizontalSpeed = 6f;

    /// <summary>
    /// 抬升的速度
    /// </summary>
    const float c_grabUpSpeed = 3f;

    float m_ledgeHeight;

    protected override PlayerState State => PlayerState.GrabLedge;

    public GrabLedgeAbility(PlayerController owner) : base(owner) { }

    protected override bool CanUpdate(Vector2 input)
    {
        //冲刺过程中不能抓边缘
        if (m_owner.State == PlayerState.Dash)
            return false;

        //需要靠着墙壁且不在地上
        var info = m_owner.CollisionInfo;
        bool touchWall = (info.m_left || info.m_right);
        if (info.m_below || !touchWall)
            return false;

        //如果玩家输入方向远离墙壁，则要停止抓住墙壁行为
        int dir = m_owner.CollisionInfo.m_left ? -1 : 1;
        if (input.x != 0 && dir != Mathf.Sign(input.x))
            return false;

        //如果已经处于抓边状态，则看看有没有落到边缘下边
        if(State == PlayerState.GrabLedge)
        {
            float distanceToLedge = m_owner.GetVerticalBorder(Defines.c_top).y - m_ledgeHeight;
            if (distanceToLedge < 0) //到边缘下边了
                return false;
        }
        return true; 
    }

    protected override void UpdateImpl(Vector2 input)
    {
        if (m_owner.State == PlayerState.GrabLedge)
        {
            HandleGrabMovement(input);
        }
        else
        {
            if(IsGrabLedge(out RaycastHit2D hit))
            {
                m_ledgeHeight = m_owner.GetVerticalBorder(Defines.c_top).y - hit.distance;
                
                int dir = m_owner.CollisionInfo.m_left ? -1 : 1;
                Vector2 v = new Vector2(c_grabHorizontalSpeed * dir, 0);
                m_owner.Velocity = v;
                m_owner.State = PlayerState.GrabLedge;
            }
            else
            {
                if (m_owner.State == State)
                    m_owner.State = PlayerState.Normal;
            }
        }
    }

    /// <summary>
    /// 处理抓住边缘时的移动逻辑
    /// </summary>
    void HandleGrabMovement(Vector2 input)
    {
        //跳跃时就切回贴墙跳
        if(InputBuffer.Instance.JumpDown)
        {
            m_owner.State = PlayerState.SlideWall;
            return;
        }

        float distanceToLedge = m_owner.GetVerticalBorder(Defines.c_top).y - m_ledgeHeight;
        int dir = m_owner.CollisionInfo.m_left ? -1 : 1;
        Vector2 v = new Vector2(c_grabHorizontalSpeed * dir, 0);
        if (input.y > 0)
        {
            v.y = c_grabUpSpeed;
        }
        else
        {
            float minVy = -distanceToLedge / Time.deltaTime;
            v.y = Mathf.Max(Defines.c_slideSpeed, minVy);
        }

        m_owner.Velocity = v;
    }

    /// <summary>
    /// 是否抓住了边缘
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>
    bool IsGrabLedge(out RaycastHit2D hit)
    {
        int dir = m_owner.CollisionInfo.m_left ? -1 : 1;
        Vector2 border = m_owner.GetVerticalBorder(Defines.c_top, dir);

        Vector2 rayOrigin = border;
        Vector2 rayDir = Vector2.right * dir;
        float rayLength = c_checkOffset;
        hit = Physics2D.Raycast(rayOrigin, rayDir, rayLength, m_owner.CollisionMask);
        if (!hit) //最上边的那个射线没有碰到墙壁
        {
            rayOrigin = border + rayDir * rayLength;
            rayDir = Vector2.down;
            rayLength = c_checkOffset;
            hit = Physics2D.Raycast(rayOrigin, rayDir, rayLength, m_owner.CollisionMask);
            if (hit && hit.distance != 0) //往下一段合法距离碰到墙壁，则视为抓住了边缘
            {
                return true;
            }
        }

        return false;
    }
}