using System;
using UnityEngine;
using GameDevTV.Saving;

namespace GameDevTV.Inventories
{
    /// <summary>
    /// Provides storage for the player inventory. A configurable number of
    /// slots are available.
    ///
    /// This component should be placed on the GameObject tagged "Player".
    /// </summary>
    public class Inventory : MonoBehaviour, ISaveable
    {
        // CONFIG DATA
        [Tooltip("Allowed size")]
        [SerializeField] int inventorySize = 16;

        // STATE
        InventorySlot[] slots;

        public struct InventorySlot
        {
            public InventoryItem item;
            public int quantity;
        }

        // PUBLIC

        /// <summary>
        /// Broadcasts when the items in the slots are added/removed.
        /// </summary>
        public event Action inventoryUpdated;

        /// <summary>
        /// Convenience for getting the player's inventory.
        /// </summary>
        public static Inventory GetPlayerInventory()
        {
            var player = GameObject.FindWithTag("Player");
            return player.GetComponent<Inventory>();
        }

        /// <summary>
        /// Could this item fit anywhere in the inventory?
        /// </summary>
        public bool HasSpaceFor(InventoryItem item)
        {
            return FindSlot(item) >= 0;
        }

        /// <summary>
        /// How many slots are in the inventory?
        /// </summary>
        public int GetSize()
        {
            return slots.Length;
        }

        /// <summary>
        /// Attempt to add the items to the first available slot.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="quantity">The number to add.</param>
        /// <returns>Whether or not the item could be added.</returns>
        public bool AddToFirstEmptySlot(InventoryItem item, int quantity)
        {
            int i = FindSlot(item);

            if (i < 0)
            {
                return false;
            }

            slots[i].item = item;
            slots[i].quantity += quantity;
            if (inventoryUpdated != null)
            {
                inventoryUpdated();
            }
            return true;
        }

        /// <summary>
        /// Is there an instance of the item in the inventory?
        /// </summary>
        public bool HasItem(InventoryItem item)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (object.ReferenceEquals(slots[i].item, item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Return the item type in the given slot.
        /// </summary>
        public InventoryItem GetItemInSlot(int index)
        {
            return slots[index].item;
        }

        /// <summary>
        /// Get the quantity of items in the given slot.
        /// </summary>
        public int GetQuantityInSlot(int index)
        {
            return slots[index].quantity;
        }

        /// <summary>
        /// Remove a number of items from the given slot. Will never remove more
        /// that there are.
        /// </summary>
        /// <param name="index">The index of the slot</param>
        /// <param name="quantity">The quantity to be removed</param>
        public void RemoveFromSlot(int index, int quantity)
        {
            slots[index].quantity -= quantity;
            // If quantity of item in slow is equal or less than 0, set item to null and quantity to 0
            if (slots[index].quantity <= 0)
            {
                slots[index].quantity = 0;
                slots[index].item = null;
            }
            if (inventoryUpdated != null)
            {
                inventoryUpdated();
            }
        }

        /// <summary>
        /// Will add an item to the given slot if possible. If there is already
        /// a stack of this type, it will add to the existing stack. Otherwise,
        /// it will be added to the first empty slot.
        /// </summary>
        /// <param name="index">The slot to attempt to add to.</param>
        /// <param name="item">The item type to add.</param>
        /// <param name="quantity">The number of items to add.</param>
        /// <returns>True if the item was added anywhere in the inventory.</returns>
        public bool AddItemToSlot(int index, InventoryItem item, int quantity)
        {
            if (slots[index].item != null)
            {
                return AddToFirstEmptySlot(item, quantity); ;
            }

            var i = FindStack(item);
            if (i >= 0)
            {
                index = i;
            }

            slots[index].item = item;
            slots[index].quantity += quantity;
            if (inventoryUpdated != null)
            {
                inventoryUpdated();
            }
            return true;
        }

        // PRIVATE

        private void Awake()
        {
            slots = new InventorySlot[inventorySize];
        }

        /// <summary>
        /// Find a slot that can accomodate the given item.
        /// </summary>
        /// <returns>-1 if no slot is found. Else return the index of the slot with the item</returns>
        private int FindSlot(InventoryItem item)
        {
            int i = FindStack(item);
            // If no slot is found that contains this item, find the empty slot with the lowest index
            if (i < 0)
            {
                i = FindEmptySlot();
            }

            // else return the index of the slot with the item/s in question
            return i;
        }

        /// <summary>
        /// Find an empty slot.
        /// </summary>
        /// <returns>-1 if all slots are full.</returns>
        private int FindEmptySlot()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item == null)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Find an existing stack of this item type.
        /// </summary>
        ///<param name="item">The item in question</param>
        /// <returns>-1 if no stack exists or if the item is not stackable. 
        /// Returns index of slot the existing item is from if it is stackable</returns>
        private int FindStack(InventoryItem item)
        {
            // If item is not stackable
            if (!item.IsStackable())
            {
                return -1;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                // ReferenceEquals check if the item in the slot is exactly equal to the item in question
                if (object.ReferenceEquals(slots[i].item, item))
                {
                    return i;
                }
            }
            return -1;
        }

        [System.Serializable]
        // Does not use default InventorySLot struct because cannot serialize the reference to InventoryItem.
        private struct InventorySlotRecord
        {
            public string itemID; 
            public int quantity;
        }
    
        object ISaveable.CaptureState()
        {
            var slotRecords = new InventorySlotRecord[inventorySize];
            for (int i = 0; i < inventorySize; i++)
            {
                if (slots[i].item != null)
                {
                    slotRecords[i].itemID = slots[i].item.GetItemID();
                    slotRecords[i].quantity = slots[i].quantity;
                }
            }
            return slotRecords;
        }

        void ISaveable.RestoreState(object state)
        {
            var slotRecords = (InventorySlotRecord[])state;
            for (int i = 0; i < inventorySize; i++)
            {
                slots[i].item = InventoryItem.GetFromID(slotRecords[i].itemID);
                slots[i].quantity = slotRecords[i].quantity;
            }
            if (inventoryUpdated != null)
            {
                inventoryUpdated();
            }
        }
    }
}