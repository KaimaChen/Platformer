using UnityEngine;

/// <summary>
/// 冲刺能力
/// </summary>
public class DashAbility : BaseAbility
{
    /// <summary>
    /// 冲刺速度
    /// </summary>
    float m_speed;

    /// <summary>
    /// 冲刺时长
    /// </summary>
    float m_duration;
    float m_dashEndTime;

    float m_cd;
    float m_cdEndTime;

    /// <summary>
    /// 本次冲刺的方向
    /// </summary>
    int m_dir;

    #region get-set
    public float Speed { set { m_speed = value; } }

    public float Duration { set { m_duration = value; } }

    public float Cd { set { m_cd = value; } }
    #endregion

    public DashAbility(PlayerController owner, float speed, float duration, float cd)
        : base(owner)
    {
        m_speed = speed;
        m_duration = duration;
        m_cd = cd;
    }

    protected override bool CanUpdate()
    {
        return true;
    }

    protected override void UpdateImpl(Vector2 input)
    {
        if(CanStartDash())
            StartDash(input);

        if (IsDashing())
            Dash();
        else
            EndDash();
    }

    bool CanStartDash()
    {
        if (IsCDing()) return false;

        if (IsDashing()) return false;

        if (!InputBuffer.Instance.DashDown) return false;

        return true;
    }

    void StartDash(Vector2 input)
    {
        m_dashEndTime = Time.time + m_duration;
        m_cdEndTime = Time.time + m_cd;
        m_owner.State = PlayerState.Dash;

        CalcDashDir(input);
    }

    void CalcDashDir(Vector2 input)
    {
        if (input.x > 0)
            m_dir = 1;
        else if (input.x < 0)
            m_dir = -1;
        else
            m_dir = m_owner.FaceDir == FaceDir.Right ? 1 : -1;
    }

    void Dash()
    {
        m_owner.Velocity = new Vector2(m_speed * m_dir, 0);
    }

    void EndDash()
    {
        if (m_owner.State != PlayerState.Dash)
            return;

        m_owner.State = PlayerState.Normal;
        m_owner.Velocity = Vector2.zero;

        m_dashEndTime = 0;
    }

    bool IsDashing()
    {
        if (m_owner.State != PlayerState.Dash) return false;

        return m_dashEndTime > Time.time;
    }

    bool IsCDing()
    {
        return m_cdEndTime > Time.time;
    }
}