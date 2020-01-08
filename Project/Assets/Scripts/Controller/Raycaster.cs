using UnityEngine;
using System;

[RequireComponent(typeof(BoxCollider2D))]
public class Raycaster : MonoBehaviour
{
    /// <summary>
    /// 射线的最少个数
    /// </summary>
    const int c_minRayCount = 2;

    /// <summary>
    /// 皮肤厚度，会影响实际的射线长度
    /// </summary>
    public const float c_skinWidth = 0.015f;

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
    protected int m_horizontalRayCount = 4;

    /// <summary>
    /// 水平射线间隔
    /// </summary>
    protected float m_horizontalRayGaps;

    /// <summary>
    /// 竖直射线个数
    /// </summary>
    protected int m_verticalRayCount = 4;

    /// <summary>
    /// 竖直射线间隔
    /// </summary>
    protected float m_verticalRayGaps;

    /// <summary>
    /// 是否正在向下穿越单向平台
    /// </summary>
    protected bool m_isFallThroughOneWayPlatform;

    protected BoxCollider2D m_collider;

    protected CollisionInfo m_collisionInfo;

    protected FaceDir m_faceDir;

    #region 回调
    public Action m_belowCollisionCB;
    #endregion

    #region get-set
    public BoxCollider2D Collider { get { return m_collider; } }

    public CollisionInfo CollisionInfo { get { return m_collisionInfo; } }

    public LayerMask CollisionMask { get { return m_collisionMask; } }

    public FaceDir FaceDir { get { return m_faceDir; } }

    public bool IsOnGround { get { return m_collisionInfo.m_below; } }
    #endregion

    protected virtual void Awake()
    {
        m_collider = GetComponent<BoxCollider2D>();
        m_faceDir = FaceDir.Right;
    }

    protected virtual void Start()
    {
        InitRayData();
    }

    void InitRayData()
    {
        Bounds bounds = m_collider.bounds;
        float width = bounds.size.x;
        float height = bounds.size.y;

        if (m_horizontalRayCount < c_minRayCount)
            m_horizontalRayCount = c_minRayCount;

        m_horizontalRayGaps = m_horizontalRayCount > 1 ? height / (m_horizontalRayCount - 1) : height;

        if (m_verticalRayCount < c_minRayCount)
            m_verticalRayCount = c_minRayCount;

        m_verticalRayGaps = m_verticalRayCount > 1 ? width / (m_verticalRayCount - 1) : width;
    }

    public void Move(Vector2 movement, bool isOnPlatform = false)
    {
        m_collisionInfo.Reset();

        HorizontalCollisions(ref movement);
        VerticalCollisions(ref movement);

        transform.Translate(movement);

        if (movement.x > 0)
            m_faceDir = FaceDir.Right;
        else if (movement.x < 0)
            m_faceDir = FaceDir.Left;

        if (isOnPlatform)
        {
            m_collisionInfo.m_below = true;
            m_belowCollisionCB?.Invoke();
        }
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

                m_collisionInfo.m_left = (dirX == Defines.c_left);
                m_collisionInfo.m_right = (dirX == Defines.c_right);
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
                    if (dirY == Defines.c_top) //单向平台不会挡住向上跳
                        continue;

                    if (m_isFallThroughOneWayPlatform)
                        continue;
                }

                movement.y = (hit.distance - c_skinWidth) * dirY;
                rayLength = hit.distance;

                m_collisionInfo.m_below = (dirY == Defines.c_bottom);
                m_collisionInfo.m_above = (dirY == Defines.c_top);

                if (m_collisionInfo.m_below)
                    m_belowCollisionCB?.Invoke();
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

    public Vector2 GetHorizontalBorder(float dir)
    {
        Vector2 s = m_collider.bounds.size / 2;
        Vector2 pos = transform.position;

        if (dir == Defines.c_left)
            return new Vector2(pos.x - s.x, pos.y - s.y);
        else
            return new Vector2(pos.x + s.x, pos.y - s.y);
    }

    public Vector2 GetVerticalBorder(float verticalDir, int horizontalSign = -1)
    {
        Vector2 s = m_collider.bounds.size / 2;
        Vector2 pos = transform.position;

        if (verticalDir == Defines.c_bottom)
            return new Vector2(pos.x + s.x * horizontalSign, pos.y - s.y);
        else
            return new Vector2(pos.x + s.x * horizontalSign, pos.y + s.y);
    }
}

public struct CollisionInfo
{
    public bool m_above, m_below, m_left, m_right;

    public void Reset()
    {
        m_above = m_below = m_left = m_right = false;
    }
}