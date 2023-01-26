using System.Collections;
using System.Collections.Generic;
using GameDevTV.Inventories;
using RPG.Stats;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Inventories
{
    public class RandomDropper : ItemDropper
    {
        [SerializeField] float scatterDistance = 1;
        [SerializeField] DropLibrary dropLibrary;

        // CONSTANT
        const int ATTEMPTS = 30;

        public void RandomDrop()
        {
            var baseStats = GetComponent<BaseStats>();      
            var drops = dropLibrary.GetRandomDrops(baseStats.GetLevel());
            foreach (var drop in drops)
            {
                DropItem(drop.item, drop.quantity);
            }
        }

        protected override Vector3 GetDropLocation()
        {
            // If random point is not on a NavMesh after 30 attempts, then return this transform position
            for (int i = 0; i < ATTEMPTS; i++)
            {
                Vector3 randomPoint = transform.position + (Random.insideUnitSphere * scatterDistance);
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 0.1f, NavMesh.AllAreas))
                {
                    return hit.position;
                }
            }

            return transform.position;
        }
    }
}
