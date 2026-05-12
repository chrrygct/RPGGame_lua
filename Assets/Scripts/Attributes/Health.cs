using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Saving;
using RPG.Stats;
using RPG.Core;
using System.Threading.Tasks;
using System;
using GameDevTV.Utils;
using UnityEngine.Events;
namespace RPG.Attributes
{
    public class Health : MonoBehaviour, ISaveable
    {


        LazyValue <float> healthPoints;
        private bool isDead = false;
        [SerializeField] float regenerationPercentage = 70f;
        //[SerializeField] UnityEvent takeDamage;
        [SerializeField] UnityEvent<float> takeDamage;
        [SerializeField] UnityEvent onDie;

        void Awake()
        {
            healthPoints=new LazyValue<float>(GetInitiaHealth);

        }
        private float GetInitiaHealth()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);

        }
        void Start()
        {
            healthPoints.ForceInit();
            


        }
        private void OnEnable()
        {
            GetComponent<BaseStats>().onLevelUp += RegenerateHealth;
        }
        private void OnDisable()
        {
            GetComponent<BaseStats>().onLevelUp -= RegenerateHealth;
        }

        private void RegenerateHealth()
        {
            float newHealthPoints = GetComponent<BaseStats>().GetStat(Stat.Health)*(regenerationPercentage/100f);
            healthPoints.value = Mathf.Max(healthPoints.value, newHealthPoints);
        }

        public bool IsDead()
        {
            return isDead;
        }

        public void TakeDamage(GameObject instigator, float damage)
        {

            print(gameObject.name + " took damage: " + damage);

            healthPoints.value = Mathf.Max(healthPoints.value - damage, 0);
            takeDamage.Invoke(damage);
            print(healthPoints);
            if (healthPoints.value == 0 && !isDead)
            {
                Die();
                AwardExperience(instigator);
            }
        }

        public void Heal(float healthToRestore)
        {
            healthPoints.value = Mathf.Min(healthPoints.value + healthToRestore, GetMaxHealthPoints());
        }

        
        public float GetHealthPoints()
        {
            return healthPoints.value;
        }

        public float GetMaxHealthPoints()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);
        }

        private void AwardExperience(GameObject instigator)
        {
            Experience experience = instigator.GetComponent<Experience>();
            if (experience == null) return;
            experience.GainExperience(GetComponent<BaseStats>().GetStat(Stat.Experience));

        }

        public float GetPercentage()
        {
            return 100 * GetFraction();
        }

        public float GetFraction()
        {
            return healthPoints.value / GetComponent<BaseStats>().GetStat(Stat.Health);
        }

        private void Die()
        {
            isDead = true;
            onDie.Invoke();
            GetComponent<Animator>().SetTrigger("die");
            GetComponent<ActionScheduler>().CancelCurrentAction();


        }


        public object CaptureState()
        {
            return healthPoints.value;
        }



        public void RestoreState(object state)
        {
            healthPoints.value = (float)state;
            if (healthPoints.value == 0)
            {
                Die();
            }
        }

    }

}
