using UnityEngine;
using UnityEngine.InputSystem;

// Testcommit

public class CharacterMovementOnly : MonoBehaviour
{
    public float moveSpeed = 5f; // Spieler-Laufgeschwindigkeit (units/Sekunde)
    public float rotationSpeed = 720f;
    public float moveThreshold = 0.1f;

    private Transform cachedTransform;
    private CharacterController characterController;
    private Animator animator;
    private Vector3 moveDirection;
    private float horizontalMovement;
    private float verticalMovement;
    private bool hasMovementInput;

    void Awake()
    {
        cachedTransform = transform;
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleInputOptimized();
        HandleMovementOptimized();
        UpdateAnimations();
    }


    private bool IsKeyPressed(Key key)
    {
        if (Keyboard.current == null)
            return false;
        var keyButton = Keyboard.current[key];
        return keyButton != null && keyButton.isPressed;
    }

    private void HandleInputOptimized()
    {
        horizontalMovement = 0f;
        verticalMovement = 0f;
        if (IsKeyPressed(Key.W)) verticalMovement = 1f;
        else if (IsKeyPressed(Key.S)) verticalMovement = -1f;
        if (IsKeyPressed(Key.D)) horizontalMovement = 1f;
        else if (IsKeyPressed(Key.A)) horizontalMovement = -1f;
        moveDirection.Set(horizontalMovement, 0f, verticalMovement);
        hasMovementInput = moveDirection.sqrMagnitude >= (moveThreshold * moveThreshold);
    }

    private void HandleMovementOptimized()
    {
        if (cachedTransform == null || characterController == null)
            return;

        // Bewegung wird nicht mehr durch IsAttacking blockiert

        Vector3 inputDirection = moveDirection;
        
        if (inputDirection.sqrMagnitude > 0.001f)
        {
            inputDirection.Normalize();
            cachedTransform.rotation = Quaternion.LookRotation(inputDirection, Vector3.up);
            
            float effectiveSpeed = moveSpeed;
            Vector3 movement = inputDirection * effectiveSpeed * Time.deltaTime;
            characterController.Move(movement);
        }
    }

    private void UpdateAnimations()
    {
        if (animator != null)
            animator.SetFloat("Speed", moveDirection.magnitude);
    }


}