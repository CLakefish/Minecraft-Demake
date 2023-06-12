using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MovmementHelperFunctions {
    public static bool ContainsLayer(this LayerMask mask, int layer) => mask == (mask | (1 << layer));
}

public class PlayerMovement : MonoBehaviour {

    [SerializeField] private float waitToFallTime;
    [SerializeField] private bool canMove = true;

    [Header("Object References")]
    [SerializeField] private Transform viewPosition;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("Walking")]
    [SerializeField] private float
        walkSpeed;
    [SerializeField] private float
        runSpeed,
        crouchSpeed,
        acceleration,
        deceleration;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpGravity, fallGravity, maxFallSpeed, coyoteTime;
    [SerializeField] private BufferTimer jumpBuffer;

    [Header("Flying")]
    [SerializeField] private float horizontalFlySpeed;
    [SerializeField] private float verticalFlySpeed;

    //[Header("Slope Parameters")]
    //[SerializeField] private float
    //    maxSlopeAngle;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private Transform groundCheck;

    private Rigidbody rb;

    private Vector3 currentVelocity;

    private enum State { grounded, jumping, falling, flying }
    private float stateDur;
    private State state, prevState;

    private void ChangeState(State newState) {
        prevState = state;
        state = newState;
        stateDur = 0;
    }

    private List<ContactPoint> groundContacts = new();

    private void Awake() {

        rb = GetComponent<Rigidbody>();

        StartCoroutine(WaitToFall());
        IEnumerator WaitToFall() {

            rb.isKinematic = true;
            canMove = false;

            yield return new WaitForSeconds(waitToFallTime);

            canMove = true;
            rb.isKinematic = false;
        }
    }

    private void FixedUpdate() {
        groundContacts.Clear();
    }

    private void OnCollisionStay(Collision collision) {
        if (groundLayerMask.ContainsLayer(collision.gameObject.layer))
            for (int i = 0; i < collision.contactCount; i++)
                if (collision.GetContact(0).normal == Vector3.up)
                    groundContacts.Add(collision.GetContact(i));
    }

    private void Update() {

        if (!canMove) return;

        // store rb.velocity so we can edit the x/y/z components individually
        // and transform it to local space, so z is forward/backward, and x is left/right
        Vector3 vel = transform.InverseTransformVector(rb.velocity);

        // input
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        bool runPressed = Input.GetKey(KeyCode.LeftShift),
             crouchPressed = Input.GetKey(KeyCode.C),
             jumpDown = Input.GetKeyDown(KeyCode.Space),
             jumpPressed = Input.GetKey(KeyCode.Space),
             flyDown = Input.GetKeyDown(KeyCode.F);

        // state of the player
        bool flying = state == State.flying,
             onGround = groundContacts.Count != 0,
             jumpBuffed = jumpBuffer.Buffer(jumpDown),
             canCoyote = prevState == State.grounded && stateDur < coyoteTime && jumpBuffed,
             jumpFinished = vel.y < 0;

        // horizontal movement
        float speed = flying ? horizontalFlySpeed
                    : runPressed ? runSpeed
                    : crouchPressed ? crouchSpeed
                    : walkSpeed,
              currentAcceleration = moveInput != Vector2.zero ? acceleration : deceleration;

        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y) * speed + Vector3.up * vel.y;
        vel = Vector3.SmoothDamp(vel, targetVelocity, ref currentVelocity, currentAcceleration);

        // vertical movement
        if (flyDown) ChangeState(state == State.flying ? State.falling : State.flying);

        if (stateDur == 0)
            switch (state) {

                case State.jumping:
                    vel.y = Mathf.Sqrt(jumpHeight * jumpGravity * 2);
                    break;
            }

        stateDur += Time.deltaTime;
        switch (state) {

            case State.grounded:
                if (jumpBuffed) ChangeState(State.jumping);
                else if (!onGround) ChangeState(State.falling);
                break;

            case State.jumping:
                vel.y -= jumpGravity * Time.deltaTime;

                if (jumpFinished) ChangeState(State.falling);
                break;

            case State.falling:
                vel.y -= fallGravity * Time.deltaTime;

                // clamp to fal speed
                vel.y = Mathf.Max(vel.y, -maxFallSpeed);

                if (onGround) ChangeState(State.grounded);
                if (canCoyote) ChangeState(State.jumping);
                break;

            case State.flying:
                vel.y = ((jumpPressed ? 1 : 0) - (crouchPressed ? 1 : 0)) * verticalFlySpeed;
                break;
        }

        // apply velocity changes
        // and put back in world space
        rb.velocity = transform.TransformVector(vel);
    }
}
