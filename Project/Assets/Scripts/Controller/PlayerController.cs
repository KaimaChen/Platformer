using UnityEngine;

public class PlayerController : Raycaster
{
    /// <summary>
    /// 在地上的加速度
    /// </summary>
    const float c_groundAcceration = 0.1f;

    /// <summary>
    /// 在空中的加速度
    /// </summary>
    const float c_airAcceration = 0.1f;

    [SerializeField]
    float m_speed = 6f;

    float m_velocityXSmooth;
    Vector2 m_velocity;
    Vector2 m_inputData;

    PlayerState m_state = PlayerState.Normal;

    JumpAbility m_jumpAbility;
    ClimbWallAbility m_climbWallAbility;
    GrabLedgeAbility m_grabLedgeAbility;
    DashAbility m_dashAbility;

    #region get-set
    public Vector2 Velocity
    {
        get { return m_velocity; }
        set { m_velocity = value; }
    }

    public Vector2 InputData
    {
        get { return m_inputData; }
    }

    public PlayerState State
    {
        get { return m_state; }
        set { m_state = value; }
    }
    #endregion

    protected override void Start()
    {
        base.Start();

        gameObject.tag = Defines.c_tagPlayer;

        m_jumpAbility = new JumpAbility(this, 2);
        m_climbWallAbility = new ClimbWallAbility(this);
        m_grabLedgeAbility = new GrabLedgeAbility(this);
        m_dashAbility = new DashAbility(this, 15, 0.4f, 1f);
    }

    void Update()
    {
        m_inputData = InputBuffer.Instance.Move;

        CalcVelocityByInput(m_inputData);

        m_jumpAbility.Update(m_inputData);
        m_climbWallAbility.Update(m_inputData);
        m_grabLedgeAbility.Update(m_inputData);
        m_dashAbility.Update(m_inputData);

        Move(m_velocity * Time.deltaTime);

        //上或下碰到障碍时，重置竖直方向速度
        if(m_collisionInfo.m_above || m_collisionInfo.m_below)
            m_velocity.y = 0;
    }

    void CalcVelocityByInput(Vector2 input)
    {
        float targetX = input.x * m_speed;
        float acceration = (m_collisionInfo.m_below ? c_groundAcceration : c_airAcceration);
        m_velocity.x = Mathf.SmoothDamp(m_velocity.x, targetX, ref m_velocityXSmooth, acceration);

        m_velocity.y += Defines.c_gravity * Time.deltaTime;
    }
}
