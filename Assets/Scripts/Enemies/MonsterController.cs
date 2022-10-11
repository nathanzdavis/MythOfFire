//Nathan Davis and Will Souers

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;

[System.Serializable]
public class serializableClass
{
    public List<Color> nestedColors;
}

public class MonsterController : MonoBehaviour
{
    public Animator anim;
    public Transform[] points;
    int index = 0;
    int indexModifier;
    public float speed;
    public float idleTime;
    bool reachedPoint;
    public int damage = 15;
    public int health = 100;
    bool canAttack = true;
    public float attackDelay;
    bool aggro;
    public GameObject target;
    bool dying;
    bool withinAttackRange;
    public float swingDelay;
    public bool attacking;
    public Transform healthBar;
    public Transform healthSlider;
    public float respawnTime;
    public int lives;
    public bool useLives;
    public Transform spawnpoint;
    public Transform textSpawn;
    public GameObject[] rendererObjs;
    public SkinnedMeshRenderer[] dissolveObjs;
    public GameObject rigObj;
    float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;
    bool toggle;
    Vector3 previous;
    float velocity;
    public GameObject damagePopup;
    public Collider[] colliders;
    public Color hitColor;
    public float dissolveValue = 1;
    private float origDissolve;
    //public List<List<Color>> origColors;
    public List<serializableClass> origColors = new List<serializableClass>();
    public GameObject particles;
    bool waiting;

    public bool isGrounded;
    public float groundCheckDistance = 0.1f;
    public Vector3 groundNormal;
    public LayerMask groundLayer;

    public float explosionRadius;
    public float explosionForce;
    public int explosionDamage;
    public AudioClip explosionSound;
    public GameObject explosion;
    public GameObject warnings;

    public bool patrol;
    public bool thrown;
    public int waveID;


    void Start()
    {
        for (int i = 0; i < rendererObjs.Length; i++)
        {
            foreach(Material m in rendererObjs[i].GetComponent<SkinnedMeshRenderer>().materials)
            {
                origColors[i].nestedColors.Add(m.GetColor("_Color"));
            }
        }
       

        origDissolve = dissolveValue;
    }

    private void FixedUpdate()
    {
        CheckGroundStatus();
    }

    void Update()
    {
        velocity = ((transform.position - previous).magnitude) / Time.deltaTime;
        previous = transform.position;

        if (health > 0)
        {

            if (aggro && withinAttackRange)
            {
                waiting = false;
                anim.SetBool("moving", false);
            }

            if (aggro && !withinAttackRange)
            {
                waiting = false;
                anim.SetBool("moving", true);
            }

            if (!aggro && !withinAttackRange && !waiting)
            {
                anim.SetBool("moving", true);
            }

            if (!aggro && !withinAttackRange && waiting)
            {
                anim.SetBool("moving", false);
            }

            /*
            if (velocity > 3f || velocity < -3f)
            {
                anim.SetBool("moving", true);
            }
            else
            {
                anim.SetBool("moving", false);
            }
            */

            MoveToNextPoint();
            healthSlider.localScale = new Vector3 (healthSlider.localScale.x, healthSlider.localScale.y, (float)health / 100);
        }
        else
        {
            healthSlider.localScale = new Vector3(healthSlider.localScale.x, healthSlider.localScale.y, 0);
            aggro = false;
            canAttack = false;
        }


        if (aggro && canAttack && withinAttackRange && health > 0)
        {
            if (!target.GetComponent<Player>().currentEnemiesAttackingUs.Contains(gameObject))
                target.GetComponent<Player>().currentEnemiesAttackingUs.Add(gameObject);

            attacking = true;
            warnings.SetActive(true);
            anim.SetTrigger("attack");
            canAttack = false;
            Invoke("dealDamage", swingDelay);
            Invoke("resetAttack", attackDelay);
            if (target.GetComponent<Player>().health <= 0)
            {
                aggro = false;
                withinAttackRange = false;
                attacking = false;
                canAttack = false;
                CancelInvoke();
            }
        }
    }

    private void LateUpdate()
    {
        anim.SetBool("Alive", health > 0);
    }

