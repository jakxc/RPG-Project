using UnityEngine;

namespace RPG.Control
{
    public class AIPath : MonoBehaviour
    {
        const float wayPointGizmoRadius = 0.3f;

        private void OnDrawGizmos() 
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                int j = GetNextIndex(i);

                // Draw a 0.3f radius sphere to represent a waypoint (which is a child of this component)
                Gizmos.DrawSphere(GetWaypoint(i),
                 wayPointGizmoRadius);

                 // Draw a line between waypoint at index i and the next index after i
                Gizmos.DrawLine(GetWaypoint(i), GetWaypoint(j));
            }
        }

        public Vector3 GetWaypoint(int i)
        {
            // returns position of child of this (a waypoint) at index i
            return transform.GetChild(i).position;
        }

        public int GetNextIndex(int i)
        {
            // If index of child of this component is index of final child, reset the index to 0
            if (i + 1 == transform.childCount)
            {
                return 0;
            }
            
            // else return index after i
            return i + 1;
        }
    }
}
