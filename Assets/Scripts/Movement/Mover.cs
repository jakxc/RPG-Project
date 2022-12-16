using RPG.Core;
using RPG.Saving;
using RPG.Attributes;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Movement {
    public class Mover : MonoBehaviour, IAction, ISaveable 
    { 
        [SerializeField] float maxSpeed = 6f;
        [SerializeField] float maxPathLength = 40f;
        Health health;
        NavMeshAgent navMeshAgent;

        private void Awake() 
        {
            health = GetComponent<Health>();
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        void Update()
        {
            navMeshAgent.enabled = !health.IsDead();
            UpdateAnimator();
        }

        public bool CanMoveTo(Vector3 destination)
        {
            NavMeshPath path = new NavMeshPath();
            bool hasPath = NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
            if(!hasPath) return false;
            
            // path.status returns whether there is a path from transform.position to destination
            if (path.status != NavMeshPathStatus.PathComplete) return false;
           
            // If length of path is longer than max allowed length path, then this cannot move to the destination
            if (GetPathLength(path) > maxPathLength) return false;
            
            return true;
        }

        public void MoveTo(Vector3 destination, float speedFraction)
        {
            navMeshAgent.destination = destination;
            navMeshAgent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
            navMeshAgent.isStopped = false;
        }

        public void StartMoveAction(Vector3 destination, float speedFraction)
        {
            // Set current action to movement
            GetComponent<ActionScheduler>().StartAction(this);
            MoveTo(destination, speedFraction);
        }

        public void Cancel()
        {
            navMeshAgent.isStopped = true;
        }

        private void UpdateAnimator()
        {
            Vector3 velocity = GetComponent<NavMeshAgent>().velocity;
            /*Covert from global velocity to local so regardless of where this is in the world,
            the velocity will be a value that will represent direction and speed*/
            Vector3 localVelocity = transform.InverseTransformDirection(velocity); 
            float speed = localVelocity.z;

            // Set forwardSpeed float parameter to speed (the velocity in the z direction)
            GetComponent<Animator>().SetFloat("forwardSpeed", speed);
        }

        private float GetPathLength(NavMeshPath path)
        {
            float total = 0f;
            // If less than 2 corners in path, return 0 length since there is only a single point in the path
            if (path.corners.Length < 2) return total;

            /* Add the distance of all path corner pairs in order from first index to last.
            This will give total distance of path */
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                total += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }

            return total;
        }

        [System.Serializable] // Serializable so this data can be converted to binary
        struct MoverSaveData
        {
            public SerializableVector3 position;
            public SerializableVector3 rotation;
        }

        public object CaptureState()
        {
            MoverSaveData data = new MoverSaveData();
            data.position = new SerializableVector3(transform.position);
            data.rotation = new SerializableVector3(transform.eulerAngles);
            return data;
        }

        public void RestoreState(object state)
        {
            MoverSaveData data = (MoverSaveData)state;
            GetComponent<NavMeshAgent>().enabled = false;
            transform.position = data.position.ToVector();
            transform.eulerAngles = data.rotation.ToVector();
            GetComponent<NavMeshAgent>().enabled = true;
        }
    }
}
