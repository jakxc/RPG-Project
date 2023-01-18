using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;

namespace RPG.Dialogue.Editor
{
    public class DialogueEditor : EditorWindow {

        Dialogue selectedDialogue = null;
        GUIStyle nodeStyle = null;
        DialogueNode draggedNode = null;
        Vector2 draggingOffset;

        [MenuItem("Window/DialogueEditor")]
        private static void ShowEditorWindow() 
        {
            var window = GetWindow(typeof(DialogueEditor), false, "Dialogue Editor");
        }

        [OnOpenAssetAttribute(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            Dialogue dialogue = EditorUtility.InstanceIDToObject(instanceID) as Dialogue;
            if (dialogue != null)
            {
                ShowEditorWindow();
                return true;
            }
            return false;
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            nodeStyle = new GUIStyle();
            nodeStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;
            nodeStyle.padding = new RectOffset(20, 20, 20, 20);
            nodeStyle.border = new RectOffset(12, 12, 12, 12);
        }

        private void OnSelectionChanged()
        {
            Dialogue newDialogue = Selection.activeObject as Dialogue;
            if (newDialogue != null)
            {
                selectedDialogue = newDialogue;
                Repaint();
            }
        }

        // Updates UI
        private void OnGUI() 
        {
            if (selectedDialogue == null)
            {
                Debug.Log("No dialogue selected");
            }
            else
            {
                ProcessEvents();
                foreach (DialogueNode node in selectedDialogue.GetAllNodes())
                {
                    OnGUINode(node);
                }
            }
        }

        private void ProcessEvents()
        {
            if (Event.current.type == EventType.MouseDown && draggedNode == null)
            {
                draggedNode = GetNodeAtPoint(Event.current.mousePosition);
                if (draggedNode != null)
                {
                    draggingOffset = draggedNode.rect.position - Event.current.mousePosition;
                }
            }
            else if (Event.current.type == EventType.MouseDrag && draggedNode != null)
            {
                Undo.RecordObject(selectedDialogue, "Move Dialogue Node");
                draggedNode.rect.position = Event.current.mousePosition + draggingOffset;
                GUI.changed = true; // Triggers OnGUI to be called again
            }
            else if (Event.current.type == EventType.MouseUp && draggedNode != null)
            {
                draggedNode = null;         
            }

        }

        private void OnGUINode(DialogueNode node)
        {
            GUILayout.BeginArea(node.rect, nodeStyle);
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Node", EditorStyles.whiteLabel);
            string newText = EditorGUILayout.TextField(node.text);
            string newUniqueID = EditorGUILayout.TextField(node.uniqueID);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(selectedDialogue, "Update Dialogue Text");
                node.text = newText;
                node.uniqueID = newUniqueID;
            }
            
            foreach (DialogueNode childNode in selectedDialogue.GetAllChildren(node))
            {
                EditorGUILayout.LabelField(childNode.text);
            }
          
            GUILayout.EndArea();
        }

        private DialogueNode GetNodeAtPoint(Vector2 point)
        {
            DialogueNode foundNode = null; // Do not return the first found node as there could be another node overlapping it
            foreach (DialogueNode node in selectedDialogue.GetAllNodes())
            {
                if (node.rect.Contains(point))
                {
                    foundNode = node;
                }
            }
            // Return foundNode only after finishing iterating all the nodes (prevent missing out on the overlapping nodes with higher index)
            return foundNode;
        }
    }
}
