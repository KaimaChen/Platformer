using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float m_Speed;

    void Start()
    {
        
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

    }

    void HandleJump()
    {
        if(Input.GetKeyDown("Space"))
        {

        }
        else if(Input.GetKey("Space"))
        {

        }
        else if(Input.GetKeyUp("Space"))
        {

        }
    }
}