    //Logic for turning
    private void Turn()
    {
        if (!toggle)
        {
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, -90, ref turnSmoothVelocity, turnSmoothTime);

        }
        else
        {
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, 90, ref turnSmoothVelocity, turnSmoothTime);
        }
        toggle = !toggle;
    }

    void dealDamage()
    {
        if (withinAttackRange)
        {
            target.GetComponent<Player>().Damage(damage);
            attacking = false;
        }
        else
        {
            if (target)
            {
                if (attacking && Mathf.Abs(transform.position.x - target.transform.position.x) <= 5)
                {
                    target.GetComponent<Player>().Damage(damage);
                    attacking = false;
                }
            }
        }

        warnings.SetActive(false);
    }

    public void Parried()
    {
        warnings.SetActive(false);
        attacking = false;
        CancelInvoke();
        GetComponent<Animator>().SetTrigger("Hit");
        Invoke("resetAttack", attackDelay);
    }

    void resetAttack()
    {
        canAttack = true;
        warnings.SetActive(false);
    }

    void MoveToNextPoint()
    {
        if (isGrounded)
        {
            Transform targetPoint = points[index];

            if (!aggro && patrol)
            {

                if (targetPoint.transform.position.x > transform.position.x)
                    rigObj.transform.localEulerAngles = new Vector3(0, 180, 0);
                else
                    rigObj.transform.localEulerAngles = new Vector3(0, 0, 0);

                //GetComponent<NavMeshAgent>().destination = targetPoint.position;
                transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);
                if ((Mathf.Abs(transform.position.x - targetPoint.position.x) < 0.1f) && !reachedPoint)
                {
                    waiting = true;
                    Invoke("switchPoint", idleTime);
                    reachedPoint = true;
                    //anim.SetBool("moving", false);
                }
                //print("Distance from next waypoint: " + Vector3.Distance(transform.position, targetPoint.position));
            }
            else if (!withinAttackRange)
            {
                //GetComponent<NavMeshAgent>().destination = target.transform.position;
                if (target)
                    transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);

                //CancelInvoke("move");
                //Invoke("move", 1f);
            }

            if (target)
            {
                if (target.transform.position.x > transform.position.x)
                    rigObj.transform.localEulerAngles = new Vector3(0, 180, 0);
                else
                    rigObj.transform.localEulerAngles = new Vector3(0, 0, 0);
            }else if (!patrol)
            {
                anim.SetBool("moving", false);
            }

        } 
    }

    void switchPoint()
    {
        if (index == points.Length - 1)
            indexModifier = -1;
        if (index == 0)
            indexModifier = 1;
        index += indexModifier;
        reachedPoint = false;
        waiting = false;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.transform.tag == "PlayerRange" && health > 0)
        {
            aggro = true;
            target = col.transform.root.gameObject;
            healthBar.gameObject.SetActive(true);
        }

        if (col.transform.tag == "PlayerHitRange")
        {
            aggro = true;
            target = col.transform.root.gameObject;
            withinAttackRange = true;
            col.transform.root.gameObject.GetComponent<Player>().insideEnemyHitbox = true;
            if (!col.transform.root.gameObject.GetComponent<Player>().currentEnemyOnUs)
                col.transform.root.gameObject.GetComponent<Player>().currentEnemyOnUs = gameObject;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.transform.tag == "PlayerHitRange")
        {
            withinAttackRange = false;
            col.transform.root.gameObject.GetComponent<Player>().insideEnemyHitbox = false;
            if (col.transform.root.gameObject.GetComponent<Player>().currentEnemyOnUs == gameObject)
                col.transform.root.gameObject.GetComponent<Player>().currentEnemyOnUs = null;
            if (target.GetComponent<Player>().currentEnemiesAttackingUs.Contains(gameObject))
                target.GetComponent<Player>().currentEnemiesAttackingUs.Remove(gameObject);
        }

        if (patrol && col.transform.tag == "PlayerRange" && health > 0)
        {
            aggro = false;
            healthBar.gameObject.SetActive(false);
            target = null;
        }
    }

    public void Damage(int damage)
    {
        if (health > 0)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GameObject dmgPopup = Instantiate(damagePopup, textSpawn.position, Quaternion.identity);
            dmgPopup.GetComponentInChildren<TextMeshPro>().text = damage.ToString();

            health -= damage;
            for (int i = 0; i < rendererObjs.Length; i++)
            {
                foreach(Material m in rendererObjs[i].GetComponent<SkinnedMeshRenderer>().materials)
                {
                    m.SetColor("_Color", hitColor);
                }
            }

            Invoke("resetColor", .1f);
            if (health <= 0 && !dying)
            {
                Die();
                return;
            }
            anim.SetTrigger("Hit");
        }
    }

    void resetColor()
    {
        for (int i = 0; i < rendererObjs.Length; i++)
        {
            for(int j = 0; j < rendererObjs[i].GetComponent<SkinnedMeshRenderer>().materials.Length; j++)
            {
                rendererObjs[i].GetComponent<SkinnedMeshRenderer>().materials[j].SetColor("_Color", origColors[i].nestedColors[j]);
            }
        }
    }

    public void Die()
    {
        dying = true;
        anim.SetTrigger("Die");
        Invoke("Deactivate", .8f);
        if (useLives)
        {
            lives--;
        }

        if (target.GetComponent<Player>().currentEnemyOnUs == gameObject)
        {
            target.GetComponent<Player>().insideEnemyHitbox = false;
            target.GetComponent<Player>().currentEnemyOnUs = null;
        }
        particles.SetActive(false);
        StartCoroutine("Dissolve");
        //GetComponent<NavMeshAgent>().isStopped = true;
        healthBar.gameObject.SetActive(false);
        
        if (waveID == 1)
        {
            WaveController.instance.enemyCountWave1--;
        }

        if (waveID == 2)
        {
            WaveController.instance.enemyCountWave2--;
        }

        if (waveID == 3)
        {
            WaveController.instance.enemyCountWave3--;
        }

        Destroy(gameObject, 5);
    }

    private IEnumerator Dissolve()
    {
        while (dissolveValue >= -2)
        {
            foreach(SkinnedMeshRenderer smr in dissolveObjs)
            {
                foreach (Material m in smr.materials)
                {
                    dissolveValue -= .1f;
                    m.SetFloat("_CutoffHeight", dissolveValue);
                }
            }
            yield return new WaitForSeconds(.1f);
        }
    }

    void Deactivate()
    {
        //rendererObj.GetComponent<SkinnedMeshRenderer>().enabled = false;
        if (lives != 0 && useLives)
        {
            Invoke("Respawn", respawnTime);
        }
        
        if (!useLives)
        {
            Invoke("Respawn", respawnTime);
        }
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<CapsuleCollider>().enabled = false;
    }

    void Respawn()
    {
        if (target != null)
            healthBar.gameObject.SetActive(true);
        canAttack = true;
        //rendererObj.GetComponent<SkinnedMeshRenderer>().enabled = true;
        GetComponent<CapsuleCollider>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
        dying = false;
        health = 100;
        transform.position = spawnpoint.position;


        foreach (Collider c in colliders)
        {
            c.enabled = true;
        }

        dissolveValue = origDissolve;

        foreach (SkinnedMeshRenderer smr in dissolveObjs)
        {
            foreach (Material m in smr.materials)
            {
                m.SetFloat("_CutoffHeight", dissolveValue);
            }
        }

        //GetComponent<NavMeshAgent>().isStopped = false;
        particles.SetActive(true);
    }

    private void CheckGroundStatus()
    {
#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * groundCheckDistance));
#endif
        // 0.1f is a small offset to start the ray from inside the character
        // it is also good to note that the transform position in the sample assets is at the base of the character
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, groundCheckDistance, groundLayer))
        {
            if (!isGrounded && health <= 0 && thrown)
            {
                StartCoroutine(HandleExplosion());
            }
            isGrounded = true;
            thrown = false;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }

        anim.SetBool("InAir", !isGrounded);
    }

    private IEnumerator HandleExplosion()
    {
        GameObject explosionSpawn = Instantiate(explosion, transform.position, transform.rotation);
        GetComponent<AudioSource>().PlayOneShot(explosionSound);

        float r = explosionRadius;
        var cols = Physics.OverlapSphere(transform.position, r);
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
                rb.AddExplosionForce(explosionForce, transform.position, r, explosionForce / 2, ForceMode.Impulse);
                rb.transform.root.gameObject.GetComponent<MonsterController>().Damage(explosionDamage);

                yield return new WaitForSeconds(.1f);
                rb.transform.root.gameObject.GetComponent<MonsterController>().anim.SetTrigger("Die");
            }
        }
    }
}