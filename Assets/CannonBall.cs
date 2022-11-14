using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour
{
    public GameObject explosion;

    public AudioClip explosionNoise;

    public int damage;

    bool played;

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag != "Player")
        {
            if (!played)
            {
                GetComponent<MeshRenderer>().enabled = false;
                Instantiate(explosion, transform.position, transform.rotation);
                GetComponent<AudioSource>().PlayOneShot(explosionNoise);
                played = true;
                Destroy(gameObject, 1f);
            }

        }
        else if (col.gameObject.tag != "Player")
        {
            if (!played)
            {
                GetComponent<MeshRenderer>().enabled = false;
                col.transform.root.GetComponent<Player>().Damage(damage);
                Instantiate(explosion, transform.position, transform.rotation);
                GetComponent<AudioSource>().PlayOneShot(explosionNoise);
                played = true;
                Destroy(gameObject, 1f);
            }
        }
    }
}
