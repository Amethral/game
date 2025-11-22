using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

// Inherit from NetworkBehaviour instead of MonoBehaviour
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    public Transform cameraTransform;

    public bool shouldFaceMovementDirection = false;


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

        Vector2 input = _playerControls.Player.Move.ReadValue<Vector2>();
        bool isJumping = _playerControls.Player.Jump.ReadValue<float>() > 0;

        if (isJumping && _characterController.isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * input.y + right * input.x;
        _characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        if (shouldFaceMovementDirection && moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 10f);
        }

        _velocity.y += gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);

    }
}