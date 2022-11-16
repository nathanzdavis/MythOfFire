using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CannonBall : MonoBehaviour
{
    public GameObject explosion;

    public AudioClip explosionNoise;

    public int damage;

    bool played;

    public ParticleSystem[] fire;


    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag != "Player" && col.gameObject.tag == "Boat")
        {
            if (!played)
            {
                GetComponent<MeshRenderer>().enabled = false;
                GetComponent<Rigidbody>().isKinematic = true;
                foreach(ParticleSystem ps in fire)
                {
                    ps.Stop();
                }
                GetComponentInChildren<Light>().enabled = false;
                GetComponent<CinemachineImpulseSource>().GenerateImpulse();
                Instantiate(explosion, transform.position, transform.rotation);
                GetComponent<AudioSource>().PlayOneShot(explosionNoise);
                played = true;
                Destroy(gameObject, 2f);
            }

        }
        else if (col.gameObject.tag == "Player")
        {
            if (!played)
            {
                GetComponent<MeshRenderer>().enabled = false;
                GetComponent<Rigidbody>().isKinematic = true;
                foreach (ParticleSystem ps in fire)
                {
                    ps.Stop();
                }
                GetComponentInChildren<Light>().enabled = false;
                GetComponent<CinemachineImpulseSource>().GenerateImpulse();
                col.GetComponent<Player>().Damage(damage);
                Instantiate(explosion, transform.position, transform.rotation);
                GetComponent<AudioSource>().PlayOneShot(explosionNoise);
                played = true;
                Destroy(gameObject, 2f);
            }
        }
    }
}
