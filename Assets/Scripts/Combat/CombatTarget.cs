using RPG.Attributes;
using RPG.Control;
using UnityEngine;

namespace RPG.Combat
{
    // If an object has CombatTarget component, it must also have Health component
    [RequireComponent(typeof(Health))]
    public class CombatTarget : MonoBehaviour, IRaycastable
    {
        public CursorType GetCursorType()
        {
            return CursorType.Combat;
        }

        public bool HandleRaycast(PlayerController controller)
        {
            if (!enabled) return false;

            // If player cannot attack this CombatTarget, do nothing
            if(!controller.GetComponent<Fighter>().CanAttack(gameObject)) 
            {
                return false;
            }

            // If left mouse button is clicked on CombatTarget, player attack this CombatTarget
            if (Input.GetMouseButton(0))
            {
                controller.GetComponent<Fighter>().Attack(gameObject);
            }
            
            return true;
        }
    }
}
