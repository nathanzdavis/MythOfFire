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
            }
            else
            {
                LockCursor();
                optionsWindow.SetActive(false);
            }
            toggle = !toggle;
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
