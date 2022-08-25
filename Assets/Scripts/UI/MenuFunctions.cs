using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuFunctions : MonoBehaviour
{
    public GameObject settingObj;
    public GameObject playObj;
    public GameObject exitObj;

    Dictionary<GameObject, TextMesh> menuObjTexts;
    GameObject previousHovered;

    private void Start()
    {
        menuObjTexts = new Dictionary<GameObject, TextMesh>();
        menuObjTexts.Add(settingObj, settingObj.transform.Find("Text").GetComponent<TextMesh>());
        menuObjTexts.Add(playObj, playObj.transform.Find("Text").GetComponent<TextMesh>());
        menuObjTexts.Add(exitObj, exitObj.transform.Find("Text").GetComponent<TextMesh>());
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000))
        {
            GameObject gameObj = hit.collider.gameObject;
            ShowText(gameObj);
            if (Input.GetMouseButtonUp(0))
            {
                if (gameObj == settingObj)
                    OpenSettings();
                else if (gameObj == playObj)
                    PlayGame();
                else if (gameObj == exitObj)
                    ExitGame(); 
            }
        }
    }

    void ShowText(GameObject menuObj)
    {
        if(!menuObjTexts.ContainsKey(menuObj) || menuObj == previousHovered)
            return;

        TextMesh text = menuObjTexts[menuObj];
        SmoothTextAnimation(text, true);

        if(previousHovered != null)
            SmoothTextAnimation(menuObjTexts[previousHovered], false);

        previousHovered = menuObj;
    }

    void OpenSettings() => Debug.Log("Open Settings");

    void PlayGame() => SceneManager.LoadScene(1);

    void ExitGame() => Application.Quit();

    void SmoothTextAnimation(TextMesh text, bool isAppearing)
    {
        Debug.Log("Animat");
        int animStart = (isAppearing) ? 1 : 0;
        int animEnd = (!isAppearing) ? 1 : 0;
        GeneralFunctions.AddProgressiveTimer(null, delegate (float timer) { text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.InverseLerp(animStart, animEnd, timer)); }, 0.5f);
    }
}
