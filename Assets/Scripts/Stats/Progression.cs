using System.Collections.Generic;
using UnityEngine;

namespace RPG.Stats
{
    [CreateAssetMenu(fileName = "Progression", menuName = "Stats/New Progression", order = 0)]
    // This is where all the progression data is stored (if a class is a specific level, the amount of max health and damage they should do)
    public class Progression : ScriptableObject {
        [SerializeField] ProgressionCharacterClass[] characterClasses;

        /* Stores CharacterClass as key and returns another Dictionary containing Stat as key and array of
        floats that represent the stat for each level*/
        Dictionary<CharacterClass, Dictionary<Stat, float[]>> searchTable = null;

        public float GetStat(Stat stat, CharacterClass characterClass, int level)
        {
            GenerateSearchTable();

            float[] statValues =  searchTable[characterClass][stat];
           
            if (statValues.Length < level) 
            {   
                return 0;
            }

            return statValues[level - 1];
        }

        public int GetLevels(Stat stat, CharacterClass characterClass)
        {
            GenerateSearchTable();

            float[] levels =  searchTable[characterClass][stat]; // Array of different values for the stat for each level
            
            // Return the size of the array (max level)
            return levels.Length;
        }

        private void GenerateSearchTable()
        {
            if (searchTable != null) return;

            searchTable = new Dictionary<CharacterClass, Dictionary<Stat, float[]>>();

            /*For each ProgressionCharacterClass, generate a new Dictionary with the class's stat as the key
            and an array for floats which have the value of the stat for each level*/
            foreach(ProgressionCharacterClass progressionClass in characterClasses)
            {
                var statSearchTable = new Dictionary<Stat, float[]>();
                
                foreach(ProgressionStat progressionStat in progressionClass.stats)
                {
                    statSearchTable[progressionStat.stat] = progressionStat.values;
                }

                searchTable[progressionClass.characterClass] = statSearchTable;
            }
        }
        
        [System.Serializable]
        // This class represents the stat of each CharacterClass (e.g Warrior and Archer have different health/damage stats each level)
        class ProgressionCharacterClass
        {
            public CharacterClass characterClass;
            public ProgressionStat[] stats; // This is set as type array as ProgressionStat will be different for each level
        }

        [System.Serializable]
        // This class represents each stat (health, experience reward etc) and the value they equal each level
        class ProgressionStat
        {
            public Stat stat;
            public float[] values; // The value of the stat based on level
        }
    }
}
