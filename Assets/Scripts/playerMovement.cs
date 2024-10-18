using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour {
    [Header("Movement")]
    public float speed;
    public float moveSpeed;
    public float groundDrag;
    public float yScale;

    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Sprint")]
    bool sprinting;
    public float sprintSpeed;
    public float sprintTimer;
    public float sprintTime;

    [Header("Crouch")]
    bool crouching;
    public float crouchSpeed;
    public float crouchYScale;

    [Header("Slide")]
    public bool sliding;
    public float slideForce;
    public float maxSlideTime;
    public float slideTime;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    bool exitingSlope;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;
    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;
    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();
        speed = moveSpeed;
        yScale = transform.localScale.y;
        rb.freezeRotation = true;
        readyToJump = true;
    }

    private void Update() {
        MyInput();
        speedControl();

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * .5f + .2f, whatIsGround);

        if (grounded) {
            rb.drag = groundDrag;
        }
        else {
            rb.drag = 0;
        }
    }

    private void FixedUpdate() {
        MovePlayer();
        if (sprinting) {
            sprintTime -= Time.deltaTime;
        }
        if (sliding) {
            slide();
        }
    }

    // gets player inputs
    private void MyInput() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // statements for sprinting and jumping
        if (Input.GetKeyDown(sprintKey) && !sprinting && grounded && horizontalInput == 0) {
            sprinting = true;
            speed = sprintSpeed;
            sprintTime = sprintTimer;
        }

        else if (Input.GetKeyDown(sprintKey) && sprinting && grounded) {
            sprinting = false;
            speed = moveSpeed;
        }

        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded) {
            readyToJump = false;
            jump();
            Invoke(nameof(resetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey) && !crouching && grounded && sprinting && sprintTime <= 0 && horizontalInput == 0) {
            startSlide();
        }

        else if (Input.GetKeyDown(crouchKey) && !crouching && grounded && !sprinting) {
            crouching = true;
            speed = crouchSpeed;
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey) && crouching) {
            crouching = false;
            speed = moveSpeed;
            transform.localScale = new Vector3(transform.localScale.x, yScale, transform.localScale.z);
        }
    }

    // moves player based on inputs
    private void MovePlayer() {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (grounded) {
            rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);
        }
        else {
            rb.AddForce(moveDirection.normalized * speed * 10f * airMultiplier, ForceMode.Force);
        }

        if (onSlope() && !exitingSlope) {
            rb.AddForce(getSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0) {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        if(sprinting && !crouching && verticalInput == 0) {
            sprinting = false;
            speed = moveSpeed;
        }

        rb.useGravity = !onSlope();
    }

    // ensures the player can't move faster than the max move speed
    private void speedControl() {
        // limit speed on slopes
        if (onSlope() && !exitingSlope) {
            if (rb.velocity.magnitude > speed) {
                rb.velocity = rb.velocity.normalized * speed;
            }
        }
        else {
            Vector3 flatVelo = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVelo.magnitude > speed) {
                Vector3 limitedVelo = flatVelo.normalized * speed;
                rb.velocity = new Vector3(limitedVelo.x, rb.velocity.y, limitedVelo.z);
            }
        }
    }

    // allows the player to jump
    private void jump() {
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void resetJump() {
        exitingSlope = false;
        readyToJump = true;
    }

    private bool onSlope() {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * .5f + .3f)) {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 getSlopeMoveDirection() {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private void startSlide() {
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        slideTime = maxSlideTime;
        sliding = true;
    }
    private void slide() {
        rb.AddForce(moveDirection * slideForce, ForceMode.Force);

        slideTime -= Time.deltaTime;
        if(slideTime <= 0) {
            sliding = false;
            transform.localScale = new Vector3(transform.localScale.x, yScale, transform.localScale.z);
            sprintTime = sprintTimer;
        }
    }
} 
