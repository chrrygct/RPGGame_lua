using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Core
{
    public class ActionScheduler : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        IAction currentAction;
        public void StartAction(IAction action)
        {
            if (currentAction == action) return;
            if (currentAction != null) 
            {
                //print("Stopping " + currentAction);
                //currentAction.StopAllCoroutines();
                currentAction.Cancel();
            }
            currentAction = action;
        }

        //停止当前行为，并清空当前行为引用
        public void CancelCurrentAction()
        {
            StartAction(null);
        }

    }

}
