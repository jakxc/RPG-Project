using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameDevTV.Inventories;
using GameDevTV.Core.UI.Dragging;

namespace GameDevTV.UI.Inventories
{
    public class InventorySlotUI : MonoBehaviour, IItemHolder, IDragContainer<InventoryItem>
    {
        // CONFIG DATA
        [SerializeField] InventoryItemIcon icon = null;

        // STATE
        int index; // Index of this slot in the inventory
        InventoryItem item; // Item that occupies this slot
        Inventory inventory;

        // PUBLIC

        public void Setup(Inventory inventory, int index)
        {
            this.inventory = inventory;
            this.index = index;
            icon.SetItem(inventory.GetItemInSlot(index), inventory.GetQuantityInSlot(index));
        }

        /// <summary>
        /// Returns the max number the inventory can store of a particular item
        /// </summary>
        /// <param name="item">
        /// The item in question (will return the max number of this item inventory can store)
        /// </param>
        public int MaxAcceptable(InventoryItem item)
        {
            if (inventory.HasSpaceFor(item))
            {
                return int.MaxValue;
            }
            return 0;
        }

        /// <summary>
        /// Add specific number of an item to this slot
        /// </summary>
        /// <param name="item">
        /// The item to be added to this slot.
        /// </param>
        /// <param name="quantity">
        /// The quantity of the item to be added to this slot.
        /// </param>
        public void AddItems(InventoryItem item, int quantity)
        {
            inventory.AddItemToSlot(index, item, quantity);
        }

        public InventoryItem GetItem()
        {
            return inventory.GetItemInSlot(index);
        }

        public int GetQuantity()
        {
            return inventory.GetQuantityInSlot(index);
        }

        public void RemoveItems(int number)
        {
            inventory.RemoveFromSlot(index, number);
        }
    }
}