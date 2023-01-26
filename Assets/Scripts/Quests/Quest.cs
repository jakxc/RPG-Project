using System;
using System.Collections;
using System.Collections.Generic;
using GameDevTV.Inventories;
using UnityEngine;

namespace RPG.Quests
{
    [CreateAssetMenu(fileName = "Quest", menuName = "RPG Project/Quest", order = 0)]
    public class Quest : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] string questID = null;
        [SerializeField] List<Objective> objectives = new List<Objective>();
        [SerializeField] List<Reward> rewards = new List<Reward>();

        [System.Serializable]
        public class Reward
        {
            [Min(1)]public int quantity; // Quantity of reward item should not be smaller than 1
            public InventoryItem item;
        }

        [System.Serializable]
        public class Objective
        {
            public string reference;
            public string description;
        }

        public string GetTitle()
        {
            return name;
        }

        public string GetQuestID()
        {
            return questID;
        }

        public int GetObjectiveCount()
        {
            return objectives.Count;
        }

        public IEnumerable<Objective> GetObjectives()
        {
            return objectives;
        }

        public IEnumerable<Reward> GetRewards()
        {
            return rewards;
        }

        public bool HasObjective(string objectiveRef)
        {
            foreach (var objective in objectives)
            {
                if (objective.reference == objectiveRef)
                {
                    return true;
                }
            }
            return false;
        }

        public static Quest GetByID(string questID)
        {
            foreach (Quest quest in Resources.LoadAll<Quest>(""))
            {
                if (quest.questID == questID)
                {
                    return quest;
                }
            };
            return null;
        }

        public void OnBeforeSerialize()
        {
            // Generate and save a new UUID if this is blank.
            if (string.IsNullOrWhiteSpace(questID))
            {
                questID = System.Guid.NewGuid().ToString();
            }
        }

        public void OnAfterDeserialize()
        {
        }
    }
}
