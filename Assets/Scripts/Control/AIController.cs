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
        [Header("Active Behaviour")]
        [SerializeField] float chaseDistance = 5f;
        [SerializeField] float mobDistance = 5f;
        [SerializeField] float suspicionTime = 3f;
        [SerializeField] float aggroCooldownTime = 5f;
        
        [Header("Inactive Behaviour")]
        [SerializeField] float waypointTolerance = 1f; // Max distance between this and waypoint before it can be considered arrived at waypoint
        float waypointDwellTime;
        [SerializeField] float minDwellTime = 1f;
        [SerializeField] float maxDwellTime = 5f;

        [Range(0,1)] [SerializeField] float defaultSpeedFraction = 0.5f;

        // Path that contains waypoints which this will move on
        [SerializeField] AIPath aiPath;
        GameObject player; // Target that this AIController interacts with (attack etc)
        Health health;
        Mover mover;
        Fighter fighter;

        LazyValue<Vector3> defaultPosition;

        /*This means time since last saw player will always be greater than suspicion time 
        at the start so can attack initially*/
        float timeSinceLastSawPlayer = Mathf.Infinity;

        /*This means time since arrive at wat point will always be greater than waypoint dwell time
        at the start so can move to next waypoint initially without dwelling*/
        float timeSinceArrivedAtWaypoint = Mathf.Infinity;

         /*This means time since AI is aggrevated will always be greater than aggro cooldown time 
        at the start so it is not aggrevated to begin with*/
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
            // If AI is dead, do nothing
            if (health.IsDead()) return;

            // If AI is aggrevated and can attack player, attack player
            if (IsAggrevated() && fighter.CanAttack(player))
            {
                AttackBehaviour();
            }
            // else if time since player is last seen is less than suspicion time, cancel movement before retreating
            else if (timeSinceLastSawPlayer < suspicionTime)
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
            /*Reset timeSinceAggrevated so it is less than aggroCooldownTime.
            This will toggle IsAggrevated() to true*/ 
            timeSinceAggrevated = 0;
        }

        private void UpdateTimers()
        {
            timeSinceLastSawPlayer += Time.deltaTime;
            timeSinceArrivedAtWaypoint += Time.deltaTime;
            timeSinceAggrevated += Time.deltaTime;
        }

        // Default bahaviour is moving on set path defined by certain waypoints
        private void DefaultBehaviour()
        {
            // AI will already return to default position when it is not attacking or suspicious
            Vector3 nextPosition = defaultPosition.value;
            
            if (aiPath != null)
            {
                // If this has arrived at waypoint at current index of path
                if (AtWaypoint())
                {
                    // Reset time because this has arrived at waypoint
                    timeSinceArrivedAtWaypoint = 0;

                    // increment index of the waypoint this is moving towards 
                    CycleWaypoint();
                }

                // Update next position of this to the waypoint at incremented index of path
                nextPosition = GetCurrentWaypoint();
            }

            /*Randomly generate dwell time between min and max dwell time. If this has stayed at
             current waypoint for longer than dwell time, starting moving to next waypoint*/
            GenerateRandomDwellTime();
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
            // Distance between this and current waypoint (waypoint that this is to going towards)
            float distanceToWaypoint = Vector3.Distance(transform.position, 
            GetCurrentWaypoint());
            
            // If this distance is less than a waypointTolerance, then this is considered arrived at waypoint 
            return distanceToWaypoint < waypointTolerance;
        }

        private void CycleWaypoint()
        {
            // Increment the waypoint index
            currentWaypointIndex = aiPath.GetNextIndex(currentWaypointIndex);
        }

        private void GenerateRandomDwellTime()
        {
            waypointDwellTime = UnityEngine.Random.Range(minDwellTime, maxDwellTime);
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
            // SphereCast with this as the centre and radius of this mobDistance
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, mobDistance, Vector3.up, 0);
            
            // For all GameObjects hit by SphereCast, if they have AIController component, aggrevate
            foreach(RaycastHit hit in hits)
            {
                AIController target = hit.transform.GetComponent<AIController>();
                if (target == null) continue;

                target.Aggrevate();
            }
        }

        bool IsAggrevated()
        {
            // Distance between player and this 
            float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
            
            /* If player is within this chase distance or time since aggrevated is less than aggro cooldown
            time, return true (this is aggrevated)*/
            return distanceToPlayer < chaseDistance || timeSinceAggrevated < aggroCooldownTime;
        }

        // Called by Unity (same to Update and Start etc)
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            // Visualise AI chase distance with sphere that has radius of chase distance and centre of AIController
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}
