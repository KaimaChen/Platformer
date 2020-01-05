using UnityEngine;

public abstract class BaseAbility
{
    protected readonly PlayerController m_owner;

    public BaseAbility(PlayerController owner)
    {
        m_owner = owner;
    }

    public void Update(Vector2 input)
    {
        if (CanUpdate() == false)
            return;

        UpdateImpl(input);
    }

    protected abstract bool CanUpdate();

    protected abstract void UpdateImpl(Vector2 input);
}