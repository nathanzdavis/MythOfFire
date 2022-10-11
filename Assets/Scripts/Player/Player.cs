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
    [Header("Input Manager")]
    Input input;

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
    private float originalGravity;
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
    private bool airJuggleAttacking;
    private bool downwardsAttack;
    private bool parrying;

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
    public AudioClip[] swordParrys;
    public GameObject[] slashEffects;
    public GameObject[] slashEffectHits;

    [Header("Health")]
    public int health = 100;
    public bool regen;
    public float timeBeforeRegenAfterHit;
    public float regenSpeed;
    private bool dead;
    public GameObject[] deactivateOnDeath;

    [Header("Embers")]
    public int embers;
    public bool style1;
    public bool style2;
    private bool styleToggle;
    public GameObject[] fireEffects;
    public AudioClip emberActivate;

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
            if (isGrounded)
                HandleAttack();
            else if (!isGrounded)
                HandleAirAttack();
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
            }
            else
            {
                UIController.instance.emberSymbol.SetActive(false);
            }

            if (embers >= 100)
            {
                embers = 100;
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
        if (embers >= 10 && style2 && isGrounded && insideEnemyHitbox && currentEnemyOnUs && currentEnemyOnUs.GetComponent<MonsterController>().health > 0)
        {
            //Normal jump stuff
            anim.applyRootMotion = false;
            anim.SetTrigger("Jump");
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

        if (isGrounded)
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

            //UIController.instance.healthText.text = "50";
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
                    GetComponent<Rigidbody>().AddForce(transform.forward * dashAmount, ForceMode.Acceleration);
                    GameObject dashObj = Instantiate(dashEffect, transform.position, transform.rotation, transform);
                    dashObj.transform.localEulerAngles = new Vector3(0, -180, 0);
                    dashObj.transform.localPosition = new Vector3(0, 1, 0);
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

    //Guard
    private void HandleGuard()
    {
        if (insideEnemyHitbox && !parrying)
        {
            foreach (GameObject go in currentEnemiesAttackingUs)
            {
                if (go == null)
                {
                    currentEnemiesAttackingUs.Remove(go);
                }
                if (go && go.GetComponent<MonsterController>().attacking)
                {
                    anim.SetTrigger("Parry");
                    parrying = true;
                    embers += 5;
                    return;
                }
            }
        }
    }

    public void ParryEffects()
    {
        audioSource.PlayOneShot(swordParrys[Random.Range(0, swordParrys.Length)]);

        foreach(GameObject go in currentEnemiesAttackingUs)
        {
            go.GetComponent<Rigidbody>().velocity = Vector3.zero;
            go.GetComponent<Rigidbody>().AddForce(transform.forward * damageForce);
            go.GetComponent<MonsterController>().Parried();
        }

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
                UIController.instance.emberSymbol.SetActive(false);
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
        }
        styleToggle = !styleToggle;
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
        if (sword.GetComponent<SwordHitbox>().inTarget && sword.GetComponent<SwordHitbox>().currentTarget != null && sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<MonsterController>().health > 0)
        {
            if (!airJuggleAttacking)
            {
                audioSource.PlayOneShot(swordhits[Random.Range(0, swordhits.Length)]);
                impulseSword.GenerateImpulse(Camera.main.transform.forward);
                sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<MonsterController>().Damage(damage);
                if (sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<MonsterController>().health <= 0)
                {
                    embers += 15;
                }
                sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<Rigidbody>().AddForce(transform.forward * damageForce);
            }
            else if (!downwardsAttack)
            {
                //Not downwards air juggle attack
                audioSource.PlayOneShot(swordhits[Random.Range(0, swordhits.Length)]);
                impulseSword.GenerateImpulse(Camera.main.transform.forward);
                sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<MonsterController>().Damage(damage * 2);
                if (sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<MonsterController>().health <= 0)
                {
                    embers += 5;
                }
                sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<Rigidbody>().AddForce(transform.forward * damageForce);
                sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<Rigidbody>().AddForce(transform.up * damageForce / 2);
                sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<Animator>().SetTrigger("Die");
                airJuggleAttacking = false;
                embers -= 10;
            }
            else
            {
                //Downwards air juggle attack
                audioSource.PlayOneShot(swordhits[Random.Range(0, swordhits.Length)]);
                impulseSword.GenerateImpulse(Camera.main.transform.forward);
                sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<MonsterController>().Damage(damage * 2);
                sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<MonsterController>().thrown = true;
                if (sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<MonsterController>().health <= 0)
                {
                    embers += 5;
                }
                sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<Rigidbody>().AddForce(transform.forward * damageForce * 1.5f);
                sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<Rigidbody>().AddForce(Vector3.down * damageForce * 1.5f);
                sword.GetComponent<SwordHitbox>().currentTarget.GetComponent<Animator>().SetTrigger("Die");
                airJuggleAttacking = false;
                downwardsAttack = false;
                embers -= 10;
            }

        }
    }

    public void generateSlashEffect()
    {
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
        health -= damage;
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
}
