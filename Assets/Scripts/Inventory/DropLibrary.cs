using System.Collections.Generic;
using GameDevTV.Inventories;
using UnityEngine;

namespace RPG.Inventories
{   
    /// <summary>
    /// To be placed on icons representing the item in a slot. Allows the item
    /// to be dragged into other slots.
    /// </summary>
    [CreateAssetMenu (menuName = ("RPG/Inventory/Drop Library"))]
    public class DropLibrary : ScriptableObject
    {
        [SerializeField] DropConfig[] potentialDrops;
        [SerializeField] float[] dropChancePercentage;
        [SerializeField] int[] minDrops; // Min number of seperate items that can be dropped
        [SerializeField] int[] maxDrops; // Max number of seperate items that can be dropped

        [System.Serializable]
        class DropConfig
        {
            public InventoryItem item;
            public float[] relativeChance; // Chance of dropping
            public int[] minNumber; // Min number of this item that can be dropped (stackables)
            public int[] maxNumber; // Max number of this item that can be dropped (stackables)
            public int GetRandomNumber(int level)
            {
                if (!item.IsStackable())
                {
                    return 1;
                }
                return UnityEngine.Random.Range(GetByLevel(minNumber, level), 
                                                GetByLevel(maxNumber, level) - 1);
            }
        }

        public struct Drop
        {
            public InventoryItem item;
            public int quantity;
        }

        /// <summary>
        /// Return a random number of drops based on the level of the object with ItemDropper component
        /// </summary>
        /// <param name="index">The level of the object used to determine how many drops it should return</param>
        /// <returns>Return null if ShouldGetRandomDrop() returns false, else return a random number of drops based on level</returns>
        public IEnumerable<Drop> GetRandomDrops(int level)
        {
            if (!ShouldGetRandomDrop(level))
            {
                yield break;
            }

            int numberOfDrops = GetRandomNumberOfDrops(level);
            for (int i = 0; i < numberOfDrops; i++)
            {
                yield return GetRandomDrop(level);
            }
        }

        private Drop GetRandomDrop(int level)
        {
            var drop = SelectRandomItem(level);
            var result = new Drop();
            result.item = drop.item;
            result.quantity = drop.GetRandomNumber(level);
            return result;
        }

        private int GetRandomNumberOfDrops(int level)
        {
            return UnityEngine.Random.Range(GetByLevel(minDrops, level), 
                                                GetByLevel(maxDrops, level) - 1);
        }

        private bool ShouldGetRandomDrop(int level)
        {
            // If random number generated is less than drop chance, then no drop is returned
            return UnityEngine.Random.Range(0, 100) < GetByLevel(dropChancePercentage, level);
        }

        DropConfig SelectRandomItem(int level)
        {   
            float maxChance = GetMaxChance(level);
            float randomNumber = UnityEngine.Random.Range(0, maxChance); // A random number between 0 and sum of all the relative chances of items
            float chanceTotal = 0;
            foreach (var drop in potentialDrops)
            {
                chanceTotal += GetByLevel(drop.relativeChance, level);
                if (chanceTotal > randomNumber)
                {
                    return drop;
                }
            }
            return null;
        }

        private float GetMaxChance(int level)
        {
            float total = 0;
            foreach (var drop in potentialDrops)
            {
                total += GetByLevel(drop.relativeChance, level);
            }
            return total;
        }

        static T GetByLevel<T>(T[] values, int level)
        {
            if (values.Length == 0 || level <= 0)
            {
                return default;
            }
            if (level > values.Length)
            {
                return values[values.Length - 1];
            }
            return values[level - 1];
        }
    }
}
