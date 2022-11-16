using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TurtleBossController : MonoBehaviour
{
    public float timeRangeStartSecs, timeRangeEndSecs;
    public float timeAboveWater;
    private Animator anims;
    public Animator turtleAnims;
    private bool played;
    public CannonShooter cannon;

    public GameObject waterSplash;
    public GameObject waterFalling;
    public Transform waterSpawn;

    public AudioClip waterSplashSound;
    public AudioClip landingSound;
    public AudioClip roar;

    public AudioSource splashSource;
    public AudioSource roarSource;

    public bool risen;
    public bool rising;
    public bool deactive;

    public Animator sailAnim;

    public GameObject shockwave;
    public Transform shockwaveSpawnL;
    public Transform shockwaveSpawnR;
    public int shockwaveDamage;
    public float stompCooldown;
    public float health;
    public bool canTakeDamage;

    public float cannonVelocity2;
    public float cannonRateOfFire2;
    public float cannonVelocity3;
    public float cannonRateOfFire3;

    void Awake()
    {
        anims = GetComponent<Animator>();
    }

    void Update()
    {
        if (!played && !deactive)
        {
            Invoke("Stomp", stompCooldown);
            played = true;
        }

        UIController.instance.bossSlider.value = health / 100;
    }

    public void AriseTurtle()
    {
        anims.SetTrigger("Arise");
        turtleAnims.SetTrigger("Arise");
        //Invoke("HideTurtle", timeAboveWater);
        roarSource.PlayOneShot(roar);
        splashSource.PlayOneShot(waterSplashSound);
        rising = true;
    }

    public void HideTurtle()
    {
        splashSource.PlayOneShot(waterSplashSound);
        anims.SetTrigger("Hide");
        //turtleAnims.SetTrigger("Hide");
        cannon.enabled = false;
        played = false;
        risen = false;
        canTakeDamage = false;
    }

    public void GenerateWaterSplash1()
    {
        GetComponent<CinemachineImpulseSource>().GenerateImpulse();
        Instantiate(waterSplash, waterSpawn.position, waterSpawn.rotation);
        Invoke("WaterFalling", 1f);
        splashSource.PlayOneShot(landingSound);
        rising = false;
        sailAnim.enabled = true;
        deactive = false;
        canTakeDamage = true;
    }

    public void GenerateWaterSplash2()
    {
        GetComponent<CinemachineImpulseSource>().GenerateImpulse();
        Instantiate(waterSplash, waterSpawn.position, waterSpawn.rotation);
    }

    private void WaterFalling()
    {
        Instantiate(waterFalling, waterSpawn.position, waterSpawn.rotation);
        cannon.enabled = true;
        risen = true;
    }

    private void Stomp()
    {
        turtleAnims.SetTrigger("Attack" + Random.Range(1, 3));
    }

    public void GenerateStompEffectsL()
    {
        if (health > 0)
        {
            Instantiate(shockwave, shockwaveSpawnL.position, shockwaveSpawnL.rotation);
            splashSource.PlayOneShot(landingSound);
            GetComponent<CinemachineImpulseSource>().GenerateImpulse();
            if (GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().isGrounded)
            {
                GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Damage(shockwaveDamage);
            }
            played = false;
        }
    }

    public void GenerateStompEffectsR()
    {
        if (health > 0)
        {
            Instantiate(shockwave, shockwaveSpawnR.position, shockwaveSpawnR.rotation);
            splashSource.PlayOneShot(landingSound);
            GetComponent<CinemachineImpulseSource>().GenerateImpulse();
            if (GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().isGrounded)
            {
                GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Damage(shockwaveDamage);
            }
            played = false;
        }
    }

    public void Damage(int damage)
    {
        health -= damage;
        turtleAnims.SetTrigger("Hit");
        roarSource.PlayOneShot(roar);
        //Invoke("HideTurtle", 4f);
        //deactive = true;
        //cannon.enabled = false;

        if (health <= 80 && health > 60)
        {
            cannon.attackCoolDown = cannonRateOfFire2;
            cannon.shootForce = cannonVelocity2;
            stompCooldown /= 1.25f;
        }

        if (health <= 60 && health > 40)
        {
            cannon.shootForce = cannonVelocity3;
        }

        if (health <= 40 && health > 20)
        {
            stompCooldown /= 1.25f;
            cannon.attackCoolDown = cannonRateOfFire3;
            cannon.shootForce = cannonVelocity3;
        }

        if (health <= 0)
        {
            turtleAnims.SetTrigger("Die");
            HideTurtle();
            deactive = true;
            cannon.enabled = false;
            foreach(GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                go.GetComponent<MonsterController>().Damage(100);
            }
            WaveController.instance.gameObject.SetActive(false);
            CancelInvoke();
            Invoke("Victory", 5f);
        }
    }

    private void Victory()
    {
        UIController.instance.gameOverScreen.SetActive(true);
        OptionsController.instance.UnLockCursor();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            go.GetComponent<MonsterController>().Damage(100);
        }
    }
}
