using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Movement;
using RPG.Combat;
using RPG.Inventories;
using RPG.Attributes;
using System;
using UnityEngine.AI;
namespace RPG.Control
{
    public class PlayerController : MonoBehaviour
    {
    // Start is called before the first frame update
        Health health;

        [System.Serializable]
        struct CursorMapping
        {
            public CursorType type;
            public Texture2D texture;
            public Vector2 hotspot;
        }

        [SerializeField] CursorMapping[] cursorMappings= null;
        [SerializeField] float maxNavMeshProjectionDistance = 1f;


        void Awake()
        {
        health = GetComponent<Health>();
        }

        // Update is called once per frame
        void Update()
        {
            CheckSpecialAbilityKeys();
            if (InteractWithUI()) return;
            if (health.IsDead()) 
            {
                SetCursorType(CursorType.None);
                return;
            }

            if(InteractWithComponent()) return;
            //if(InteractWithCombat()) return;
            if(InteractWithMovement()) return;
            SetCursorType(CursorType.None);

        }

        private void CheckSpecialAbilityKeys()
        {
            var actionStore = GetComponent<ActionStore>();
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                actionStore.Use(0, gameObject);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                actionStore.Use(1, gameObject);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                actionStore.Use(2, gameObject);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                actionStore.Use(3, gameObject);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                actionStore.Use(4, gameObject);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                actionStore.Use(5, gameObject);
            }
        }
        private bool InteractWithComponent()
        {
            RaycastHit[] hits = RaycastAllSorted();
            foreach (RaycastHit hit in hits)
            {
                IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();
                foreach (IRaycastable raycastable in raycastables)
                {
                    if (raycastable.HandleRaycast(this))
                    {
                        SetCursorType(raycastable.GetCursorType());
                        return true;
                    }
                }
            }
            return false;
        }

        RaycastHit[] RaycastAllSorted()
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());
            float[] distances =new float[hits.Length];
            for (int i = 0; i < hits.Length; i++)
            {
                distances[i] = hits[i].distance;
            }
            Array.Sort(distances, hits);
            return hits;
        }

        private bool InteractWithUI()
        {
            if(UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                SetCursorType(CursorType.UI);
                return true;
            }
            return false;
        }

        // private bool InteractWithCombat()
        // {
        //     RaycastHit[] hits=Physics.RaycastAll(GetMouseRay());
        //     foreach (RaycastHit hit in hits)
        //     {
        //         CombatTarget target = hit.transform.GetComponent<CombatTarget>();
        //         if (target == null) continue;

        //         if(!GetComponent<Fighter>().CanAttack(target.gameObject)) continue;
        //         //if (target == null) continue;
        //         if (Input.GetMouseButtonDown(0))
        //         {
        //             GetComponent<Fighter>().Attack(target.gameObject);
        //         }
        //         SetCursorType(CursorType.Combat);
        //         return true;
        //     }
        //     return false;
        // }

        private void SetCursorType(CursorType type)
        {
            CursorMapping mapping = GetCursorMapping(type);
            Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto); 
        }

        private CursorMapping GetCursorMapping(CursorType type)
        {
            foreach (CursorMapping mapping in cursorMappings)
            {
                if (mapping.type == type)
                {
                    return mapping;
                }
            }
            return cursorMappings[0]; // Return default cursor if type not found
        }

        private bool InteractWithMovement()
        {
            Vector3 target;
            bool hasHit = RaycastNavMesh(out target);
            if (hasHit)
            {
                if(!GetComponent<Mover>().CanMoveTo(target)) return false;
                if (Input.GetMouseButton(0))
                {
                GetComponent<Mover>().StartMoveAction(target, 1f);
                }
                SetCursorType(CursorType.Movement);
                return true;
            }
            return false;
        }
        private bool RaycastNavMesh(out Vector3 target)
        {
            target = new Vector3();
            RaycastHit hit;
            bool hasHit = Physics.Raycast(GetMouseRay(), out hit);
            if (!hasHit) return false;


            NavMeshHit navMeshHit;
            bool hasCastToNavMesh = NavMesh.SamplePosition(hit.point, out navMeshHit, maxNavMeshProjectionDistance, NavMesh.AllAreas);
            if (!hasCastToNavMesh) return false;
            target = navMeshHit.position;


            // NavMeshPath path= new NavMeshPath();
            // bool hasPath = NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);
            // if (!hasPath) return false;
            // if(path.status != NavMeshPathStatus.PathComplete) return false;
            // if(GetPathLength(path) > maxPathLength) return false;
            
            return true;
        }



        private static Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }
    }


}