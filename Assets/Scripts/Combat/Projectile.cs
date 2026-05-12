using System.Collections;
using System.Collections.Generic;
using RPG.Core;
using UnityEditor;
using UnityEngine;
using RPG.Attributes;
using UnityEngine.Events;
namespace RPG.Combat
{   
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float ProjectileSpeed = 1f;
        [SerializeField] bool isHoming = false;
        [SerializeField] GameObject hitEffect = null;
        [SerializeField] float maxLifeTime = 10f;
        [SerializeField] GameObject[] destroyOnHit = null;
        [SerializeField] UnityEvent onHit;
        Health target= null;
        GameObject instigator = null;
        float damage = 0f;

        [SerializeField] float lifeAfterHit = 0.2f;

        // Start is called before the first frame update
        void Start()
        {
            if (!isHoming)
            {
                transform.LookAt(GetAimLocation());
            }
        }

        void Update()
        {
            if (target == null) return;
            if (isHoming && !target.IsDead())
            {
                transform.LookAt(GetAimLocation());
            }

            transform.Translate(Vector3.forward * ProjectileSpeed * Time.deltaTime);
        }

        public void SetTarget(Health target, GameObject instigator, float damage=0f)
        {
            this.target = target;
            this.instigator = instigator;
            this.damage = damage;
            Destroy(gameObject, maxLifeTime);
        }

        private Vector3 GetAimLocation()
        {
            CapsuleCollider targetCapsule = target.GetComponent<CapsuleCollider>();
            if (targetCapsule != null)
            {
                return target.transform.position + Vector3.up * targetCapsule.height * 0.5f;
            }
            return target.transform.position;
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Health>() != target) return;
            if (target.IsDead()) return;
            target.TakeDamage(instigator, damage);
            
            onHit.Invoke();

            ProjectileSpeed=0;
            if (hitEffect != null)
            {
                Instantiate(hitEffect, GetAimLocation(), Quaternion.identity);
            }



            foreach (GameObject toDestroy in destroyOnHit)
            {
                Destroy(toDestroy);
            }
            Destroy(gameObject,lifeAfterHit);
        }
    }
}
