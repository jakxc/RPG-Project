using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Quests;

namespace RPG.UI.Quests
{
    public class QuestListUI : MonoBehaviour
    {
        [SerializeField] QuestItemUI questPrefab;
        QuestList questList;

        // Start is called before the first frame update
        void Start()
        {
            questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
            questList.onUpdate += Redraw;
            Redraw();
        }

        private void Redraw()
        {
            // Remove dangling placeholder quests
            foreach (Transform item in transform)
            {
                Destroy(item.gameObject);
            }

            foreach (QuestStatus status in questList.GetStatuses())
            {
                QuestItemUI questItemInstance = Instantiate<QuestItemUI>(questPrefab, transform);
                questItemInstance.SetUp(status);
            }
        }
    }
}
