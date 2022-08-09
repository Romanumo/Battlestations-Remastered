using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Special bullet type that have contol over player code
public class ProjectileStorage : MonoBehaviour
{
    [SerializeField] private bool isSeeking;
    [SerializeField] private bool isAntiEnemy;
    [SerializeField] private bool isAntiPlayer;
    [SerializeField] private GameObject onHitEffect;
    Transform target;

    Bullet bullet;
    CharacterController controller;

    public void AssignBullet(Bullet bullet)
    {
        this.bullet = bullet;
        this.target = GlobalLibrary.player.transform;
        controller = this.GetComponent<CharacterController>();

        StartCoroutine(Dissapear());
    }

    private void Update()
    {
        if(isSeeking)
        {
            float yRot = Quaternion.LookRotation(target.position - this.transform.position).eulerAngles.y;
            this.transform.rotation = Quaternion.Euler(0, yRot, 0);
        }
        controller.Move(this.transform.forward * bullet.speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision other)
    {
        bool isDesignedTarget = (isAntiPlayer && other.gameObject.tag == "Player") || (isAntiEnemy && other.gameObject.tag == "Enemy");
        if (isDesignedTarget)
        {
            OnHit();
            IEntityBehaviour targetEntity = other.gameObject.GetComponent<IEntityBehaviour>();
            targetEntity.ReceiveDamage(bullet.attackInfo, bullet.GetSender());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Obstacle")
            OnHit();
    }

    private void OnHit()
    {
        if(onHitEffect != null)
        {
            GameObject effect = GameObject.Instantiate(onHitEffect, this.transform.position, this.transform.rotation);
            GeneralFunctions.AddTimer(delegate () { Destroy(effect); }, 3f);
        }
        Destroy(this.gameObject);
    }

    IEnumerator Dissapear()
    {
        yield return new WaitForSeconds(5f);
        Destroy(this.gameObject);
    }
}