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

    public override void Update(Vector2 input)
    {
        if(CanStartDash())
            StartDash();

        if (IsDashing())
        {
            m_owner.IsFree = false;
            Dash();
        }
        else
        {
            EndDash();
        }
    }

    bool CanStartDash()
    {
        if (IsCDing()) return false;

        if (IsDashing()) return false;

        if (!Input.GetKeyDown(KeyCode.LeftControl)) return false;

        return true;
    }

    void StartDash()
    {
        m_dashEndTime = Time.time + m_duration;
        m_cdEndTime = Time.time + m_cd;
        m_dir = m_owner.FaceDir == FaceDir.Right ? 1 : -1;
    }

    void Dash()
    {
        m_owner.Velocity = new Vector2(m_speed * m_dir, 0);
    }

    void EndDash()
    {
        if (m_owner.IsFree)
            return;

        m_owner.IsFree = true;
        m_owner.Velocity = Vector2.zero;

        m_dashEndTime = 0;
    }

    bool IsDashing()
    {
        return m_dashEndTime > Time.time;
    }

    bool IsCDing()
    {
        return m_cdEndTime > Time.time;
    }
}