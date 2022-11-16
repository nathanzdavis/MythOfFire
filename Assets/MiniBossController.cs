using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MiniBossController : MonoBehaviour
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
    void Start()
    {
        anims = GetComponent<Animator>();
    }

    void Update()
    {
        if (!played && !deactive)
        {
            Invoke("AriseTurtle", Random.Range(timeRangeStartSecs, timeRangeEndSecs));
            played = true;
        }
    }

    private void AriseTurtle()
    {
        if (!deactive)
        {
            anims.SetTrigger("Arise");
            turtleAnims.SetTrigger("Arise");
            Invoke("HideTurtle", timeAboveWater);
            roarSource.PlayOneShot(roar);
            splashSource.PlayOneShot(waterSplashSound);
            rising = true;
        }
    }

    public void HideTurtle()
    {
        splashSource.PlayOneShot(waterSplashSound);
        anims.SetTrigger("Hide");
        //turtleAnims.SetTrigger("Hide");
        cannon.enabled = false;
        played = false;
        risen = false;
    }

    public void GenerateWaterSplash1()
    {
        GetComponent<CinemachineImpulseSource>().GenerateImpulse();
        Instantiate(waterSplash, waterSpawn.position, waterSpawn.rotation);
        Invoke("WaterFalling", 1f);
        splashSource.PlayOneShot(landingSound);
        rising = false;
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
}
