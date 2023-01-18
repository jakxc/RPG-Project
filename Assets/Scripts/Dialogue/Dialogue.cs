using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Dialogue
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue", order = 0)]
    public class Dialogue : ScriptableObject
    {
        [SerializeField] List<DialogueNode> nodes = new List<DialogueNode>();
        Dictionary<string, DialogueNode> nodeLookup = new Dictionary<string, DialogueNode>();

#if UNITY_EDITOR
        private void Awake() 
        {
            if (nodes.Count == 0)
            {
                nodes.Add(new DialogueNode());
            }

            OnValidate();
        }
#endif

        // Called when values of SO are changed or when it is loaded
        private void OnValidate() 
        {
            nodeLookup.Clear();
            foreach (DialogueNode node in GetAllNodes())
            {
                nodeLookup[node.uniqueID] = node;
            }
        }

        // Return IEnumerable because it allows the changing of the nodes type from List to another iterable type
        public IEnumerable<DialogueNode> GetAllNodes()
        {
            return nodes;
        }

        public DialogueNode GetRootNode()
        {
            return nodes[0];
        }

        public IEnumerable<DialogueNode> GetAllChildren(DialogueNode parentNode)
        {
            foreach (string childID in parentNode.children)
            {
                if (nodeLookup.ContainsKey(childID)) // If Dictionary contains this key return it
                {
                    yield return nodeLookup[childID]; // Repeats method and returns the next child to the node 
                }  
            }
        }
    }
}
