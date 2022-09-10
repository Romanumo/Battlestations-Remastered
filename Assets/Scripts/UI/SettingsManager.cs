using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public GameObject musicLever;
    public GameObject soundLever;
    public Transform minPos;
    public Transform maxPos;

    GameObject leverCaught = null;

    public GameObject settingMenu;
    public Transform defaultView;
    public Transform settingsView;
    bool isSettings = false;

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000))
        {
            GameObject gameObj = hit.collider.gameObject;
            SettingsManipulation(gameObj);
        }

        if (isSettings && Input.GetKeyUp(KeyCode.Escape))
            CloseSettings();
    }

    public void OpenSettings()
    {
        isSettings = true;
        CamAnimation(true);
    }

    public void CloseSettings()
    {
        isSettings = false;
        CamAnimation(false);
    }

    void CamAnimation(bool isReverse)
    {
        Transform start = (isReverse) ? settingsView : defaultView;
        Transform end = (!isReverse) ? settingsView : defaultView;
        GeneralFunctions.AddProgressiveTimer(null, delegate (float timer) { Camera.main.transform.position = Vector3.Lerp(start.position, end.position, timer * timer); }, 1.5f);
        GeneralFunctions.AddProgressiveTimer(null, delegate (float timer) { Camera.main.transform.rotation = Quaternion.Lerp(start.rotation, end.rotation, timer * timer); }, 1.5f);
    }

    void SettingsManipulation(GameObject settingItem)
    {
        ScrollManipulation(settingItem);
    }

    void ScrollManipulation(GameObject settingItem)
    {
        ScrollCatch(settingItem);
        if (leverCaught == null)
            return;

        ScrollMovement();
    }

    void ScrollCatch(GameObject settingItem)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (settingItem.tag == "Lever")
                leverCaught = settingItem;
        }
    }

    void ScrollMovement()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 leverPos = leverCaught.transform.position;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray , out hit, 1000))
            {
                float y = LeverClamp(hit.point.y);
                leverCaught.transform.position = new Vector3(leverPos.x, y, leverPos.z);
            }
        }

        if (Input.GetMouseButtonUp(0))
            leverCaught = null;
    }

    float LeverClamp(float y)
    {
        float result = y;
        result = Mathf.Max(result, minPos.position.y);
        result = Mathf.Min(result, maxPos.position.y);
        return result;
    }
}
