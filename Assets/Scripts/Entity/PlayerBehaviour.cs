using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Spells;

public class PlayerBehaviour : EntityBehaviour<Player>
{
    SpellProfileManager spellProfileManager;
    SpellBehaviour spellBehaviour;
    int chosenSpell = -1;
    Rigidbody controller;
    Transform cam;

    [HideInInspector] public bool isMagicBlocked = false;
    [HideInInspector] public bool isBasicBlocked = false;

    [Header("UI")]
    [SerializeField] RectTransform maxHealth;
    [SerializeField] RectTransform health;
    [SerializeField] RectTransform weaponChoser;

    //Add effect list of modifiers
    protected override void Start()
    {
        spellProfileManager = GlobalLibrary.spellProfileManager;
        cam = Camera.main.gameObject.transform;
        controller = this.gameObject.GetComponent<Rigidbody>();
        base.Start();
    }

    void Update()
    {
        Aim();
        Controls();
        Movement();
        UpdateEntity();
    }

    #region UpdateMethods
    private void Controls()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (!GeneralFunctions.isCursorOverUI)
            {
                if (chosenSpell == -1 && !isBasicBlocked)
                    Shoot(stats);
                else if (chosenSpell != -1 && !isMagicBlocked)
                    UseSpell();
            }
        }

        if(Input.GetKeyUp(KeyCode.Q))
            GeneralFunctions.MenuWindowState(true);

        cam.transform.position = new Vector3(0, 23f, 0) + this.transform.position;
    }

    private void Aim()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        float angle = 0;
        if (Physics.Raycast(ray, out hit))
            angle = Quaternion.LookRotation(hit.point - this.transform.position).eulerAngles.y;
        this.transform.rotation = Quaternion.Euler(0, angle, 0);
    }

    private void Movement()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (input != Vector3.zero)
            controller.AddForce(input.normalized * Time.deltaTime * stats.speed * Entity.speedModifier);
    }

    private void UpdateEntity()
    {
        stats.health += stats.regen * Time.deltaTime * 5f;
    }

    public override void CheckHealth()
    {
        maxHealth.sizeDelta = new Vector2(stats.maxHealth * 5, health.sizeDelta.y);
        health.sizeDelta = new Vector2(maxHealth.sizeDelta.x * ((float)((float)stats.health / (float)stats.maxHealth)) ,health.sizeDelta.y);

        if (stats.health <= 0)
            GeneralFunctions.GameOver();
    }
    #endregion

    #region Spells
    private void UseSpell()
    {
        if (!isReloaded)
            return;

        SpellProfile profile = spellProfileManager.profiles[chosenSpell];
        profile.UseSpell();
        spellBehaviour = BulletFactory.GetSpellBehaviour(profile.type);
        spellBehaviour.Use(this.stats);
        if(spellBehaviour is SpellProjectileBehaviour)
        {
            stats.SetProjectileBehaviour(BulletFactory.GetSpellBehaviour(profile.type));
            Shoot(stats, profile.spellObj);
        }
    }

    public void UnlockSpell(SpellType type, Button button)
    {
        SpellProfile spellProfile = spellProfileManager.FindProfile(type);
        spellProfile.isBought = true;
        spellProfile.AssignOverloadTransfrom(button.transform.Find("OverloadMax").Find("Overload").GetComponent<RectTransform>());

        //On Click button will assign this chosen spell index to the specific spell type
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate () { chosenSpell = spellProfileManager.FindSpellIndex(type); weaponChoser.transform.position = button.transform.position; });
    }
    #endregion

    #region ForeignCode
    public Player GetStats() => stats;

    public void BackToNormalWepon(Vector3 iconPos)
    {
        chosenSpell = -1;
        stats.SetProjectileBehaviour(BulletFactory.GetBulletBehaviour(ProjectileEffect.None));
        weaponChoser.transform.position = iconPos;
    }
    #endregion
}