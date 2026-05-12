using System;
using System.Collections.Generic;
using UnityEngine;
using RPG.Control;
using RPG.Inventories;
using RPG.Saving;

namespace RPG.Shops
{
    /// <summary>
    /// A shop that the player can buy from and sell to. Place on an NPC GameObject.
    /// Players click the NPC to open the shop.
    /// </summary>
    public class Shop : MonoBehaviour, IRaycastable, ISaveable
    {
        [SerializeField] string shopName = "Shop";
        [Tooltip("Fraction of base price the shop pays when buying items from the player.")]
        [Range(0, 100)] [SerializeField] float sellingPercentage = 50;

        [Serializable]
        public class StockItemConfig
        {
            public InventoryItem item;
            public int initialStock;
            [Range(-100, 100)] public float buyingDiscountPercentage;
        }

        [SerializeField] StockItemConfig[] stockConfig = null;

        public enum Mode { Buying, Selling }

        Mode mode = Mode.Buying;
        Dictionary<InventoryItem, int> stockSold = new Dictionary<InventoryItem, int>();
        Dictionary<InventoryItem, int> basket = new Dictionary<InventoryItem, int>();

        Shopper currentShopper = null;

        public event Action onChange;

        public string GetShopName()
        {
            return shopName;
        }

        public Mode GetMode()
        {
            return mode;
        }

        public void SetMode(Mode newMode)
        {
            if (mode == newMode) return;
            mode = newMode;
            basket.Clear();
            if (onChange != null) onChange();
        }

        public bool IsBuyingMode()
        {
            return mode == Mode.Buying;
        }

        public void SetShopper(Shopper shopper)
        {
            currentShopper = shopper;
        }

        /// <summary>
        /// Snapshot of all rows the UI should render for the current mode.
        /// </summary>
        public IEnumerable<ShopRow> GetRows()
        {
            if (mode == Mode.Buying)
            {
                foreach (StockItemConfig config in stockConfig)
                {
                    int availability = GetAvailability(config);
                    int inBasket = basket.ContainsKey(config.item) ? basket[config.item] : 0;
                    float price = GetBuyingPrice(config);
                    yield return new ShopRow(config.item, availability, price, inBasket);
                }
            }
            else
            {
                foreach (var pair in GetInventoryStacks())
                {
                    int inBasket = basket.ContainsKey(pair.Key) ? basket[pair.Key] : 0;
                    float price = GetSellingPrice(pair.Key);
                    yield return new ShopRow(pair.Key, pair.Value, price, inBasket);
                }
            }
        }

        /// <summary>
        /// Add or remove items from the basket. Use a negative number to remove.
        /// </summary>
        public void AddToBasket(InventoryItem item, int quantity)
        {
            if (!basket.ContainsKey(item)) basket[item] = 0;

            int availability = GetAvailability(item);
            int inBasket = basket[item];
            if (inBasket + quantity > availability)
            {
                quantity = availability - inBasket;
            }

            basket[item] += quantity;
            if (basket[item] <= 0) basket.Remove(item);

            if (onChange != null) onChange();
        }

        public float GetBasketTotal()
        {
            float total = 0;
            foreach (var row in GetRows())
            {
                total += row.price * row.inBasket;
            }
            return total;
        }

        public bool HasSufficientFunds()
        {
            if (mode == Mode.Selling) return true;
            if (currentShopper == null) return false;
            Purse purse = currentShopper.GetComponent<Purse>();
            if (purse == null) return false;
            return purse.GetBalance() >= GetBasketTotal();
        }

        public bool HasInventorySpace()
        {
            if (mode == Mode.Selling) return true;
            if (currentShopper == null) return false;
            Inventory inventory = currentShopper.GetComponent<Inventory>();
            if (inventory == null) return false;

            foreach (var pair in basket)
            {
                if (!inventory.HasSpaceFor(pair.Key)) return false;
            }
            return true;
        }

        public bool CanTransact()
        {
            if (basket.Count == 0) return false;
            if (!HasSufficientFunds()) return false;
            if (!HasInventorySpace()) return false;
            return true;
        }

        /// <summary>
        /// Confirm the basket. Buying: pay money, gain items, reduce stock.
        /// Selling: lose items, gain money.
        /// </summary>
        public void ConfirmTransaction()
        {
            if (currentShopper == null) return;
            Inventory inventory = currentShopper.GetComponent<Inventory>();
            Purse purse = currentShopper.GetComponent<Purse>();
            if (inventory == null || purse == null) return;

            if (mode == Mode.Buying)
            {
                ConfirmBuy(inventory, purse);
            }
            else
            {
                ConfirmSell(inventory, purse);
            }

            // Clean out empty entries.
            var keys = new List<InventoryItem>(basket.Keys);
            foreach (var key in keys)
            {
                if (basket[key] <= 0) basket.Remove(key);
            }

            if (onChange != null) onChange();
        }

        // PRIVATE

