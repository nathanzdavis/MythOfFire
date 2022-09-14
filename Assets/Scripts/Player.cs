using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    public Vector2 _move;
    public float inputMagnitude;
    bool movePressed;

    public Vector3 nextPosition;
    public float speed = 1f;

    Actions input;
    public float speedSmoothTime = 0.1f;
    private bool isGrounded;
    public float groundCheckDistance = 0.1f;
    public Vector3 groundNormal;
    bool jumped;
    public LayerMask groundLayer;
    public float jumpHeight;
    float origGroundCheckDist;
    public Vector3 velocity;
    public float magnitude;
    public Cinemachine.CinemachineImpulseSource impulseFeet;
    public Cinemachine.CinemachineImpulseSource impulseLand;

    [Space, Header("Check Wall Settings")]
    [SerializeField] private LayerMask obstacleLayers;
    [Range(0f, 1f)] [SerializeField] private float rayObstacleLength = 0.1f;
    public bool m_hitWall;
    public Transform wallDetection;
    private RaycastHit hitInfo;

    void Awake()
    {
        input = new Actions();

        input.Player.Move.performed += ctx =>
        {
            _move = ctx.ReadValue<Vector2>();
            inputMagnitude = ctx.ReadValue<Vector2>().magnitude;
            movePressed = _move.x != 0 || _move.y != 0;
        };

        input.Player.Jump.performed += ctx =>
        {
            HandleJump();
        };
    }

    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }

    private void Start()
    {
        origGroundCheckDist = groundCheckDistance;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        //HandleInAir();
        CheckGroundStatus();
        CheckIfWall();

        velocity = GetComponent<Rigidbody>().velocity;
        magnitude = velocity.normalized.magnitude;
    }
    
    protected virtual void CheckIfWall()
    {
        if (isGrounded)
        {
            bool _hitWall = false;
            _hitWall = Physics.Raycast(wallDetection.position + (transform.forward * .03f), transform.forward, rayObstacleLength, obstacleLayers);
            Debug.DrawRay(wallDetection.position + (transform.forward * .03f), transform.forward * rayObstacleLength, Color.blue);
            m_hitWall = _hitWall ? true : false;
        }
    }

    void HandleMovement()
    {
        if (!m_hitWall)
        {
            if(_move.x > 0){
                transform.Translate(Vector3.right * speed * Time.deltaTime);
            }
            if (_move.x < 0)
            {
                transform.Translate(Vector3.left * speed * Time.deltaTime);
            }
            if (_move.x == 0)
            {
                GetComponent<Rigidbody>().velocity = new Vector3(0, GetComponent<Rigidbody>().velocity.y, 0);
            }
        }
        else
        {
            GetComponent<Rigidbody>().velocity = new Vector3(0, GetComponent<Rigidbody>().velocity.y, 0);
        }
    }

    void HandleJump()
    {
        if (isGrounded)
        {
            generateCameraShakeLand();
            jumped = true;
            groundCheckDistance = .001f;
            Invoke(nameof(ResetGroundCheck), .1f);
            //Using own gravity variable incase different levels have different gravity requirements
            GetComponent<Rigidbody>().AddForce(Vector3.up * jumpHeight);
        }
    }

    /*
    void HandleInAir()
    {
        if (!isGrounded)
        {
            if (movePressed)
            {
                GetComponent<Rigidbody>().AddForce(transform.forward * airControlForce * Time.deltaTime);
                GetComponent<Rigidbody>().velocity = new Vector3 (GetComponent<Rigidbody>().velocity.x * (airFriction * Time.deltaTime), 
                    GetComponent<Rigidbody>().velocity.y, GetComponent<Rigidbody>().velocity.z * (airFriction * Time.deltaTime));
            }
        }
    }
    */

    void CheckGroundStatus()
    {
#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * groundCheckDistance));
#endif
        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, groundCheckDistance, groundLayer))
        {

            groundNormal = hitInfo.normal;
            isGrounded = true;
            if (jumped)
            {
                jumped = false;
                generateCameraShakeLand();
            }
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }
    }

    void ResetGroundCheck()
    {
        groundCheckDistance = origGroundCheckDist;
    }

    public void generateCameraShake()
    {
        impulseFeet.GenerateImpulse(Camera.main.transform.forward);
    }

    public void generateCameraShakeLand()
    {
        impulseLand.GenerateImpulse(Camera.main.transform.forward);
    }
}
