using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

    [SerializeField] private GameManager gameManager;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject leftArm;
    [SerializeField] private GameObject rightArm;

    [Header("Trigger Zone")]
    [SerializeField] private Collider swimTriggerZone;

    [Header("Settings")]
    [SerializeField] float speed = 5f;

    private Vector2 moveInput;
    private Vector3 initForward;

    void Awake() {
        initForward = transform.forward;
    }

    void OnMove(InputValue value) {
        moveInput = value.Get<Vector2>();
    }

    void OnTriggerEnter(Collider other) {
        if (other == swimTriggerZone) {
            gameManager.OnPlayerEnterSwimTriggerZone();
        }
    }

    void OnTriggerExit(Collider other) {
        if (other == swimTriggerZone) {
            gameManager.OnPlayerExitSwimTriggerZone();
        }
    }

    void FixedUpdate() {
        if (Math.Abs(moveInput.x) < 0.1f) {
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
