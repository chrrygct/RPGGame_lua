using System.Collections;
using System.Collections.Generic;
using RPG.Quests;
using TMPro;
using UnityEngine;

public class QuestItemUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI progress;
    QuestStatus status;
    public void Setup(QuestStatus status)
    {
        this.status = status;
        Quest quest = status.GetQuest();
        title.text = status.GetQuest().GetTitle();
        progress.text = status.GetCompletedCount() + "/" + quest.GetObjectiveCount();
    }

    public QuestStatus GetQuestStatus()
    {
        return status;
    }
}