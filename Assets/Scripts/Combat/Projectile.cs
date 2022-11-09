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
        [SerializeField] UnityEvent onHit;
        Health target = null;
        GameObject sourceOfDamage = null;
        float damage = 0;

        private void Start() 
        {
            transform.LookAt(GetTargetLocation());
        }

        // Update is called once per frame
        void Update()
        {
            if (target == null) return;
            
            if (isHoming && !target.IsDead()) 
            {
                transform.LookAt(GetTargetLocation());
            }      
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Health>() != target) return;
            if (target.IsDead()) return;
            target.TakeDamage(sourceOfDamage, damage);
            
            speed = 0;

            onHit.Invoke();

            if (hitEffect != null)
            {
                Instantiate(hitEffect, GetTargetLocation(), transform.rotation);
            }

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
            this.sourceOfDamage = sourceOfDamage;

            Destroy(gameObject, maxLifeTime);
        }

        private Vector3 GetTargetLocation()
        {
            CapsuleCollider targetCollider = target.GetComponent<CapsuleCollider>();           
            if (targetCollider == null) 
            {
                return target.transform.position;
            }

            return target.transform.position + Vector3.up * targetCollider.height / 2;
        }
    }
}