        private void ConfirmBuy(Inventory inventory, Purse purse)
        {
            foreach (var pair in new Dictionary<InventoryItem, int>(basket))
            {
                InventoryItem item = pair.Key;
                int quantity = pair.Value;
                float price = GetPrice(item);

                for (int i = 0; i < quantity; i++)
                {
                    if (purse.GetBalance() < price) break;
                    if (!inventory.HasSpaceFor(item)) break;

                    purse.UpdateBalance(-price);
                    inventory.AddToFirstEmptySlot(item, 1);

                    if (!stockSold.ContainsKey(item)) stockSold[item] = 0;
                    stockSold[item]++;
                    basket[item]--;
                }
            }
        }

        private void ConfirmSell(Inventory inventory, Purse purse)
        {
            foreach (var pair in new Dictionary<InventoryItem, int>(basket))
            {
                InventoryItem item = pair.Key;
                int quantity = pair.Value;
                float price = GetPrice(item);

                int removed = RemoveFromInventory(inventory, item, quantity);
                if (removed > 0)
                {
                    purse.UpdateBalance(price * removed);
                    basket[item] -= removed;
                }
            }
        }

        private int RemoveFromInventory(Inventory inventory, InventoryItem item, int quantity)
        {
            int remaining = quantity;
            for (int i = 0; i < inventory.GetSize() && remaining > 0; i++)
            {
                if (inventory.GetItemInSlot(i) != item) continue;
                int inSlot = inventory.GetNumberInSlot(i);
                int take = Mathf.Min(inSlot, remaining);
                inventory.RemoveFromSlot(i, take);
                remaining -= take;
            }
            return quantity - remaining;
        }

        /// <summary>
        /// Walk the player's inventory and aggregate slots of the same item type.
        /// Items with no price are skipped (cannot be sold).
        /// </summary>
        private Dictionary<InventoryItem, int> GetInventoryStacks()
        {
            var stacks = new Dictionary<InventoryItem, int>();
            if (currentShopper == null) return stacks;
            Inventory inventory = currentShopper.GetComponent<Inventory>();
            if (inventory == null) return stacks;

            for (int i = 0; i < inventory.GetSize(); i++)
            {
                InventoryItem item = inventory.GetItemInSlot(i);
                if (item == null) continue;
                if (item.GetPrice() <= 0) continue;

                if (!stacks.ContainsKey(item)) stacks[item] = 0;
                stacks[item] += inventory.GetNumberInSlot(i);
            }
            return stacks;
        }

        private int GetAvailability(StockItemConfig config)
        {
            int sold = stockSold.ContainsKey(config.item) ? stockSold[config.item] : 0;
            return Mathf.Max(0, config.initialStock - sold);
        }

        private int GetAvailability(InventoryItem item)
        {
            if (mode == Mode.Buying)
            {
                foreach (var config in stockConfig)
                {
                    if (config.item == item) return GetAvailability(config);
                }
                return 0;
            }
            else
            {
                var stacks = GetInventoryStacks();
                return stacks.ContainsKey(item) ? stacks[item] : 0;
            }
        }

        private float GetBuyingPrice(StockItemConfig config)
        {
            return config.item.GetPrice() * (1 - config.buyingDiscountPercentage / 100);
        }

        private float GetSellingPrice(InventoryItem item)
        {
            return item.GetPrice() * sellingPercentage / 100;
        }

        private float GetPrice(InventoryItem item)
        {
            if (mode == Mode.Buying)
            {
                foreach (var config in stockConfig)
                {
                    if (config.item == item) return GetBuyingPrice(config);
                }
                return item.GetPrice();
            }
            return GetSellingPrice(item);
        }

        // IRaycastable

        public CursorType GetCursorType()
        {
            return CursorType.Shop;
        }

        public bool HandleRaycast(PlayerController callingController)
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                Shopper shopper = callingController.GetComponent<Shopper>();
                if (shopper != null)
                {
                    shopper.SetActiveShop(this);
                }
            }
            return true;
        }

        // ISaveable

        [Serializable]
        struct StockRecord
        {
            public string itemID;
            public int sold;
        }

        object ISaveable.CaptureState()
        {
            var records = new List<StockRecord>();
            foreach (var pair in stockSold)
            {
                records.Add(new StockRecord { itemID = pair.Key.GetItemID(), sold = pair.Value });
            }
            return records;
        }

        void ISaveable.RestoreState(object state)
        {
            stockSold.Clear();
            var records = (List<StockRecord>)state;
            foreach (var record in records)
            {
                var item = InventoryItem.GetFromID(record.itemID);
                if (item != null) stockSold[item] = record.sold;
            }
        }
    }

    /// <summary>
    /// Snapshot of a single row in the shop UI.
    /// </summary>
    public struct ShopRow
    {
        public InventoryItem item;
        public int availability;
        public float price;
        public int inBasket;

        public ShopRow(InventoryItem item, int availability, float price, int inBasket)
        {
            this.item = item;
            this.availability = availability;
            this.price = price;
            this.inBasket = inBasket;
        }
    }
}
