using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Керування гравцем з клавіатури + реакція на зіткнення за тегами.
/// WASD / стрілки - рух вправо-вліво та вперед-назад, Space - стрибок, Shift - прискорення.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Рух")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float sprintMultiplier = 1.8f;
    [SerializeField] private float jumpForce = 6f;

    [Header("Перевірка землі")]
    [SerializeField] private float groundCheckDistance = 1.1f;

    [Header("Реакція на теги")]
    [SerializeField] private float bouncerForce = 10f;
    [SerializeField] private int obstacleDamage = 10;

    private Rigidbody body;
    private Vector3 moveInput;
    private bool sprinting;
    private bool jumpRequested;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.freezeRotation = true;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            moveInput = Vector3.zero;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;

        moveInput = new Vector3(horizontal, 0f, vertical);
        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

        sprinting = keyboard.leftShiftKey.isPressed;

        if (keyboard.spaceKey.wasPressedThisFrame && IsGrounded())
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        float speed = sprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        Vector3 velocity = body.linearVelocity;
        Vector3 planar = moveInput * speed;

        body.linearVelocity = new Vector3(planar.x, velocity.y, planar.z);

        if (jumpRequested)
        {
            Vector3 current = body.linearVelocity;
            body.linearVelocity = new Vector3(current.x, jumpForce, current.z);
            jumpRequested = false;
        }

        if (moveInput.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(moveInput, Vector3.up);
            body.MoveRotation(Quaternion.Slerp(body.rotation, look, 12f * Time.fixedDeltaTime));
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
    }

    // Зіткнення з тригерами (монети, фініш)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            GameManager.Instance?.AddScore(1);
            Destroy(other.gameObject);
            return;
        }

        if (other.CompareTag("Finish"))
        {
            GameManager.Instance?.Finish();
        }
    }

    // Фізичні зіткнення (перешкоди, батут)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            GameManager.Instance?.TakeDamage(obstacleDamage);
            return;
        }

        if (collision.gameObject.CompareTag("Bouncer"))
        {
            Vector3 current = body.linearVelocity;
            body.linearVelocity = new Vector3(current.x, bouncerForce, current.z);
            GameManager.Instance?.ShowMessage("Батут! Підкидання вгору");
        }
    }
}
