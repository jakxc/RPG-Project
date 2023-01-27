using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RPG.Core;
using UnityEngine;

namespace RPG.Dialogue
{
    public class PlayerConversant : MonoBehaviour
    {
        [SerializeField] string playerName;
        AIConversant currentConversant = null;
        Dialogue currentDialogue;
        DialogueNode currentNode = null;
        bool isSelecting = false;
        public event Action onDialogueUpdated;

        public void StartDialogue(AIConversant newConversant, Dialogue newDialogue)
        {
            currentConversant = newConversant;
            currentDialogue = newDialogue;
            currentNode = currentDialogue.GetRootNode();
            TriggerEnterAction();
            onDialogueUpdated();
        }

        public void QuitDialogue()
        {
            currentDialogue = null;
            TriggerExitAction();
            currentNode = null;
            isSelecting = false;
            currentConversant = null;
            onDialogueUpdated();
        }

        public bool IsActive()
        {
            return currentDialogue != null;
        }

        public bool IsSelectingOption()
        {
            return isSelecting;
        }

        public string GetText()
        {
            if (currentNode == null) 
            {
                return "";
            }
           
            return currentNode.GetText();
        }

        public IEnumerable<DialogueNode> GetOptions()
        {
            return FilterOnCondition(currentDialogue.GetPlayerChildren(currentNode));
        }

        public void SelectOption(DialogueNode chosenNode)
        {
            currentNode = chosenNode;
            TriggerEnterAction();
            isSelecting = false;
            NextDialogue();
        }

        public void NextDialogue()
        {
            // If the dialogue is by player and there is more than one response, enter selection mode in dialogue
            int numOfPlayerOptions = FilterOnCondition(currentDialogue.GetPlayerChildren(currentNode)).Count();
            if (numOfPlayerOptions > 0)
            {
                isSelecting = true;
                TriggerExitAction();
                onDialogueUpdated();
                return;
            }

            DialogueNode[] childNodes = FilterOnCondition(currentDialogue.GetNPCChildren(currentNode)).ToArray();
            int randomIndex = UnityEngine.Random.Range(0, childNodes.Count());
            TriggerEnterAction();
            currentNode = childNodes[randomIndex];
            TriggerExitAction();
            onDialogueUpdated();
        }

        public bool HasNext()
        {
            // If the current node has children, return true as it can progress to next dialogue, otherwise return false
            return FilterOnCondition(currentDialogue.GetAllChildren(currentNode)).Count() > 0;
        }

        public IEnumerable<DialogueNode> FilterOnCondition(IEnumerable<DialogueNode> inputNodes)
        {
            foreach (DialogueNode node in inputNodes)
            {
                if (node.CheckCondition(GetEvaluators()))
                {
                    yield return node;
                }
            }
        }

        // PRIVATE

        private IEnumerable<IPredicateEvaluator> GetEvaluators()
        {
            return GetComponents<IPredicateEvaluator>();
        }

        private void TriggerEnterAction()
        {
            if (currentNode != null)
            {
               TriggerAction(currentNode.GetOnEnterAction());
            }        
        }

        private void TriggerExitAction()
        {
            if (currentNode != null)
            {
                TriggerAction(currentNode.GetOnExitAction());
            }     
        }

        private void TriggerAction(string action)
        {
            if (action == "") return;

            foreach (DialogueTrigger trigger in currentConversant.GetComponents<DialogueTrigger>())
            {
                trigger.Trigger(action);
            }
        }

        public string GetCurrentConversantName()
        {
            if (isSelecting)
            {
                return playerName;
            }
            else
            {
                return currentConversant.GetName();
            }
        }
    }
}
