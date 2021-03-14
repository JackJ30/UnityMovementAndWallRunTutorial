using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Transform orientation;

    [Header("Movement")]
    [SerializeField] public float movementSpeed = 6f;
    float movementMult = 10f;
    [SerializeField] public Vector3 gravity = Vector3.down * 30f;
    [SerializeField] public float airResistance = 0.45f;

    [Header("Drag")]
    [SerializeField] float groundDrag = 5f;
    [SerializeField] float airDrag = 2f;

    [Header("Jumping")]
    public float jumpForce = 15f;

    [Header("Ground Detection")]
    bool isGrounded;
    float groundDistance = 0.4f;
    float playerHeight = 2f;
    [SerializeField] LayerMask groundMask;

    Vector3 moveDirection;

    Rigidbody rigidbody;
    PlayerInput playerInput;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(transform.position - ((Vector3.up * playerHeight) / 2), groundDistance, groundMask);

        ProcessInput();
        SwitchDrag();

        if(playerInput.jumpInput && isGrounded)
        {
            Jump();
        }
    }

    void Jump()
    {
        rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void ProcessInput()
    {
        moveDirection = (orientation.forward * playerInput.verticalInput) + (orientation.right * playerInput.horizontalInput);
    }

    private void FixedUpdate()
    {
        Movement();
    }

    void SwitchDrag()
    {
        if(isGrounded)
        {
            rigidbody.drag = groundDrag;
        }
        else
        {
            rigidbody.drag = airDrag;
        }
    }

    public void Movement()
    {
        if(isGrounded)
        {
            rigidbody.AddForce(moveDirection.normalized * movementSpeed * movementMult, ForceMode.Acceleration);
        }
        else
        {
            rigidbody.AddForce(moveDirection.normalized * movementSpeed * movementMult * airResistance + gravity, ForceMode.Acceleration);
        }
    }
}
