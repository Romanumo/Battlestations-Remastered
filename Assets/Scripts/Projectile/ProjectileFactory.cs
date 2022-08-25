using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Spells;

public enum ProjectileEffect { None, ArmorReducer, SpeedReducer, Overloader, MagicBlocker, BasicBlocker };
public static class BulletFactory
{
    const float minSpeed = 4;
    static Dictionary<ProjectileEffect, ProjectileEffectProfile> projectileProfiles;
    static Dictionary<ProjectileEffect, BulletBehaviour> projectileDictionary;
    static Dictionary<SpellType, SpellBehaviour> spellDictionary;

    public static void Start()
    {
        ProjectileEffectManager projectileProfileManager = GameObject.FindObjectOfType<ProjectileEffectManager>();
        if (projectileProfileManager == null)
        {
            Debug.Log("No Projectile Profile Manager Found");
            return;
        }

        projectileProfiles = new Dictionary<ProjectileEffect, ProjectileEffectProfile>(projectileProfileManager.AssignIconToEffect());
        projectileDictionary = new Dictionary<ProjectileEffect, BulletBehaviour>(AssignDictionary<ProjectileEffect, BulletBehaviour>());
        spellDictionary = new Dictionary<SpellType, SpellBehaviour>(AssignDictionary<SpellType, SpellBehaviour>());
    }

    public static BulletBehaviour GetBulletBehaviour(ProjectileEffect type) => projectileDictionary[type];

    public static SpellBehaviour GetSpellBehaviour(SpellType type) => spellDictionary[type];

    #region SupportFunctions
    static Dictionary<Type, Stats> AssignDictionary<Type, Stats>() where Stats : WeaponBehaviour where Type : Enum
    {
        Dictionary<Type, Stats> dictionary = new Dictionary<Type, Stats>();
        foreach (Type type in Enum.GetValues(typeof(Type)))
        {
            Stats stat = Define<Type, Stats>(type);
            dictionary.Add(type, stat);
        }
        return dictionary;
    }

    //The old way of finding bullet stats by type. Decided to leave this. Newer and more optimal version in PerksIntrefaceClass
    static Stats Define<Type, Stats>(Type type) where Stats : WeaponBehaviour where Type : Enum
    {
        string methodName = type.ToString() + "Default";
        MethodInfo mi = typeof(BulletFactory).GetMethod(methodName);
        Stats buff = (Stats)mi.Invoke(typeof(BulletFactory), null);

        if (buff == null)
            Debug.Log("Bullet of type: " + type + ", havent been founded");

        return buff;
    }

    static ProjectileEffectProfile GetProjectileProfile(ProjectileEffect effect, float time)
    {
        ProjectileEffectProfile profile = projectileProfiles[effect];
        if (time > 0)
            profile.effect.duration = time;
        return profile;
    }

    static void OnHitPlayerEffectUI(ProjectileEffectProfile profile, Entity target)
    {
        if (!(target is Player))
            return;

        GeneralFunctions.AddEffectIcon(profile);
        GeneralFunctions.KeyTipShow(profile.keyTipText);
    }
    #endregion

    #region BulletStats
    public static BulletBehaviour Overloader(int overloadGain)
    {
        ProjectileEffectProfile profile = projectileProfiles[ProjectileEffect.Overloader];
        Action<OnHitInfo> onHit = delegate (OnHitInfo info)
        {
            if (!(info.target is Player))
                return;

            Player player = (Player)info.target;
            player.ReceiveOverload(overloadGain);

            GeneralFunctions.KeyTipShow("Magic unstabilise!");
        };

        return new BulletBehaviour(onHit);
    }

    public static TimedBulletBehaviour ArmorReducer(float duration = -1)
    {
        ProjectileEffectProfile profile = GetProjectileProfile(ProjectileEffect.ArmorReducer, duration);
        Action<OnHitInfo> onHit = delegate (OnHitInfo info)
        {
            if (info.target.armor == 0)
                return;

            int temp = info.target.armor;
            info.target.armor = 0;

            profile.effect.AddHolder(info.target);
            profile.effect.timer = GeneralFunctions.AddGetTimer(delegate () { info.target.armor = temp; profile.effect.TimerExpire(); }, profile.effect.duration);

            OnHitPlayerEffectUI(profile, info.target);
        };

        return new TimedBulletBehaviour(onHit, profile);
    }

    public static TimedBulletBehaviour MagicBlocker(float duration = -1)
    {
        ProjectileEffectProfile profile = GetProjectileProfile(ProjectileEffect.MagicBlocker, duration);
        Action<OnHitInfo> onHit = delegate (OnHitInfo info)
        {
            if (!(info.target is Player))
                return;

            Player player = (Player)info.target;
            player.MagicState(false);

            profile.effect.AddHolder(info.target);
            profile.effect.timer = GeneralFunctions.AddGetTimer(delegate () { player.MagicState(true); profile.effect.TimerExpire(); }, profile.effect.duration);

            OnHitPlayerEffectUI(profile, info.target);
        };

        return new TimedBulletBehaviour(onHit, profile);
    }

