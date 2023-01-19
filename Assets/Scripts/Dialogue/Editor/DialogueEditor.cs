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
        [NonSerialized] GUIStyle nodeStyle = null;
        [NonSerialized] DialogueNode nodeToDrag = null;
        [NonSerialized] Vector2 draggingOffset;
        [NonSerialized] DialogueNode nodeToCreate = null;
        [NonSerialized] DialogueNode nodeToDelete = null;
        [NonSerialized] DialogueNode nodeToLink = null;

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
                    DrawConnections(node);
                }
                foreach (DialogueNode node in selectedDialogue.GetAllNodes())
                {
                    DrawNode(node);
                }
                if (nodeToCreate != null)
                {
                    Undo.RecordObject(selectedDialogue, "Added Dialogue Node");
                    selectedDialogue.CreateNode(nodeToCreate);
                    nodeToCreate = null;
                }
                if (nodeToDelete != null)
                {
                    Undo.RecordObject(selectedDialogue, "Deleted Dialogue Node");
                    selectedDialogue.DeleteNode(nodeToDelete);
                    nodeToDelete = null;
                }
            }
        }

        private void ProcessEvents()
        {
            if (Event.current.type == EventType.MouseDown && nodeToDrag == null)
            {
                nodeToDrag = GetNodeAtPoint(Event.current.mousePosition);
                if (nodeToDrag != null)
                {
                    draggingOffset = nodeToDrag.rect.position - Event.current.mousePosition;
                }
            }
            else if (Event.current.type == EventType.MouseDrag && nodeToDrag != null)
            {
                Undo.RecordObject(selectedDialogue, "Move Dialogue Node");
                nodeToDrag.rect.position = Event.current.mousePosition + draggingOffset;
                GUI.changed = true; // Triggers OnGUI to be called again
            }
            else if (Event.current.type == EventType.MouseUp && nodeToDrag != null)
            {
                nodeToDrag = null;         
            }

        }

        private void DrawNode(DialogueNode node)
        {
            GUILayout.BeginArea(node.rect, nodeStyle);
            EditorGUI.BeginChangeCheck();

            string newText = EditorGUILayout.TextField(node.text);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(selectedDialogue, "Update Dialogue Text");
                node.text = newText;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                nodeToCreate = node;
            }
            DrawLinkButtons(node);
            if (GUILayout.Button("-"))
            {
                nodeToDelete = node;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void DrawLinkButtons(DialogueNode node)
        {
            if (nodeToLink == null)
            {
                if (GUILayout.Button("edit links"))
                {
                    nodeToLink = node;
                }
            }
            else if (nodeToLink == node)
            {
                if (GUILayout.Button("cancel"))
                {
                    nodeToLink = null;
                }
            }
            else if (nodeToLink.children.Contains(node.uniqueID))
            {
                 if (GUILayout.Button("unlink"))
                {
                    Undo.RecordObject(selectedDialogue, "Add Dialogue Link");
                    nodeToLink.children.Remove(node.uniqueID);
                    nodeToLink = null;
                }
            }
            else
            {
                if (GUILayout.Button("link"))
                {
                    Undo.RecordObject(selectedDialogue, "Add Dialogue Link");
                    nodeToLink.children.Add(node.uniqueID);
                    nodeToLink = null;
                }
            }
        }

        private void DrawConnections(DialogueNode node)
        {
            Vector3 startPosition = new Vector2(node.rect.xMax, node.rect.center.y);
            foreach (DialogueNode childNode in selectedDialogue.GetAllChildren(node))
            {
                Vector3 endPosition = new Vector2(childNode.rect.xMin, childNode.rect.center.y);
                Vector3 controlPointOffset = endPosition - startPosition;
                controlPointOffset.y = 0;
                controlPointOffset.x *= 0.8f;
                Handles.DrawBezier(
                    startPosition, endPosition,
                    startPosition + controlPointOffset, 
                    endPosition - controlPointOffset, 
                    Color.white, null, 4f);
            }
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
