using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using Cinemachine;
using TMPro;


[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    [Header("Input Manager")]
    [HideInInspector] public Input input;

    [Header("Animation")]
    private Animator anim;

    [Header("Movement")]
    public Vector2 _move;
    public float inputMagnitude;
    bool movePressed;
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
    public float maxVelocity;
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
    public CinemachineImpulseSource impulseLand;
    public AudioClip jumpSound;
    public float gravity = -9.81f;
    private float originalGravity;
    public float hitAngleDown;
    private int jumpCounter;

    [Header("Wall Check")]
    [SerializeField] private LayerMask obstacleLayers;
    [Range(0f, 1f)] [SerializeField] private float rayObstacleLength = 0.1f;
    public bool m_hitWall;
    public Transform wallDetection;
    private RaycastHit hitInfo;
    public float hitAngle;
    public Vector3 hitNormal;

    [Header("Feet Impacts/Sound")]
    public CinemachineImpulseSource impulseFeet;
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
    public Transform textSpawn;
    public GameObject dashEffect;
    private bool airJuggleAttacking;
    private bool downwardsAttack;
    private bool parrying;
    public int groundSlamAttackCost;
    public CinemachineImpulseSource impulseSlash;
    public Vector3 hitboxRadius;
    public Vector3 playerHitRadius;
    private List<GameObject> parriedEnemies = new List<GameObject>();
    public float parryCooldown;
    public int groundSlamAttackDamage;
    public AudioClip groundSlamSound;
    public GameObject groundSlamEffect;
    public int airAttackCost;
    public int chargeAttackCost;
    public int emberGainKill;
    public int emberGainParry;
    private bool held;

    [Header("Slide")]
    public AudioClip slideSound;
    public float slideCooldown;
    private bool sliding;
    private bool canSlideAttack;
    public float canSlideAttackCooldown;

    [Header("AirControl")]
    public float airFriction;
    public float airControlForce;
    private float originalAirControlForce;

    [Header("Sword")]
    public GameObject sword;
    public int damage;
    public Cinemachine.CinemachineImpulseSource impulseSword;
    public AudioClip[] swordSlashes;
    public AudioClip[] swordhits;
    public AudioClip[] groundToAirSound;
    public AudioClip[] swordParrys;
    public GameObject[] slashEffects;
    public GameObject[] slashEffectHits;
    public GameObject fireSlash;

    [Header("Health")]
    public float maxHealth;
    public float health = 100;
    public bool regen;
    public float timeBeforeRegenAfterHit;
    public float regenSpeed;
    private bool dead;
    public GameObject[] deactivateOnDeath;
    bool fillingScreen;
    public AudioClip[] damagedSounds;

    [Header("Embers")]
    public float embers;
    private bool style1 = true;
    private bool style2;
    private bool styleToggle;
    public GameObject[] fireEffects;
    public AudioClip emberActivate;
    public AudioClip emberActivate2;
    public GameObject powerUpFire;
    public ParticleSystem[] powerUpFireParticles;
    public float timeBetweenEmberDissapation;
    public float numEmbersToRemovePerCycle;
    public float maxEmbers;
    public AudioClip fireRanOut;
    bool playedAudio;

    [Header("Debug")]
    public bool insideEnemyHitbox;
    public GameObject currentEnemyOnUs;
    public List<GameObject> currentEnemiesAttackingUs = new List<GameObject>();
    private bool slowMoToggle;

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
            if (!held)
            {
                if (isGrounded)
                    HandleAttack();
                else if (!isGrounded)
                    HandleAirAttack();
            }
            else if (held && style2)
            {
                HandleChargeRelease();
            }

        };

        //Special Attack input pressed
        input.Player.Special.performed += ctx =>
        {
            HandleSpecialAttack();
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

        //Style input pressed
        input.Player.SwitchStyle.performed += ctx =>
        {
            HandleStyleSwitch();
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

        input.Player.SlowMo.performed += ctx =>
        {
            HandleSlowMo();
        };
    }

    private void OnCharge(InputValue ia)
    {
        float val = ia.Get<float>();

        if (val >= .1f)
        {
            print("held");
            HandleChargeStart();
        }
        else
        {
            if (val <= .1f)
            {
                anim.SetBool("Charge", false);
            }
        }
    }

    private void Start()
    {
        //Initialize these on start
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        origGroundCheckDist = groundCheckDistance;
        chestOffsetOrig = spineBone.localEulerAngles.y;
        originalGravity = gravity;
        originalAirControlForce = airControlForce;
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

    private void Update()
    {
        if (!dead)
        {
            UIController.instance.health.value = (float)health / 100;
            UIController.instance.embers.value = (float)embers / 100;

            if (embers == 100 && !style2)
            {
                UIController.instance.emberSymbol.SetActive(true);
                UIController.instance.defaultSymbol.SetActive(false);
                playedAudio = false;
            }
            else if (style1 && embers != 100)
            {
                UIController.instance.emberSymbol.SetActive(false);
                UIController.instance.defaultSymbol.SetActive(true);
                if (!playedAudio)
                {
                    GetComponent<AudioSource>().PlayOneShot(fireRanOut);
                    playedAudio = true;
                }

            }

            if (health <= 25)
            {
                UIController.instance.StartCoroutine("fillScreen");
                UIController.instance.StopCoroutine("defillScreen");
            }

            if (health > 25)
            {
                UIController.instance.StartCoroutine("defillScreen");
                UIController.instance.StopCoroutine("fillScreen");
            }

            if (embers <= 0 && style2)
            {
                HandleStyleSwitch();
            }

            if (embers >= maxEmbers)
            {
                embers = maxEmbers;
            }

            if (health >= maxHealth)
            {
                health = maxHealth;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!dead)
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


        //Make sure we dont zip around from any glitches
        GetComponent<Rigidbody>().velocity = Vector3.ClampMagnitude(GetComponent<Rigidbody>().velocity, maxVelocity);
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
            //GetComponent<Rigidbody>().velocity = new Vector3(0, GetComponent<Rigidbody>().velocity.y, 0);
            anim.SetBool("Run", false);
            anim.SetFloat("Speed", 1);
            speedToggle = false;
        }

        if (_move.x == 0 || !isGrounded)
        {
            GetComponentInChildren<OffsetRotation>().offsetRotation = 0;
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
                jumpCounter = 0;
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
        if (isGrounded && jumpCounter < 2)
        {
            //Normal jump
            anim.applyRootMotion = false;
            anim.SetTrigger("Jump");
            generateCameraShakeJump();
            jumped = true;
            groundCheckDistance = .001f;
            Invoke(nameof(ResetGroundCheck), .1f);
            audioSource.PlayOneShot(jumpSound);
            GetComponent<Rigidbody>().AddForce(Vector3.up * jumpHeight);
            jumpCounter++;
        }
        else if (!isGrounded && jumpCounter == 1)
        {
            //Normal jump
            anim.applyRootMotion = false;
            anim.SetTrigger("DoubleJump");
            generateCameraShakeJump();
            jumped = true;
            groundCheckDistance = .001f;
            Invoke(nameof(ResetGroundCheck), .1f);
            audioSource.PlayOneShot(jumpSound);
            GetComponent<Rigidbody>().AddForce(Vector3.up * jumpHeight);
            jumpCounter++;
        }

    }

    private void ResetAllAirAttackStuff()
    {
        gravity = originalGravity;
        airControlForce = originalAirControlForce;

        airJuggleAttacking = false;
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
                    /*
                    if (hitAngle == 0)
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
                    */
                    GetComponent<Rigidbody>().AddForce(transform.forward * dashAmount, ForceMode.VelocityChange);
                    GetComponent<Rigidbody>().AddForce(-transform.up * 50, ForceMode.VelocityChange);
                    GameObject dashObj = Instantiate(dashEffect, transform.position, transform.rotation, transform);
                    dashObj.transform.localEulerAngles = new Vector3(0, -180, 0);
                    dashObj.transform.localPosition = new Vector3(0, 1, 0);
                }
                
                if (_move.y > .3f && style2)
                {
                    if (embers >= 10  && isGrounded && insideEnemyHitbox && currentEnemyOnUs && currentEnemyOnUs.GetComponent<MonsterController>().health > 0)
                    {
                        //Air attack stuff
                        anim.SetTrigger("AirAttackJump");
                    }
                }
                else
                {
                    if (style1)
                        anim.SetTrigger("Attack" + Random.Range(1, 3));
                    else if (style2)
                        anim.SetTrigger("Attack" + Random.Range(1, 6));
                }

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

    //Air Attack
    private void HandleAirAttack()
    {
        //Ground slam
        if (!isGrounded && !attacked && _move.y < 0)
        {
            anim.applyRootMotion = true;
            anim.SetTrigger("AirAttack");

            generateCameraShake();
            audioSource.PlayOneShot(attackSound);

            attacked = true;
            Invoke(nameof(AttackReset), cooldownBetweenAttacks);
            downwardsAttack = true;
        }
        else if (_move.y >= 0 && airJuggleAttacking)
        {
            anim.SetTrigger("Attack1");
            generateCameraShake();
            audioSource.PlayOneShot(attackSound);
            attacked = true;
            Invoke(nameof(AttackReset), cooldownBetweenAttacks);
        }
    }

    private void HandleSpecialAttack()
    {
        if (embers >= groundSlamAttackCost && style2)
        {
            if (isGrounded && !attacked)
            {
                anim.SetTrigger("GroundSlam");
                generateCameraShake();
                audioSource.PlayOneShot(attackSound);
                attacked = true;
                Invoke(nameof(AttackReset), cooldownBetweenAttacks);
            }
        }
    }

    private void HandleChargeStart()
    {
        anim.SetBool("Charge", true);
        held = true;
    }

    private void HandleChargeRelease()
    {
        GetComponent<Rigidbody>().AddForce(transform.forward * dashAmount * 5, ForceMode.VelocityChange);
        GetComponent<Rigidbody>().AddForce(-transform.up * 50, ForceMode.VelocityChange);
        anim.SetTrigger("ChargeAttack");
        Invoke("ResetCharge", .5f);
        held = false;
    }

    private void ResetCharge()
    {
        anim.SetBool("Charge", false);
    }

    //Guard
    private void HandleGuard()
    {
        var cols = Physics.OverlapBox(transform.position, playerHitRadius);
        var rigidbodies = new List<Rigidbody>();
        foreach (var col in cols)
        {
            if (col.attachedRigidbody != null && !rigidbodies.Contains(col.attachedRigidbody))
            {
                rigidbodies.Add(col.attachedRigidbody);
            }
        }
        foreach (var rb in rigidbodies)
        {
            if (!parrying)
            {
                if (rb.GetComponent<MonsterController>())
                {
                    if (rb.GetComponent<MonsterController>().attacking)
                    {
                        parriedEnemies.Add(rb.gameObject);
                        anim.SetTrigger("Parry");
                        parrying = true;
                        embers += emberGainParry;
                        Invoke("ParryCooldown", parryCooldown);
                        return;
                    }
                }
            }
        }
    }

    public void ParryEffects()
    {
        audioSource.PlayOneShot(swordParrys[Random.Range(0, swordParrys.Length)]);

        foreach(GameObject go in parriedEnemies.ToArray())
        {
            go.GetComponent<Rigidbody>().velocity = Vector3.zero;
            go.GetComponent<Rigidbody>().AddForce(transform.forward * damageForce);
            go.GetComponent<MonsterController>().Parried();
            parriedEnemies.Remove(go);
        }
    }

    private void ParryCooldown()
    {
        parrying = false;
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

    //Style Swap
    private void HandleStyleSwitch()
    {
        if (!styleToggle)
        {
            if (embers == 100)
            {
                style2 = true;
                style1 = false;

                foreach(GameObject go in fireEffects)
                {
                    go.SetActive(true);
                }

                GetComponent<AudioSource>().PlayOneShot(emberActivate);
                generateCameraShake();
                generateCameraShake();
                UIController.instance.emberFire.SetActive(true);
                //UIController.instance.emberSymbol.SetActive(false);
                //UIController.instance.defaultSymbol.SetActive(true);
                anim.SetTrigger("PowerUp");
                sword.GetComponent<AudioSource>().PlayOneShot(emberActivate2);
                powerUpFire.SetActive(true);
                foreach (ParticleSystem ps in powerUpFireParticles)
                {
                    ps.Play();
                }
                Invoke("StopPowerUpParticles", 1f);
                anim.SetBool("Style2", true);
                anim.SetBool("Style1", false);

                StartCoroutine("EmberDissapation");
            }
            else
            {
                return;
            }
        }
        else
        {
            style2 = false;
            style1 = true;

            foreach (GameObject go in fireEffects)
            {
                go.SetActive(false);
            }
            anim.SetBool("Style2", false);
            anim.SetBool("Style1", true);
            StopCoroutine("EmberDissapation");
            UIController.instance.emberFire.SetActive(false);
            GetComponent<AudioSource>().PlayOneShot(fireRanOut);
        }
        //styleToggle = !styleToggle;
    }

    private void StopPowerUpParticles()
    {
        foreach (ParticleSystem ps in powerUpFireParticles)
        {
            ps.Stop();
        }
        powerUpFire.SetActive(false);
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
        impulseFeet.GenerateImpulse();
    }

    public void generateCameraShakeLand()
    {
        impulseLand.GenerateImpulse();
    }

    public void generateCameraShakeJump()
    {
        impulseLand.GenerateImpulse();
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
        impulseSlash.GenerateImpulse();
    }

    public void generateSwordDamage()
    {
        var viableTargets = new List<GameObject>();
        var cols = Physics.OverlapBox(sword.transform.position, hitboxRadius);
        var rigidbodies = new List<Rigidbody>();
        foreach (var col in cols)
        {
            if (col.attachedRigidbody != null && !rigidbodies.Contains(col.attachedRigidbody))
            {
                rigidbodies.Add(col.attachedRigidbody);
            }
        }
        foreach (var rb in rigidbodies)
        {
            if (rb.transform.root.gameObject.GetComponent<MonsterController>())
            {
                if (rb.GetComponent<MonsterController>().health > 0)
                {
                    viableTargets.Add(rb.gameObject);
                    if (!airJuggleAttacking)
                    {
                        audioSource.PlayOneShot(swordhits[Random.Range(0, swordhits.Length)]);
                        impulseSword.GenerateImpulse();
                        rb.GetComponent<MonsterController>().Damage(damage);
                        if (rb.GetComponent<MonsterController>().health <= 0)
                        {
                            StartCoroutine(StartTimeDilation(.2f, .35f));
                            embers += emberGainKill;
                        }
                        rb.GetComponent<Rigidbody>().AddForce(transform.forward * damageForce);
                    }
                    else if (!downwardsAttack)
                    {
                        //Not downwards air juggle attack
                        audioSource.PlayOneShot(swordhits[Random.Range(0, swordhits.Length)]);
                        impulseSword.GenerateImpulse();
                        rb.GetComponent<MonsterController>().Damage(damage * 2);
                        if (rb.GetComponent<MonsterController>().health <= 0)
                        {
                            StartCoroutine(StartTimeDilation(.2f, .35f));
                        }
                        rb.GetComponent<Rigidbody>().AddForce(transform.forward * damageForce);
                        rb.GetComponent<Rigidbody>().AddForce(transform.up * damageForce / 2);
                        rb.GetComponent<Animator>().SetTrigger("Die");
                        airJuggleAttacking = false;
                        embers -= airAttackCost;
                    }
                    else
                    {
                        //Downwards air juggle attack
                        audioSource.PlayOneShot(swordhits[Random.Range(0, swordhits.Length)]);
                        impulseSword.GenerateImpulse();
                        rb.GetComponent<MonsterController>().Damage(damage * 2);
                        rb.GetComponent<MonsterController>().thrown = true;
                        if (rb.GetComponent<MonsterController>().health <= 0)
                        {
                            StartCoroutine(StartTimeDilation(.2f, .35f));
                        }
                        rb.GetComponent<Rigidbody>().AddForce(transform.forward * damageForce * 1.5f);
                        rb.GetComponent<Rigidbody>().AddForce(Vector3.down * damageForce * 1.5f);
                        rb.GetComponent<Animator>().SetTrigger("Die");
                        airJuggleAttacking = false;
                        downwardsAttack = false;
                        embers -= airAttackCost;
                    }
                }
            }      
        }
        if (viableTargets.Count <= 0)
        {
            sword.GetComponent<SwordHitbox>().inTarget = false;
        }
    }

    public void generateSlamSwordDamage()
    {
        var cols = Physics.OverlapBox(sword.transform.position, hitboxRadius * 3);
        var rigidbodies = new List<Rigidbody>();
        foreach (var col in cols)
        {
            if (col.attachedRigidbody != null && !rigidbodies.Contains(col.attachedRigidbody))
            {
                rigidbodies.Add(col.attachedRigidbody);
            }
        }
        foreach (var rb in rigidbodies)
        {
            if (rb.transform.root.gameObject.GetComponent<MonsterController>())
            {
                if (rb.GetComponent<MonsterController>().health > 0)
                {
                    audioSource.PlayOneShot(swordhits[Random.Range(0, swordhits.Length)]);
                    impulseSword.GenerateImpulse();
                    rb.GetComponent<MonsterController>().Damage(groundSlamAttackDamage);
                    if (rb.GetComponent<MonsterController>().health <= 0)
                    {
                        StartCoroutine(StartTimeDilation(.2f, .35f));
                    }
                    rb.GetComponent<Rigidbody>().AddForce(transform.forward * damageForce);
                }
            }
        }

        if (GameObject.FindGameObjectWithTag("Boss")){
            if (GameObject.FindGameObjectWithTag("Boss").GetComponent<TurtleBossController>().canTakeDamage)
            {
                GameObject.FindGameObjectWithTag("Boss").GetComponent<TurtleBossController>().Damage(20);
                if (GameObject.FindGameObjectWithTag("Boss").GetComponent<TurtleBossController>().health <= 0)
                {
                    StartCoroutine(StartTimeDilation(.2f, .35f));
                }
            }
        }



        sword.GetComponent<AudioSource>().PlayOneShot(groundSlamSound);
        Instantiate(groundSlamEffect, sword.transform.position, Quaternion.identity);
        embers -= groundSlamAttackCost;
    }

    public void generateSlashEffect()
    {
        //impulseFeet.GenerateImpulse();
        if (!sword.GetComponent<SwordHitbox>().inTarget)
        {
            GameObject slashObj = Instantiate(slashEffects[Random.Range(0, slashEffects.Length)], sword.transform.position, sword.transform.rotation);
            slashObj.transform.Rotate(-90, 0, 0);
        }
        else if (sword.GetComponent<SwordHitbox>().currentTarget != null && sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<MonsterController>().health > 0)
        {
            GameObject slashObj = Instantiate(slashEffectHits[Random.Range(0, slashEffectHits.Length)], sword.transform.position, sword.transform.rotation);
            slashObj.transform.Rotate(-90, 0, 0);
        }
    }

    public void generateSlashEffectFire()
    {
        impulseLand.GenerateImpulse();
        GameObject slashObj = Instantiate(fireSlash, sword.transform.position, sword.transform.rotation);
        slashObj.transform.Rotate(-90, 0, 0);
    }

    public void GenerateGroundToAirEffects()
    {
        StartCoroutine(StartTimeDilation(2f, .75f));
        impulseLand.GenerateImpulse();
        GameObject slashObj = Instantiate(fireSlash, sword.transform.position, sword.transform.rotation);
        slashObj.transform.Rotate(-90, 0, 0);
        GetComponent<AudioSource>().PlayOneShot(swordhits[Random.Range(0, swordhits.Length)]);


        //Normal jump first 
        anim.applyRootMotion = false;
        generateCameraShakeJump();
        jumped = true;
        groundCheckDistance = .001f;
        Invoke(nameof(ResetGroundCheck), .1f);
        audioSource.PlayOneShot(jumpSound);
        GetComponent<Rigidbody>().AddForce(Vector3.up * jumpHeight / 1.5f);

        //Air juggling/attack
        gravity = 0;
        currentEnemyOnUs.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpHeight / 1.5f);
        airControlForce = 0;
        airJuggleAttacking = true;

        Invoke("ResetAllAirAttackStuff", 2f);
        return;
    }

    public void generateSlashEffectFireNoise()
    {
        sword.GetComponent<AudioSource>().PlayOneShot(emberActivate2);
    }

    public void ChangeSpineForRun()
    {
        GetComponentInChildren<OffsetRotation>().offsetRotation = -25;
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
    private void HandleInAir()
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

    public void Damage(int damage)
    {
        StopCoroutine("Regen");
        CancelInvoke("StartRegen");
        anim.SetTrigger("GetHit");

        GameObject dmgPopup = Instantiate(damagePopup, textSpawn.position, Quaternion.identity);
        dmgPopup.GetComponentInChildren<TextMeshPro>().text = damage.ToString();
        sword.GetComponent<AudioSource>().PlayOneShot(swordhits[2]);

        health -= damage;

        impulseLand.GenerateImpulse();
        GetComponent<AudioSource>().PlayOneShot(damagedSounds[Random.Range(0, damagedSounds.Length)]);
        if (health <= 0)
        {
            UIController.instance.health.value = 0;
            Die();
            return;
        }

        if (regen)
        {
            Invoke("StartRegen", timeBeforeRegenAfterHit);  
        }
    }

    private void StartRegen()
    {
        StartCoroutine("Regen");
    }

    private IEnumerator Regen()
    {
        while (health <= 100)
        {
            health++;

            yield return new WaitForSeconds(regenSpeed);
        }
    }

    public void Die()
    {
        GetComponent<Animator>().SetTrigger("Die");
        dead = true;
        GetComponent<Animator>().applyRootMotion = false;
        GetComponent<IKFeet>().enabled = false;
        input.Player.Disable();
        UIController.instance.deathscreen.SetActive(true);

        foreach(GameObject go in deactivateOnDeath)
        {
            go.SetActive(false);
        }

        OptionsController.instance.UnLockCursor();
    }

    public void HandleSlowMo()
    {
        if (!slowMoToggle)
        {
            Time.timeScale = .5f;
        }
        else
        {
            Time.timeScale = 1;
        }
        slowMoToggle = !slowMoToggle;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Lantern" && this.gameObject.name == "Player")
        {
            health += other.gameObject.GetComponent<Lantern>().healthToRestore;
            GetComponent<AudioSource>().PlayOneShot(other.gameObject.GetComponent<Lantern>().lanternPickup);
            Instantiate(other.gameObject.GetComponent<Lantern>().pickupEffect, other.transform.position, other.transform.rotation);
            other.gameObject.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
            Destroy(other.gameObject);
        }
    }

    private IEnumerator StartTimeDilation(float delay, float amount)
    {
        Time.timeScale = amount;
        yield return new WaitForSeconds(delay);
        Time.timeScale = 1;
    }

    public IEnumerator EmberDissapation()
    {
        while (embers > 0)
        {
            embers -= numEmbersToRemovePerCycle;

            yield return new WaitForSeconds(timeBetweenEmberDissapation);
        }
    }
}
