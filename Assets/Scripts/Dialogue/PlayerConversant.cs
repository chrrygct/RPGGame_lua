﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPG.Core;
namespace RPG.Dialogue
{
    public class PlayerConversant : MonoBehaviour
    {

        [SerializeField] string playerName = "Player";
        Dialogue currentDialogue;


        DialogueNode currentNode = null;
        AIConversant currentConversant = null;
        bool isChoosing = false;

        public event System.Action onConversationUpdated;






        public void StartDialogue(AIConversant newConversant,Dialogue dialogue)
        {
            currentConversant = newConversant;
            currentDialogue = dialogue;
            currentNode = currentDialogue.GetRootNode();
            TriggerEnterAction();
            onConversationUpdated();
        }

        public void Quit()
        {
            TriggerExitAction();
            currentNode=null;
            isChoosing = false;
            currentDialogue = null;
            onConversationUpdated();
        }   


        public bool IsChoosing()
        {
            return isChoosing;
        }

        public string GetText()
        {
            if (currentNode == null)
            {
                return "";
            }

            return currentNode.GetText();
        }

        public string GetCurrentConversantName()
        {
            if (isChoosing)
            {
                return playerName;
            }
            else
            {
                return currentConversant.GetName();
            }

            
        }

        public bool IsActive()
        {
            return currentDialogue != null;
        }

        //返回玩家选项
        public IEnumerable<DialogueNode> GetChoices()
        {
            return FilterOnCondition(currentDialogue.GetPlayerChildren(currentNode));
        }
    
        //玩家选择后，进入下一节点
        public void SelectChoice(DialogueNode chosenNode)
        {
            currentNode = chosenNode;
            TriggerEnterAction();
            isChoosing = false;
            Next();
        }
        
        //进入下一节点
        public void Next()
        {
            //如果玩家有选项，进入选择状态
            int numPlayerResponses = FilterOnCondition(currentDialogue.GetPlayerChildren(currentNode)).Count();
            if (numPlayerResponses > 0)
            {
                isChoosing = true;
                TriggerExitAction();
                onConversationUpdated();
                return;
            }

            //如果没有选项，随机进入一个AI节点
            DialogueNode[] children = FilterOnCondition(currentDialogue.GetAIChildren(currentNode)).ToArray();
            if (children.Length == 0)
            {
                currentNode = null;
                isChoosing = false;
                return;
            }
            int randomIndex = Random.Range(0, children.Count());
            TriggerExitAction();
            currentNode = children[randomIndex];
            TriggerEnterAction();
            onConversationUpdated();
            
        }

        //判断是否还有下一节点
        public bool HasNext()
        {
            return FilterOnCondition(currentDialogue.GetAllChildren(currentNode)).Count() > 0;
        }

        //根据条件过滤节点
        private IEnumerable<DialogueNode> FilterOnCondition(IEnumerable<DialogueNode> inputNode)
        {
            foreach (var node in inputNode)
            {
                if (node.CheckCondition(GetEvaluators()))
                {
                    yield return node;
                }
            }
        }

        //获取当前对象上的所有条件评估器
        private IEnumerable<IPredicateEvaluator> GetEvaluators()
        {
            return GetComponents<IPredicateEvaluator>();
        }




        private void TriggerEnterAction()
        {
            if (currentNode != null )
            {
                TriggerAction(currentNode.GetOnEnterAction());
            }
        }

        private void TriggerExitAction()
        {
            if (currentNode != null )
            {
                TriggerAction(currentNode.GetOnExitAction());
            }
        }

        //触发节点上的事件
        private void TriggerAction(string action)
        {
            if (action == "") return;

            foreach (DialogueTrigger trigger in currentConversant.GetComponents<DialogueTrigger>())
            {
                trigger.Trigger(action);
            }
        }

    }
}