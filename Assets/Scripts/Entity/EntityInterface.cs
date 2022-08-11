using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Entity 
{
    [SerializeField] protected int _maxHealth;
    [SerializeField] protected float _health;
    [SerializeField] protected int _armor;
    [SerializeField] protected float _reloadTime;
    [SerializeField] protected float _speed;
    [SerializeField] protected Bullet _bulletStats;
    [NonSerialized] protected IEntityBehaviour owner;
    public const float speedModifier = 1000f;

    public int maxHealth{ get { return _maxHealth; } set { _maxHealth = GeneralFunctions.AdjustToRange(value, 1000); } }
    public float health { get { return _health; } set { _health = GeneralFunctions.AdjustToRange(value, _maxHealth); owner.CheckHealth(); } }
    public int armor { get { return _armor; } set { _armor = GeneralFunctions.AdjustToRange(value, 50); } }
    public float reloadTime { get { return _reloadTime; } set { _reloadTime = GeneralFunctions.AdjustToRange(value, 10, 0.5f); } }
    public float speed { get { return _speed; } set { _speed = GeneralFunctions.AdjustToRange(value, 35); } }
    public Bullet bulletStats { get { return _bulletStats; } }

    public virtual void Init(GameObject owner)
    {
        this.owner = owner.GetComponent<IEntityBehaviour>();
        this.owner.CheckHealth();
        bulletStats.Init(this);
    }

    public virtual void ReceiveDamage(AttackInfo attackInfo, Entity sender, Vector3 hitPos)
    {
        int damageTaken = attackInfo.attack - armor;
        damageTaken = Mathf.Max(damageTaken, 1);
        this.health -= damageTaken;

        if (attackInfo.isEffectIncluded())
        {
            OnHitInfo info = new OnHitInfo(sender, this, hitPos);
            attackInfo.OnHit(info);
        }
    }
}

[System.Serializable]
public class Enemy : Entity
{
    [SerializeField] protected int _attackRange;
    [HideInInspector] public int sqrRange;

    public int attackRange { get { return _attackRange; } set { _attackRange = GeneralFunctions.AdjustToRange(value, 200, 10); } }

    public bool isInRange(Vector3 pos, Vector3 target)
    {
        if ((pos - target).sqrMagnitude < sqrRange)
            return true;

        return false;
    }
}

[System.Serializable]
public class Player : Entity
{
    [SerializeField] protected int _magicAttackAdd;
    [SerializeField] protected float _overloadDefence;
    [SerializeField] protected float _regen;
    private PlayerBehaviour ownerBehaviour;

    public int magicAttackAdd { get { return _magicAttackAdd; } set { _magicAttackAdd = GeneralFunctions.AdjustToRange(value, 100); } }
    public float overloadDefence { get { return _overloadDefence; } set { _overloadDefence = GeneralFunctions.AdjustToRange(value, 100); } }
    public float regen { get { return _regen; } set { _regen = GeneralFunctions.AdjustToRange(value, 5); } }

    public override void Init(GameObject owner)
    {
        base.Init(owner);
        if (owner.GetComponent<PlayerBehaviour>())
            ownerBehaviour = owner.GetComponent<PlayerBehaviour>();
    }

    public override void ReceiveDamage(AttackInfo attackInfo, Entity sender, Vector3 hitPos)
    {
        base.ReceiveDamage(attackInfo, sender, hitPos);
        ProjectileEffectProfile profile = attackInfo.GetProjectileProfile();
        if (!profile.IsNull())
        {
            GeneralFunctions.AddEffectIcon(profile);
            GeneralFunctions.KeyTipShow(profile.keyTipText);
        }
    }

    public void TemporaryInvincibility()
    {
        int tempArmor = this.armor;
        this._armor = 100000;
        GeneralFunctions.AddTimer(delegate () { this.armor = tempArmor; }, 0.1f);
    }

