using System;
using UnityEngine;

namespace RPG.Shops
{
    /// <summary>
    /// Holds the player's currently active shop. Place on the Player GameObject.
    /// </summary>
    public class Shopper : MonoBehaviour
    {
        Shop activeShop = null;

        public event Action activeShopChange;

        public Shop GetActiveShop()
        {
            return activeShop;
        }

        public void SetActiveShop(Shop shop)
        {
            if (activeShop != null)
            {
                activeShop.SetShopper(null);
            }

            activeShop = shop;

            if (activeShop != null)
            {
                activeShop.SetShopper(this);
            }

            if (activeShopChange != null)
            {
                activeShopChange();
            }
        }
    }
}