    public static TimedBulletBehaviour BasicBlocker(float duration = -1)
    {
        ProjectileEffectProfile profile = GetProjectileProfile(ProjectileEffect.BasicBlocker, duration);
        Action<OnHitInfo> onHit = delegate (OnHitInfo info)
        {
            //Important
            if (!(info.target is Player))
                return;

            Player player = (Player)info.target;
            player.BasicState(false);

            profile.effect.AddHolder(info.target);
            profile.effect.timer = GeneralFunctions.AddGetTimer(delegate () { player.BasicState(true); profile.effect.TimerExpire(); }, profile.effect.duration);

            OnHitPlayerEffectUI(profile, info.target);
        };

        return new TimedBulletBehaviour(onHit, profile);
    }

    public static TimedBulletBehaviour SpeedReducer(int speedDecrease, float duration = -1)
    {
        ProjectileEffectProfile profile = GetProjectileProfile(ProjectileEffect.SpeedReducer, duration);
        Action<OnHitInfo> onHit = delegate (OnHitInfo info)
        {
            if (info.target.speed < minSpeed)
                return;

            info.target.speed -= speedDecrease;

            profile.effect.AddHolder(info.target);
            profile.effect.timer = GeneralFunctions.AddGetTimer(delegate () { info.target.speed += speedDecrease; profile.effect.TimerExpire(); }, profile.effect.duration);

            OnHitPlayerEffectUI(profile, info.target);
        };

        return new TimedBulletBehaviour(onHit, profile);
    }
    #endregion

    #region SpellStats
    //ALWAYS name function like this => [SpellType] + Default
    //Spell only for players
    public static SpellProjectileBehaviour FireballDefault()
    {
        Action<OnHitInfo> onHit = delegate (OnHitInfo info)
        {
            Player player = (Player)info.sender;

            AttackInfo explosionStats = new AttackInfo(Mathf.RoundToInt(20f + player.magicAttackAdd * 1.5f), ProjectileEffect.None);
            GeneralFunctions.AddExplosion(info.hitPos, explosionStats, player, 2 + (player.magicAttackAdd / 10f), ExplosionsEffects.Blast);
        };

        Action<OnOverloadInfo> onOverload = delegate (OnOverloadInfo info)
        {
            Player player = (Player)info.sender;
            AttackInfo explosionStats = new AttackInfo(Mathf.RoundToInt(30f + player.magicAttackAdd * 2.2f), ProjectileEffect.None);
            GeneralFunctions.AddExplosion(info.playerPos, explosionStats, player, 5 + (player.magicAttackAdd / 5f), ExplosionsEffects.Blast);

            GeneralFunctions.KeyTipShow("Fireball overload!");
            player.health += (int)player.overloadDefence * 10;
        };

        return new SpellProjectileBehaviour(onHit, onOverload);
    }

    public static SpellBehaviour SpeederDefault()
    {
        Action<Player> onActivation = delegate (Player player)
        {
            int speedAdd = player.magicAttackAdd / 2 + 2;
            TimedBulletBehaviour effect = BulletFactory.SpeedReducer(speedAdd * -1, 30f);
            effect.ApplyEffect(player);
        };

        Action<OnOverloadInfo> onOverload = delegate (OnOverloadInfo info)
        {
            Player player = (Player)info.sender;
            float tempSpeed = player.speed;
            player.speed += player.magicAttackAdd + 6;
            player.health = (int)player.overloadDefence * 20 + 20;
            GeneralFunctions.AddTimer(delegate () { player.speed = tempSpeed; }, 45f);

            GeneralFunctions.KeyTipShow("Speed overload!");
        };

        return new SpellBehaviour(onActivation, onOverload);
    }

    public static SpellProjectileBehaviour FreezerDefault()
    {
        Action<OnHitInfo> onHit = delegate (OnHitInfo info)
        {
            Player player = (Player)info.sender;
            AttackInfo explosionStats = new AttackInfo(0, BulletFactory.SpeedReducer(1, 30f));
            GeneralFunctions.AddExplosion(info.hitPos, explosionStats, player, 2 + (player.magicAttackAdd / 10f), ExplosionsEffects.Freeze);
        };

        Action<OnOverloadInfo> onOverload = delegate (OnOverloadInfo info)
        {
            Player player = (Player)info.sender;
            AttackInfo explosionStats = new AttackInfo(0, BulletFactory.SpeedReducer(3, 60f));
            player.speed += player.overloadDefence * 0.3f;
            GeneralFunctions.AddExplosion(info.playerPos, explosionStats, player, 5 + (player.magicAttackAdd / 5f), ExplosionsEffects.Freeze);

            GeneralFunctions.KeyTipShow("Freeze spell overload!");
            GeneralFunctions.AddEffectIcon(projectileProfiles[ProjectileEffect.SpeedReducer]);
        };

        return new SpellProjectileBehaviour(onHit, onOverload);
    }