    public void SetProjectileBehaviour(WeaponBehaviour bulletBehaviour) => this.bulletStats.SetBehaviour(bulletBehaviour);
    public void ReceiveOverload(int amount) => GlobalLibrary.spellProfileManager.ReceiveOverload(amount);
    public void ReduceOverloadGain(int amount) => GlobalLibrary.spellProfileManager.ReduceOverloadGain(amount);
    public void MagicState(bool state) => ownerBehaviour.isMagicBlocked = state;
    public void BasicState(bool state) => ownerBehaviour.isBasicBlocked = state;
}

[Serializable]
public class AttackInfo
{
    [SerializeField] private int _attack;
    [SerializeField] private ProjectileEffect _projectileEffect;
    private WeaponBehaviour behaviour;

    public int attack { get { return _attack; } set { _attack = GeneralFunctions.AdjustToRange(value, 500); } }
    public ProjectileEffect projectileEffect { get { return _projectileEffect; } }

    public AttackInfo(int attack, ProjectileEffect projectileEffect)
    {
        this.attack = attack;
        _projectileEffect = projectileEffect;
        SetBehaviour();
    }

    public AttackInfo(int attack, BulletBehaviour projectileBehaviour)
    {
        this.attack = attack;
        this.behaviour = projectileBehaviour;
    }

    public ProjectileEffectProfile GetProjectileProfile()
    {
        if (behaviour is TimedBulletBehaviour)
            return ((TimedBulletBehaviour)behaviour).profile;
        return ProjectileEffectProfile.Null();
    }

    public void SetBehaviour() => behaviour = BulletFactory.GetBulletBehaviour(projectileEffect);
    public void SetBehaviour(WeaponBehaviour behaviour) => this.behaviour = behaviour;
    public void OnHit(OnHitInfo onHitInfo) => behaviour.OnHit(onHitInfo);
    public bool isEffectIncluded() => (behaviour.GetOnHit() != null);
}

[Serializable]
public class Bullet
{
    [NonSerialized] private Entity sender;
    [SerializeField] private float _speed;
    public AttackInfo attackInfo;

    public float speed { get { return _speed; } set { _speed = GeneralFunctions.AdjustToRange(value, 500); } }

    public void Init(Entity sender)
    {
        attackInfo.SetBehaviour();
        this.sender = sender;
    }

    public void SetBehaviour(WeaponBehaviour behaviour) => attackInfo.SetBehaviour(behaviour);
    public Entity GetSender() => sender;
}

public interface IEntityBehaviour 
{
    public abstract void ReceiveDamage(AttackInfo attackInfo, Entity sender);
    public abstract void CheckHealth();
}

public abstract class EntityBehaviour<EntityStats> : MonoBehaviour, IEntityBehaviour where EntityStats : Entity
{
    [SerializeField] protected GameObject basicBullet;
    [SerializeField] protected float bulletOffset = 3f;
    [SerializeField] protected EntityStats stats;
    public bool isReloaded = true;

    //OnEvent patter would be useful here if there was some spell that triggers on entity death
    protected Action OnDeath = default;

    protected virtual void Start()
    {
        stats.Init(this.gameObject);
    }

    protected void Shoot(Entity stats, GameObject bulletObj = null)
    {
        if (!isReloaded)
            return;

        GameObject bullet = (bulletObj == null) ? basicBullet : bulletObj;
        GameObject projectile = GameObject.Instantiate(bullet, this.transform.position + this.transform.forward * bulletOffset, this.transform.rotation);
        projectile.GetComponent<ProjectileStorage>().AssignBullet(stats.bulletStats);
        StartCoroutine(Reload(stats));
    }

    protected IEnumerator Reload(Entity entity)
    {
        isReloaded = false;
        yield return new WaitForSeconds(entity.reloadTime);
        isReloaded = true;
    }

    public void ReceiveDamage(AttackInfo attackInfo, Entity sender) => stats.ReceiveDamage(attackInfo, sender, this.transform.position);

    public virtual void CheckHealth()
    {
        if (stats.health <= 0 && OnDeath != null)
            OnDeath.Invoke();
    }
}