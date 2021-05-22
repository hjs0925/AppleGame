using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonControl : MonoBehaviour
{
    public void OnStartButton()
    {
        SceneManager.LoadScene("Main");
    }

    public void OnQuitButton()
    {
        Application.Quit(0);
    }
}
