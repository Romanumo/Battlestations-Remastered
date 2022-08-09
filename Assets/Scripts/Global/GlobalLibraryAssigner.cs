using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spells;

public enum ExplosionsEffects { Blast, Freeze, Hack };
public class GlobalLibraryAssigner : MonoBehaviour
{
    [SerializeField] private ExplosionProfile[] explosions;
    [SerializeField] private GameObject describtorWindow;
    [SerializeField] private GameObject menuWindow;
    [SerializeField] private Text keyTipText;
    [SerializeField] private RawImage effectIcons;
    [SerializeField] private GameObject gameOverWindow;

    void Awake()
    {
        GeneralFunctions.Start();
        GlobalLibrary.Start(explosions,
                            describtorWindow,
                            menuWindow,
                            keyTipText,
                            effectIcons,
                            gameOverWindow);
    }

    void Update()
    {
        GeneralFunctions.Update();
    }
}

public static class GlobalLibrary
{
    public static GameObject player { get; private set; }
    public static RoundManager roundManager { get; private set; }
    public static SpellProfileManager spellProfileManager { get; private set; }
    public static ExplosionProfile[] explosions { get; private set; }
    public static GameObject describtorWindow { get; private set; }
    public static Text describtorText { get; private set; }
    public static GameObject windowMenu { get; private set; }
    public static Text keyTipText { get; private set; }
    public static RawImage effectsIcons { get; private set; }
    public static Transform effectsParent { get; private set; }
    public static GameObject gameOverWindow { get; private set; }

    public static void Start(ExplosionProfile[] explosions, GameObject describtor, GameObject windowMenu, Text keyTipText, RawImage effectsIcon, GameObject gameOverWindow)
    {
        GlobalLibrary.player = GameObject.FindObjectOfType<PlayerBehaviour>().gameObject;
        GlobalLibrary.roundManager = GameObject.FindObjectOfType<RoundManager>();
        GlobalLibrary.spellProfileManager = GameObject.FindObjectOfType<SpellProfileManager>();
        GlobalLibrary.describtorWindow = describtor;
        describtorText = describtor.transform.Find("Describtion").GetComponent<Text>();
        GlobalLibrary.windowMenu = windowMenu;
        GlobalLibrary.explosions = explosions;
        GlobalLibrary.keyTipText = keyTipText;
        GlobalLibrary.effectsIcons = effectsIcon;
        GlobalLibrary.effectsParent = effectsIcon.transform.parent;
        GlobalLibrary.gameOverWindow = gameOverWindow;
    }
}
