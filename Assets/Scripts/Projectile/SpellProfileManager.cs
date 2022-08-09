using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Spells
{
    public enum SpellType { Fireball, Speeder, Freezer, DivideZero };

    public class SpellProfileManager : MonoBehaviour
    {
        [SerializeField] private SpellProfile[] _profiles;
        public SpellProfile[] profiles { get { return _profiles; } }

        // Start is called before the first frame update
        void Start()
        {
            PlayerBehaviour player = GlobalLibrary.player.GetComponent<PlayerBehaviour>();
            foreach (SpellProfile profile in profiles)
            {
                profile.Start(player);
            }
        }

        // Update is called once per frame
        void Update()
        {
            foreach (SpellProfile profile in profiles)
            {
                if (profile.isBought)
                    profile.Update();
            }
        }

        public int FindSpellIndex(SpellType type)
        {
            for (int i = 0; i < profiles.Length; i++)
            {
                if (profiles[i].type == type)
                {
                    return i;
                }
            }
            return -1;
        }

        public SpellProfile FindProfile(SpellType type)
        {
            int index = FindSpellIndex(type);
            if (index == -1)
                return null;

            return profiles[index];
        }

        void ThroughSpellsAction(Action<SpellProfile> action, bool isBoughtNeeded = true)
        {
            if (action == null)
                return;

            foreach (SpellProfile spell in profiles)
            {
                if (spell.isBought || !isBoughtNeeded)
                {
                    action.Invoke(spell);
                }
            }
        }

        public void ReceiveOverload(int amount) => ThroughSpellsAction(delegate (SpellProfile spell) { spell.AddOverload(amount); });

        public void ReduceOverloadGain(int amount) => ThroughSpellsAction(delegate (SpellProfile spell) { spell.overloadGain = Mathf.Max(spell.overloadGain - amount, 7); }, false);
    }

    [System.Serializable]
    public class SpellProfile : Profile
    {
        //It will get "On Hit" and "On Overload" in factory through string and reflection
        public SpellType type;
        public int cost;
        public float maxOverload;
        public float overloadGain;
        public float overloadDecay;
        public GameObject spellObj;
        public bool isBought;

        RectTransform overloadTransform;
        float overload;
        float maxOverloadLength;
        PlayerBehaviour player;
        SpellBehaviour stats;

        public void Start(PlayerBehaviour player)
        {
            stats = BulletFactory.GetSpellBehaviour(type);
            this.player = player;
        }

        public void AssignOverloadTransfrom(RectTransform transfrom)
        {
            overloadTransform = transfrom;
            maxOverloadLength = overloadTransform.parent.GetComponent<RectTransform>().sizeDelta.x;
        }

        public void UseSpell()
        {
            overload += overloadGain;
            if (overload > maxOverload)
            {
                if (stats.onOverload != null)
                {
                    OnOverloadInfo info = new OnOverloadInfo(player.GetStats(), player, GlobalLibrary.player.transform.position);
                    stats.onOverload.Invoke(info);
                }

                overload = 0;
            }
        }

        public void Update()
        {
            overload -= Time.deltaTime * overloadDecay;

            if (isBought && overloadTransform != null)
                overloadTransform.sizeDelta = new Vector2(maxOverloadLength * GetOverloadPercentage(), overloadTransform.sizeDelta.y);
        }

        public void AddOverload(int amount) => overload += amount;

        public float GetOverloadPercentage() => ((float)((float)overload / (float)maxOverload));
    } 
}