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
        walkingSpeed;
    [SerializeField] private float
        runningSpeed,
        crouchingSpeed,
        acceleration,
        deceleration;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpGravity, fallGravity, maxFallSpeed, coyoteTime;
    [SerializeField] private BufferTimer jumpBuffer;

    //[Header("Slope Parameters")]
    //[SerializeField] private float
    //    maxSlopeAngle;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private Transform groundCheck;

    private Rigidbody rb;

    private Vector3 currentVelocity;

    private enum State { Grounded, Jumping, Falling }
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
        bool runDown = Input.GetKey(KeyCode.LeftShift),
             crouchDown = Input.GetKey(KeyCode.C),
             jumpDown = Input.GetKeyDown(KeyCode.Space);

        // state of the player
        bool onGround = groundContacts.Count != 0,
             jumpBuffed = jumpBuffer.Buffer(jumpDown),
             canCoyote = prevState == State.Grounded && stateDur < coyoteTime && jumpBuffed,
             jumpFinished = vel.y < 0;

        // horizontal movement
        float speed = runDown ? runningSpeed
                    : crouchDown ? crouchingSpeed
                    : walkingSpeed,
              currentAcceleration = moveInput != Vector2.zero ? acceleration : deceleration;

        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y) * speed + Vector3.up * vel.y;
        vel = Vector3.SmoothDamp(vel, targetVelocity, ref currentVelocity, currentAcceleration);

        // vertical movement
        if (stateDur == 0)
            switch (state) {

                case State.Jumping:
                    vel.y = Mathf.Sqrt(jumpHeight * jumpGravity * 2);
                    break;
            }

        stateDur += Time.deltaTime;
        switch (state) {

            case State.Grounded:
                if (jumpBuffed) ChangeState(State.Jumping);
                else if (!onGround) ChangeState(State.Falling);
                break;

            case State.Jumping:
                vel.y -= jumpGravity * Time.deltaTime;

                if (jumpFinished) ChangeState(State.Falling);
                break;

            case State.Falling:
                vel.y -= fallGravity * Time.deltaTime;

                // clamp to fal speed
                vel.y = Mathf.Max(vel.y, -maxFallSpeed);

                if (onGround) ChangeState(State.Grounded);
                if (canCoyote) ChangeState(State.Jumping);
                break;
        }

        // apply velocity changes
        // and put back in world space
        rb.velocity = transform.TransformVector(vel);
    }
}
