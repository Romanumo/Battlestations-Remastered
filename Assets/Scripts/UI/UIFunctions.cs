using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIFunctions : MonoBehaviour
{
    public void MoveToScene(int scene)
    {
        SceneManager.LoadScene(scene);
        Time.timeScale = 1;
    }

    public void CloseMenu()
    {
        GeneralFunctions.MenuWindowState(false);
    }

    public void CloseThisMenu(GameObject menu)
    {
        menu.SetActive(true);
        Time.timeScale = 1;
    }

    public void Exit()
    {
        Application.Quit();
    }
}
