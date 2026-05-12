using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPG.Inventories;
using RPG.Shops;

namespace RPG.UI.Shops
{
    /// <summary>
    /// Root of the shop panel. Hidden until a shop is opened by the player.
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI shopName = null;
        [SerializeField] Transform listRoot = null;
        [SerializeField] RowUI rowPrefab = null;
        [SerializeField] TextMeshProUGUI totalText = null;
        [SerializeField] TextMeshProUGUI balanceText = null;
        [SerializeField] Button confirmButton = null;
        [SerializeField] TextMeshProUGUI confirmButtonLabel = null;
        [SerializeField] Button closeButton = null;
        [SerializeField] Button buyTabButton = null;
        [SerializeField] Button sellTabButton = null;

        Shopper shopper = null;
        Shop currentShop = null;

        private void Start()
        {
            shopper = GameObject.FindGameObjectWithTag("Player").GetComponent<Shopper>();
            if (shopper == null) return;

            shopper.activeShopChange += OnShopChanged;
            confirmButton.onClick.AddListener(OnConfirm);
            closeButton.onClick.AddListener(Close);
            if (buyTabButton != null) buyTabButton.onClick.AddListener(() => SetMode(Shop.Mode.Buying));
            if (sellTabButton != null) sellTabButton.onClick.AddListener(() => SetMode(Shop.Mode.Selling));

            OnShopChanged();
        }

        private void OnShopChanged()
        {
            if (currentShop != null)
            {
                currentShop.onChange -= Refresh;
            }

            currentShop = shopper.GetActiveShop();

            if (currentShop == null)
            {
                gameObject.SetActive(false);
                return;
            }

            currentShop.onChange += Refresh;
            shopName.text = currentShop.GetShopName();
            gameObject.SetActive(true);
            Refresh();
        }

        private void SetMode(Shop.Mode mode)
        {
            if (currentShop != null) currentShop.SetMode(mode);
        }

        private void Refresh()
        {
            foreach (Transform child in listRoot)
            {
                Destroy(child.gameObject);
            }

            foreach (var row in currentShop.GetRows())
            {
                RowUI rowUI = Instantiate(rowPrefab, listRoot);
                rowUI.Setup(currentShop, row);
            }

            bool buying = currentShop.IsBuyingMode();
            string totalLabel = buying ? "Total" : "Earn";
            totalText.text = $"{totalLabel}: {currentShop.GetBasketTotal():N2}";

            Purse purse = shopper.GetComponent<Purse>();
            if (purse != null && balanceText != null)
            {
                balanceText.text = $"Balance: {purse.GetBalance():N2}";
            }

            confirmButton.interactable = currentShop.CanTransact();
            if (confirmButtonLabel != null)
            {
                confirmButtonLabel.text = buying ? "Buy" : "Sell";
            }

            if (buyTabButton != null) buyTabButton.interactable = !buying;
            if (sellTabButton != null) sellTabButton.interactable = buying;
        }

        private void OnConfirm()
        {
            if (currentShop != null) currentShop.ConfirmTransaction();
        }

        public void Close()
        {
            if (shopper != null) shopper.SetActiveShop(null);
        }
    }
}
