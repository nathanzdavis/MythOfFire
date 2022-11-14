using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class WeatherEffects : MonoBehaviour
{
    public GameObject lightning;
    public float timeRangeStartSecs, timeRangeEndSecs;
    bool playing;
    public AudioClip lightningSound;
    public GameObject[] lightningEffects;
    public float lightningEffectsHideTime;
    //public Volume skyVolume;

    // Update is called once per frame
    void Update()
    {
        if (!playing)
        {
            Invoke("Lightning", Random.Range(timeRangeStartSecs, timeRangeEndSecs));
            playing = true;
        }
    }

    private void Lightning()
    {
        StartCoroutine(LightningCoroutine());
        //StartCoroutine(SkyLightingCoroutine());
    }

    /*
    IEnumerator SkyLightingCoroutine()
    {
        VisualEnvironment ve;
        while (true)
        {
            if (skyVolume.profile.TryGet<HDRISky>(out ve))
            {
                ve.exp
                yield return new WaitForSeconds(Random.Range(.1f, .5f));

                yield return new WaitForSeconds(Random.Range(.1f, .5f));
            }
        }
    }
    */
    IEnumerator LightningCoroutine()
    {
        lightning.GetComponent<AudioSource>().PlayOneShot(lightningSound);
        GetComponent<Animator>().SetTrigger("lightning");
        Invoke("playingReset", 3f);

        lightningEffects[Random.Range(0, lightningEffects.Length)].SetActive(true);
        yield return new WaitForSeconds(.3f);
        lightningEffects[Random.Range(0, lightningEffects.Length)].SetActive(true);
        yield return new WaitForSeconds(.2f);
        lightningEffects[Random.Range(0, lightningEffects.Length)].SetActive(true);

        Invoke("HideAllLightning", lightningEffectsHideTime);
    }

    void HideAllLightning()
    {
        foreach(GameObject go in lightningEffects)
        {
            go.SetActive(false);
        }
    }

    void playingReset()
    {
        playing = false;
    }
}
