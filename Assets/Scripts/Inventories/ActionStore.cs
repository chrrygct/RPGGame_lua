using System;
using System.Collections.Generic;
using UnityEngine;
using RPG.Saving;

namespace RPG.Inventories
{
    /// <summary>
    /// 为动作栏提供存储功能。该动作栏具有有限数量的槽位，
    /// 这些槽位可以被填充，并且槽位中的动作可以被“使用”。
    /// 
    /// 此组件应放置在带有 "Player" 标签的 GameObject 上。
    /// </summary>
    public class ActionStore : MonoBehaviour, ISaveable
    {
        // 状态
        Dictionary<int, DockedItemSlot> dockedItems = new Dictionary<int, DockedItemSlot>();

        private class DockedItemSlot 
        {
            public ActionItem item;
            public int number;
        }

        // 公共接口

        /// <summary>
        /// 当槽位中的物品被添加或移除时触发该事件。
        /// </summary>
        public event Action storeUpdated;

        /// <summary>
        /// 获取指定索引位置的动作。
        /// </summary>
        public ActionItem GetAction(int index)
        {
            if (dockedItems.ContainsKey(index))
            {
                return dockedItems[index].item;
            }
            return null;
        }

        /// <summary>
        /// 获取指定索引位置剩余的物品数量。
        /// </summary>
        /// <returns>
        /// 如果该位置没有物品，或者物品已经被完全消耗，则返回0。
        /// </returns>
        public int GetNumber(int index)
        {
            if (dockedItems.ContainsKey(index))
            {
                return dockedItems[index].number;
            }
            return 0;
        }

        /// <summary>
        /// 向指定索引位置添加物品。
        /// </summary>
        /// <param name="item">要添加的物品。</param>
        /// <param name="index">添加到的位置。</param>
        /// <param name="number">添加的数量。</param>
        public void AddAction(InventoryItem item, int index, int number)
        {
            if (dockedItems.ContainsKey(index))
            {  
                if (object.ReferenceEquals(item, dockedItems[index].item))
                {
                    dockedItems[index].number += number;
                }
            }
            else
            {
                var slot = new DockedItemSlot();
                slot.item = item as ActionItem;
                slot.number = number;
                dockedItems[index] = slot;
            }
            if (storeUpdated != null)
            {
                storeUpdated();
            }
        }

        /// <summary>
        /// 使用指定槽位中的物品。如果该物品是可消耗的，
        /// 则每次使用会消耗一个，直到该物品被完全移除。
        /// </summary>
        /// <param name="user">执行该动作的角色对象。</param>
        /// <returns>如果动作无法执行则返回 false。</returns>
        public bool Use(int index, GameObject user)
        {
            if (dockedItems.ContainsKey(index))
            {
                dockedItems[index].item.Use(user);
                if (dockedItems[index].item.isConsumable())
                {
                    RemoveItems(index, 1);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 从指定槽位中移除一定数量的物品。
        /// </summary>
        public void RemoveItems(int index, int number)
        {
            if (dockedItems.ContainsKey(index))
            {
                dockedItems[index].number -= number;
                if (dockedItems[index].number <= 0)
                {
                    dockedItems.Remove(index);
                }
                if (storeUpdated != null)
                {
                    storeUpdated();
                }
            }
        }

        /// <summary>
        /// 返回该槽位最多可以接收的物品数量。
        /// 
        /// 该方法会考虑槽位中是否已经存在物品，以及是否为相同类型。
        /// 只有当物品是可消耗的情况下，才允许堆叠多个。
        /// </summary>
        /// <returns>如果没有上限，则返回 int.MaxValue。</returns>
        public int MaxAcceptable(InventoryItem item, int index)
        {
            var actionItem = item as ActionItem;
            if (!actionItem) return 0;

            if (dockedItems.ContainsKey(index) && !object.ReferenceEquals(item, dockedItems[index].item))
            {
                return 0;
            }
            if (actionItem.isConsumable())
            {
                return int.MaxValue;
            }
            if (dockedItems.ContainsKey(index))
            {
                return 0;
            }

            return 1;
        }

        // 私有

        [System.Serializable]
        private struct DockedItemRecord
        {
            public string itemID;
            public int number;
        }

        /// <summary>
        /// 捕获当前状态，用于存档。
        /// </summary>
        object ISaveable.CaptureState()
        {
            var state = new Dictionary<int, DockedItemRecord>();
            foreach (var pair in dockedItems)
            {
                var record = new DockedItemRecord();
                record.itemID = pair.Value.item.GetItemID();
                record.number = pair.Value.number;
                state[pair.Key] = record;
            }
            return state;
        }

        /// <summary>
        /// 从存档中恢复状态。
        /// </summary>
        void ISaveable.RestoreState(object state)
        {
            var stateDict = (Dictionary<int, DockedItemRecord>)state;
            foreach (var pair in stateDict)
            {
                AddAction(InventoryItem.GetFromID(pair.Value.itemID), pair.Key, pair.Value.number);
            }
        }
    }
}