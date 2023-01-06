using System.Collections;
using GameDevTV.Inventories;
using RPG.Movement;
using UnityEngine;

namespace RPG.Control
{
    [RequireComponent(typeof(Pickup))]
    public class ClickablePickup : MonoBehaviour, IRaycastable
    {
        Pickup pickup;
        [SerializeField] bool isRunoverPickup = true;

        private void Awake()
        {
            pickup = GetComponent<Pickup>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                pickup.PickupItem();
            }
        }

        public CursorType GetCursorType()
        {
            if (pickup.CanBePickedUp())
            {
                return CursorType.Pickup;
            }
            else
            {
                return CursorType.None;
            }
        }

        public bool HandleRaycast(PlayerController callingController)
        {
            print("hovering on clickable pickup " + name);
            if (Input.GetMouseButtonDown(0))
            {
                DedideOnPickupMethod(callingController);
            }
            return true;
        }

        private void DedideOnPickupMethod(PlayerController controller)
        {
            if (isRunoverPickup)
            {
                print("this is a runover pickup... be right there!");
                controller.GetComponent<Mover>().StartMoveAction(transform.position, 1);
            }
            else
            {
                pickup.PickupItem();
            }
        }   
    }
}