using UnityEngine;

namespace RPG.Quests
{
    public class QuestCompletion : MonoBehaviour
    {
        [SerializeField] Quest quest;
        [SerializeField] string objective;

        public void CompleteObject()
        {
           QuestList questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
           questList.CompleteObjective(quest, objective); 
        }
    }
}
