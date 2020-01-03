using System.Collections.Generic;
using UnityEngine;

//TODO
[RequireComponent(typeof(WaypointsMotor))]
public class PlatformController : RaycastController
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

            for(int i = 0; i < m_verticalRayCount; i++)
            {
                Vector2 origin = GetVerticalBorder(dirY);
                origin += Vector2.right * (m_verticalRayGaps * i);

                Vector2 dir = Vector2.up * dirY;

                Debug.DrawLine(origin, origin + dir * rayLength, Color.cyan);
                RaycastHit2D hit = Physics2D.Raycast(origin, dir, rayLength, m_collisionMask);
                if(hit && hit.distance != 0)
                {
                    if(added.Add(hit.transform))
                    {
                        Vector2 pos = hit.transform.position;
                        Debug.DrawLine(pos, pos + dir, Color.cyan);
                        
                        float pushX = (dirY == Defines.c_top ? movement.x : 0);
                        float pushY = movement.y - (hit.distance - c_skinWidth) * dirY;
                        Vector2 m = new Vector2(pushX, pushY);
                        bool isMovingUp = dirY == Defines.c_top;

                        Passenger p = new Passenger(hit.transform, m, isMovingUp, true);
                        result.Add(p);
                    }
                }
            }
        }

        //水平方向移动
        if(movement.x != 0)
        {
            float rayLength = Mathf.Abs(movement.x) + c_skinWidth;

            for(int i = 0; i < m_horizontalRayCount; i++)
            {
                Vector2 origin = GetHorizontalBorder(dirX);
                origin += Vector2.up * (m_horizontalRayGaps * i);

                Vector2 dir = Vector2.right * dirX;

                Debug.DrawLine(origin, origin + dir * rayLength, Color.cyan);
                RaycastHit2D hit = Physics2D.Raycast(origin, dir, rayLength, m_collisionMask);
                if(hit && hit.distance != 0)
                {
                    if(added.Add(hit.transform))
                    {
                        Vector2 pos = hit.transform.position;
                        Debug.DrawLine(pos, pos + dir, Color.cyan);

                        float pushX = movement.x - (hit.distance - c_skinWidth) * dirX;
                        float pushY = -c_skinWidth;
                        Vector2 m = new Vector2(pushX, pushY);

                        Passenger p = new Passenger(hit.transform, m, false, true);
                        result.Add(p);
                    }
                }
            }
        }

        //乘客在水平移动或向下移动的平台上
        if(dirY == Defines.c_bottom || (movement.y == 0 && movement.x != 0))
        {
            float rayLength = c_skinWidth * 2;

            for(int i = 0; i < m_verticalRayCount; i++)
            {
                Vector2 origin = GetVerticalBorder(Defines.c_top);
                origin += Vector2.right * (m_verticalRayGaps * i);

                Debug.DrawLine(origin, origin + Vector2.up * rayLength, Color.red);
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, rayLength, m_collisionMask);
                if(hit && hit.distance != 0)
                {
                    if(added.Add(hit.transform))
                    {
                        Vector2 pos = hit.transform.position;
                        Debug.DrawLine(pos, pos + Vector2.up, Color.red);

                        Passenger p = new Passenger(hit.transform, movement, true, false);
                        result.Add(p);
                    }
                }
            }
        }

        HashSetPool<Transform>.Release(ref added);
    }

    void MovePassengers(List<Passenger> passengers, bool moveBefore)
    {
        for(int i = 0; i < passengers.Count; i++)
        {
            var p = passengers[i];
            if(p.m_moveBefore == moveBefore)
            {
                var controller = p.m_transform.GetComponent<RaycastController>();
                controller.Move(p.m_velocity);
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