using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsController : MonoBehaviour
{
    [Header("Input Manager")]
    Input input;

    bool toggle;
    public GameObject optionsWindow;
    public static OptionsController instance;

    private void OnEnable()
    {
        input.UI.Enable();
    }

    private void OnDisable()
    {
        input.UI.Disable();
    }

    private void Awake()
    {
        LockCursor();

        input = new Input();

        input.UI.Options.performed += ctx =>
        {
            if (!toggle)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                optionsWindow.SetActive(true);
                GameObject.FindGameObjectWithTag("Player").GetComponent<Player>()._move = Vector2.zero;
                GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().input.Player.Disable();
            }
            else
            {
                if (GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().health > 0)
                {
                    LockCursor();
                    optionsWindow.SetActive(false);
                    GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().input.Player.Enable();
                }
            }
            toggle = !toggle;
            if (Time.timeScale == 0)
                Time.timeScale = 1;
            else
                Time.timeScale = 0;
        };

        input.UI.Submit.performed += ctx =>
        {
            if (WaveController.instance.ttpActive)
            {
                WaveController.instance.ttpActive = false;
                Time.timeScale = 1;
            }
        };
    }

    private void Start()
    {
        instance = this;
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ToggleMenu()
    {
        toggle = !toggle;
        if (GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().health > 0)
            GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().input.Player.Enable();

        if (Time.timeScale == 0)
            Time.timeScale = 1;
        else
            Time.timeScale = 0;
    }

    public void UnLockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
