
using UnityEngine;
using RPG.Core;
using RPG.Combat;
using RPG.Movement;
using GameDevTV.Inventories;

namespace RPG.Control
{
    // Only used for Weapon pick ups at the moment...
    public class Interactor : MonoBehaviour, IAction
    {
        [SerializeField] float interactRange = 1f;
        private Mover mover;
        private Pickup target = null;

        private void Awake() 
        {
            mover = GetComponent<Mover>();
        }

        private void Update()
        {
            if (target == null) return;

            if (!GetIsInRange())
            {
                mover.MoveTo(target.transform.position, 1f);
            }
            else
            {
                mover.Cancel();
                InteractBehaviour();
            }
        }

        public void Interact(GameObject interactable)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            target = interactable.GetComponent<Pickup>();
        }

        private bool GetIsInRange()
        {
            return Vector3.Distance(transform.position, target.transform.position) < interactRange;
        }

        private void InteractBehaviour()
        {
            // TODO: Add animation
        }

        #region IAction Interface
        public void Cancel()
        {
            StopAttack();
            mover.Cancel();
            target = null;
        }

        private void StopAttack()
        {
            GetComponent<Animator>().ResetTrigger("attack");
            GetComponent<Animator>().SetTrigger("stopAttack");
        }
        #endregion
    }
}