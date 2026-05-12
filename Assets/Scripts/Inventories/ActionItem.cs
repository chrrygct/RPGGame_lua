using System;
using UnityEngine;

namespace RPG.Inventories
{
    /// <summary>
    /// 一种可以放置在动作栏中并被“使用”的物品。
    /// </summary>
    /// <remarks>
    /// 该类应作为基类使用，子类必须实现 `Use` 方法。
    /// </remarks>
    [CreateAssetMenu(menuName = ("GameDevTV/GameDevTV.UI.InventorySystem/Action Item"))]
    public class ActionItem : InventoryItem
    {
        // 配置数据
        [Tooltip("每次使用该物品时，是否会消耗一个实例。")]
        [SerializeField] bool consumable = false;

        // 公共方法

        /// <summary>
        /// 触发该物品的使用。可重写此方法以实现具体功能。
        /// </summary>
        /// <param name="user">使用该动作的角色对象。</param>
        public virtual void Use(GameObject user)
        {
            Debug.Log("Using action: " + this);
        }

        /// <summary>
        /// 判断该物品是否为可消耗物品。
        /// </summary>
        public bool isConsumable()
        {
            return consumable;
        }
    }
}