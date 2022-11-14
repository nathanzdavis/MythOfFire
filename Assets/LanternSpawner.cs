using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternSpawner : MonoBehaviour
{
    public float timeRangeStartSecs, timeRangeEndSecs;
    bool lanternActive;
    public Transform lanternSpawnpoint;
    public GameObject lantern;
    public GameObject activeLantern;
    public Transform target;

    private void Update()
    {
        if (activeLantern == null)
        {
            lanternActive = false;
        }

        if (!lanternActive)
        {
            Invoke("LanternSpawn", Random.Range(timeRangeStartSecs, timeRangeEndSecs));
            activeLantern = new GameObject();
            lanternActive = true;
        }
    }

    private void LanternSpawn()
    {
        activeLantern = Instantiate(lantern, lanternSpawnpoint.position, lanternSpawnpoint.rotation);
        activeLantern.GetComponent<SineMovement>().target = target;
    }
}
