using UnityEngine;

//TODO: 站在单向平台上时按下键也要能够爬楼梯下去

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

    Collider2D m_ladder;

    protected override PlayerState State => PlayerState.ClimbLadder;

    public ClimbLadderAbility(PlayerController owner) : base(owner) { }

    protected override bool CanUpdate(Vector2 input)
    {
        if (m_owner.State != PlayerState.Normal && m_owner.State != PlayerState.ClimbLadder)
            return false;

        if (input.y < 0 && m_owner.IsOnGround)
            return false;

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
            GoToLadder(input);
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
            float distToTop = (m_ladder.bounds.max.y - m_owner.transform.position.y);
            float maxSpeed = distToTop / Time.deltaTime;
            v.y = Mathf.Min(c_climbSpeed, maxSpeed);
        }
        else if(input.y < 0)
        {
            v.y = -c_climbSpeed;
            m_owner.FallThrough();
        }

        m_owner.Velocity = v;
    }

    //TODO: 吸附过去
    //TODO: 向上键的吸附范围比向下键要大
    void GoToLadder(Vector2 input)
    {
        if (input.y == 0)
            return;

        Bounds bounds = m_owner.Collider.bounds;
        m_ladder = Physics2D.OverlapBox(bounds.center, bounds.size, 0, LayerMask.GetMask(Defines.c_layerLadder));
        if (m_ladder != null)
        {
            m_owner.State = PlayerState.ClimbLadder;

            Vector2 pos = m_owner.transform.position;
            pos.x = m_ladder.transform.position.x;
            m_owner.transform.position = pos;
        }
    }
}