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
    [SerializeField] private float speed = 5f;
    [SerializeField] private float extraJumpHeight = 2f;

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

    // Ballistic jump to target using projectile-motion kinematics; lands at target with apex from extraJumpHeight.
    // Math reference: https://openstax.org/books/university-physics-volume-1/pages/4-3-projectile-motion
    public void JumpToPos(Transform target) {
        if (target == null) {
            Debug.LogWarning("JumpToPos failed: target is null.");
            return;
        }

        Vector3 startPosition = rb.position;
        Vector3 displacement = target.position - startPosition;

        float gravity = Mathf.Abs(Physics.gravity.y);
        if (gravity <= Mathf.Epsilon) {
            Debug.LogWarning("JumpToPos failed: gravity is too small.");
            return;
        }

        float apexY = Mathf.Max(startPosition.y, target.position.y) + Mathf.Max(0.1f, extraJumpHeight);
        float upDistance = apexY - startPosition.y;
        float downDistance = apexY - target.position.y;

        float timeUp = Mathf.Sqrt(2f * upDistance / gravity);
        float timeDown = Mathf.Sqrt(2f * downDistance / gravity);
        float totalTime = timeUp + timeDown;

        if (totalTime <= Mathf.Epsilon) {
            Debug.LogWarning("JumpToPos failed: invalid jump time.");
            return;
        }

        Vector3 horizontalDisplacement = new Vector3(displacement.x, 0f, displacement.z);
        Vector3 horizontalVelocity = horizontalDisplacement / totalTime;
        float verticalVelocity = gravity * timeUp;

        Vector3 launchVelocity = horizontalVelocity + Vector3.up * verticalVelocity;

        rb.linearVelocity = launchVelocity;
    }
}
