using UnityEngine;
using RPG.Attributes;
using System;
namespace RPG.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/New Weapon", order = 0)]
    public class WeaponConfig : ScriptableObject
    {
        [SerializeField] AnimatorOverrideController animatorOverride=null;
        [SerializeField] Weapon equippedPrefab=null;
        [SerializeField] float weaponRange=2f; 
        [SerializeField] float weaponDamage=5f;
        [SerializeField] float percentageBonus=0f;
        [SerializeField] bool isRightHanded=true;

        [SerializeField] Projectile projectileProefab=null;
        const string weaponName = "Weapon";

        public Weapon Spawn(Transform rightHandTransform,Transform leftHandTransform, Animator animator)
        {
            DestroyOldWeapon(rightHandTransform, leftHandTransform);
            Weapon weapon = null;
            if (equippedPrefab != null)
            {
                Transform handTransform = isRightHanded ? rightHandTransform : leftHandTransform;
                weapon=Instantiate(equippedPrefab, handTransform);
                weapon.name = weaponName;
            }
            if (animatorOverride != null)
            {
                animator.runtimeAnimatorController = animatorOverride;
            }
            else
            {
                var overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
                if (overrideController != null)                
                {
                    animator.runtimeAnimatorController = overrideController.runtimeAnimatorController;
                }
            }

            return weapon;
        }

        private void DestroyOldWeapon(Transform rightHandTransform, Transform leftHandTransform)
        {
            Transform oldWeapon = rightHandTransform.Find(weaponName);
            if (oldWeapon == null)
            {
                oldWeapon = leftHandTransform.Find(weaponName);
            }
            if (oldWeapon == null) return;
            oldWeapon.name = "Destroying";
            Destroy(oldWeapon.gameObject);
        }

        public bool HasProjectile()
        {
            return projectileProefab != null;
        }


        public void LaunchProjectile(Transform spawnPoint, Health target, GameObject instigator,float calculatedDamage)
        {
            Projectile projectile = Instantiate(
                projectileProefab,
                spawnPoint.position,
                Quaternion.identity
            );
            projectile.SetTarget(target,instigator, calculatedDamage);
        }


        
        public float GetRange()
        {
            return weaponRange;
        }

        public float GetDamage()
        {
            return weaponDamage;
        }

        public float GetPercentageBous()
        {
            return percentageBonus;
        }
        //[SerializeField] string weaponName;
        //[SerializeField] float damage;
        //[SerializeField] float range;
    }



}