    public static SpellProjectileBehaviour DivideZeroDefault()
    {
        Action<OnHitInfo> onHit = delegate (OnHitInfo info)
        {
            info.target.health = 0;
        };

        Action<OnOverloadInfo> onOverload = delegate (OnOverloadInfo info)
        {
            Player player = (Player)info.sender;

            player.TemporaryInvincibility();
            AttackInfo explosionStats = new AttackInfo(1000, ProjectileEffect.None);
            GeneralFunctions.AddExplosion(info.playerPos, explosionStats, player, 7 + (player.magicAttackAdd / 2f), ExplosionsEffects.Hack);
            float timer = 50f / Mathf.Max(player.overloadDefence / 3, 1);

            TimedBulletBehaviour basicBlock = BulletFactory.BasicBlocker(timer);
            TimedBulletBehaviour magicBlock = BulletFactory.MagicBlocker(timer);
            basicBlock.ApplyEffect(player);
            magicBlock.ApplyEffect(player);

            GeneralFunctions.KeyTipShow("Hack spell overload! Weapon and spells are unavailable!");
            GeneralFunctions.AddEffectIcon(basicBlock.profile);
            GeneralFunctions.AddEffectIcon(magicBlock.profile);
        };

        return new SpellProjectileBehaviour(onHit, onOverload);
    }
    #endregion

    #region DefaultBulletBehaviour
    //ALWAYS name function like this => [ProjectileEffect] + Default
    public static TimedBulletBehaviour SpeedReducerDefault() => BulletFactory.SpeedReducer(1);
    public static TimedBulletBehaviour BasicBlockerDefault() => BulletFactory.BasicBlocker();
    public static TimedBulletBehaviour MagicBlockerDefault() => BulletFactory.MagicBlocker();
    public static TimedBulletBehaviour ArmorReducerDefault() => BulletFactory.ArmorReducer();
    public static BulletBehaviour OverloaderDefault() => BulletFactory.Overloader(20);
    public static BulletBehaviour NoneDefault() => new BulletBehaviour(null);
    #endregion
}

#region WeaponClasses
public abstract class WeaponBehaviour
{
    public abstract void OnHit(OnHitInfo onHitInfo);
    public abstract Action<OnHitInfo> GetOnHit();
}

[System.Serializable]
public class BulletBehaviour : WeaponBehaviour
{
    protected Action<OnHitInfo> onHit;

    public BulletBehaviour(Action<OnHitInfo> onHit)
    {
        this.onHit = onHit;
    }

    public override void OnHit(OnHitInfo onHitInfo)
    {
        if (onHit != null)
            onHit.Invoke(onHitInfo);
    }

    public void ApplyEffect(Entity entity)
    {
        if (onHit != null)
            onHit.Invoke(new OnHitInfo(null, entity, Vector3.zero));
    }

    public override Action<OnHitInfo> GetOnHit() => onHit;
}

[System.Serializable]
public class TimedBulletBehaviour : BulletBehaviour
{
    public readonly ProjectileEffectProfile profile;

    public TimedBulletBehaviour(Action<OnHitInfo> onHit, ProjectileEffectProfile profile) : base(onHit)
    {
        this.profile = profile;
    }
}

[System.Serializable]
public class SpellBehaviour : WeaponBehaviour
{
    public Action<Player> onActivating { get; protected set; }
    public Action<OnOverloadInfo> onOverload { get; protected set; }

    public SpellBehaviour(Action<Player> onActivating, Action<OnOverloadInfo> onOverload)
    {
        this.onActivating = onActivating;
        this.onOverload = onOverload;
    }

    public void Use(Player player)
    {
        if (onActivating != null)
            onActivating.Invoke(player);
    }

    public override void OnHit(OnHitInfo onHitInfo) { }
    public override Action<OnHitInfo> GetOnHit() => null;
}

[System.Serializable]
public class SpellProjectileBehaviour : SpellBehaviour
{
    protected Action<OnHitInfo> onHit;

    public SpellProjectileBehaviour(Action<OnHitInfo> onHit, Action<OnOverloadInfo> onOverload, Action<Player> onActivating = null)
        : base(onActivating: onActivating, onOverload: onOverload)
    {
        this.onHit = onHit;
    }

    public override void OnHit(OnHitInfo onHitInfo)
    {
        if (onHit != null)
            onHit.Invoke(onHitInfo);
    }

    public override Action<OnHitInfo> GetOnHit() => onHit;
} 
#endregion

#region SupportClasses
public struct OnHitInfo
{
    public Entity sender;
    public Entity target;
    public Vector3 hitPos;

    public OnHitInfo(Entity sender, Entity target, Vector3 pos)
    {
        this.sender = sender;
        this.target = target;
        this.hitPos = pos;
    }
}

public struct OnOverloadInfo
{
    public PlayerBehaviour player;
    public Entity sender;
    public Vector3 playerPos;

    public OnOverloadInfo(Entity sender, PlayerBehaviour player, Vector3 pos)
    {
        this.player = player;
        this.sender = sender;
        this.playerPos = pos;
    }
} 
#endregion