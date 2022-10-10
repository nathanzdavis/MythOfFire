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
    public GameObject gameOverScreen;
    public TextMeshProUGUI waveText;


    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }
}
