using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuTarget : MonoBehaviour
{
    public float timer;
    float maxTimer = 40;

    void Update()
    {
        this.transform.position = new Vector3(-6f, 0, timer * -1);
        timer += Time.deltaTime * 30f;
        if (timer > maxTimer)
            timer = 0;
    }
}
