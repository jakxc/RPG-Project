using System;
using System.Collections.Generic;
using GameDevTV.Inventories;
using GameDevTV.Saving;
using UnityEngine;

namespace RPG.Quests
{
    public class QuestList : MonoBehaviour, ISaveable
    {
        List<QuestStatus> statuses = new List<QuestStatus>();

        public event Action onUpdate;

        public void AddQuest(Quest quest)
        {
            if (HasQuest(quest)) return;
            
            QuestStatus newStatus = new QuestStatus(quest);
            statuses.Add(newStatus);
            if (onUpdate != null)
            {
                onUpdate();
            }
        }

        public bool HasQuest(Quest quest)
        {
            return GetQuestStatus(quest) != null;
        }

        public IEnumerable<QuestStatus> GetStatuses()
        {
            return statuses;
        }

        public void CompleteObjective(Quest quest, string objective)
        {
            QuestStatus status = GetQuestStatus(quest);
            status.CompleteObjective(objective);
            if (status.IsComplete())
            {
                GiveReward(quest);
            }
            if (onUpdate != null)
            {
                onUpdate();
            }
        }

        // PRIVATE 

         private void GiveReward(Quest quest)
        {
            foreach (var reward in quest.GetRewards())
            {
                /* If reward is not stackable and more than 1 is given, iterate through the quantity 
                and add them individually*/
                if (!reward.item.IsStackable() && reward.quantity > 1)
                {
                    for (int i = 0; i < reward.quantity; i++)
                    {
                        ProcessReward(reward.item, 1);
                    }
                }
                // else add them as stackables
                else
                {
                    ProcessReward(reward.item, reward.quantity);
                }
            }
        }

        private void ProcessReward(InventoryItem reward, int quantity)
        {
            bool success = GetComponent<Inventory>().AddToPreferredSlot(reward, quantity);
            if (!success)
            {
                Debug.Log("Not enough space in inventory for this reward, please make space and try again.");
                //GetComponent<ItemDropper>().DropItem(reward, quantity);
            }
        }

        private QuestStatus GetQuestStatus(Quest quest)
        {
            foreach (QuestStatus status in statuses)
            {
                if (status.GetQuest() == quest)
                {
                    return status;
                }
            }
            return null;
        }

        // ISaveable interface
        public object CaptureState()
        {
            List<object> state = new List<object>();

            foreach (QuestStatus status in statuses)
            {
                state.Add(status.CaptureState());
            }
            return state;
        }

        public void RestoreState(object state)
        {
            List<object> stateList = state as List<object>;
            if (stateList == null) return;

            statuses.Clear(); // Rebuild statuses list
            foreach (object objectState in stateList)
            {
                statuses.Add(new QuestStatus(objectState));
            }
        }
    }
}