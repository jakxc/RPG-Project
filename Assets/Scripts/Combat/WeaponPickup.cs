using System.Collections;
using RPG.Attributes;
using RPG.Control;
using UnityEngine;

namespace RPG.Combat
{
    public class WeaponPickup : MonoBehaviour, IRaycastable
    {
        [SerializeField] WeaponConfig weapon = null; // The weapon that will be equipped if player picks this up
        [SerializeField] float healthToRestore = 0f;
        [SerializeField] float respawnTime = 5f;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                Pickup(other.gameObject);
            }
        }

        private void Pickup(GameObject other)
        {   
            if (weapon != null) 
            {
                other.GetComponent<Fighter>().SetWeapon(weapon);
            }

            if (healthToRestore > 0)
            {
                other.GetComponent<Health>().Heal(healthToRestore);
            }
            
            StartCoroutine(HideForSeconds(respawnTime));
        }

        private IEnumerator HideForSeconds(float seconds)
        {   
            ShowPickup(false);
            yield return new WaitForSeconds(seconds);
            ShowPickup(true);
        }

        private void ShowPickup(bool shouldShow)
        {
            // Enable/Disable collider of this when other objects should/should not interact with this
            GetComponent<Collider>().enabled = shouldShow;
            
            // For each child transform of this, set active to shouldShow toggle boolean
            foreach(Transform child in transform) 
            {
                child.gameObject.SetActive(shouldShow);
            }
        }

        public bool HandleRaycast(PlayerController controller)
        {
            if (Input.GetMouseButton(0))
            {
                controller.GetComponent<Interactor>().Interact(gameObject);
            }
            return true;
        }

        public CursorType GetCursorType()
        {
            return CursorType.Pickup;
        }
    }
}
