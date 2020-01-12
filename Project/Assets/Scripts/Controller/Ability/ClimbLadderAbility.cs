using UnityEngine;

/// <summary>
/// 爬楼梯
/// 
/// 规则（参考Braid）：
/// 1. 在楼梯附近按up，会吸附到楼梯那里然后上去（地面和空中都是）
/// 2. 一旦按了left或right或jump，就会解除爬楼梯状态
/// 3. 如果要通过爬楼梯来上平台，则这个平台需要是单向平台
/// </summary>
public class ClimbLadderAbility : BaseAbility
{
    /// <summary>
    /// 爬楼梯的速度
    /// </summary>
    const float c_climbSpeed = 5f;

    /// <summary>
    /// 吸附到楼梯的速度
    /// </summary>
    const float c_snapSpeed = 1f;

    /// <summary>
    /// 大的吸附距离（在地面且按的是up）
    /// </summary>
    const float c_bigSnapDistance = 1f;
    const float c_smallSnapDistance = c_bigSnapDistance / 2f;

    /// <summary>
    /// x方向小于等于该距离认为可以开始爬楼梯了
    /// </summary>
    const float c_validDistance = 0.02f;

    Collider2D m_ladder;

    protected override PlayerState State => PlayerState.ClimbLadder;

    public ClimbLadderAbility(PlayerController owner) : base(owner) { }

    protected override bool CanUpdate(Vector2 input)
    {
        if (m_owner.State != PlayerState.Normal && m_owner.State != PlayerState.ClimbLadder)
            return false;

        //在地上如果按的是down，则需要地面是单向平台才能下楼梯
        if (input.y < 0 && m_owner.IsOnGround && !m_owner.IsOnOneWayPlatform)
            return false;

        //按x方向会结束爬楼梯
        if (input.x != 0)
            return false;

        if (m_owner.State == PlayerState.ClimbLadder)
        {
            if (m_ladder == null)
                return false;

            if (m_ladder.bounds.Contains(m_owner.transform.position) == false)
                return false;
        }

        return true;
    }

    protected override void UpdateImpl(Vector2 input)
    {
        if (m_owner.State == PlayerState.ClimbLadder)
            Climb(input);
        else
            SnapToLadder(input);
    }

    protected override void SwitchStateTo(PlayerState nextState)
    {
        if(m_owner.State == State)
        {
            m_owner.State = nextState;
            m_ladder = null;
        }
    }

    void Climb(Vector2 input)
    {
        if(InputBuffer.Instance.UseJumpDown())
        {
            m_owner.Velocity = Vector2.zero;
            SwitchStateTo(PlayerState.Normal);
            return;
        }

        Vector2 v = Vector2.zero;
        if (input.y > 0)
        {
            //角色中心不能超过楼梯顶部
            float distToTop = (m_ladder.bounds.max.y - m_owner.transform.position.y);
            float maxSpeed = distToTop / Time.deltaTime;
            v.y = Mathf.Min(c_climbSpeed, maxSpeed);
        }
        else if(input.y < 0)
        {
            v.y = -c_climbSpeed;
            m_owner.FallThrough(); //下楼梯时无视单向平台
        }

        m_owner.Velocity = v;
    }

    void SnapToLadder(Vector2 input)
    {
        if (input.y == 0)
            return;

        Bounds bounds = m_owner.Collider.bounds;
        m_ladder = Physics2D.OverlapBox(bounds.center, bounds.size, 0, LayerMask.GetMask(Defines.c_layerLadder));
        if (m_ladder != null)
        {
            Vector2 pos = m_owner.transform.position;
            Vector2 ladderPos = m_ladder.transform.position;
            float deltaX = ladderPos.x - pos.x;
            float absX = Mathf.Abs(deltaX);

            if(absX <= GetSnapDistance(input))
            {
                if(absX > c_validDistance)
                {
                    float movement = m_owner.Velocity.x * Time.deltaTime;
                    if(Mathf.Sign(movement) == Mathf.Sign(deltaX) && //如果移动经过了梯子，则直接可以爬
                        Mathf.Abs(movement) > absX)
                    {
                        m_owner.State = PlayerState.ClimbLadder;
                    }
                    else
                    {
                        Vector2 v = m_owner.Velocity;
                        v.x += c_snapSpeed * Mathf.Sign(deltaX);
                        m_owner.Velocity = v;
                    }
                }
                else
                {
                    m_owner.State = PlayerState.ClimbLadder;
                }

                if(m_owner.State == PlayerState.ClimbLadder)
                {
                    pos.x = m_ladder.transform.position.x;
                    m_owner.transform.position = pos;
                    m_owner.VelocityX = 0;
                }
            }
        }
    }

    /// <summary>
    /// 获取吸附距离（地面并且按向上键时吸附距离大，其他情况都时短距离）
    /// </summary>
    /// <param name="input">玩家当前帧输入</param>
    /// <returns>吸附距离</returns>
    float GetSnapDistance(Vector2 input)
    {
        if(m_owner.IsOnGround)
        {
            return input.y > 0 ? c_bigSnapDistance : c_smallSnapDistance;
        }
        else
        {
            return c_smallSnapDistance;
        }
    }
}