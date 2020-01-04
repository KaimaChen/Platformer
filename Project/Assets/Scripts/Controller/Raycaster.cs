using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Raycaster : MonoBehaviour
{
    const int c_right = 1;
    const int c_left = -1;
    const int c_bottom = -1;
    const int c_top = 1;

    /// <summary>
    /// 射线的间隔
    /// </summary>
    protected const float c_rayGaps = 0.25f;

    /// <summary>
    /// 皮肤厚度，会影响实际的射线长度
    /// </summary>
    protected const float c_skinWidth = 0.015f;

    /// <summary>
    /// 隔多久重置m_isFallThroughOneWayPlatform状态
    /// </summary>
    const float c_resetThroughTime = 0.5f;

    /// <summary>
    /// 会与哪些层的物体碰撞
    /// </summary>
    [SerializeField]
    protected LayerMask m_collisionMask;

    /// <summary>
    /// 水平射线个数
    /// </summary>
    protected int m_horizontalRayCount;

    /// <summary>
    /// 实际的水平射线间隔
    /// </summary>
    protected float m_horizontalRayGaps;

    /// <summary>
    /// 竖直射线个数
    /// </summary>
    protected int m_verticalRayCount;

    /// <summary>
    /// 实际的竖直射线间隔
    /// </summary>
    protected float m_verticalRayGaps;

    /// <summary>
    /// 是否正在向下穿越单向平台
    /// </summary>
    protected bool m_isFallThroughOneWayPlatform;

    protected BoxCollider2D m_collider;

    protected CollisionInfo m_collisionInfo;

    #region get-set
    public BoxCollider2D Collider { get { return m_collider; } }

    public CollisionInfo CollisionInfo { get { return m_collisionInfo; } }
    #endregion

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

    public void Move(Vector2 movement, bool isOnPlatform = false)
    {
        m_collisionInfo.Reset(movement);

        HorizontalCollisions(ref movement);
        VerticalCollisions(ref movement);

        transform.Translate(movement);

        if (isOnPlatform)
            m_collisionInfo.m_below = true;
    }

    void HorizontalCollisions(ref Vector2 movement)
    {
        float dirX = Mathf.Sign(movement.x);
        float rayLength = Mathf.Abs(movement.x) + c_skinWidth;

        for(int i = 0; i < m_horizontalRayCount; i++)
        {
            Vector2 origin = GetHorizontalBorder(dirX);
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
        if (movement.y == 0)
            return;

        float dirY = Mathf.Sign(movement.y);
        float rayLength = Mathf.Abs(movement.y) + c_skinWidth;

        for(int i = 0; i < m_verticalRayCount; i++)
        {
            Vector2 origin = GetVerticalBorder(dirY);
            origin += Vector2.right * (m_verticalRayGaps * i + movement.x);

            Vector2 rayDir = Vector2.up * dirY;

            Debug.DrawRay(origin, rayDir, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(origin, rayDir, rayLength, m_collisionMask);
            if (hit)
            {
                //特殊处理单向平台
                if (hit.transform.CompareTag(Defines.c_tagOneWayPlatform))
                {
                    if (dirY == c_top) //单向平台不会挡住向上跳
                        continue;

                    if (m_isFallThroughOneWayPlatform)
                        continue;
                }

                movement.y = (hit.distance - c_skinWidth) * dirY;
                rayLength = hit.distance;

                m_collisionInfo.m_below = (dirY == c_bottom);
                m_collisionInfo.m_above = (dirY == c_top);
            }
        }
    }

    public void FallThrough()
    {
        m_isFallThroughOneWayPlatform = true;

        Invoke("ResetData", c_resetThroughTime); //TODO: 感觉这样用时间来定时重置会有问题，比如玩家动作就是快，那么第二次跳平台就会发现跳不上去
    }

    void ResetData()
    {
        m_isFallThroughOneWayPlatform = false;
    }

    protected Vector2 GetHorizontalBorder(float dir)
    {
        Vector2 s = m_collider.bounds.size / 2;
        Vector2 pos = transform.position;

        if (dir == c_left)
            return new Vector2(pos.x - s.x, pos.y - s.y);
        else
            return new Vector2(pos.x + s.x, pos.y - s.y);
    }

    protected Vector2 GetVerticalBorder(float dir)
    {
        Vector2 s = m_collider.bounds.size / 2;
        Vector2 pos = transform.position;

        if (dir == c_bottom)
            return new Vector2(pos.x - s.x, pos.y - s.y);
        else
            return new Vector2(pos.x - s.x, pos.y + s.y);
    }
}

public struct CollisionInfo
{
    public bool m_above, m_below, m_left, m_right;
    public Vector2 m_originMovement;
    public int m_faceDir;

    public void Reset(Vector2 originMovement)
    {
        m_above = m_below = m_left = m_right = false;
        m_originMovement = originMovement;
    }
}