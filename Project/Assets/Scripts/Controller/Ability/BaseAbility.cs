using UnityEngine;

public abstract class BaseAbility
{
    protected readonly PlayerController m_owner;

    public BaseAbility(PlayerController owner)
    {
        m_owner = owner;
    }

    public abstract void Update(Vector2 input);
}