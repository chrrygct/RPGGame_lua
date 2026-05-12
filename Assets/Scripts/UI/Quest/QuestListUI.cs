using System.Collections;
using System.Collections.Generic;
using RPG.Quests;
using UnityEngine;

public class QuestListUI : MonoBehaviour
{

    QuestList questList;
    [SerializeField] QuestItemUI questPrefab;
    
    // Start is called before the first frame update

    void Start()
    {
        //找到玩家身上的QuestList组件，并订阅它的更新事件，以便在任务状态发生变化时刷新UI
        questList = GameObject.FindGameObjectWithTag("Player").GetComponent<QuestList>();
        questList.onUpdate += Redraw;
        Redraw();
    }
    private void Redraw()
    {
        transform.DetachChildren();
        // foreach (Transform item in transform)
        // {
        //     Destroy(item.gameObject);
        // }
        foreach (QuestStatus status in questList.GetStatuses())
        {
            QuestItemUI uiInstance = Instantiate<QuestItemUI>(questPrefab, transform);
            uiInstance.Setup(status);
        }
    }
}