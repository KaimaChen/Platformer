using UnityEngine;

/// <summary>
/// 摄像机跟随
/// </summary>
public class CameraFollow : MonoBehaviour
{
    /// <summary>
    /// 跟随的目标
    /// </summary>
    [SerializeField]
    PlayerController m_target;

    /// <summary>
    /// 焦点与跟随目标的偏移值
    /// </summary>
    [SerializeField]
    Vector2 m_offset;

    /// <summary>
    /// 聚焦的范围
    /// </summary>
    [SerializeField]
    Vector2 m_focusAreaSize = new Vector2(3, 5);

    /// <summary>
    /// 聚焦到目标前方多远距离
    /// </summary>
    [SerializeField]
    float m_lookAheadDistance = 4;

    /// <summary>
    /// 水平方向到聚焦点的平滑时长
    /// </summary>
    [SerializeField]
    float m_horizontalSmoothTime = 0.5f;

    /// <summary>
    /// 竖直方向到聚焦点的平滑时长
    /// </summary>
    [SerializeField]
    float m_verticalSmoothTime = 0.1f;

    FocusArea m_area;

    float m_smoothX;
    float m_smoothY;

    float m_curtLookAheadX;
    float m_targetLookAheadX;
    bool m_lookAheadStop;

    void Start()
    {
        m_area = new FocusArea(m_target.Collider.bounds, m_focusAreaSize);
    }

    void LateUpdate()
    {
        m_area.Update(m_target.Collider.bounds);

        Vector2 focusPos = m_area.m_center + m_offset;
        focusPos.y = Mathf.SmoothDamp(transform.position.y, focusPos.y, ref m_smoothY, m_verticalSmoothTime);

        if(m_area.m_movement.x != 0)
        {
            float dirX = Mathf.Sign(m_area.m_movement.x);
            float inputDirX = Mathf.Sign(m_target.InputData.x);
            if(inputDirX == dirX && m_target.InputData.x != 0)
            {
                m_lookAheadStop = false;
                m_targetLookAheadX = dirX * m_lookAheadDistance;
            }
            else
            {
                if(!m_lookAheadStop)
                {
                    m_lookAheadStop = true;
                    m_targetLookAheadX = m_curtLookAheadX + (dirX * m_lookAheadDistance - m_curtLookAheadX) / 4;
                }
            }
        }
        m_curtLookAheadX = Mathf.SmoothDamp(m_curtLookAheadX, m_targetLookAheadX, ref m_smoothX, m_horizontalSmoothTime);
        focusPos.x += m_curtLookAheadX;

        transform.position = new Vector3(focusPos.x, focusPos.y, -10);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Gizmos.DrawCube(m_area.m_center, m_focusAreaSize);
    }

    #region 内部
    struct FocusArea
    {
        public Vector2 m_center;
        public Vector2 m_movement;

        float m_left, m_right, m_top, m_bottom;

        public FocusArea(Bounds target, Vector2 size)
        {
            m_left = target.center.x - size.x / 2;
            m_right = target.center.x + size.x / 2;
            m_bottom = target.center.y - size.y / 2;
            m_top = target.center.y + size.y / 2;

            float midX = (m_left + m_right) / 2;
            float midY = (m_bottom + m_top) / 2;
            m_center = new Vector2(midX, midY);

            m_movement = Vector2.zero;
        }

        public void Update(Bounds target)
        {
            float shiftX = 0;
            if (target.min.x < m_left)
                shiftX = target.min.x - m_left;
            else if (target.max.x > m_right)
                shiftX = target.max.x - m_right;

            float shiftY = 0;
            if (target.min.y < m_bottom)
                shiftY = target.min.y - m_bottom;
            else if (target.max.y > m_top)
                shiftY = target.max.y - m_top;

            m_left += shiftX;
            m_right += shiftX;
            m_bottom += shiftY;
            m_top += shiftY;

            m_center = new Vector2((m_left + m_right) / 2, (m_bottom + m_top) / 2);
            m_movement = new Vector2(shiftX, shiftY);
        }
    }
    #endregion
}