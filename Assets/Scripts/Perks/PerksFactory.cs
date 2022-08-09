using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

public enum PerkType { Damage, Defence, Speed, Reload, OverloadDefence, Health, OverloadStabilise, MagicMight, Regeneration };
public static class PerksFactory
{
    static Dictionary<PerkType, Perk> perksList;

    public static void Start()
    {
        perksList = new Dictionary<PerkType, Perk>();
        foreach (PerkType perkType in Enum.GetValues(typeof(PerkType)))
        {
            Perk perk = new Perk(FindPerkAction(perkType));
            perksList.Add(perkType, perk);
        }
    }
 
	///<summary> Returns perk stats by perks type </summary>
    public static Perk Define(PerkType type)
    {
        return perksList[type];
    }

    static Action<Player> FindPerkAction(PerkType effect)
    {
        MethodInfo mi = typeof(PerksFactory).GetMethod(effect.ToString());
        Action<Player> buff = (Action<Player>)mi.CreateDelegate(typeof(Action<Player>));

        if (buff == null)
            Debug.LogWarning("Perks type havent been founded");

        return buff;
    }

    public static void Damage(Player player)
    {
        player.bulletStats.attackInfo.attack += 8;
    }

    public static void Defence(Player player)
    {
        player.armor += 3;
    }

    public static void Speed(Player player)
    {
        player.speed += 0.8f;
    }

    public static void Reload(Player player)
    {
        player.reloadTime /= 1.25f;
    }

    public static void OverloadDefence(Player player)
    {
        player.overloadDefence++;
    }

    public static void Health(Player player)
    {
        player.maxHealth += 15;
        player.health += 30;
    }

    public static void OverloadStabilise(Player player)
    {
        player.ReduceOverloadGain(8);
        player.magicAttackAdd -= 2;
    }

    public static void MagicMight(Player player)
    {
        player.magicAttackAdd += 8;
    }

    public static void Regeneration(Player player)
    {
        player.regen += 0.015f;
    }
}
//Kekewgweg
public class Perk
{
    public Action<Player> onTaking;

    public Perk(Action<Player> onTaking)
    {
        this.onTaking = onTaking;
    }
}
