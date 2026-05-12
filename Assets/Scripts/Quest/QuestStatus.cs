using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Quests
{

    public class QuestStatus
    {

        Quest quest;
        List<string> completedObjectives = new List<string>();

        [System.Serializable]

        //这个类用于保存任务状态的序列化数据
        class QuestStatusRecord
        {
            public string questName;
            public List<string> completedObjectives;
        }

        //构造函数
        public QuestStatus(Quest quest)
        {
            this.quest = quest;
        }
        
        //反序列化构造函数
        public QuestStatus(object objectState)
        {
            //将objectState转换为QuestStatusRecord类型
            QuestStatusRecord state = objectState as QuestStatusRecord;
            //通过任务名称获取任务对象
            quest = Quest.GetByName(state.questName);
            //将完成的任务目标列表赋值给completedObjectives
            completedObjectives = state.completedObjectives;
        }


        public Quest GetQuest()
        {
            return quest;
        }

        public int GetCompletedCount()
        {
            return completedObjectives.Count;
        }

        //检查任务目标是否完成
        public bool IsObjectiveComplete(string objective)
        {
            return completedObjectives.Contains(objective);
        }

        //检查整个任务是否完成
        public bool IsComplete()
        {
            foreach (var objective in quest.GetObjectives())
            {
                if (!completedObjectives.Contains(objective.reference))
                {
                    return false;
                }
            }
            return true;
        }

        //完成任务目标
        public void CompleteObjective(string objective)
        {
            if (quest.HasObjective(objective))
            {
                completedObjectives.Add(objective);
            }
        }
        
        //函数负责捕获任务状态的序列化数据
        public object CaptureState()
        {
            QuestStatusRecord state = new QuestStatusRecord();
            state.questName = quest.name;
            state.completedObjectives = completedObjectives;
            return state;
        }
    }
}