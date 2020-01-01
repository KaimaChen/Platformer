using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    const int c_right = 1;
    const int c_left = -1;
    const int c_bottom = -1;
    const int c_top = 1;

    /// <summary>
    /// 射线的间隔
    /// </summary>
    const float c_rayGaps = 0.25f;

    /// <summary>
    /// 皮肤厚度，会影响实际的射线长度
    /// </summary>
    const float c_skinWidth = 0.015f;

    /// <summary>
    /// 会与哪些层的物体碰撞
    /// </summary>
    [SerializeField]
    LayerMask m_collisionMask;

    /// <summary>
    /// 水平射线个数
    /// </summary>
    int m_horizontalRayCount;

    /// <summary>
    /// 实际的水平射线间隔
    /// </summary>
    float m_horizontalRayGaps;

    /// <summary>
    /// 竖直射线个数
    /// </summary>
    int m_verticalRayCount;

    /// <summary>
    /// 实际的竖直射线间隔
    /// </summary>
    float m_verticalRayGaps;

    BoxCollider2D m_collider;
    RectCorners m_corners;
    CollisionInfo m_collisionInfo;

    protected virtual void Awake()
    {
        m_collider = GetComponent<BoxCollider2D>();
    }

    protected virtual void Start()
    {
        InitRayData();

        m_collisionInfo.m_faceDir = c_right;
    }

    void InitRayData()
    {
        Bounds bounds = m_collider.bounds;
        float width = bounds.size.x;
        float height = bounds.size.y;

        m_horizontalRayCount = Mathf.RoundToInt(height / c_rayGaps);
        m_horizontalRayGaps = m_horizontalRayCount > 1 ? height / (m_horizontalRayCount - 1) : height;

        m_verticalRayCount = Mathf.RoundToInt(width / c_rayGaps);
        m_verticalRayGaps = m_verticalRayCount > 1 ? width / (m_verticalRayCount - 1) : width;
    }

    void HorizontalCollisions(ref Vector2 movement)
    {
        int dirX = m_collisionInfo.m_faceDir;
        float rayLength = Mathf.Abs(movement.x) + c_skinWidth;

        for(int i = 0; i < m_horizontalRayCount; i++)
        {
            Vector2 origin = (dirX == c_left ? m_corners.m_BL : m_corners.m_BR);
            origin += Vector2.up * (m_horizontalRayGaps * i);

            Vector2 rayDir = Vector2.right * dirX;

            Debug.DrawRay(origin, rayDir, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(origin, rayDir, rayLength, m_collisionMask);
            if(hit)
            {
                if (hit.distance == 0)
                    continue;

                movement.x = (hit.distance - c_skinWidth) * dirX;
                rayLength = hit.distance;

                m_collisionInfo.m_left = (dirX == c_left);
                m_collisionInfo.m_right = (dirX == c_right);
            }
        }
    }

    void VerticalCollisions(ref Vector2 movement)
    {
        float dirY = Mathf.Sign(movement.y);
        float rayLength = Mathf.Abs(movement.y) + c_skinWidth;

        for(int i = 0; i < m_verticalRayCount; i++)
        {
            Vector2 origin = (dirY == c_bottom ? m_corners.m_BL : m_corners.m_TL);
            origin += Vector2.right * (m_verticalRayGaps * i + movement.x);

            Vector2 rayDir = Vector2.up * dirY;

            Debug.DrawRay(origin, rayDir, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(origin, rayDir, rayLength, m_collisionMask);
            if(hit)
            {
                if (hit.distance == 0)
                    continue;

                movement.y = (hit.distance - c_skinWidth) * dirY;
                rayLength = hit.distance;

                m_collisionInfo.m_blow = (dirY == c_bottom);
                m_collisionInfo.m_above = (dirY == c_top);
            }
        }
    }

    #region 内部
    /// <summary>
    /// 矩形的四个端点
    /// </summary>
    struct RectCorners
    {
        public Vector2 m_BL, m_BR, m_TL, m_TR;

        public void Update(Bounds b)
        {
            m_BL = new Vector2(b.min.x, b.min.y);
            m_BR = new Vector2(b.max.x, b.min.y);
            m_TL = new Vector2(b.min.x, b.max.y);
            m_TR = new Vector2(b.max.x, b.max.y);
        }
    }

    struct CollisionInfo
    {
        public bool m_above, m_blow, m_left, m_right;
        public Vector2 m_lastMovement;
        public int m_faceDir;

        public void Reset()
        {
            m_above = m_blow = m_left = m_right = false;
        }
    }
    #endregion
}