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
    private bool m_loop = false;

    /// <summary>
    /// 移动速度
    /// </summary>
    [SerializeField]
    private int m_speed = 1;

    /// <summary>
    /// 当前要移动到的目标点索引
    /// </summary>
    private int m_index;

    /// <summary>
    /// 是否通过递减索引来找下一个目标
    /// </summary>
    private bool m_descend;

    /// <summary>
    /// 是否手动触发更新
    /// </summary>
    private bool m_triggerUpdate;

    private Waypoints m_waypoints;

    #region get-set
    public bool TriggerUpdate { set { m_triggerUpdate = value; } }
    #endregion

    void Start()
    {
        m_waypoints = GetComponent<Waypoints>();
    }

    void Update()
    {
        if(!m_triggerUpdate)
            transform.position = Move();
    }

    public Vector2 Move()
    {
        if (m_waypoints.Count <= 0)
            return Vector2.zero;

        Vector2 result;
        Vector2 curtPos = transform.position;
        Vector2 targetPos = m_waypoints.GetPoint(m_index);
        Vector2 toTarget = targetPos - curtPos;

        float targetDist = toTarget.magnitude;
        float moveDist = m_speed * Time.deltaTime;

        if (moveDist >= targetDist)
        {
            result = targetPos;
            UpdateIndex();
        }
        else
        {
            Vector2 move = toTarget.normalized * moveDist;
            result = transform.position + new Vector3(move.x, move.y);
        }

        return result;
    }

    void UpdateIndex()
    {
        if(m_descend)
        {
            m_index--;
            if(m_index < 0)
            {
                if (m_loop)
                {
                    m_index = m_waypoints.Count - 1;
                }
                else
                {
                    m_index = 1;
                    m_descend = false;
                }
            }
        }
        else
        {
            m_index++;
            if(m_index >= m_waypoints.Count)
            {
                if(m_loop)
                {
                    m_index = 0;
                }
                else
                {
                    m_index -= 2;
                    m_descend = true;
                }
            }
        }

        m_index = Mathf.Clamp(m_index, 0, m_waypoints.Count - 1);
    }
}
