using System.Collections.Generic;
using UnityEngine;

namespace RPG.Stats
{
    [CreateAssetMenu(fileName = "Progression", menuName = "Stats/New Progression", order = 0)]
    public class Progression : ScriptableObject {
        [SerializeField] ProgressionCharacterClass[] characterClasses;
        Dictionary<CharacterClass, Dictionary<Stat, float[]>> searchTable = null;

        public float GetStat(Stat stat, CharacterClass characterClass, int level)
        {
            GenerateSearchTable();

            float[] levels =  searchTable[characterClass][stat];
           
            if (levels.Length < level) 
            {   
                return 0;
            }

            return levels[level - 1];
        }

        public int GetLevels(Stat stat, CharacterClass characterClass)
        {
            GenerateSearchTable();

            float[] levels =  searchTable[characterClass][stat];
            
            return levels.Length;
        }

        private void GenerateSearchTable()
        {
            if (searchTable != null) return;

            searchTable = new Dictionary<CharacterClass, Dictionary<Stat, float[]>>();

            foreach(ProgressionCharacterClass progressionClass in characterClasses)
            {
                var statSearchTable = new Dictionary<Stat, float[]>();
                
                foreach(ProgressionStat progressionStat in progressionClass.stats)
                {
                    statSearchTable[progressionStat.stat] = progressionStat.levels;
                }

                searchTable[progressionClass.characterClass] = statSearchTable;
            }
        }
        
        [System.Serializable]
        class ProgressionCharacterClass
        {
            public CharacterClass characterClass;
            public ProgressionStat[] stats;
        }

        [System.Serializable]
        class ProgressionStat
        {
            public Stat stat;
            public float[] levels;
        }
    }
}
