using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations; // Note: Standard Unity Animations namespace if needed
using FishNet.Component.Animating;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))] // Ensure PlayerStats is on the same object
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1f;

    public bool CanMove = true;

    [Header("Stamina Settings")]
    public float jumpStaminaCost = 20f; // Amount of stamina to use per jump

    [Header("References")]
    public Transform cameraTransform;
    public bool shouldFaceMovementDirection = false;
    public Animator animator;
    public NetworkAnimator networkAnimator;

    private PlayerControls _playerControls;
    private CharacterController _characterController;
    private PlayerStats _playerStats; // Reference to the stats script
    private Vector3 _velocity;

    private void Awake()
    {
        _playerControls = new PlayerControls();
        _characterController = GetComponent<CharacterController>();
        _playerStats = GetComponent<PlayerStats>(); // Get the reference
    }

    private void OnEnable()
    {
        _playerControls.Enable();
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }

    void Update()
    {
        if (!IsOwner)
            return;

        if (!CanMove)
            return;

        Vector2 input = _playerControls.Player.Move.ReadValue<Vector2>();
        bool isJumping = _playerControls.Player.Jump.ReadValue<float>() > 0;

        // MOVEMENT LOGIC
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * input.y + right * input.x;
        _characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        animator.SetFloat("velocity", _characterController.velocity.magnitude);

        if (shouldFaceMovementDirection && moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 10f);
        }

        // JUMP LOGIC
        if (isJumping && _characterController.isGrounded)
        {
            // Check if we have enough stamina before jumping
            if (_playerStats.Stamina.Value >= jumpStaminaCost)
            {
                // Apply Physics
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

                // Play Animation
                networkAnimator.SetTrigger("Jump");

                // Deduct Stamina (This calls the ServerRpc in PlayerStats)
                _playerStats.CmdUseStamina(jumpStaminaCost);
            }
        }

        // GRAVITY
        _velocity.y += gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
    }
}