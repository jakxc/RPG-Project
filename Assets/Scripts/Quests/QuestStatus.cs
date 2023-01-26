using System.Collections.Generic;

namespace RPG.Quests
{
    public class QuestStatus
    {
        Quest quest;
        List<string> completedObjectives = new List<string>();
        private object objectState;

        [System.Serializable]
        class QuestStatusRecord
        {
            public string questID;
            public List<string> completeObjectives;
        }

        // Constructor
        public QuestStatus(Quest quest)
        {
            this.quest = quest;
        }

        public QuestStatus(object objectState)
        {
            QuestStatusRecord state = objectState as QuestStatusRecord;
            quest = Quest.GetByID(state.questID);
            completedObjectives = state.completeObjectives;
        }

        public Quest GetQuest()
        {
            return quest;
        }

        public int GetCompletedCount()
        {
            return completedObjectives.Count;
        }

        public bool IsObjectiveComplete(string objective)
        {
            return completedObjectives.Contains(objective);
        }

        public void CompleteObjective(string objective)
        {
            if (quest.HasObjective(objective) && !completedObjectives.Contains(objective))
            {
                completedObjectives.Add(objective);
            }
        }

        public object CaptureState()
        {
            QuestStatusRecord state = new QuestStatusRecord();
            state.questID = quest.GetQuestID();
            state.completeObjectives = completedObjectives;
            return state;
        }

        public bool IsComplete()
        {
            foreach (var objective in quest.GetObjectives())
            {
                if (!completedObjectives.Contains(objective.reference))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
