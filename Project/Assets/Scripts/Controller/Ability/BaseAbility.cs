using UnityEngine;

public abstract class BaseAbility
{
    protected readonly PlayerController m_owner;

    protected abstract PlayerState State { get; }

    public BaseAbility(PlayerController owner)
    {
        m_owner = owner;
    }

    public void Update(Vector2 input)
    {
        if (CanUpdate(input))
        {
            UpdateImpl(input);
        }
        else
        {
            if (m_owner.State == State)
                m_owner.State = PlayerState.Normal;
        }
    }

    protected abstract bool CanUpdate(Vector2 input);

    protected abstract void UpdateImpl(Vector2 input);
}