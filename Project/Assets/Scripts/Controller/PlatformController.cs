using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WaypointsMotor))]
public class PlatformController : Raycaster
{
    WaypointsMotor m_waypointsMotor;

    protected override void Awake()
    {
        base.Awake();

        m_waypointsMotor = GetComponent<WaypointsMotor>();
        m_waypointsMotor.TriggerUpdate = true;
    }

    private void Update()
    {
        Vector2 originPos = transform.position;
        Vector2 targetPos = m_waypointsMotor.Move();
        Vector2 movement = targetPos - originPos;

        List<Passenger> passengers = ListPool<Passenger>.Get();

        DetectPassengers(movement, passengers);

        MovePassengers(passengers, true);
        transform.Translate(movement);
        MovePassengers(passengers, false);

        ListPool<Passenger>.Release(ref passengers);
    }

    void DetectPassengers(Vector2 movement, List<Passenger> result)
    {
        HashSet<Transform> added = HashSetPool<Transform>.Get();

        float dirX = Mathf.Sign(movement.x);
        float dirY = Mathf.Sign(movement.y);

        //竖直方向移动
        if(movement.y != 0)
        {
            float rayLength = Mathf.Abs(movement.y) + c_skinWidth;

            void action(RaycastHit2D hit)
            {
                if (added.Add(hit.transform))
                {
                    float pushX = (dirY == Defines.c_top ? movement.x : 0);
                    float pushY = movement.y - (hit.distance - c_skinWidth) * dirY;
                    Vector2 m = new Vector2(pushX, pushY);
                    bool isMovingUp = dirY == Defines.c_top;

                    Passenger p = new Passenger(hit.transform, m, isMovingUp, true);
                    result.Add(p);
                }
            }

            CheckRaycast(true, rayLength, dirY, action);
        }

        //水平方向移动
        if(movement.x != 0)
        {
            float rayLength = Mathf.Abs(movement.x) + c_skinWidth;

            void action(RaycastHit2D hit)
            {
                if (added.Add(hit.transform))
                {
                    float pushX = movement.x - (hit.distance - c_skinWidth) * dirX;
                    float pushY = -c_skinWidth;
                    Vector2 m = new Vector2(pushX, pushY);

                    Passenger p = new Passenger(hit.transform, m, false, true);
                    result.Add(p);
                }
            }

            CheckRaycast(false, rayLength, dirX, action);
        }

        //乘客在水平移动或向下移动的平台上
        if(dirY == Defines.c_bottom || (movement.y == 0 && movement.x != 0))
        {
            float rayLength = c_skinWidth * 2;

            void action(RaycastHit2D hit)
            {
                if (added.Add(hit.transform))
                {
                    Passenger p = new Passenger(hit.transform, movement, true, false);
                    result.Add(p);
                }
            }

            CheckRaycast(true, rayLength, 1, action);
        }

        HashSetPool<Transform>.Release(ref added);
    }

    void CheckRaycast(bool isVertical, float rayLength, float dir, Action<RaycastHit2D> hitAction)
    {
        int rayCount;
        float rayGaps;
        Vector2 border;
        Vector2 rayDir;
        Vector2 axisDir;

        if(isVertical)
        {
            rayCount = m_verticalRayCount;
            rayGaps = m_verticalRayGaps;
            border = GetVerticalBorder(dir);
            rayDir = Vector2.up * dir;
            axisDir = Vector2.right;
        }
        else
        {
            rayCount = m_horizontalRayCount;
            rayGaps = m_horizontalRayGaps;
            border = GetHorizontalBorder(dir);
            rayDir = Vector2.right * dir;
            axisDir = Vector2.up;
        }

        for(int i = 0; i < rayCount; i++)
        {
            Vector2 origin = border + axisDir * (rayGaps * i);

            RaycastHit2D hit = Physics2D.Raycast(origin, rayDir, rayLength, m_collisionMask);
            if(hit && hit.distance != 0)
            {
                hitAction(hit);
            }
        }
    }

    void MovePassengers(List<Passenger> passengers, bool moveBefore)
    {
        for(int i = 0; i < passengers.Count; i++)
        {
            var p = passengers[i];
            if(p.m_moveBefore == moveBefore)
            {
                var controller = p.m_transform.GetComponent<Raycaster>();
                controller.Move(p.m_velocity, p.m_isStandingOnPlatform);
            }
        }
    }

    #region 内部
    struct Passenger
    {
        public Transform m_transform;
        public Vector2 m_velocity;
        public bool m_isStandingOnPlatform;
        public bool m_moveBefore;

        public Passenger(Transform trans, Vector2 v, bool isStandingOn, bool moveBefore)
        {
            m_transform = trans;
            m_velocity = v;
            m_isStandingOnPlatform = isStandingOn;
            m_moveBefore = moveBefore;
        }
    }
    #endregion
}