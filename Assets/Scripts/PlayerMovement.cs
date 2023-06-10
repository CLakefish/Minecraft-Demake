using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public enum PlayerStates
    {
        Grounded,
        Jumping,
        Falling
    }

    [Header("Object References")]
    [SerializeField] Transform viewPosition;
    [SerializeField] LayerMask layer;
    internal Rigidbody rb;

    [Header("States")]
    [HideInInspector] float stateDur; // Duration of the state
    [HideInInspector] public PlayerStates // State data
        state,
        prevState;
    internal void ChangeState(PlayerStates s)
    {
        stateDur = 0;
        prevState = state;
        state = s;
    } // State Change function


    [Header("Movement Parameters")]
    [SerializeField] public bool canMove = true;
    [SerializeField]
    float // Player Speeds
        walkingSpeed,
        runningSpeed,
        crouchingSpeed;
    [SerializeField]
    float // Player Acceleration 
        acceleration,
        deceleration;

    [Header("Slope Parameters")]
    [SerializeField] float maxSlopeAngle;

    [Header("View-Tilt Parameters")]
    internal Vector2 viewTilt, currentTiltSpeed;
    const float smoothDampSpeed = 0.1f,
                maxSmoothDampSpeed = 10f;

    [Header("Movement Variables")]
    [HideInInspector] public Vector2 input;
    [HideInInspector] Vector3 currentVelocity;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();

        StartCoroutine(WaitToFall());
    }

    // Update is called once per frame
    void Update()
    {
        if (!canMove) return;

        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.5f, layer);

        float speed = (Input.GetKey(KeyCode.LeftShift) ? runningSpeed : Input.GetKey(KeyCode.C) ? crouchingSpeed : walkingSpeed);
        float speedI = (input != new Vector2(0f, 0f)) ? acceleration : deceleration;

        Vector3 moveDir = (transform.forward * input.y + transform.right * input.x).normalized * speed;

        // Velocity
        Vector3 velocity = new Vector3(moveDir.x, rb.velocity.y, moveDir.z);

        rb.velocity = Vector3.SmoothDamp(rb.velocity, velocity, ref currentVelocity, speedI * Time.deltaTime);

        Vector3.ClampMagnitude(rb.velocity, speed);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * 8f, ForceMode.VelocityChange);
        }
        else if (!isGrounded)
        {
            rb.AddForce(Vector3.down * 1.6f, ForceMode.Force);
        }
    }

    
    IEnumerator WaitToFall()
    {
        rb.isKinematic = true;
        canMove = false;

        yield return new WaitForSeconds(5f);

        canMove = true;
        rb.isKinematic = false;
    }
}
