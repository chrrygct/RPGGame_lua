using UnityEngine;
using RPG.Combat;

namespace RPG.Inventories
{
    [CreateAssetMenu(menuName = ("GameDevTV/GameDevTV.UI.InventorySystem/Weapon Item"))]
    public class WeaponItem : EquipableItem
    {
        [SerializeField] WeaponConfig weaponConfig = null;

        public WeaponConfig GetWeapon()
        {
            return weaponConfig;
        }
    }
}
