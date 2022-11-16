using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class WaveController : MonoBehaviour
{
    public int waves;
    public int enemyCountWave1;
    public int enemyCountWave2;
    public int enemyCountWave3;
    public int enemyCountWave4;

    private int origEnemyCountWave1;
    private int origEnemyCountWave2;
    private int origEnemyCountWave3;
    private int origEnemyCountWave4;
    public float timeBetweenWaves1;

    public float timeBetweenEnemies1;
    public float timeBetweenEnemies2;
    public float timeBetweenEnemies3;
    public float timeBetweenEnemies4;
    public Transform[] spawns;

    public GameObject[] wave1EnemyTypes;
    public GameObject[] wave2EnemyTypes;
    public GameObject[] wave3EnemyTypes;

    public static WaveController instance;

    bool wave1complete;
    bool wave2complete;
    bool wave3complete;

    private int currentEnemyCounter = 0;


    public float bossFOV;
    public CinemachineVirtualCamera mainCam;

    public MiniBossController turtleMiniBoss;
    public float timeBeforeBossFight;
    public TurtleBossController Boss;
    public float valueToZoomBy;
    public float valueToRotateBy;
    public float targetCameraXRotation = -2f;
    private float negX;

    public AudioClip bossMusic;
    public AudioSource bossMusicsource;

    public float ttp1time;
    public float ttp2time;
    public float ttp3time;
    public float ttp4time;

    void Start()
    {
        instance = this;

        Invoke("HandleWave1", timeBetweenWaves1);
        UIController.instance.wave.maxValue = (float)enemyCountWave1 / 100;

        origEnemyCountWave1 = enemyCountWave1;
        origEnemyCountWave2 = enemyCountWave2;
        origEnemyCountWave3 = enemyCountWave3;
        origEnemyCountWave4 = enemyCountWave4;
        Invoke("WaveUI", timeBetweenWaves1 / 2);
        Invoke("ttp1", ttp1time);
        Invoke("ttp2", ttp2time);
        Invoke("ttp3", ttp3time);
        Invoke("ttp4", ttp4time);
    }

    private void ttp1()
    {
        UIController.instance.tooltip1.SetActive(true);
        Invoke("hidettp1", 15);
    }

    private void hidettp1()
    {
        UIController.instance.tooltip1.SetActive(false);
    }

    private void ttp2()
    {
        UIController.instance.tooltip2.SetActive(true);
        Invoke("hidettp2", 15);
    }

    private void hidettp2()
    {
        UIController.instance.tooltip2.SetActive(false);
    }

    private void ttp3()
    {
        UIController.instance.tooltip3.SetActive(true);
        Invoke("hidettp3", 15);
    }

    private void hidettp3()
    {
        UIController.instance.tooltip3.SetActive(false);
    }

    private void ttp4()
    {
        UIController.instance.tooltip4.SetActive(true);
        Invoke("hidettp4", 15);
    }

    private void hidettp4()
    {
        UIController.instance.tooltip4.SetActive(false);
    }

    private void WaveUI()
    {
        UIController.instance.gameObject.GetComponent<Animator>().SetTrigger("WaveChange");
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

    private void HandleWave4()
    {
        StartCoroutine("Wave4Spawns");
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
            GameObject enemySpawn = Instantiate(wave2EnemyTypes[Random.Range(0, wave2EnemyTypes.Length)], spawns[Random.Range(0, spawns.Length)].position, Quaternion.identity);
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
            GameObject enemySpawn = Instantiate(wave3EnemyTypes[Random.Range(0, wave3EnemyTypes.Length)], spawns[Random.Range(0, spawns.Length)].position, Quaternion.identity);
            enemySpawn.GetComponent<MonsterController>().target = GameObject.FindGameObjectWithTag("Player");
            enemySpawn.GetComponent<MonsterController>().waveID = 3;
            enemySpawn.transform.Rotate(0, -90, 0);
            currentEnemyCounter++;
            yield return new WaitForSeconds(timeBetweenEnemies3);
        }
    }

    private IEnumerator Wave4Spawns()
    {
        print("Spawninig wave 4 enemies");
        while (currentEnemyCounter < origEnemyCountWave4)
        {
            GameObject enemySpawn = Instantiate(wave3EnemyTypes[Random.Range(0, wave3EnemyTypes.Length)], spawns[Random.Range(0, spawns.Length)].position, Quaternion.identity);
            enemySpawn.GetComponent<MonsterController>().target = GameObject.FindGameObjectWithTag("Player");
            enemySpawn.GetComponent<MonsterController>().waveID = 4;
            enemySpawn.transform.Rotate(0, -90, 0);
            currentEnemyCounter++;
            yield return new WaitForSeconds(timeBetweenEnemies4);
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
            UIController.instance.waveText2.text = "2";
            Invoke("WaveUI", timeBetweenWaves1 / 2);
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
            UIController.instance.waveText2.text = "3";
            Invoke("WaveUI", timeBetweenWaves1 / 2);
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
            //UIController.instance.gameOverScreen.SetActive(true);
            //OptionsController.instance.UnLockCursor();

            Invoke("BossFight", timeBeforeBossFight);

            turtleMiniBoss.HideTurtle();
            turtleMiniBoss.deactive = true;

            wave3complete = true;
        }

        negX = mainCam.transform.localEulerAngles.x;
        negX = (negX > 180) ? negX - 360 : negX;
    }

    private void BossFight()
    {
        bossMusicsource.clip = bossMusic;
        bossMusicsource.Play();
        UIController.instance.bossBar.SetActive(true);
        Boss.gameObject.SetActive(true);
        Boss.AriseTurtle();
        StartCoroutine(zoomOut());
        StartCoroutine(rotateCamera());
        //mainCam.m_Lens.FieldOfView = bossFOV;

        Invoke("HandleWave4", timeBetweenWaves1);
        currentEnemyCounter = 0;
    }

    private IEnumerator zoomOut()
    {
        while (mainCam.m_Lens.FieldOfView != bossFOV)
        {
            if (mainCam.m_Lens.FieldOfView > bossFOV)
                mainCam.m_Lens.FieldOfView -= valueToZoomBy;
            else
                mainCam.m_Lens.FieldOfView += valueToZoomBy;

            yield return new WaitForSeconds(.01f);
        }
    }

    private IEnumerator rotateCamera()
    {

        while (negX > targetCameraXRotation)
        {

            print(negX);
            if (mainCam.transform.localEulerAngles.x > targetCameraXRotation)
                mainCam.transform.localEulerAngles = new Vector3(mainCam.transform.localEulerAngles.x - valueToRotateBy, mainCam.transform.localEulerAngles.y, mainCam.transform.localEulerAngles.z);
            else
                mainCam.transform.localEulerAngles = new Vector3(mainCam.transform.localEulerAngles.x + valueToRotateBy, mainCam.transform.localEulerAngles.y, mainCam.transform.localEulerAngles.z);

            yield return new WaitForSeconds(.01f);
        }
    }

    private IEnumerator zoomIn()
    {
        while (mainCam.m_Lens.FieldOfView > 28)
        {
            mainCam.m_Lens.FieldOfView -= valueToZoomBy;

            yield return new WaitForSeconds(.01f);
        }
    }
}
