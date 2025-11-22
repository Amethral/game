using FishNet.Object;
using UnityEngine;

// Inherit from NetworkBehaviour instead of MonoBehaviour
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    private PlayerControls _playerControls;
    private CharacterController _characterController;
    private Vector3 _velocity;

    private void Awake()
    {
        _playerControls = new PlayerControls();
        _characterController = GetComponent<CharacterController>();
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
        // Only run this code on the object the local client owns.
        // This prevents us from moving other players' objects.
        if (!IsOwner)
            return;

        // Apply gravity
        if (_characterController.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // Small downward force to keep grounded
        }

        Vector2 input = _playerControls.Player.Move.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(input.x, 0f, input.y);

        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();

        // Move based on input
        _characterController.Move(moveSpeed * Time.deltaTime * moveDirection);

        // Apply gravity over time
        _velocity.y += gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
    }
}