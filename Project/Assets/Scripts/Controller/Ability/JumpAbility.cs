using UnityEngine;

public class JumpAbility : BaseAbility
{
    float m_maxJumpVelocity = 20;
    float m_minJumpVelocity = 10;

    public override void Update(PlayerController controller, Vector2 input)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnJumpKeyDown(controller, input);
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            OnJumpKeyUp(controller);
        }
    }

    private void OnJumpKeyDown(PlayerController controller, Vector2 input)
    {
        Vector2 v = controller.Velocity;

        if (controller.CollisionInfo.m_below)
        {
            if(input.y < 0)
            {
                controller.FallThrough();
            }
            else
            {
                v.y = m_maxJumpVelocity;
                controller.Velocity = v;
            }
        }
    }

    private void OnJumpKeyUp(PlayerController controller)
    {
        Vector2 v = controller.Velocity;

        if (v.y > m_minJumpVelocity)
        {
            v.y = m_minJumpVelocity;
            controller.Velocity = v;
        }
    }
}