// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Events;

// namespace RPG.Dialogue
// {
//     public class DialogueTrigger : MonoBehaviour
//     {
//         [SerializeField] string action;
//         [SerializeField] UnityEvent onTrigger;

//         public void Trigger(string actionToTrigger)
//         {
//             if (actionToTrigger == action)
//             {
//                 Debug.Log("Triggering dialogue action: " + action);
//                 onTrigger.Invoke();
//             }
//         }
//     }
// }


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RPG.Dialogue
{


    public class DialogueTrigger : MonoBehaviour
    {
        [System.Serializable]
        public class ActionEvent
        {
        public string action;
        public UnityEvent onTrigger;
        }
        
        [SerializeField] List<ActionEvent> actions;

        public void Trigger(string actionToTrigger)
        {
            foreach (var a in actions)
            {
                if (a.action == actionToTrigger)
                {
                    a.onTrigger.Invoke();
                }
            }
        }
    }
}