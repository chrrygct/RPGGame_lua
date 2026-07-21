// using System.Collections;
using System;
using UnityEngine;
using RPG.Saving;
namespace RPG.Stats
{

    public class Experience : MonoBehaviour,ISaveable
    {

        [SerializeField] float experiencePoints = 0f;
        //public delegate void ExperienceGainedDelegate();
        public event Action onExperienceGained;



        public void GainExperience(float experience)
        {
            experiencePoints += experience;
            onExperienceGained();
        }

        public object CaptureState()
        {
            return experiencePoints;
        }

        public void RestoreState(object state)
        {
            experiencePoints = (float)state;
            // 恢复经验值后必须触发事件，让 BaseStats 刷新缓存的等级
            onExperienceGained?.Invoke();
        }

        public float GetExperiencePoints()
        {
            return experiencePoints;
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }


    }

}

