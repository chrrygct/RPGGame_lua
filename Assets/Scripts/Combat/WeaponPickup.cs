using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Attributes;
using RPG.Control;
using Unity.VisualScripting;
using UnityEngine;
namespace RPG.Combat
{   
    public class WeaponPickup : MonoBehaviour,IRaycastable
    {
        // Start is called before the first frame update
        void Start()
        {
            
        }
        [SerializeField] WeaponConfig weapon=null;
        [SerializeField] float respawnTime = 5f;
        [SerializeField] float healthToRestore=0;
        // Update is called once per frame
        void Update()
        {

        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                Pickup(other.gameObject);
            }
        }

        private void Pickup(GameObject subject)
        {
            if(weapon!=null)
            {
            subject.GetComponent<Fighter>().EquipWeapon(weapon);
            }
            if(healthToRestore>0)
            {
                subject.GetComponent<Health>().Heal(healthToRestore);
            }
            StartCoroutine(HideForSeconds(respawnTime));

        }

        private IEnumerator HideForSeconds(float seconds)
        {
            HidePickup(false);
            yield return new WaitForSeconds(seconds);
            ShowPickup(true);

        }

        private void ShowPickup(bool shouldshow)
        {
            GetComponent<Collider>().enabled = shouldshow;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(shouldshow);
            }
        }

        private void HidePickup(bool shouldshow)
        {
            GetComponent<Collider>().enabled = shouldshow;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(shouldshow);
            }
        }

        public bool HandleRaycast(PlayerController callingController)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Pickup(callingController.gameObject);
            }
            return true;

        }

        public CursorType GetCursorType()
        {
            return CursorType.Pickup;
        }
    }
}
