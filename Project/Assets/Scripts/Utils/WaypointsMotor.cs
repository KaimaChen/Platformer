using UnityEngine;

/// <summary>
/// 沿着Waypoints移动
/// </summary>
[RequireComponent(typeof(Waypoints))]
public class WaypointsMotor : MonoBehaviour
{
    /// <summary>
    /// true：从头开始
    /// false：反过来继续
    /// </summary>
    [SerializeField]
    private bool m_Loop;

    /// <summary>
    /// 移动速度
    /// </summary>
    [SerializeField]
    private int m_Speed;

    /// <summary>
    /// 当前要移动到的目标点索引
    /// </summary>
    private int m_Index;

    /// <summary>
    /// 是否通过递减索引来找下一个目标
    /// </summary>
    private bool m_Descend;

    private Waypoints m_Waypoints;

    void Start()
    {
        m_Waypoints = GetComponent<Waypoints>();
    }

    void Update()
    {
        if (m_Waypoints.Count <= 0)
            return;

        //TODO: 这种实现会导致端点处不是顺滑的
        Vector2 curtPos = transform.position;
        Vector2 targetPos = m_Waypoints.GetPoint(m_Index);
        Vector2 toTarget = targetPos - curtPos;

        float targetDist = toTarget.magnitude;
        float moveDist = m_Speed * Time.deltaTime;

        if(moveDist >= targetDist)
        {
            transform.position = targetPos;
            UpdateIndex();
        }
        else
        {
            Vector2 move = toTarget.normalized * moveDist;
            transform.position += new Vector3(move.x, move.y);
        }
    }

    void UpdateIndex()
    {
        //TODO：又丑又长，改一下
        if(m_Descend)
        {
            m_Index--;
            if(m_Index < 0)
            {
                if (m_Loop)
                {
                    m_Index = m_Waypoints.Count - 1;
                }
                else
                {
                    m_Index = 1;
                    m_Descend = false;
                }
            }
        }
        else
        {
            m_Index++;
            if(m_Index >= m_Waypoints.Count)
            {
                if(m_Loop)
                {
                    m_Index = 0;
                }
                else
                {
                    m_Index -= 2;
                    m_Descend = true;
                }
            }
        }

        m_Index = Mathf.Clamp(m_Index, 0, m_Waypoints.Count - 1);
    }
}
