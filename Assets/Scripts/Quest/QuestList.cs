using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Saving;
using RPG.Inventories;
using RPG.Core;

namespace RPG.Quests
{
    public class QuestList : MonoBehaviour, ISaveable, IPredicateEvaluator
    {
        List<QuestStatus> statuses = new List<QuestStatus>();

        public event Action onUpdate;

        public void AddQuest(Quest quest)
        {
            if (HasQuest(quest)) return;
            QuestStatus newStatus = new QuestStatus(quest);
            statuses.Add(newStatus);
            if (onUpdate != null)
            {
                onUpdate();
            }
        }

        //函数负责完成一个任务目标
        public void CompleteObjective(Quest quest, string objective)
        {
           
            QuestStatus status = GetQuestStatus(quest);
            status.CompleteObjective(objective);
            if (status.IsComplete())
            {
                GiveReward(quest);
            }
            if (onUpdate != null)
            {
                onUpdate();
            }
          
        }

        //函数负责检查是否有一个任务
        public bool HasQuest(Quest quest)
        {
            return GetQuestStatus(quest) != null;
        }

        //函数负责获取所有任务状态
        public IEnumerable<QuestStatus> GetStatuses()
        {
            return statuses;
        }



        //函数负责获取一个任务的状态，如果没有这个任务则返回null
        private QuestStatus GetQuestStatus(Quest quest)
        {
            foreach (QuestStatus status in statuses)
            {
                if (status.GetQuest() == quest)
                {
                    return status;
                }
            }
            return null;
        }

        //函数负责给予奖励，如果这个任务的所有目标都完成了，就给玩家奖励
        private void GiveReward(Quest quest)
        {
            foreach (var reward in quest.GetRewards())
            {
                bool success = GetComponent<Inventory>().AddToFirstEmptySlot(reward.item, reward.number);
                if (!success)
                {
                    GetComponent<ItemDropper>().DropItem(reward.item, reward.number);
                }
            }
        }


        public object CaptureState()
        {
            List<object> state = new List<object>();
            //把每个任务状态的序列化数据添加到列表中
            foreach (QuestStatus status in statuses)
            {
                state.Add(status.CaptureState());
            }
            return state;
        }

        public void RestoreState(object state)
        {
            //把传入的状态转换为一个列表
            List<object> stateList = state as List<object>;
            //如果列表为null，说明没有保存的状态，直接返回
            if (stateList == null) return;
            //清空当前的任务状态列表
            statuses.Clear();
            //把每个序列化数据转换为一个任务状态，并添加到列表中
            foreach (object objectState in stateList)
            {
                statuses.Add(new QuestStatus(objectState));
            }
        }
        
        //函数负责评估一个条件谓词，如果谓词是"HasQuest"，就检查玩家是否有指定的任务
        public bool? Evaluate(string predicate, string[] parameters)
        {
            switch (predicate)
            {
                case "HasQuest": 
                return HasQuest(Quest.GetByName(parameters[0]));
                case "CompletedQuest":
                return GetQuestStatus(Quest.GetByName(parameters[0])).IsComplete();
            }

            return null;
        }
    }

}