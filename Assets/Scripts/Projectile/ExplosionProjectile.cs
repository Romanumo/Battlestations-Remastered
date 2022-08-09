using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionProjectile : MonoBehaviour
{
    float radius;
    AttackInfo attackInfo;
    Entity sender;

    public void SetExplosion(AttackInfo attackInfo, Entity sender, Vector3 pos, float radius)
    {
        this.transform.localScale = Vector3.one * radius / 2f;
        this.transform.position = pos;
        this.radius = radius;
        this.sender = sender;
        this.attackInfo = attackInfo;

        ExplosionDamage();
    }

    private void ExplosionDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, radius);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.tag == "Enemy" || hitCollider.tag == "Player")
            {
                IEntityBehaviour targetEntity = hitCollider.gameObject.GetComponent<IEntityBehaviour>();
                targetEntity.ReceiveDamage(attackInfo, sender);
            }
        }
    }
}
