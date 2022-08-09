using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DeathAnimation { Sinking, Explosion };
public class EnemyBehaviour : EntityBehaviour<Enemy>
{
    [SerializeField] private DeathAnimation deathAnim;

    Transform hitPointsBar;
    Rigidbody controller;
    Transform target;

    protected override void Start()
    {
        target = GlobalLibrary.player.transform;
        controller = this.GetComponent<Rigidbody>();
        stats.sqrRange = stats.attackRange * stats.attackRange;

        CreateHitBar();
        base.Start();
    }

    void Update()
    {
        Move();
        Shoot();
    }

    void Move()
    {
        if (stats.speed < 0)
            return;

        float yRot = Quaternion.LookRotation(target.position - this.transform.position, this.transform.up).eulerAngles.y;
        this.transform.rotation = Quaternion.Euler(0, yRot, 0);
        controller.AddForce(this.transform.forward * Time.deltaTime * stats.speed * Entity.speedModifier);
    }

    void Shoot()
    {
        if (stats.isInRange(this.transform.position, target.position))
            Shoot(stats);
    }

    public override void CheckHealth()
    {
        hitPointsBar.transform.localScale = new Vector3(5 * (float)((float)stats.health / (float)stats.maxHealth), 0.5f, 0.5f) / 120f;

        if (stats.health <= 0)
            Death();
    }

    void Death()
    {
        GlobalLibrary.roundManager.UnsubscribeEnemy(this.gameObject);
        this.transform.tag = "Untagged";
        this.enabled = false;
        Destroy(hitPointsBar.gameObject);

        //Choose death animation
        switch(deathAnim)
        {
            case DeathAnimation.Sinking:
                this.GetComponent<BoxCollider>().enabled = false;
                GeneralFunctions.AddTimer(delegate () { Destroy(this.gameObject); }, 5f);
                break;
            case DeathAnimation.Explosion:
                GeneralFunctions.AddExplosionEffect(this.transform.position);
                Destroy(this.gameObject);
                break;
        }
    }

    void CreateHitBar()
    {
        GameObject hitBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hitBar.GetComponent<MeshRenderer>().material.color = Color.red;
        hitBar.transform.parent = this.transform;
        hitBar.transform.position = this.transform.position + new Vector3(0, 3, 0);
        hitPointsBar = hitBar.transform;
    }

    //public override void ReceiveDamage(AttackInfo attackInfo, Entity sender) => stats.ReceiveDamage(attackInfo, sender, this.transform.position);
}