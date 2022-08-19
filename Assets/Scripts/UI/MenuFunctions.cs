using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuFunctions : MonoBehaviour
{
    public GameObject settingObj;
    public GameObject playObj;
    public GameObject exitObj;

    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, 1000))
            {
                GameObject gameObj = hit.collider.gameObject;
                if (gameObj == settingObj)
                    OpenSettings();
                else if (gameObj == playObj)
                    PlayGame();
                else if (gameObj == exitObj)
                    ExitGame();
            }
        }
    }

    void OpenSettings()
    {
        Debug.Log("Open Settings");
    }

    void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    void ExitGame()
    {
        Application.Quit();
    }
}
