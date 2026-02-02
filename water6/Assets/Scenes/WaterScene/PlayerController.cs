using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] Rigidbody rb;

    private Vector2 moveInput;
    
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        Vector3 localMove = new Vector3(0f, 0f, moveInput.x);
        Vector3 worldMove = transform.TransformDirection(localMove);
        
        rb.MovePosition(rb.position + worldMove * speed * Time.fixedDeltaTime);
    }
}
