using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    [Header("Animation")]
    private Animator anim;

    [Header("Movement")]
    public Vector2 _move;
    public float inputMagnitude;
    bool movePressed;
    Actions input;
    public float speedSmoothTime = 0.1f;
    float movePressedTime = 0;
    public float turnTime;
    bool turning;
    float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;
    public Vector3 velocity;
    public float magnitude;
    bool turn2;

    [Header("Jump")]
    public Vector3 nextPosition;
    public float speed = 1f;
    public bool isGrounded;
    float origGroundCheckDist;
    public float groundCheckDistance = 0.1f;
    public Vector3 groundNormal;
    bool jumped;
    public LayerMask groundLayer;
    public float jumpHeight;
    public Cinemachine.CinemachineImpulseSource impulseLand;
    public AudioClip jumpSound;

    [Header("Wall Check")]
    [SerializeField] private LayerMask obstacleLayers;
    [Range(0f, 1f)] [SerializeField] private float rayObstacleLength = 0.1f;
    public bool m_hitWall;
    public Transform wallDetection;
    private RaycastHit hitInfo;

    [Header("Feet Impacts/Sound")]
    public Cinemachine.CinemachineImpulseSource impulseFeet;
    public Transform leftFoot;
    public Transform rightFoot;
    public GameObject dustParticle;
    public AudioClip[] footsteps;
    private AudioSource audioSource;

    private void Awake()
    {
        input = new Actions();

        //Move input pressed
        input.Player.Move.performed += ctx =>
        {
            _move = ctx.ReadValue<Vector2>();
            inputMagnitude = ctx.ReadValue<Vector2>().magnitude;
            movePressed = _move.x != 0 || _move.y != 0;
        };

        //Jump input pressed
        input.Player.Jump.performed += ctx =>
        {
            HandleJump();
        };
    }
    private void Start()
    {
        //Initialize these on start
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        origGroundCheckDist = groundCheckDistance;
    }

    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        //HandleInAir();
        CheckGroundStatus();
        CheckIfWall();

        //For debugging
        velocity = GetComponent<Rigidbody>().velocity;
        magnitude = velocity.normalized.magnitude;
    }

    //All movement update logic
    private void HandleMovement()
    {
        anim.SetBool("MovePressed", movePressed);
        if (_move.x > 0)
        {
            if (transform.localEulerAngles.y > 250 && !turning)
            {
                turn2 = false;
                //anim.SetTrigger("Turn");
                Invoke("Turn90", turnTime);
                Invoke("TurnGo", turnTime / 2);
                turning = true;
            }

            anim.SetBool("Run", true);

            movePressedTime += Time.deltaTime;
        }
        if (_move.x < 0)
        {

            if (transform.localEulerAngles.y < 100 && !turning)
            {
                turn2 = false;
                //anim.SetTrigger("Turn");
                Invoke("Turn270", turnTime);
                Invoke("TurnGo", turnTime / 2);
                turning = true;
            }

            anim.SetBool("Run", true);
            movePressedTime += Time.deltaTime;
        }
        if (_move.x == 0)
        {
            GetComponent<Rigidbody>().velocity = new Vector3(0, GetComponent<Rigidbody>().velocity.y, 0);
            anim.SetBool("Run", false);
        }

        if (m_hitWall)
        {
            GetComponent<Rigidbody>().velocity = new Vector3(0, GetComponent<Rigidbody>().velocity.y, 0);
            anim.SetBool("Run", false);
        }
        Turn(_move.normalized);
    }

    //Logic for turning
    private void Turn(Vector2 inputDir)
    {
        if (inputDir != Vector2.zero)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, 0) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime);
        }
    }

    //Wall check to flag if running into something
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

    //Grounded check to make sure we can't jump in the air
    private void CheckGroundStatus()
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
                anim.applyRootMotion = true;
                jumped = false;
                generateCameraShakeLand();
            }
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }

        anim.SetBool("InAir", !isGrounded);
    }

    //Add jump force
    private void HandleJump()
    {
        if (isGrounded)
        {
            anim.applyRootMotion = false;
            anim.SetTrigger("Jump");
            generateCameraShakeJump();
            jumped = true;
            groundCheckDistance = .001f;
            Invoke(nameof(ResetGroundCheck), .1f);
            audioSource.PlayOneShot(jumpSound);

            GetComponent<Rigidbody>().AddForce(Vector3.up * jumpHeight);

            //UIController.instance.healthText.text = "50";
        }
    }

    private void ResetGroundCheck()
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
        audioSource.PlayOneShot(footsteps[Random.Range(0, footsteps.Length)]);
    }

    public void generateCameraShakeJump()
    {
        impulseLand.GenerateImpulse(Camera.main.transform.forward);
    }

    public void generateDustParticleLeftFoot()
    {
        Instantiate(dustParticle, leftFoot.position, Quaternion.Euler(0, -90, 0));
    }

    public void generateDustParticleRightFoot()
    {
        Instantiate(dustParticle, rightFoot.position, Quaternion.Euler(0,-90,0));
    }

    public void generateRandomFootstepNoise()
    {
        audioSource.PlayOneShot(footsteps[Random.Range(0, footsteps.Length)]);
    }

    /*
     * Quick turn stuff, keep commented out for now
    private void TurnGo()
    {
        turn2 = true;
    }

    private void Turn90()
    {
        //transform.localEulerAngles = new Vector3(transform.localRotation.x, 90, transform.localRotation.z);
        turning = false;
        CancelInvoke();
    }

    private void Turn270()
    {
        //transform.localEulerAngles = new Vector3(transform.localRotation.x, 270, transform.localRotation.z);
        turning = false;
        CancelInvoke();
    }
    */


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
}
