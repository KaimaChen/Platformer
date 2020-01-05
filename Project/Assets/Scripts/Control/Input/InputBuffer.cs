using UnityEngine;

/// <summary>
/// 输入缓冲
/// </summary>
public class InputBuffer : MonoBehaviour
{
    /// <summary>
    /// 按下跳跃键后持续一段时间都视为按下
    /// </summary>
    const float c_jumpDownTime = 0.1f;

    static InputBuffer m_instance;

    [SerializeField]
    string m_horizontal = "Horizontal";
    float m_horizontalData;

    [SerializeField]
    string m_vertical = "Vertical";
    float m_verticalData;

    [SerializeField]
    KeyCode m_jump = KeyCode.Space;
    KeyState m_jumpData;

    [SerializeField]
    KeyCode m_dash = KeyCode.LeftControl;
    KeyState m_dashData;

    float m_jumpDownEndTime;

    #region get-set
    public static InputBuffer Instance
    {
        get
        {
            if (m_instance == null)
            {
                GameObject go = new GameObject("InputBuffer");
                m_instance = go.AddComponent<InputBuffer>();
            }

            return m_instance;
        }
    }

    public Vector2 Move
    {
        get { return new Vector2(m_horizontalData, m_verticalData); }
    }

    public KeyState Jump
    {
        get { return m_jumpData; }
    }

    public bool JumpDown
    {
        get { return m_jumpData == KeyState.Down; }
    }

    public bool JumpUp
    {
        get { return m_jumpData == KeyState.Up; }
    }

    public KeyState Dash
    {
        get { return m_dashData; }
    }

    public bool DashDown
    {
        get { return m_dashData == KeyState.Down; }
    }
    #endregion

    void Awake()
    {
        m_instance = this;
    }

    void Update()
    {
        m_horizontalData = Input.GetAxisRaw(m_horizontal);
        m_verticalData = Input.GetAxisRaw(m_vertical);
        m_dashData = GetKeyState(m_dash);

        UpdateJumpInput();
    }

    KeyState GetKeyState(KeyCode key)
    {
        if (Input.GetKeyDown(key))
            return KeyState.Down;
        else if (Input.GetKey(key))
            return KeyState.Pressing;
        else if (Input.GetKeyUp(key))
            return KeyState.Up;
        else
            return KeyState.None;
    }

    void UpdateJumpInput()
    {
        if(Input.GetKeyDown(m_jump))
            m_jumpDownEndTime = Time.time + c_jumpDownTime;

        if(m_jumpDownEndTime > Time.time)
        {
            m_jumpData = KeyState.Down;
        }
        else
        {
            if (Input.GetKey(m_jump))
                m_jumpData = KeyState.Pressing;
            else if (Input.GetKeyUp(m_jump))
                m_jumpData = KeyState.Up;
            else
                m_jumpData = KeyState.None;
        }
    }

    /// <summary>
    /// 在使用了跳跃等之后要重置跳跃数据，否则会一直判定为按下导致连续跳跃
    /// </summary>
    public void ResetJump()
    {
        m_jumpDownEndTime = 0;
        m_jumpData = GetKeyState(m_jump);
    }
}
