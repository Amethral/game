using FishNet.Object;
using UnityEngine;

// Inherit from NetworkBehaviour instead of MonoBehaviour
public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    private PlayerControls _playerControls;

    private void Awake()
    {
        _playerControls = new PlayerControls();
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
        Vector3 moveDirection = new Vector3(input.x, 0f, input.y);

        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();

        transform.position += moveSpeed * Time.deltaTime * moveDirection;
    }
}