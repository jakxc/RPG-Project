using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using RPG.Movement;
using RPG.Attributes;

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

        void Awake() 
        {
            health = GetComponent<Health>();
        }

        void Update()
        {
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
                /*Get all IRaycastable that is hit and perform the action in HandleRaycast().
                Set the cursor type to the cursor type set in IRaycastables*/
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
            if (EventSystem.current.IsPointerOverGameObject())
            {
                SetCursor(CursorType.UI);
                return true;
            }
            
            return false;
        }

        bool InteractWithMovement()
        { 
            Vector3 target;
            bool hasHit = RaycastNavMesh(out target);
            if (hasHit)
            {
                if(!GetComponent<Mover>().CanMoveTo(target)) return false;
                if (Input.GetMouseButton(0))
                {
                    GetComponent<Mover>().StartMoveAction(target, 1f);
                }
                SetCursor(CursorType.Movement);
                return true;
            }

            return false;
        }

        private bool RaycastNavMesh(out Vector3 target)
        {
            target = new Vector3();
            RaycastHit hit;
            bool hasHit = Physics.Raycast(GetMouseRay(), out hit);
            if(!hasHit) return false;
            
            NavMeshHit navMashHit;
            
            /*SamplePosition finds the nearest point based on the NavMesh within a specified range.
            In this case, it will return false if this is not within maxNavMeshProjectionDistance 
            from hit.point */
            bool hasCastToNavMesh = NavMesh.SamplePosition(
                hit.point, out navMashHit, maxNavMeshProjectionDistance, NavMesh.AllAreas);
            if (!hasCastToNavMesh) return false;

            target = navMashHit.position; // Position on the NavMesh that has been cast to (hit)

            return true;
        }

        Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        private void SetCursor(CursorType type)
        {
            CursorMapping mapping = GetCursorMapping(type);
            Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
        }

        private CursorMapping GetCursorMapping(CursorType type)
        {
            foreach (CursorMapping mapping in cursorMappings)
            {
                if (mapping.type == type)
                {
                    return mapping;
                }
            }
            return cursorMappings[0];
        }
    }
}
