using UnityEngine;

namespace RPG.Core
{
    public class ActionScheduler : MonoBehaviour
    {
        IAction currentAction;
        public void StartAction(IAction action)
        {
            // If current action is already the action that is called to start, return
            if (currentAction == action) return;

            // Cancel the current action if it does not equal the action being started and its not null
            if (currentAction != null)
            {
                currentAction.Cancel();
            }
            currentAction = action;
        }

        public void CancelCurrentAction()
        {
            StartAction(null);
        }
    }
}
