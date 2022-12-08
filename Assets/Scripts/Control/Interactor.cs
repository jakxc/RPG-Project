
using UnityEngine;
using RPG.Core;
using RPG.Combat;
using RPG.Movement;

namespace RPG.Control
{
    // Only used for Weapon pick ups at the moment...
    public class Interactor : MonoBehaviour, IAction
    {
        private WeaponPickup target = null;

        private void Update()
        {
            if (target == null) { return; }

            if (!GetIsInRange())
            {
                MoveToPosition(target.transform.position);
            }
            else
            {
                GetComponent<Mover>().Cancel();
                CollectBehaviour();
            }
        }

        public void MoveToPosition(Vector3 movePosition)
        {
            GetComponent<Mover>().MoveTo(movePosition, 1f);
        }


        public void Collect(GameObject collectTarget)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            target = collectTarget.GetComponent<WeaponPickup>();
        }

        private bool GetIsInRange()
        {
            return Vector3.Distance(transform.position, target.transform.position) < 1f;
        }

        private void CollectBehaviour()
        {
            // TODO: Add animation
        }

        #region IAction Interface
        public void Cancel()
        {
            StopAttack();
            GetComponent<Mover>().Cancel();
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