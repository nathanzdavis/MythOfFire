using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    public int waves;
    public int enemyCountWave1;
    public int enemyCountWave2;
    public int enemyCountWave3;

    private int origEnemyCountWave1;
    private int origEnemyCountWave2;
    private int origEnemyCountWave3;

    public float timeBetweenWaves1;

    public float timeBetweenEnemies1;
    public float timeBetweenEnemies2;
    public float timeBetweenEnemies3;

    public Transform[] spawns;

    public GameObject[] wave1EnemyTypes;
    public GameObject[] wave2EnemyTypes;
    public GameObject[] wave3EnemyTypes;

    public static WaveController instance;

    bool wave1complete;
    bool wave2complete;
    bool wave3complete;

    private int currentEnemyCounter = 0;

    void Start()
    {
        instance = this;

        Invoke("HandleWave1", timeBetweenWaves1);
        UIController.instance.wave.maxValue = (float)enemyCountWave1 / 100;

        origEnemyCountWave1 = enemyCountWave1;
        origEnemyCountWave2 = enemyCountWave2;
        origEnemyCountWave3 = enemyCountWave3;
    }

    private void HandleWave1()
    {
        StartCoroutine("Wave1Spawns");
    }

    private void HandleWave2()
    {
        StartCoroutine("Wave2Spawns");
    }

    private void HandleWave3()
    {
        StartCoroutine("Wave3Spawns");
    }

    private IEnumerator Wave1Spawns()
    {
        print("Spawninig wave 1 enemies");
        while (currentEnemyCounter < origEnemyCountWave1)
        {
            GameObject enemySpawn = Instantiate(wave1EnemyTypes[Random.Range(0, wave1EnemyTypes.Length)], spawns[Random.Range(0, spawns.Length)].position, Quaternion.identity);
            enemySpawn.GetComponent<MonsterController>().target = GameObject.FindGameObjectWithTag("Player");
            enemySpawn.GetComponent<MonsterController>().waveID = 1;
            enemySpawn.transform.Rotate(0, -90, 0);
            currentEnemyCounter++;
            yield return new WaitForSeconds(timeBetweenEnemies1);
        }
    }

    private IEnumerator Wave2Spawns()
    {
        print("Spawninig wave 2 enemies");
        while (currentEnemyCounter < origEnemyCountWave2)
        {
            GameObject enemySpawn = Instantiate(wave2EnemyTypes[Random.Range(0, wave1EnemyTypes.Length)], spawns[Random.Range(0, spawns.Length)].position, Quaternion.identity);
            enemySpawn.GetComponent<MonsterController>().target = GameObject.FindGameObjectWithTag("Player");
            enemySpawn.GetComponent<MonsterController>().waveID = 2;
            enemySpawn.transform.Rotate(0, -90, 0);
            currentEnemyCounter++;
            yield return new WaitForSeconds(timeBetweenEnemies2);
        }
    }

    private IEnumerator Wave3Spawns()
    {
        print("Spawninig wave 3 enemies");
        while (currentEnemyCounter < origEnemyCountWave3)
        {
            GameObject enemySpawn = Instantiate(wave3EnemyTypes[Random.Range(0, wave1EnemyTypes.Length)], spawns[Random.Range(0, spawns.Length)].position, Quaternion.identity);
            enemySpawn.GetComponent<MonsterController>().target = GameObject.FindGameObjectWithTag("Player");
            enemySpawn.GetComponent<MonsterController>().waveID = 3;
            enemySpawn.transform.Rotate(0, -90, 0);
            currentEnemyCounter++;
            yield return new WaitForSeconds(timeBetweenEnemies3);
        }
    }

    private void Update()
    {
        if (enemyCountWave1 > 0 && !wave1complete)
        {
            print("Wave 1 starting");
            UIController.instance.wave.value = (float)(origEnemyCountWave1 - enemyCountWave1) / 100;
        }
        else if (!wave1complete)
        {
            print("Wave 1 done");
            StopCoroutine("Wave1Spawns");
            UIController.instance.wave.value = 0;
            Invoke("HandleWave2", timeBetweenWaves1);
            UIController.instance.wave.maxValue = (float)enemyCountWave2 / 100;
            wave1complete = true;
            currentEnemyCounter = 0;
            UIController.instance.waveText.text = "2";
        }

        if (enemyCountWave2 > 0 && wave1complete && !wave2complete)
        {
            print("Wave 2 starting");
            UIController.instance.wave.value = (float)(origEnemyCountWave2 - enemyCountWave2) / 100;
        }
        else if (!wave2complete && wave1complete)
        {
            StopCoroutine("Wave2Spawns");
            print("Wave 2 done");
            UIController.instance.wave.value = 0;
            Invoke("HandleWave3", timeBetweenWaves1);
            UIController.instance.wave.maxValue = (float)enemyCountWave3 / 100;
            wave2complete = true;
            currentEnemyCounter = 0;
            UIController.instance.waveText.text = "3";
        }

        if (enemyCountWave3 > 0 && wave1complete && wave2complete && !wave3complete)
        {
            print("Wave 3 starting");
            UIController.instance.wave.value = (float)(origEnemyCountWave3 - enemyCountWave3) / 100;
        }
        else if (!wave3complete && wave1complete && wave2complete && enemyCountWave3 == 0)
        {
            StopCoroutine("Wave3Spawns");
            print("Wave 3 done");
            UIController.instance.wave.value = 0;
            wave3complete = true;
            UIController.instance.gameOverScreen.SetActive(true);
            OptionsController.instance.UnLockCursor();
        }
    }
}
