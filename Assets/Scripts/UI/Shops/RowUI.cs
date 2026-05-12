using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPG.Inventories;
using RPG.Shops;

namespace RPG.UI.Shops
{
    /// <summary>
    /// Single row in the shop list. Shows item icon, name, price, stock,
    /// and +/- buttons to change quantity in the basket.
    /// </summary>
    public class RowUI : MonoBehaviour
    {
        [SerializeField] Image icon = null;
        [SerializeField] TextMeshProUGUI nameText = null;
        [SerializeField] TextMeshProUGUI priceText = null;
        [SerializeField] TextMeshProUGUI availabilityText = null;
        [SerializeField] TextMeshProUGUI quantityText = null;
        [SerializeField] Button addButton = null;
        [SerializeField] Button removeButton = null;

        Shop shop;
        InventoryItem item;

        public void Setup(Shop shop, ShopRow row)
        {
            this.shop = shop;
            this.item = row.item;

            icon.sprite = row.item.GetIcon();
            nameText.text = row.item.GetDisplayName();
            priceText.text = $"{row.price:N2}";
            availabilityText.text = $"x{row.availability}";
            quantityText.text = row.inBasket.ToString();

            addButton.onClick.RemoveAllListeners();
            removeButton.onClick.RemoveAllListeners();
            addButton.onClick.AddListener(() => shop.AddToBasket(item, 1));
            removeButton.onClick.AddListener(() => shop.AddToBasket(item, -1));

            addButton.interactable = row.inBasket < row.availability;
            removeButton.interactable = row.inBasket > 0;
        }
    }
}
