using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsController : MonoBehaviour
{
    [Header("Input Manager")]
    Input input;

    bool toggle;
    public GameObject optionsWindow;

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
        input = new Input();

        input.UI.Options.performed += ctx =>
        {
            if (!toggle)
            {
                optionsWindow.SetActive(true);
            }
            else
            {
                optionsWindow.SetActive(false);
            }
            toggle = !toggle;
        };
    }
}
