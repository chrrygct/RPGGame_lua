using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Attributes;
using RPG.Combat;
using RPG.Core;
using RPG.Movement;
using UnityEngine;
using GameDevTV.Utils;
namespace RPG.Control
{   

    public class AIController : MonoBehaviour
    {

        [SerializeField] private float chaseDistance = 5f;
        [SerializeField] private float shoutDistance = 5f;
        [SerializeField] private float suspicionTime = 3f;
        [SerializeField] private float aggrevationCooldownTime = 5f;
        [SerializeField] private PatrolPath patrolPath;
        [SerializeField] private float waypointTolerance = 3f;
        [SerializeField] private float waypointDwellTime = 2f;
        [Range(0,1)]
        [SerializeField] private float patrolSpeedFraction = 0.2f;
        Fighter fighter;
        Health health;
        Mover mover;
        GameObject player;
        LazyValue<Vector3> guardLocation;
        float timeSinceLastSawPlayer = Mathf.Infinity;
        float timeSinceAggrevated = Mathf.Infinity;
        float timeSinceArrivedAtWaypoint = Mathf.Infinity;
        int currentWaypointIndex = 0;

        void Awake()
        {
            fighter = GetComponent<Fighter>();
            health = GetComponent<Health>();
            mover = GetComponent<Mover>();
            player = GameObject.FindWithTag("Player");
            guardLocation = new LazyValue<Vector3>(GetGuardPosition);

        }

        private Vector3 GetGuardPosition()
        {
            return transform.position;
        }

        void Start()
        {            
            guardLocation.ForceInit();
        }
        // Update is called once per frame
        void Update()
        {             
            if (health.IsDead()) return;
            if (InAggrevated() && fighter.CanAttack(player))
            {
                // Chase the player
                AttackBehaviour();
            }


            else if (timeSinceLastSawPlayer < suspicionTime)
            {
                // suspicious state
                SuspicionBehaviour();

            }
            else
            {
                //fighter.Cancel();
                // Return to guard location
                PatrolBehaviour();
            }
            timeSinceLastSawPlayer += Time.deltaTime;
            timeSinceArrivedAtWaypoint += Time.deltaTime;
            timeSinceAggrevated += Time.deltaTime;
        }
        public void Aggrevate()
        {
            timeSinceAggrevated = 0;
        }

        private void PatrolBehaviour()
        {

            Vector3 nextPosition = guardLocation.value;
            if (patrolPath != null)
            {
                if (AtWaypoint())
                {
                    timeSinceArrivedAtWaypoint = 0;
                    CycleWaypoint();

                }
                nextPosition = GetCurrentWaypoint();
                
            }
            if(timeSinceArrivedAtWaypoint > waypointDwellTime)
            {
            mover.StartMoveAction(nextPosition, patrolSpeedFraction);
            }
        }

        private Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypoint(currentWaypointIndex);
        }

        private void CycleWaypoint()
        {
            currentWaypointIndex = patrolPath.GetNextIndex(currentWaypointIndex);
            //print("New waypoint: " + currentWaypointIndex);
        }

        private bool AtWaypoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
            //print("Distance to waypoint: " + distanceToWaypoint);
            //print("Waypoint tolerance: " + waypointTolerance);
            return distanceToWaypoint < waypointTolerance;
        }






        private void SuspicionBehaviour()
        {
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        private void AttackBehaviour()
        {
            timeSinceLastSawPlayer = 0;
            fighter.Attack(player);

            AggrevateNearbyEnemies();
        }

        private void AggrevateNearbyEnemies()
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, shoutDistance, Vector3.up, 0);
            foreach (RaycastHit hit in hits)
            {
                AIController ai = hit.collider.GetComponent<AIController>();
                if (ai == null) continue;
                ai.Aggrevate();
            }
        }

        private bool InAggrevated()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            return distanceToPlayer < chaseDistance || timeSinceAggrevated < aggrevationCooldownTime;
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}