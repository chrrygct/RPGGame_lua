using System.Collections;
using System.Collections.Generic;
using RPG.Attributes;
using RPG.Combat;
using RPG.Control;
using UnityEngine;

namespace RPG.Dialogue
{
    public class AIConversant : MonoBehaviour, IRaycastable
    {
        [SerializeField] Dialogue dialogue = null;
        [SerializeField] string conversantName;

        public CursorType GetCursorType()
        {
            return CursorType.Dialogue;
        }

        public bool HandleRaycast(PlayerController callingController)
        {
            if(gameObject.GetComponent<Health>().IsDead())  return false;
            if (dialogue == null)
            {
                return false;
            }

            CombatTarget combatTarget = GetComponent<CombatTarget>();
            if (combatTarget != null && combatTarget.enabled) return false;

            if (Input.GetMouseButtonDown(0))
            {
                callingController.GetComponent<PlayerConversant>().StartDialogue(this, dialogue);
            }
            return true;
        }

        public string GetName()
        {
            return conversantName;
        }
    }
}