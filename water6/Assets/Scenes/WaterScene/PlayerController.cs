using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] Rigidbody rb;

    [SerializeField] GameObject leftArm;
    [SerializeField] GameObject rightArm;

    private Vector2 moveInput;
    private Vector3 initForward;

    void Awake()
    {
        initForward = transform.forward;
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        if (Math.Abs(moveInput.x) < 0.1f)
        {
            return;
        }

        int moveDirection = Math.Sign(moveInput.x);

        Vector3 targetForward = initForward * moveDirection;
        transform.rotation = Quaternion.LookRotation(targetForward, Vector3.up);

        Vector3 localMove = new Vector3(0f, 0f, moveDirection * moveInput.x);
        Vector3 worldMove = transform.TransformDirection(localMove);        
        rb.MovePosition(rb.position + worldMove * speed * Time.fixedDeltaTime);
    }
}
