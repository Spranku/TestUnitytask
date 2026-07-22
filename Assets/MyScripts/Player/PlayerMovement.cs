using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] 
    private float moveSpeed = 5f;
    [SerializeField] 
    private float jumpForce = 5f;
    [SerializeField] 
    private float rotationSpeed = 10f;
    [SerializeField] 
    private float jumpHeight = 2f;
    [SerializeField] 
    private float gravity = -9.81f;

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;

    /* Actions for other systems */
    public System.Action<bool> OnGroundedChanged;
    public System.Action<Vector3> OnPositionChanged;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null) { characterController = gameObject.AddComponent<CharacterController>(); }
    }

    /* Input handler */
    public void Move(Vector2 input)
    {
        if (characterController == null) return;

        UpdateGroundedState();
        ApplyGravity();

        var moveDirection = CalculateMoveDirection(input);
        ApplyMovement(moveDirection);
    }

    /* Jump for ability*/
    public void Jump()
    {
        if (!isGrounded) return;

        velocity.y = jumpForce;
    }

    public bool IsGrounded() => isGrounded;

    private void UpdateGroundedState()
    {
        bool wasGrounded = isGrounded;
        isGrounded = characterController.isGrounded;

        /* Send event */
        if (wasGrounded != isGrounded) { OnGroundedChanged?.Invoke(isGrounded); }
    }

    public void SetMoveSpeed(float newSpeed) { moveSpeed = Mathf.Max(0, newSpeed); } 

    public float GetMoveSpeed() => moveSpeed;

    private void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
    }

    private Vector3 CalculateMoveDirection(Vector2 input)
    {
        var direction = new Vector3(input.x, 0, input.y).normalized;

        /* Rotation to direction */
        if (direction.magnitude > 0.1f)
        {
            var targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        return direction;
    }

    private void ApplyMovement(Vector3 direction)
    {
        characterController.Move(direction * moveSpeed * Time.deltaTime);
        characterController.Move(velocity * Time.deltaTime);

        if (OnPositionChanged != null)
        {
            OnPositionChanged.Invoke(transform.position);
        }
    }
}