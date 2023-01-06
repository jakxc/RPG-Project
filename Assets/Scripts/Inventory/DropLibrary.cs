using System;
using System.Collections;
using System.Collections.Generic;
using GameDevTV.Inventories;
using UnityEngine;

namespace RPG.Inventory
{   
    [CreateAssetMenu (menuName = ("RPG/Inventory/Drop Library"))]
    public class DropLibrary : ScriptableObject
    {
        [SerializeField] DropConfig[] potentialDrops;
        [SerializeField] float[] dropChancePercentage;
        [SerializeField] int[] minDrops;
        [SerializeField] int[] maxDrops;

        [System.Serializable]
        class DropConfig
        {
            public InventoryItem item;
            public float[] relativeChance;
            public int[] minNumber;
            public int[] maxNumber;
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

        public struct Loot
        {
            public InventoryItem item;
            public int quantity;
        }

        public IEnumerable<Loot> GetRandomDrops(int level)
        {
            if (!ShouldGetRandomDrop(level))
            {
                yield break;
            }

            for (int i = 0; i < GetRandomNumberOfDrops(level); i++)
            {
                yield return GetRandomDrop(level);
            }
        }

        private Loot GetRandomDrop(int level)
        {
            var drop = SelectRandomItem(level);
            var result = new Loot();
            result.item = drop.item;
            result.quantity = drop.GetRandomNumber(level);
            return result;
        }

        private int GetRandomNumberOfDrops(int level)
        {
            return UnityEngine.Random.Range(GetByLevel(minDrops, level), 
                                                GetByLevel(maxDrops, level));
        }

        private bool ShouldGetRandomDrop(int level)
        {
            return UnityEngine.Random.Range(0, 100) < GetByLevel(dropChancePercentage, level);
        }

        DropConfig SelectRandomItem(int level)
        {   
            float maxChance = GetMaxChance(level);
            float randomNumber = UnityEngine.Random.Range(0, maxChance);
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
