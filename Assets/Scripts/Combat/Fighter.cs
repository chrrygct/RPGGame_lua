using UnityEngine;
using RPG.Movement;
using RPG.Core;
using RPG.Attributes;
using System;
using RPG.Saving;
using RPG.Stats;
using System.Collections.Generic;
using System.Collections;
using GameDevTV.Utils;
namespace RPG.Combat
{
    public class Fighter : MonoBehaviour,IAction,ISaveable,IModifierProvider
    {
        //[SerializeField] float weaponRange =2f;
        [SerializeField] float timeBetweenAttacks=1f;

        //[SerializeField] float weaponDamage = 10f;

        [SerializeField] Transform rightHandTransform=null;
        [SerializeField] Transform leftHandTransform=null;
        [SerializeField] WeaponConfig defaultWeapon=null;
        //[SerializeField] string defaultWeaponName="Unarmed"; 

        Health target;
        float timeSinceLastAttack= Mathf.Infinity;
        WeaponConfig currentWeaponConfig;
        LazyValue<Weapon> currentWeapon;
        // Start is called before the first frame update
        void Awake()
        {
            currentWeaponConfig = defaultWeapon;
            currentWeapon = new LazyValue<Weapon>(SetupDefaultWeapon);

        }

        private Weapon SetupDefaultWeapon()
        {
            return AttachWeapon(defaultWeapon);
        }

        void Start()
        {
           currentWeapon.ForceInit();
        }

        public void EquipWeapon(WeaponConfig weapon)
        {
            //if(weapon == null) return;
            currentWeaponConfig = weapon;
            currentWeapon.value=AttachWeapon(weapon);
        }

        private Weapon AttachWeapon(WeaponConfig weapon)
        {
            Animator animator = GetComponent<Animator>();
            return weapon.Spawn(rightHandTransform, leftHandTransform, animator);
        }

        public Health GetTarget()
        {
            return target;
        }
      
        

        // Update is called once per frame
        void Update()
        {
            timeSinceLastAttack += Time.deltaTime;
            
            if(target == null) return;//在没有目标时，什么都不做，防止使用空引用的位置
            if (target.IsDead()) return;//如果目标死了，也什么都不做
            
            if (!GetIsInRange(target.transform))
            {
                GetComponent<Mover>().MoveTo(target.transform.position, 1f);
            }
            else
            {
                GetComponent<Mover>().Cancel();
                AttackBehavior();
            }



        }

        public bool CanAttack(GameObject combatTarget)
        {
            if (combatTarget == null) return false;
            if (!GetComponent<Mover>().CanMoveTo(combatTarget.transform.position)&& !GetIsInRange(combatTarget.transform)) 
            //if (!GetComponent<Mover>().CanMoveTo(combatTarget.transform.position)) 
            {
                return false;
            }
            // if (!GetIsInRange(combatTarget.transform))
            // {
            //     if (!GetComponent<Mover>().CanMoveTo(combatTarget.transform.position))
            //     {
            //         return false;
            //     }
            // }
            Health targetHealth = combatTarget.GetComponent<Health>();
            if (targetHealth == null) return false;
            if (targetHealth.IsDead()) return false;
            return true;
        }

        private void AttackBehavior()
        {
             transform.LookAt(target.transform);
            if (timeSinceLastAttack > timeBetweenAttacks)
            {
                TriggerAttack();
                timeSinceLastAttack = 0f;
            }
        }

        private void TriggerAttack()
        {
            GetComponent<Animator>().ResetTrigger("stopAttack");
            GetComponent<Animator>().SetTrigger("attack");
        }

        private bool GetIsInRange(Transform targetTransform)
        {
            return Vector3.Distance(transform.position, targetTransform.position) < currentWeaponConfig.GetRange();
        }

        public void Attack(GameObject combatTarget)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            target=combatTarget.GetComponent<Health>();
        }

        public void Cancel()
        {
            StopAttack();
            target = null;
            GetComponent<Mover>().Cancel();
        }

        private void StopAttack()
        {
            //GetComponent<Animator>().ResetTrigger("attack");
            GetComponent<Animator>().SetTrigger("stopAttack");
        }


        //Animation Event
        void Hit()
        {
            if (target == null) return;
            float damage = GetComponent<BaseStats>().GetStat(Stat.Damage);

            if(currentWeapon.value != null)
            {
                currentWeapon.value.OnHit();
            }
            if (currentWeaponConfig.HasProjectile())
            {
                currentWeaponConfig.LaunchProjectile(rightHandTransform, target, gameObject, damage);
            }
            else   
            {
                target.TakeDamage(gameObject, damage);
            }
        }


        void Shoot()
        {
            Hit();
        }

        public object CaptureState()
        {
            return currentWeaponConfig.name;
        }

        public void RestoreState(object state)
        {
            string weaponName = (string)state;
            WeaponConfig weapon = UnityEngine.Resources.Load<WeaponConfig>(weaponName);
            EquipWeapon(weapon);
        }

        public IEnumerable<float> GetAdditiveModifier(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return currentWeaponConfig.GetDamage();

                
            }
        }

        public IEnumerable<float> GetPercentageModifier(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return currentWeaponConfig.GetPercentageBous();

                
            }
        }
    }    
}