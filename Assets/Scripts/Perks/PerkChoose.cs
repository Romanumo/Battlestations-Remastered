using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Spells;

public class PerkChoose : MonoBehaviour
{
    [Header("Profiles")]
    [SerializeField] PerkProfile[] perksProfiles;
    [SerializeField] Texture2D normalWeaponIcon;
    SpellProfileManager spellProfileManager;

    [Header("Perks Settings")]
    [SerializeField] GameObject perksRawImage;
    [SerializeField] int perksOffset;
    [SerializeField] Text perksText;
    [SerializeField] Transform perksParent;

    [Header("Magic Settings")]
    [SerializeField] GameObject magicRawImage;
    [SerializeField] int magicOffset;

    Dictionary<PerkType, Text> perkAmountText;
    PlayerBehaviour playerStats;
    int perksP = 2;
    int perkPoints { get { return perksP; } set { perksP = value; perksText.text = "Upgrade points : " + value; } }

    void Start()
    {
        spellProfileManager = GlobalLibrary.spellProfileManager;
        perkAmountText = new Dictionary<PerkType, Text>();
        playerStats = GameObject.FindObjectOfType<PlayerBehaviour>();

        InitializePerksShop();
        InitializeMagicShop();
    }

    public void AddPerkPoint() => perkPoints++;

    void AddPerk(PerkType type)
    {
        if(SpendPoints(1))
        {
            int perkAmount = int.Parse(perkAmountText[type].text);
            perkAmount++;
            perkAmountText[type].text = perkAmount.ToString();
            PerksFactory.Define(type).onTaking(playerStats.GetStats());
        }
    }

    void AddSpell(SpellType type, Button button)
    {
        if (SpendPoints(spellProfileManager.FindProfile(type).cost))
        {
            playerStats.UnlockSpell(type, button);
            Destroy(button.transform.Find("Text").gameObject);
        }
    }

    //Creates purchase buttons for each spell and perks
    #region InitializeShops
    void InitializeMagicShop()
    {
        int i = 0;
        float canvasWidth = GameObject.FindObjectOfType<Canvas>().GetComponent<RectTransform>().rect.width;
        foreach (SpellType spellType in Enum.GetValues(typeof(SpellType)))
        {
            SpellProfile profile = spellProfileManager.FindProfile(spellType);
            Button button = GameObject.Instantiate(magicRawImage, new Vector3(canvasWidth - 200, i * magicOffset + 100, 0), new Quaternion()).GetComponent<Button>();
            button.transform.parent = perksParent;
            button.onClick.AddListener(delegate () { AddSpell(spellType, button); });
            DescribtionWindow describtor = button.gameObject.AddComponent<DescribtionWindow>();
            describtor.describtion = profile.describtion;

            button.GetComponent<RawImage>().texture = profile.icon;
            Text text = button.transform.Find("Text").GetComponent<Text>();
            text.text = "Cost: " + profile.cost;
            i++;
        }

        CreateToNormalWeaponButton(canvasWidth, i);
    }

    void InitializePerksShop()
    {
        int i = 0;
        foreach (PerkType perkType in Enum.GetValues(typeof(PerkType)))
        {
            PerkProfile profile = FindProfile(perkType);
            Button button = GameObject.Instantiate(perksRawImage, new Vector3(i * perksOffset + 200, 200, 0), new Quaternion()).GetComponent<Button>();
            DescribtionWindow describtor = button.gameObject.AddComponent<DescribtionWindow>();

            button.transform.parent = perksParent;
            describtor.describtion = profile.describtion;

            button.onClick.AddListener(delegate () { AddPerk(perkType); });
            button.GetComponent<RawImage>().texture = profile.icon;
            Text text = button.transform.Find("Text").GetComponent<Text>();
            perkAmountText.Add(perkType, text);

            Text nameText = button.transform.Find("Name").transform.Find("NameText").GetComponent<Text>();
            string[] name = System.Text.RegularExpressions.Regex.Split(perkType.ToString(), @"(?<!^)(?=[A-Z])");
            string secondWord = (name.Length > 1) ? name[1] : "";
            nameText.text =  name[0] + " " + secondWord;
            i++;
        }
    }

    void CreateToNormalWeaponButton(float canvasWidth, float spellsAmount)
    {
        Button normal = GameObject.Instantiate(magicRawImage, new Vector3(canvasWidth - 200, spellsAmount * magicOffset + 100, 0), new Quaternion()).GetComponent<Button>();
        normal.transform.parent = perksParent;
        normal.onClick.AddListener(delegate () { playerStats.BackToNormalWepon(normal.transform.position); });
        normal.gameObject.AddComponent<DescribtionWindow>().describtion = "Switch to normal weapon";
        Destroy(normal.transform.Find("Text").gameObject);
        Destroy(normal.transform.Find("OverloadMax").gameObject);
        normal.GetComponent<RawImage>().texture = normalWeaponIcon;
    }

    PerkProfile FindProfile(PerkType type)
    {
        foreach (PerkProfile profile in perksProfiles)
        {
            if (profile.type == type)
            {
                return profile;
            }
        }
        return null;
    }

    bool SpendPoints(int amount)
    {
        if (amount > perkPoints)
            return false;

        perkPoints -= amount;
        return true;
    } 
    #endregion
}