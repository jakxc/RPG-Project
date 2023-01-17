using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using RPG.Movement;
using RPG.Attributes;
using GameDevTV.Inventories;

namespace RPG.Control 
{
    public class PlayerController : MonoBehaviour
    {
        Health health;

        [System.Serializable]
        struct CursorMapping
        {
            public CursorType type;
            public Texture2D texture;
            public Vector2 hotspot;
        }

        [SerializeField] CursorMapping[] cursorMappings = null; // Array of different cursors for each interaction
        [SerializeField] float maxNavMeshProjectionDistance = 1f;
        [SerializeField] float rayCastRadius = 0.5f;
        [SerializeField] int actionStoreSize = 6;

        bool isDraggingUI = false;

        void Awake() 
        {
            health = GetComponent<Health>();
        }

        void Update()
        {
            CheckSpecialAbilityKeys();
            if (InteractWithUI()) return;
            
            if (health.IsDead()) 
            {
                SetCursor(CursorType.None); // If player is dead, set cursor type to CursorType.None
                return;
            }

            if (InteractWithComponent()) return;
            if (InteractWithMovement()) return;

            SetCursor(CursorType.None);
        }

        private bool InteractWithComponent()
        {
            RaycastHit[] hits = RaycastAllSorted(); // hits array sorted by distance to ray origin

            foreach (RaycastHit hit in hits)
            {
                /*Get all IRaycastable that is hit and call HandleRaycast() from thet specific
                IRaycastable hit. Set the cursor type to the cursor type specified in IRaycastables*/
                IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();
                foreach (IRaycastable raycastable in raycastables)
                {
                    if (raycastable.HandleRaycast(this))
                    {
                        SetCursor(raycastable.GetCursorType());
                        return true;
                    }
                }
            }

            return false;
        }

        RaycastHit[] RaycastAllSorted()
        {
            // Uses SphereCast to allow of ease of clicking on smaller GameObjects (pickups, enemies etc.)
            RaycastHit[] hits = Physics.SphereCastAll(GetMouseRay(), rayCastRadius);
            float[] distances = new float[hits.Length];

            // For all the hits in the hits array, add the distance of the ray to the hit to the distances array
            for (int i = 0; i < hits.Length; i++)
            {
                distances[i] = hits[i].distance;
            }
            
            // Sort the hits array based on distances (so closest to ray origin will be hits[0])
            Array.Sort(distances, hits);

            return hits;
        } 
        
        private bool InteractWithUI()
        {   
            // If mouse button is released, then not draggining anymore
            if (Input.GetMouseButtonUp(0))
            {
                isDraggingUI = false;
            }

            // Returns true if cursor is over UI
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // If mouse button is down, then dragging is in place
                if (Input.GetMouseButtonDown(0))
                {
                    isDraggingUI = true;
                }
                SetCursor(CursorType.UI);
                return true;
            }
            if (isDraggingUI) return true; // It is still interacting with UI even when not over UI, if mouse is still down (dragging)
            
            return false;
        }

        private bool InteractWithMovement()
        { 
            Vector3 target;
            bool hasHit = RaycastNavMesh(out target);
            if (hasHit)
            {
                if(!GetComponent<Mover>().CanMoveTo(target)) return false;
                if (Input.GetMouseButton(0))
                {
                    // If can move to and input is right mouse button, move to target at 1x base speed
                    GetComponent<Mover>().StartMoveAction(target, 1f);
                }
                SetCursor(CursorType.Movement);
                return true;
            }

            return false;
        }

        // This method is used to toggle whether the cursor is over NavMesh
        private bool RaycastNavMesh(out Vector3 target)
        {
            target = new Vector3();
            RaycastHit hit;
            //  Returns true when the ray intersects any collider, otherwise false. 
            bool hasHit = Physics.Raycast(GetMouseRay(), out hit);
            if(!hasHit) return false;
            
            NavMeshHit navMeshHit;
            
            /* SamplePosition finds the nearest point based on the NavMesh (terrain etc) within a specified range.
            In this case, it will return false if this is not within maxNavMeshProjectionDistance 
            from hit.point */
            bool hasCastToNavMesh = NavMesh.SamplePosition(
                hit.point, out navMeshHit, maxNavMeshProjectionDistance, NavMesh.AllAreas);
            if (!hasCastToNavMesh) return false;

            target = navMeshHit.position; // Position on the NavMesh that has been cast to (hit)

            return true;
        }

        private Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        private void SetCursor(CursorType type)
        {
            // Find CursorMapping that has matching CursorType as type in the cursorMappings array 
            CursorMapping mapping = GetCursorMapping(type);
            // and set the cursor to that type 
            Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
        }

        private CursorMapping GetCursorMapping(CursorType type)
        {
            /* For each element in cursorMappings, if the element has matching type, return 
            the mapping. Otherwise, return the first element (CursorType.None) */
            foreach (CursorMapping mapping in cursorMappings)
            {
                if (mapping.type == type)
                {
                    return mapping;
                }
            }
            return cursorMappings[0];
        }

        private void CheckSpecialAbilityKeys()
        {
            var actionStore = GetComponent<ActionStore>();
            var keyOffset = (int)KeyCode.Alpha1;
            var slotCount = actionStoreSize; // this will probably be configured
            for (var i = 0; i < slotCount; i++)
            {
                if (Input.GetKeyDown((KeyCode)(i + keyOffset)))
                    actionStore.Use(i, gameObject);
            }
        }
    }
}
