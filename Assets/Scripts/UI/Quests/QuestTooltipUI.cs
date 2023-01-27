using RPG.Quests;
using UnityEngine;
using TMPro;
using System;

namespace RPG.UI.Quests
{
    public class QuestTooltipUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] Transform objectiveContainer;
        [SerializeField] GameObject objectivePrefab;
        [SerializeField] GameObject objectIncompletePrefab;
        [SerializeField] TextMeshProUGUI rewardText;
        
        public void SetUp(QuestStatus status)
        {
            Quest quest = status.GetQuest();
            title.text = quest.GetTitle();

            foreach (Transform item in objectiveContainer)
            {
                Destroy(item.gameObject);
            }

            foreach (var objective in quest.GetObjectives())
            {
                GameObject prefab = objectIncompletePrefab;
                if (status.IsObjectiveComplete(objective.reference))
                {
                    prefab = objectivePrefab;
                }
                GameObject objectiveInstance = Instantiate(prefab, objectiveContainer);
                TextMeshProUGUI objectiveText = objectiveInstance.GetComponentInChildren<TextMeshProUGUI>();
                objectiveText.text = objective.description;
            }
            rewardText.text = GetRewardText(quest);
        }

        private string GetRewardText(Quest quest)
        {
            string rewardText = "";
            foreach (var reward in quest.GetRewards())
            {
                if (rewardText != "")
                {
                    rewardText += ", ";
                }
                if (reward.quantity > 1)
                {
                    rewardText += reward.quantity + " ";
                }
                rewardText += reward.item.GetDisplayName();
            }
            
            if (rewardText == "") 
            {
                return "No reward";
            }
            rewardText += ".";
            return rewardText;
        }
    }
}
