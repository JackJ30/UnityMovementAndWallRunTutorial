﻿using System.Collections;
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
    [SerializeField] float jumpForce = 15f;

    [Header("Ground Detection")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundCheckRadius = 0.4f;
    [SerializeField] float groundCheckOffset = 0.1f;
    [SerializeField] float groundMoveDownForce = 80f;
    Collider[] groundCheckCollidingWith;
    List<Collider> colliderColldingWith = new List<Collider>();
    bool isGrounded;
    float playerHeight = 2f;

    [Header("Slopes")]
    [SerializeField] float slopeRaycastExtention = 0.8f;
    [SerializeField] float maxSlopeAngle = 60f;
    [SerializeField] float slopeHopCounterForce = 85f;
    [SerializeField] float slopeHopHitDistance = .1f;
    RaycastHit currentSlopeRaycastHit;
    Vector3 groundSlopeDirection;
    float groundSlopeAngle;
    float minSlopeHopSpeed = .5f;
    float maxSlopeHopSpeed = 100f;

    [Header("Stairs")] 
    [SerializeField] float maxStepHeight = .5f;
    [SerializeField] float stepRayLength = .6f;
    [SerializeField] int stepRaysAmount = 10;
    private int highestStepRay;
    private float stepRayDistance;

    // Movement Direction Variables
    Vector3 moveDirection;
    Vector3 slopeMoveDirection;
    
    // Required Components
    Rigidbody rigidbody;
    PlayerInput playerInput;

    #region Environment Detection

    private bool DetectStair()
    {
        highestStepRay = 0;
        
        for (int i = 0; i <= stepRaysAmount; i++)
        {
            Vector3 playerBottom = transform.position - (transform.up * (playerHeight / 2));
            Vector3 rayPos = playerBottom + (Vector3.up * (maxStepHeight * ((float) i / (float) stepRaysAmount)));

            RaycastHit hit;
            if (Physics.Raycast(rayPos,
                Vector3.Scale(rigidbody.velocity.normalized, new Vector3(1, 0, 1)),
                out hit,
                stepRayLength,
                groundMask
            ))
            {
                if (GetAngleFromNormal(hit.normal) > maxSlopeAngle)
                {
                    highestStepRay = i;
                    stepRayDistance = hit.distance;
                }
            }
        }

        if (highestStepRay == stepRaysAmount) highestStepRay = 0;
        if (highestStepRay != 0) return true;

        return false;
    }
    
    private bool CanWalkSlope()
    {
        if(groundSlopeAngle <= maxSlopeAngle)
        {
            return true;
        }
        return false;
    }

    private float GetAngleFromNormal(Vector3 normal)
    {
        return Vector3.Angle(normal, Vector3.up);
    }
    
    private bool DetectSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out currentSlopeRaycastHit, (playerHeight / 2) + slopeRaycastExtention, groundMask))
        {
            if(!Mathf.Approximately(Vector3.Dot(currentSlopeRaycastHit.normal, Vector3.up), 1f))
            {
                Vector3 slopeCross = Vector3.Cross(currentSlopeRaycastHit.normal, Vector3.down);
                groundSlopeDirection = Vector3.Cross(slopeCross, currentSlopeRaycastHit.normal);
                groundSlopeAngle = GetAngleFromNormal(currentSlopeRaycastHit.normal);
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    private bool DetectGroundCheckMatchesCollision()
    {
        foreach(Collider c in groundCheckCollidingWith)
        {
            foreach(Collider c2 in colliderColldingWith)
            {
                if(c == c2)
                {
                    return true;
                }
            }
        }
        return false;
    }

    #endregion

    #region Unity Messages

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }
    
    void Update()
    {
        CanWalkSlope();
        ProcessInput();
        SwitchDrag();

        if(playerInput.jumpInput && isGrounded && (DetectSlope() ? CanWalkSlope() : true))
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        groundCheckCollidingWith = Physics.OverlapSphere(transform.position - ((transform.up * playerHeight) / 2) + (groundCheckOffset * transform.up), groundCheckRadius, groundMask);
        isGrounded = groundCheckCollidingWith.Length > 0;
        
        Movement();
    }
    
    #endregion
    
    public void Movement()
    {
        Vector3 force;

        if(isGrounded && !DetectSlope())
        {
            force = (moveDirection.normalized * movementSpeed * movementMult);

            if(!DetectGroundCheckMatchesCollision())
            {
                force += -transform.up * groundMoveDownForce;
            }

            if (!Physics.Raycast(transform.position - (transform.up * playerHeight) / 2 + (moveDirection * slopeHopHitDistance) + (transform.up * slopeHopHitDistance), -currentSlopeRaycastHit.normal, slopeHopHitDistance + .1f, groundMask))
            {
                if (rigidbody.velocity.magnitude > minSlopeHopSpeed && rigidbody.velocity.magnitude < maxSlopeHopSpeed)
                {
                    force += -currentSlopeRaycastHit.normal * slopeHopCounterForce;
                }
            }

        }
        else if (isGrounded && DetectSlope() && CanWalkSlope())
        {
            force = (slopeMoveDirection.normalized * movementSpeed * movementMult);

            if (!DetectGroundCheckMatchesCollision())
            {
                force += -transform.up * groundMoveDownForce;
            }

            if(!Physics.Raycast(transform.position - (transform.up * playerHeight) / 2 + (slopeMoveDirection * slopeHopHitDistance) + (transform.up * slopeHopHitDistance), -currentSlopeRaycastHit.normal, slopeHopHitDistance + .2f, groundMask))
            {
                if(rigidbody.velocity.magnitude > minSlopeHopSpeed && rigidbody.velocity.magnitude < maxSlopeHopSpeed)
                {
                    force += -currentSlopeRaycastHit.normal * slopeHopCounterForce;
                }
            }
        }
        else
        {
            force = (moveDirection.normalized * movementSpeed * movementMult * airResistance + gravity);
        }

        rigidbody.AddForce(force, ForceMode.Acceleration);
    }

    void MoveUpSteps()
    {
        if (DetectStair() && isGrounded && !DetectSlope() && rigidbody.velocity.magnitude > .3f)
        {
            float forwardDistance = .01f + stepRayDistance;

            Vector3 forwardMovement =
                Vector3.Scale(rigidbody.velocity.normalized, new Vector3(1, 0, 1)) * forwardDistance;
            float percentageUp = ((float) (highestStepRay) / (float) stepRaysAmount);
            Vector3 upwardMovement = Vector3.up * (maxStepHeight * percentageUp);

            rigidbody.position += upwardMovement + forwardMovement;
        }
    }

    void ProcessInput()
    {
        moveDirection = (orientation.forward * playerInput.verticalInput) + (orientation.right * playerInput.horizontalInput);
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, currentSlopeRaycastHit.normal);
    }

    void SwitchDrag()
    {
        if (isGrounded)
        {
            rigidbody.drag = groundDrag;
        }
        else
        {
            rigidbody.drag = airDrag;
        }
    }

    void Jump()
    {
        rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    #region Unity Event Functions

    private void OnCollisionEnter(Collision collision)
    {
        colliderColldingWith.Add(collision.collider);
        
        MoveUpSteps();
    }

    private void OnCollisionExit(Collision collision)
    {
        if (colliderColldingWith.Contains(collision.collider)) colliderColldingWith.Remove(collision.collider);
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position - ((transform.up * playerHeight) / 2) + (groundCheckOffset * transform.up), groundCheckRadius);
        Gizmos.DrawRay(transform.position - (Vector3.up * playerHeight) / 2 + (moveDirection * slopeHopHitDistance) + (transform.up * slopeHopHitDistance), -currentSlopeRaycastHit.normal);
    }
}
