using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Component.Animating;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float rotationSpeed = 10f;

    public bool CanMove = true;

    [Header("Stamina Settings")]
    [SerializeField] private float jumpStaminaCost = 20f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;

    // Private Logic Variables
    private PlayerControls _playerControls;
    private CharacterController _characterController;
    private PlayerStats _playerStats;
    private Vector3 _verticalVelocity; // We separate vertical from horizontal for cleaner math
    private Vector2 _currentInput;

    private void Awake()
    {
        _playerControls = new PlayerControls();
        _characterController = GetComponent<CharacterController>();
        _playerStats = GetComponent<PlayerStats>();

        // Auto-find camera if not assigned
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void OnEnable() => _playerControls.Enable();
    private void OnDisable() => _playerControls.Disable();

    private void Update()
    {
        // 1. Network Guard: Only the owner controls this character
        if (!IsOwner) return;

        // 2. State Guard: If we shouldn't move, stop here
        if (!CanMove)
        {
            UpdateAnimator(Vector3.zero); // Ensure we go back to Idle
            return;
        }

        // 3. Logic Step-by-Step
        HandleGravity();
        HandleMovement(); // Includes Rotation
        HandleJump();
    }

    private void HandleGravity()
    {
        // If we are on the ground, reset the downward force.
        // We use a small number (-2f) instead of 0 to keep the controller snapped to the floor.
        if (_characterController.isGrounded && _verticalVelocity.y < 0)
        {
            _verticalVelocity.y = -2f;
        }

        // Apply Gravity over time
        _verticalVelocity.y += gravity * Time.deltaTime;
    }

    private void HandleMovement()
    {
        // --- INPUT ---
        _currentInput = _playerControls.Player.Move.ReadValue<Vector2>();

        // --- DIRECTION CALCULATION ---
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Flatten camera vectors so looking up/down doesn't slow us down
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * _currentInput.y + right * _currentInput.x;

        // --- ROTATION ---
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * rotationSpeed);
        }

        // --- APPLY MOVEMENT ---
        // Combine Horizontal (Input) + Vertical (Gravity/Jump) into one Move call
        Vector3 finalMovement = (moveDirection * moveSpeed) + _verticalVelocity;
        _characterController.Move(finalMovement * Time.deltaTime);

        // --- ANIMATION UPDATE ---
        // Crucial: Pass 'moveDirection' magnitude or local velocity, NOT total velocity.
        // This fixes the bug where Jumping made the Animator think you were Walking.
        UpdateAnimator(moveDirection * moveSpeed);
    }

    private void HandleJump()
    {
        // Use .triggered to only register the frame the button was pressed
        if (_playerControls.Player.Jump.triggered && _characterController.isGrounded)
        {
            if (_playerStats.Stamina.Value >= jumpStaminaCost)
            {
                // Physics calculation for exact jump height
                _verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

                // Animation
                networkAnimator.SetTrigger("Jump");

                // Logic
                _playerStats.CmdUseStamina(jumpStaminaCost);
            }
        }
    }

    private void UpdateAnimator(Vector3 horizontalVelocity)
    {
        if (animator == null) return;

        // We only send the Horizontal Speed to the animator.
        // This ensures that falling or jumping straight up keeps "velocity" at 0.
        float speed = horizontalVelocity.magnitude;
        animator.SetFloat("velocity", speed);
    }
}