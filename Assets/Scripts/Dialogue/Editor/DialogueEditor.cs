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
        Vector2 scrollPosition;
        [NonSerialized] GUIStyle nodeStyle = null;
        [NonSerialized] GUIStyle playerNodeStyle = null;
        [NonSerialized] DialogueNode nodeToDrag = null;
        [NonSerialized] Vector2 draggingOffset;
        [NonSerialized] DialogueNode nodeToCreate = null;
        [NonSerialized] DialogueNode nodeToDelete = null;
        [NonSerialized] DialogueNode nodeToLink = null;
        [NonSerialized] bool isDraggingCanvas = false;
        [NonSerialized] Vector2 draggingCanvasOffset; 

        const float canvasSize = 4000;
        const float backgroundSize = 50;

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

            playerNodeStyle = new GUIStyle();
            playerNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
            playerNodeStyle.padding = new RectOffset(20, 20, 20, 20);
            playerNodeStyle.border = new RectOffset(12, 12, 12, 12);
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

        // Callback method that updates UI
        private void OnGUI() 
        {
            if (selectedDialogue == null)
            {
                Debug.Log("No dialogue selected");
            }
            else
            {
                ProcessEvents();

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
               
                Rect canvas = GUILayoutUtility.GetRect(canvasSize, canvasSize);
                Texture2D backgroundTexture = Resources.Load("background") as Texture2D;
                Rect textCoords = new Rect(0, 0, canvasSize/backgroundSize, canvasSize/backgroundSize);
                GUI.DrawTextureWithTexCoords(canvas, backgroundTexture, textCoords);

                foreach (DialogueNode node in selectedDialogue.GetAllNodes())
                {
                    DrawConnections(node);
                }
                foreach (DialogueNode node in selectedDialogue.GetAllNodes())
                {
                    DrawNode(node);
                }

                EditorGUILayout.EndScrollView();

                if (nodeToCreate != null)
                {
                    selectedDialogue.CreateNode(nodeToCreate);
                    nodeToCreate = null;
                }
                if (nodeToDelete != null)
                {
                    selectedDialogue.DeleteNode(nodeToDelete);
                    nodeToDelete = null;
                }
            }
        }

        private void ProcessEvents()
        {
            if (Event.current.type == EventType.MouseDown && nodeToDrag == null)
            {
                nodeToDrag = GetNodeAtPoint(Event.current.mousePosition + scrollPosition);
                if (nodeToDrag != null)
                {
                    draggingOffset = nodeToDrag.GetRect().position - Event.current.mousePosition;
                    Selection.activeObject = nodeToDrag;
                }
                else
                {
                    isDraggingCanvas = true;
                    draggingCanvasOffset = Event.current.mousePosition + scrollPosition; // The position of mouse on canvas
                    Selection.activeObject = selectedDialogue;
                }
            }
            else if (Event.current.type == EventType.MouseDrag && nodeToDrag != null)
            {
               
                nodeToDrag.SetPosition(Event.current.mousePosition + draggingOffset);
               
                GUI.changed = true; // Triggers OnGUI to be called again
            }
            else if (Event.current.type == EventType.MouseDrag && isDraggingCanvas)
            {
                scrollPosition = draggingCanvasOffset - Event.current.mousePosition;

                GUI.changed = true; // Triggers OnGUI to be called again
            }
            else if (Event.current.type == EventType.MouseUp && nodeToDrag != null)
            {
                nodeToDrag = null;     
            }
            else if (Event.current.type == EventType.MouseUp && isDraggingCanvas)
            {
                isDraggingCanvas = false;
            }

        }

        private void DrawNode(DialogueNode node)
        {
            GUIStyle style = nodeStyle;
            if (node.GetSpeaker() == Speaker.Player)
            {
                style = playerNodeStyle;
            }

            GUILayout.BeginArea(node.GetRect(), style);

            node.SetText(EditorGUILayout.TextField(node.GetText()));

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
            else if (nodeToLink.GetChildren().Contains(node.name))
            {
                 if (GUILayout.Button("unlink"))
                {
                    nodeToLink.RemoveChild(node.name);
                    nodeToLink = null;
                }
            }
            else
            {
                if (GUILayout.Button("link"))
                {
                    nodeToLink.AddChild(node.name);
                    nodeToLink = null;
                }
            }
        }

        private void DrawConnections(DialogueNode node)
        {
            Vector3 startPosition = new Vector2(node.GetRect().xMax, node.GetRect().center.y);
            foreach (DialogueNode childNode in selectedDialogue.GetAllChildren(node))
            {
                Vector3 endPosition = new Vector2(childNode.GetRect().xMin, childNode.GetRect().center.y);
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
                if (node.GetRect().Contains(point))
                {
                    foundNode = node;
                }
            }
            // Return foundNode only after finishing iterating all the nodes (prevent missing out on the overlapping nodes with higher index)
            return foundNode;
        }
    }
}
