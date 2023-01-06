using System.Collections.Generic;
using UnityEngine;
using GameDevTV.Saving;
using UnityEngine.SceneManagement;

namespace GameDevTV.Inventories
{
    /// <summary>
    /// To be placed on anything that wishes to drop pickups into the world (e.g Player, Enemy prefab).
    /// Tracks the drops for saving and restoring.
    /// </summary>
    public class ItemDropper : MonoBehaviour, ISaveable
    {
        // STATE
        private List<Pickup> droppedItems = new List<Pickup>();
        private List<DropRecord> otherSceneDroppedItems = new List<DropRecord>();

        // PUBLIC

        /// <summary>
        /// Create a pickup at the current position.
        /// </summary>
        /// <param name="item">The item type for the pickup.</param>
        /// <param name="quantity">
        /// The number of items contained in the pickup. Only used if the item
        /// is stackable.
        /// </param>
        public void DropItem(InventoryItem item, int quantity)
        {
            SpawnPickup(item, GetDropLocation(), quantity);
        }

        /// <summary>
        /// Create a pickup at the current position.
        /// </summary>
        /// <param name="item">The item type for the pickup.</param>
        public void DropItem(InventoryItem item)
        {
            SpawnPickup(item, GetDropLocation(), 1);
        }

        // PROTECTED

        /// <summary>
        /// Override to set a custom method for locating a drop. The implementation can change depending on where it is inherited
        /// </summary>
        /// <returns>The location the drop should be spawned.</returns>
        protected virtual Vector3 GetDropLocation()
        {
            // Spawns item at location of this gameobject
            return transform.position;
        }

        // PRIVATE

        public void SpawnPickup(InventoryItem item, Vector3 spawnLocation, int number)
        {
            var pickup = item.SpawnPickup(spawnLocation, number);
            droppedItems.Add(pickup);
        }

        [System.Serializable]
        private struct DropRecord
        {
            public string itemID;
            public SerializableVector3 position;
            public int quantity;
            public int sceneBuildIndex;
        }

        object ISaveable.CaptureState()
        {
            RemoveDestroyedDrops();
            var droppedItemsList = new List<DropRecord>();
            int buildIndex = SceneManager.GetActiveScene().buildIndex;
            foreach (Pickup pickup in droppedItems)
            {
                var droppedItem = new DropRecord();
                droppedItem.itemID = pickup.GetItem().GetItemID();
                droppedItem.position = new SerializableVector3(pickup.transform.position);
                droppedItem.quantity = pickup.GetQuantity();
                droppedItem.sceneBuildIndex = buildIndex;
                droppedItemsList.Add(droppedItem);
            }
            droppedItemsList.AddRange(otherSceneDroppedItems);
            return droppedItemsList;
        }

        void ISaveable.RestoreState(object state)
        {
            var droppedItemsList = (List<DropRecord>)state;
            int buildIndex = SceneManager.GetActiveScene().buildIndex;
            otherSceneDroppedItems.Clear();
            foreach (var item in droppedItemsList)
            {
                if (item.sceneBuildIndex != buildIndex)
                {
                    otherSceneDroppedItems.Add(item);
                    continue;
                }
                
                var pickupItem = InventoryItem.GetFromID(item.itemID);
                Vector3 position = item.position.ToVector();
                int quantity = item.quantity;
                SpawnPickup(pickupItem, position, quantity);
            }
        }

        /// <summary>
        /// Remove any drops in the world that have subsequently been picked up.
        /// </summary>
        private void RemoveDestroyedDrops()
        {
            var newList = new List<Pickup>();
            foreach (var item in droppedItems)
            {
                if (item != null)
                {
                    newList.Add(item);
                }
            }
            droppedItems = newList;
        }
    }
}