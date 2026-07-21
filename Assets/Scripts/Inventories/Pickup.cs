using UnityEngine;

namespace RPG.Inventories
{
    /// <summary>
    /// To be placed at the root of a Pickup prefab. Contains the data about the
    /// pickup such as the type of item and the number.
    /// </summary>
    public class Pickup : MonoBehaviour
    {
        // STATE
        InventoryItem item;
        int number = 1;

        // CACHED REFERENCE
        Inventory inventory;
        Equipment equipment;

        // LIFECYCLE METHODS

        private void Awake()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            inventory = player.GetComponent<Inventory>();
            equipment = player.GetComponent<Equipment>();
        }

        // PUBLIC

        /// <summary>
        /// Set the vital data after creating the prefab.
        /// </summary>
        /// <param name="item">The type of item this prefab represents.</param>
        /// <param name="number">The number of items represented.</param>
        public void Setup(InventoryItem item, int number)
        {
            this.item = item;
            if (!item.IsStackable())
            {
                number = 1;
            }
            this.number = number;
        }

        public InventoryItem GetItem()
        {
            return item;
        }

        public int GetNumber()
        {
            return number;
        }

        public void PickupItem()
        {
            if (TryAutoEquip())
            {
                Destroy(gameObject);
                return;
            }

            bool foundSlot = inventory.AddToFirstEmptySlot(item, number);
            if (foundSlot)
            {
                Destroy(gameObject);
            }
        }

        public bool CanBePickedUp()
        {
            if (CanAutoEquip()) return true;
            return inventory.HasSpaceFor(item);
        }

        // PRIVATE

        private bool CanAutoEquip()
        {
            if (equipment == null) return false;
            var equipable = item as EquipableItem;
            if (equipable == null) return false;
            return equipment.GetItemInSlot(equipable.GetAllowedEquipLocation()) == null;
        }

        private bool TryAutoEquip()
        {
            if (!CanAutoEquip()) return false;
            var equipable = (EquipableItem)item;
            equipment.AddItem(equipable.GetAllowedEquipLocation(), equipable);
            return true;
        }
    }
}