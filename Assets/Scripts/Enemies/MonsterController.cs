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
    private GameObject target;
    bool dying;
    bool withinAttackRange;
    public float swingDelay;
    public Transform healthBar;
    public Transform healthSlider;
    public float respawnTime;
    public int lives;
    public Text monsterLives;
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

    void Start()
    {
        for (int i = 0; i < rendererObjs.Length; i++)
        {
            foreach(Material m in rendererObjs[i].GetComponent<SkinnedMeshRenderer>().materials)
            {
                origColors[i].nestedColors.Add(m.GetColor("_Color"));
            }
        }
        

        if (useLives)
        monsterLives.text = lives.ToString();

        origDissolve = dissolveValue;
    }

    void Update()
    {
        velocity = ((transform.position - previous).magnitude) / Time.deltaTime;
        previous = transform.position;

        print(velocity);
        if (health > 0)
        {
            if (velocity > 2 || velocity < -2)
            {
                anim.SetBool("moving", true);
            }
            else
            {
                anim.SetBool("moving", false);
            }
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
            anim.SetTrigger("attack");
            canAttack = false;
            Invoke("dealDamage", swingDelay);
            Invoke("resetAttack", attackDelay);
            if (target.GetComponent<Player>().health <= 0)
            {
                aggro = false;
            }
            //anim.SetBool("moving", false);
        }

        if (waiting && !withinAttackRange)
        {
            anim.SetBool("moving", false);
        }

        if (aggro)
        {
            waiting = false;
        }
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
            target.GetComponent<Player>().Damage(damage);
    }

    void resetAttack()
    {
        canAttack = true;
    }

    void MoveToNextPoint()
    {
        Transform targetPoint = points[index];

        if (!aggro)
        {
            
            if (targetPoint.transform.position.x > transform.position.x)
                rigObj.transform.localEulerAngles = new Vector3(0, 180, 0);
            else
                rigObj.transform.localEulerAngles = new Vector3(0, 0, 0);
            
            //GetComponent<NavMeshAgent>().destination = targetPoint.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);
            if ((transform.position.x - targetPoint.position.x < 0.2f) && !reachedPoint)
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
            if (target.transform.position.x > transform.position.x)
                rigObj.transform.localEulerAngles = new Vector3(0, 180, 0);
            else
                rigObj.transform.localEulerAngles = new Vector3(0, 0, 0);

            //GetComponent<NavMeshAgent>().destination = target.transform.position;
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);

            //CancelInvoke("move");
            //Invoke("move", 1f);

        }
        else if (withinAttackRange)
        {
            //anim.SetBool("moving", false);
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
            col.transform.root.gameObject.GetComponent<Player>().currentEnemyOnUs = gameObject;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.transform.tag == "PlayerHitRange")
        {
            withinAttackRange = false;
            col.transform.root.gameObject.GetComponent<Player>().insideEnemyHitbox = false;
            col.transform.root.gameObject.GetComponent<Player>().currentEnemyOnUs = null;
        }

        if (col.transform.tag == "PlayerRange" && health > 0)
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
        anim.SetBool("Die", true);
        Invoke("Deactivate", .8f);
        if (useLives)
        {
            lives--;
            monsterLives.text = lives.ToString();
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
            yield return new WaitForSeconds(.05f);
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
        anim.SetBool("Die", false);
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
}