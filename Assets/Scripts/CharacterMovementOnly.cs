using UnityEngine;

// Testcommit

public class CharacterMovementOnly : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;
    public float moveThreshold = 0.1f;
    public Animator animator;
    public bool useAnimator = true;

    private Transform cachedTransform;
    private Vector3 moveDirection;
    private float horizontalMovement;
    private float verticalMovement;
    private bool hasMovementInput;

    void Awake()
    {
        cachedTransform = transform;
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleInputOptimized();
        HandleMovementOptimized();
        if (useAnimator && animator != null)
            UpdateAnimationsOptimized();
    }

    private void HandleInputOptimized()
    {
        horizontalMovement = 0f;
        verticalMovement = 0f;
        if (Input.GetKey(KeyCode.W)) verticalMovement = 1f;
        else if (Input.GetKey(KeyCode.S)) verticalMovement = -1f;
        if (Input.GetKey(KeyCode.D)) horizontalMovement = 1f;
        else if (Input.GetKey(KeyCode.A)) horizontalMovement = -1f;
        moveDirection.Set(horizontalMovement, 0f, verticalMovement);
        hasMovementInput = moveDirection.sqrMagnitude >= (moveThreshold * moveThreshold);
    }

    private void HandleMovementOptimized()
    {
        Vector3 inputDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) inputDirection += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) inputDirection += Vector3.back;
        if (Input.GetKey(KeyCode.A)) inputDirection += Vector3.left;
        if (Input.GetKey(KeyCode.D)) inputDirection += Vector3.right;

        if (inputDirection.sqrMagnitude > 0.01f)
        {
            inputDirection.Normalize();
            // Immediate rotation, so the character looks directly in the new direction
            cachedTransform.rotation = Quaternion.LookRotation(inputDirection, Vector3.up);
        }
    }

    private void UpdateAnimationsOptimized()
    {
        animator.SetFloat("Speed", moveDirection.magnitude);
    }
}