using System;
using System.Collections;
using System.Collections.Generic;
using GameDevTV.Utils;
using Unity.VisualScripting;
using UnityEngine;
namespace RPG.Stats
{   
    public class BaseStats : MonoBehaviour
    {
        [Range(1, 99)]
        [SerializeField] int startingLevel = 0;
        [SerializeField] CharacterClass characterClass;
        [SerializeField] Progression progression = null;
        [SerializeField] GameObject levelUpParticleEffect = null;
        [SerializeField] bool shouldUseModifier=false;

        LazyValue<int> currentLevel ;
        public event Action onLevelUp;
        Experience experience;

        void Awake()
        {
            experience = GetComponent<Experience>();
            currentLevel = new LazyValue<int>(CalculateLevel);


        }
        void Start()
        {
            currentLevel.ForceInit();
        }

        private void OnDisable()
        {
            if (experience != null)
            {
                experience.onExperienceGained -= UpdateLevel;
            }
        }

        private void OnEnable()
        {
            if (experience != null)            
            {
                experience.onExperienceGained += UpdateLevel;
            }

        }


        private void UpdateLevel()
        {
            int newLevel = CalculateLevel();
            if (newLevel > currentLevel.value)
            {
                currentLevel.value = newLevel;
                LevelUpEffect();
                if (onLevelUp != null)
                {
                    onLevelUp();
                }
            }
        }

        private void LevelUpEffect()
        {
            if (levelUpParticleEffect != null)
            {
                Instantiate(levelUpParticleEffect, transform);
            }
            print("Level up!");
            print(GetComponent<BaseStats>().GetStat(Stat.Health) + "HPMAX");
            
        }

        public float GetStat(Stat stat)
        {
            return (progression.GetStat(stat, characterClass, currentLevel.value)+GetAdditiveModifier(stat))*(1+GetPercentageModifier(stat)/100);
        }

        private float GetPercentageModifier(Stat stat)
        {
            float total = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetPercentageModifier(stat))
                {
                    total += modifier;
                }
            }
            return total;
        }

        private float GetAdditiveModifier(Stat stat)
        {
            if (!shouldUseModifier) return 0;
            float total = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetAdditiveModifier(stat))
                {
                    total += modifier;
                }
            }
            return total;
        }

        public int GetLevel()
        {
            return currentLevel.value;
        }
        public int CalculateLevel()
        {
            Experience experience = GetComponent<Experience>();
            if (experience == null) return startingLevel;

            
            float currentXP = GetComponent<Experience>().GetExperiencePoints();
            int penultimateLevel = progression.GetLevels(Stat.ExperienceToLevelUp, characterClass);
            for (int level = 1; level <= penultimateLevel; level++)         
            {
                float XPToLevelUp = progression.GetStat(Stat.ExperienceToLevelUp, characterClass, level);
                if (currentXP < XPToLevelUp)
                {
                    return level;
                }
            }
            return penultimateLevel;
        }
    }
}