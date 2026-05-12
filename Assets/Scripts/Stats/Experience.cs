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

