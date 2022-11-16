using RPG.Core;
using RPG.Combat;
using RPG.Movement;
using RPG.Attributes;
using UnityEngine;
using System;
using GameDevTV.Utils;

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] float chaseDistance = 5f;
        [SerializeField] float mobDistance = 5f;
        [SerializeField] float suspisionTime = 3f;
        [SerializeField] float aggroCooldownTime = 5f;
        [SerializeField] float waypointTolerance = 1f;
        [SerializeField] float waypointDwellTime = 2f;
        [Range(0,1)]
        [SerializeField] float defaultSpeedFraction = 0.2f;
        [SerializeField] AIPath aiPath;
        GameObject player;
        Health health;
        Mover mover;
        Fighter fighter;

        LazyValue<Vector3> defaultPosition;
        float timeSinceLastSawPlayer = Mathf.Infinity;
        float timeSinceArrivedAtWaypoint = Mathf.Infinity;
        float timeSinceAggrevated = Mathf.Infinity;
        int currentWaypointIndex = 0;

        private void Awake() 
        {
            player = GameObject.FindWithTag("Player");
            health = GetComponent<Health>();
            mover = GetComponent<Mover>();
            fighter = GetComponent<Fighter>();  
            
            defaultPosition = new LazyValue<Vector3>(GetDefaultPosition);
        }

        private Vector3 GetDefaultPosition()
        {
            return transform.position;
        }

        void Start() 
        {
            defaultPosition.ForceInit();
        }

        void Update()
        {
            if (health.IsDead()) return;

            if (IsAggrevated() && fighter.CanAttack(player))
            {
                AttackBehaviour();
            }
            else if (timeSinceLastSawPlayer < suspisionTime)
            {
                SuspicionBehaviour();
            }
            else
            {
                // Patrol behaviour
                DefaultBehaviour();
            }

            UpdateTimers();
        }

        public void Aggrevate()
        {
            timeSinceAggrevated = 0;
        }

        private void UpdateTimers()
        {
            timeSinceLastSawPlayer += Time.deltaTime;
            timeSinceArrivedAtWaypoint += Time.deltaTime;
            timeSinceAggrevated += Time.deltaTime;
        }

        // Default bahaviour is patrol
        private void DefaultBehaviour()
        {
            Vector3 nextPosition = defaultPosition.value;
            if (aiPath != null)
            {
                if (AtWaypoint())
                {
                    timeSinceArrivedAtWaypoint = 0;
                    CycleWaypoint();
                }
                nextPosition = GetCurrentWaypoint();
            }

            if (timeSinceArrivedAtWaypoint > waypointDwellTime)
            {
                mover.StartMoveAction(nextPosition, defaultSpeedFraction);
            }
        }

        private Vector3 GetCurrentWaypoint()
        {
            return aiPath.GetWaypoint(currentWaypointIndex);
        }

        private bool AtWaypoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, 
            GetCurrentWaypoint());
            
            return distanceToWaypoint < waypointTolerance;
        }

        private void CycleWaypoint()
        {
            currentWaypointIndex = aiPath.GetNextIndex(currentWaypointIndex);
        }

        private void SuspicionBehaviour()
        {
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        private void AttackBehaviour()
        {
            timeSinceLastSawPlayer = 0;
            fighter.Attack(player);

            AggrevateNearbyUnits();
        }

        private void AggrevateNearbyUnits()
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, mobDistance, Vector3.up, 0);
            foreach(RaycastHit hit in hits)
            {
                AIController target = hit.transform.GetComponent<AIController>();
                if (target == null) continue;

                target.Aggrevate();
            }
        }

        bool IsAggrevated()
        {
            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
            return distanceToPlayer < chaseDistance || timeSinceAggrevated < aggroCooldownTime;
        }

        // Called by Unity
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}
