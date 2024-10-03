using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour{
    [Header("Movement")]
    public float moveSpeed;
    public float sprintSpeed;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;
    bool sprinting;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

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
    void Start(){
        rb = GetComponent<Rigidbody>();
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
    }

    // gets player inputs
    private void MyInput() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // statements for sprinting and jumping
        if (Input.GetKeyDown(sprintKey) && !sprinting) {
            float temp = moveSpeed;
            moveSpeed = sprintSpeed;
            sprintSpeed = temp;
        }

        else if(Input.GetKeyDown(sprintKey) && sprinting) {
            float temp = moveSpeed;
            moveSpeed = sprintSpeed;
            sprintSpeed = temp;
        }

        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded) {
            readyToJump = false;
            jump();
            Invoke(nameof(resetJump), jumpCooldown);
        }
    }

    // moves player based on inputs
    private void MovePlayer() {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (grounded) {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    // ensures the player can't move faster than the max move speed
    private void speedControl() {
        Vector3 flatVelo = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if(flatVelo.magnitude > moveSpeed) {
            Vector3 limitedVelo = flatVelo.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVelo.x, rb.velocity.y , limitedVelo.z);
        }
    }

    // allows the player to jump
    private void jump() {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void resetJump() {
        readyToJump = true;
    }
}
