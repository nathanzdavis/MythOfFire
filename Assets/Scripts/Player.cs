using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using TMPro;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    [Header("Animation")]
    private Animator anim;

    [Header("Movement")]
    public Vector2 _move;
    public float inputMagnitude;
    bool movePressed;
    Input input;
    public float speedSmoothTime = 0.1f;
    float movePressedTime = 0;
    public float turnTime;
    bool turning;
    float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;
    public Vector3 velocity;
    public float magnitude;
    private float prevMovX;
    private bool speedToggle;
    public float runMultiplier;
    //bool turn2;

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
    public float gravity = -9.81f;
    public float hitAngleDown;

    [Header("Wall Check")]
    [SerializeField] private LayerMask obstacleLayers;
    [Range(0f, 1f)] [SerializeField] private float rayObstacleLength = 0.1f;
    public bool m_hitWall;
    public Transform wallDetection;
    private RaycastHit hitInfo;
    public float hitAngle;
    public Vector3 hitNormal;

    [Header("Feet Impacts/Sound")]
    public Cinemachine.CinemachineImpulseSource impulseFeet;
    public Transform leftFoot;
    public Transform rightFoot;
    public GameObject dustParticle;
    public AudioClip[] footsteps;
    private AudioSource audioSource;

    [Header("Attack")]
    public AudioClip attackSound;
    public float cooldownBetweenAttacks;
    private bool attacked;
    private bool runAttack;
    public float runAttackChestOffset;
    private float chestOffsetOrig;
    public Transform spineBone;
    public float chestTurnSmoothTime = 0.1f;
    public float dashAmount;
    public float damageForce;
    public GameObject damagePopup;
    public GameObject dashEffect;

    [Header("Slide")]
    public AudioClip slideSound;
    public float slideCooldown;
    private bool sliding;
    private bool canSlideAttack;
    public float canSlideAttackCooldown;

    [Header("AirControl")]
    public float airFriction;
    public float airControlForce;

    [Header("Sword")]
    public GameObject sword;
    public int damage;
    public Cinemachine.CinemachineImpulseSource impulseSword;
    public AudioClip[] swordSlashes;
    public AudioClip[] swordhits;
    public AudioClip[] swordParrys;
    public GameObject[] slashEffects;
    public GameObject[] slashEffectHits;

    [Header("Debug")]
    public bool insideEnemyHitbox;
    public GameObject currentEnemyOnUs;

    private void Awake()
    {
        input = new Input();

        //Move input pressed
        input.Player.Move.performed += ctx =>
        {
            _move = ctx.ReadValue<Vector2>();
            inputMagnitude = ctx.ReadValue<Vector2>().magnitude;
            movePressed = _move.x != 0;
        };

        //Jump input pressed
        input.Player.Jump.performed += ctx =>
        {
            HandleJump();
        };

        //Attack input pressed
        input.Player.Attack.performed += ctx =>
        {
            HandleAttack();
        };

        //Guard input pressed
        input.Player.Guard.performed += ctx =>
        {
            HandleGuard();
        };

        //Attack input pressed
        input.Player.Slide.performed += ctx =>
        {
            HandleSlide();
        };

        //Attack input pressed
        input.Player.Run.performed += ctx =>
        {
            if (!speedToggle)
            {
                anim.SetFloat("Speed", runMultiplier);
            }
            else
            {
                anim.SetFloat("Speed", 1);
            }
            speedToggle = !speedToggle;
        };
    }
    private void Start()
    {
        //Initialize these on start
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        origGroundCheckDist = groundCheckDistance;
        chestOffsetOrig = spineBone.localEulerAngles.y;

    }

    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }

    private void LateUpdate()
    {
        //TurnIKChest();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleInAir();
        CheckGroundStatus();
        CheckIfWall();

        //For debugging
        velocity = GetComponent<Rigidbody>().velocity;
        magnitude = velocity.normalized.magnitude;

        //Gravity
        GetComponent<Rigidbody>().AddForce(Vector3.down * gravity);
    }

    //All movement update logic
    private void HandleMovement()
    {
        anim.SetBool("MovePressed", movePressed);
        if (_move.x > 0)
        {
            if (transform.localEulerAngles.y > 250 && !turning)
            {
                //turn2 = false;
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
                //turn2 = false;
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
            anim.SetFloat("Speed", 1);
            speedToggle = false;
        }

        if (m_hitWall)
        {
            GetComponent<Rigidbody>().velocity = new Vector3(0, GetComponent<Rigidbody>().velocity.y, 0);
            anim.SetBool("Run", false);
        }

        if (_move.x != 0)
        {
            prevMovX = _move.x;
        }

        Turn(prevMovX);
    }

    //Logic for turning
    private void Turn(float x)
    {
        if (x < 0)
        {
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, -90, ref turnSmoothVelocity, turnSmoothTime);
        }
        if (x > 0)
        {
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, 90, ref turnSmoothVelocity, turnSmoothTime);
        }

        /*
        if (inputDir != Vector2.zero)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, 0) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
        }
        */
    }

    /*
    private void TurnIKChest()
    {
        if (runAttack)
        {
            spineBone.localEulerAngles = Vector3.up * Mathf.SmoothDampAngle(spineBone.localEulerAngles.y, runAttackChestOffset, ref chestTurnSmoothVelocity, chestTurnSmoothTime);
        }
        if (!runAttack)
        {
            spineBone.localEulerAngles = Vector3.up * Mathf.SmoothDampAngle(spineBone.localEulerAngles.y, chestOffsetOrig, ref chestTurnSmoothVelocity, chestTurnSmoothTime);
        }
    }
    */

    //Wall check to flag if running into something
    protected virtual void CheckIfWall()
    {
        bool _hitWall = false;
        RaycastHit hitobject;
        _hitWall = Physics.Raycast(wallDetection.position + (transform.forward * .03f), transform.forward, out hitobject, rayObstacleLength, obstacleLayers);
        Debug.DrawRay(wallDetection.position + (transform.forward * .03f), transform.forward * rayObstacleLength, Color.blue);
        hitNormal = hitobject.normal;
        hitAngle = Vector3.Angle(transform.forward, hitobject.normal) - 90;
        if (hitAngle > 60)
        {
            m_hitWall = _hitWall ? true : false;
        }
        else
        {
            m_hitWall = false;
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
            hitAngleDown = Vector3.Angle(transform.forward, hitInfo.normal) - 90;
            groundNormal = hitInfo.normal;

            if (!isGrounded)
            {
                generateCameraShakeLand();
                audioSource.PlayOneShot(footsteps[Random.Range(0, footsteps.Length)]);
            }

            isGrounded = true;

            if (jumped)
            {
                anim.applyRootMotion = true;
                jumped = false;
            }
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }

        anim.SetBool("InAir", !isGrounded);
        if (!isGrounded)
        {
            GetComponent<IKFeet>().raycastDownDistance = 0f;
        }
        else
        {
            GetComponent<IKFeet>().raycastDownDistance = GetComponent<IKFeet>().origDownDistance;
        }
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

    //Attack
    private void HandleAttack()
    {
        if (isGrounded && !attacked)
        {
            if (movePressed)
            {
                runAttack = true;
            }

            if (!canSlideAttack)
            {
                if (!sword.GetComponent<SwordHitbox>().inTarget)
                {
                    if (hitAngleDown != 0)
                    {
                        Vector3 dir;
                        if (transform.forward.x < 0)
                        {
                            dir = Quaternion.AngleAxis(hitAngleDown, transform.forward) * transform.forward;
                        }
                        else
                        {
                            dir = Quaternion.AngleAxis(hitAngleDown + 180, transform.forward) * transform.right;
                        }
                        GetComponent<Rigidbody>().AddForce(dir * dashAmount, ForceMode.Acceleration);
                        GameObject dashObj = Instantiate(dashEffect, transform.localPosition, transform.localRotation, transform);
                    }
                    else
                    {
                        GetComponent<Rigidbody>().AddForce(transform.forward * dashAmount, ForceMode.Acceleration);
                        GameObject dashObj = Instantiate(dashEffect, transform.position, transform.rotation, transform);
                        dashObj.transform.localEulerAngles = new Vector3(0, -180, 0);
                        dashObj.transform.localPosition = new Vector3(0, 1, 0);
                    }
                }

                anim.SetTrigger("Attack" + Random.Range(1, 3));
            }
            else if (canSlideAttack)
                anim.SetTrigger("AttackSlide");

            generateCameraShake();
            audioSource.PlayOneShot(attackSound);
            //UIController.instance.healthText.text = "50";

            attacked = true;
            Invoke(nameof(AttackReset), cooldownBetweenAttacks);
        }
    }

    //Guard
    private void HandleGuard()
    {
        if (insideEnemyHitbox)
        {
            anim.SetTrigger("Parry");
        }
    }

    public void ParryEffects()
    {
        audioSource.PlayOneShot(swordParrys[Random.Range(0, swordParrys.Length)]);
        currentEnemyOnUs.GetComponent<Rigidbody>().velocity = Vector3.zero;
        currentEnemyOnUs.GetComponent<Rigidbody>().AddForce(transform.forward * damageForce);
    }

    /*
    private IEnumerator Dash()
    {
        anim.applyRootMotion = false;
        //canDash = false;
        //isDashing = true;
        //float originalGravity = gravity;
        //gravity = 0f;
        GetComponent<Rigidbody>().velocity = transform.forward * dashingPower;
        yield return new WaitForSeconds(dashingTime);
        //tr.emitting = false;
        //gravity = originalGravity;
        //isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        anim.applyRootMotion = true;
        //canDash = true;
    }
    */

    private void AttackReset()
    {
        attacked = false;
        runAttack = false;
    }

    //Slide
    private void HandleSlide()
    {
        if (isGrounded && movePressed && !sliding)
        {
            canSlideAttack = true;
            sliding = true;
            Invoke(nameof(ResetSlide), slideCooldown);
            Invoke(nameof(ResetSlideAttack), canSlideAttackCooldown);
            anim.SetTrigger("Slide");
            //generateCameraShakeJump();
            audioSource.PlayOneShot(slideSound);
            //UIController.instance.healthText.text = "50";
        }
    }

    private void ResetSlide()
    {
        sliding = false;
    }

    private void ResetSlideAttack()
    {
        canSlideAttack = false;
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
        Instantiate(dustParticle, rightFoot.position, Quaternion.Euler(0, -90, 0));
    }

    public void generateRandomFootstepNoise()
    {
        audioSource.PlayOneShot(footsteps[Random.Range(0, footsteps.Length)]);
    }

    public void generateSwordEffects()
    {
        audioSource.PlayOneShot(swordSlashes[Random.Range(0, swordSlashes.Length)]);
        impulseFeet.GenerateImpulse(Camera.main.transform.forward);
    }

    public void generateSwordDamage()
    {
        if (sword.GetComponent<SwordHitbox>().inTarget)
        {
            audioSource.PlayOneShot(swordhits[Random.Range(0, swordhits.Length)]);
            impulseSword.GenerateImpulse(Camera.main.transform.forward);
            sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<Enemy>().Damage(damage);
            sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<Rigidbody>().velocity = Vector3.zero;
            sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<Rigidbody>().AddForce(transform.forward * damageForce);
            GameObject dmgPopup = Instantiate(damagePopup, sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<Enemy>().textSpawn.position, Quaternion.identity);
            dmgPopup.GetComponentInChildren<TextMeshPro>().text = damage.ToString();
        }
    }

    public void generateSlashEffect()
    {
        if (!sword.GetComponent<SwordHitbox>().inTarget)
        {
            GameObject slashObj = Instantiate(slashEffects[Random.Range(0, slashEffects.Length)], sword.transform.position, sword.transform.rotation);
            slashObj.transform.Rotate(-90, 0, 0);
        }
        else
        {
            GameObject slashObj = Instantiate(slashEffectHits[Random.Range(0, slashEffectHits.Length)], sword.transform.position, sword.transform.rotation);
            slashObj.transform.Rotate(-90, 0, 0);
        }
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
    void HandleInAir()
    {
        if (!isGrounded)
        {
            if (movePressed && !m_hitWall)
            {
                GetComponent<Rigidbody>().AddForce(transform.forward * airControlForce * Time.deltaTime);
                GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x * (airFriction * Time.deltaTime),
                    GetComponent<Rigidbody>().velocity.y, GetComponent<Rigidbody>().velocity.z * (airFriction * Time.deltaTime));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "EnemyHitbox")
        {
            currentEnemyOnUs = other.transform.root.gameObject;
            insideEnemyHitbox = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "EnemyHitbox")
        {
            currentEnemyOnUs = null;
            insideEnemyHitbox = false;
        }
    }
}
