using RPG.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace RPG.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float speed = 1f;
        [SerializeField] float maxLifeTime = 7f;
        [SerializeField] float durationAfterImpact = 2f;
        [SerializeField] GameObject[] destroyOnHit = null;
        [SerializeField] bool isHoming = true;
        [SerializeField] GameObject hitEffect;
        [SerializeField] UnityEvent onHit; // Event triggered when Projectile hits something
        Health target = null;
        GameObject sourceOfDamage = null; // The object that fired the projectile (e.g player etc)
        float damage = 0;

        private void Start() 
        {
            transform.LookAt(GetTargetLocation()); // Ensure projectile is facing target to begin with
        }

        // Update is called once per frame
        void Update()
        {
            if (target == null) return;
            
            // If projectile is homing and target is not dead, face target each frame with updated target position
            if (isHoming && !target.IsDead()) 
            {
                transform.LookAt(GetTargetLocation());
            }      
            transform.Translate(Vector3.forward * speed * Time.deltaTime); // Moves projectile in direction it is facing
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Health>() != target) return; // If projectile does not collide with target, do nothing
            if (target.IsDead()) return; // If it collides with dead target, do nothing
            target.TakeDamage(sourceOfDamage, damage);
            
            speed = 0; // Stop the projectile from moving after colliding with target

            onHit.Invoke(); // Call onHit event

            // Instantiate onHit particle effect at position that this collides with target
            if (hitEffect != null)
            {
                Instantiate(hitEffect, GetTargetLocation(), transform.rotation);
            }

            // Destroy certain GameObjects on collision defined in the destroyOnHit array
            foreach(GameObject toDestroy in destroyOnHit)
            {
                Destroy(toDestroy);
            }

            Destroy(gameObject, durationAfterImpact);
        }

        public void SetTarget(Health target, float damage, GameObject sourceOfDamage)
        {
            this.target = target;
            this.damage = damage;
            this.sourceOfDamage = sourceOfDamage; // The GameObject that launches projectile (player etc)

            Destroy(gameObject, maxLifeTime); // Destroy the projectile after maxLifeTime
        }

        private Vector3 GetTargetLocation()
        {
            CapsuleCollider targetCollider = target.GetComponent<CapsuleCollider>();           
            if (targetCollider == null) 
            {
                return target.transform.position;
            }

            /* Move the location of the projectile Z axis by half the height of the target collider.
             If this height is not added, target position is at the bottom of collider (e.g bottom of enemy)*/
            return target.transform.position + Vector3.up * targetCollider.height / 2;
        }
    }
}
