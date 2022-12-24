using System;
using GameDevTV.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace RPG.Stats
{
    public class BaseStats : MonoBehaviour
    {
        [SerializeField] [Range(1, 99)] int startingLevel = 1;
        [SerializeField] bool shouldUseModifiers = false;
        [SerializeField] CharacterClass characterClass;
        [SerializeField] Progression progression = null; 
        [SerializeField] UnityEvent onLevelUp;
        Experience experience; 
        public event Action onLevelUpdated; 
        LazyValue<int> currentLevel;
        
        private void Awake() 
        {
            experience = GetComponent<Experience>();
            currentLevel = new LazyValue<int>(CalculateLevel);
        }

        private void Start() 
        {
            currentLevel.ForceInit();
        }

        private void OnEnable() 
        {
            if (experience != null)
            {
                experience.onExperienceGained += UpdateLevel;
            }
        }

        private void OnDisable() 
        {
            if (experience != null)
            {
                experience.onExperienceGained -= UpdateLevel;
            }
        }

        private void UpdateLevel() 
        {
            int newLevel = CalculateLevel();
            if (newLevel > currentLevel.value)
            {
                currentLevel.value = newLevel;
                onLevelUp.Invoke();
                onLevelUpdated();
            }
        }

        public float GetStat(Stat stat)
        {
            /*Add the additive modifiers before calculating the % added by the percentage modifiers.
            E.g if additive modifier is 20 and percentage modifier is 10, when added to base stat of 100,
            it would get the 10% of 100+20 instead of the base stat of 100*/
            return (GetBaseStat(stat) + GetAdditiveModifiers(stat)) * (1 + GetPercentageModifiers(stat) / 100);
        }

        private float GetBaseStat(Stat stat)
        {
            return progression.GetStat(stat, characterClass, GetLevel());
        }

        private float GetPercentageModifiers(Stat stat)
        {
            if (!shouldUseModifiers) return 0;

            float total = 0f;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetPercentageModifiers(stat))
                {
                    total += modifier;
                }
            }
            return total;
        }

        private float GetAdditiveModifiers(Stat stat)
        {
            if (!shouldUseModifiers) return 0;

            float total = 0f;

            /*Find all the IModifierProvider and add all the additivemodifiers of stat (e.g health, damage etc.).
            This total represents the added bonuses that these components give (e.g Sword damage + base damage of player)*/
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetAdditiveModifiers(stat))
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

        private int CalculateLevel()
        {
            Experience experience = GetComponent<Experience>();
            
            if (experience == null)
            {
                return startingLevel;
            }
            
            float currentXP = experience.GetExperience();
            // Get the level one below max level
            int penultimateLevel = progression.GetLevels(Stat.ExperienceToLevelUp, characterClass);
            
            for (int level = 1; level <= penultimateLevel; level++)
            {
                float XPToLevelUp = progression.GetStat(Stat.ExperienceToLevelUp, characterClass, level);
                // If the currentXP is less than the cumulative XP required to reach level, return the level
                if (XPToLevelUp > currentXP)
                {
                    return level;
                }
            }
            
            // Else if the currentXP is greater than the mx XPToLevelUp, return the max level (cannot go beyond max level)
            return penultimateLevel + 1;
        }
    }   
}
