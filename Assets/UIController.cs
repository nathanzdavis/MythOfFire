using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController instance;
    public Slider health;
    public Slider embers;
    public Slider wave;
    public GameObject deathscreen;
    public GameObject emberSymbol;
    public GameObject defaultSymbol;
    public GameObject gameOverScreen;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI waveText2;
    public Image bloodyScreen;
    public GameObject bossBar;
    public Slider bossSlider;
    public GameObject emberFire;
    public GameObject tooltip1;
    public GameObject tooltip2;
    public GameObject tooltip3;
    public GameObject tooltip4;
    public IEnumerator fillScreen()
    {
        Color c = bloodyScreen.color;

        while (bloodyScreen.color.a < 1)
        {
            c.a += .05f;
            bloodyScreen.color = c;

            yield return new WaitForSeconds(.05f);
        }
    }

    public IEnumerator defillScreen()
    {
        Color c = bloodyScreen.color;

        while (bloodyScreen.color.a > 0)
        {
            c.a -= .05f;
            bloodyScreen.color = c;

            yield return new WaitForSeconds(.05f);
        }
    }


    // Start is called before the first frame update
    void Awake()
    {
        instance = this;

        GetComponent<Animator>().SetTrigger("Black");
    }
}
