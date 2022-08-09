using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DescribtionWindow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public string describtion;

    public void OnPointerEnter(PointerEventData eventData)
    {
        GeneralFunctions.ShowDescribtion(describtion);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GeneralFunctions.HideDescribtion();
    }
}
