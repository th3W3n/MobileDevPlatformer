using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Joystick joystick;
    [Range(0f, 1f)][SerializeField] private float joystickSensitivityH = 0.5f;
    [Range(0f, 1f)][SerializeField] private float joystickSensitivityV = 0.5f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float responsiveness = 10f;
    //change jumpImpulse & rb gravity scale to get the right jump height and speed
    [SerializeField] private float jumpImpulse = 30f;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.9f, 0.1f);

    private const int IDLE = 0, RUN = 1, JUMP = 2, FALL = 3;
    private float moveInput;
    private bool upHeld;
    private bool isGrounded;
    private bool jumpRequested, isJumping;

    private Rigidbody2D rb;
    private Animator ac;
    private SpriteRenderer sr;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ac = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        GroundCheck();
        HandleInput();
        UpdateAnimation();
    }
    private void FixedUpdate()
    {
        Move();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DeathZone") && spawnPoint != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.position = spawnPoint.position;
            isJumping = false;
            jumpRequested = false;
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }

    //-------------------------------------------------------------

    private void HandleInput()
    {
        float h = joystick.Horizontal;
        float v = joystick.Vertical;
        moveInput = Mathf.Abs(h) > joystickSensitivityH ? h : 0f;

        bool upNow = v > joystickSensitivityV;
        if (upNow && !upHeld && isGrounded && !isJumping)
            jumpRequested = true;
        upHeld = upNow;

        if (moveInput > 0) sr.flipX = false;
        else if (moveInput < 0) sr.flipX = true;
    }
    private void Move()
    {
        float targetSpeed = moveInput * maxSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        rb.AddForce(Vector2.right * speedDiff * responsiveness * rb.mass);

        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
            jumpRequested = false;
            isJumping = true;
        }
    }
    private void GroundCheck()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        if (isGrounded && rb.linearVelocity.y <= 0.01f)
            isJumping = false;
    }
    private void UpdateAnimation()
    {
        int state;
        if (isGrounded && !isJumping)
            state = moveInput != 0 ? RUN : IDLE;
        else if (rb.linearVelocity.y > 0.1f)
            state = JUMP;
        else
            state = FALL;

        ac.SetInteger("AnimState", state);
    }
}