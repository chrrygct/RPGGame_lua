using System.Collections;
using UnityEngine;
using RPG.Attributes;
using RPG.Control;

namespace RPG.Combat
{
    public class HealthPickup : MonoBehaviour, IRaycastable
    {
        [SerializeField] float healthToRestore = 0;
        [SerializeField] float respawnTime = 5f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Pickup(other.gameObject);
            }
        }

        private void Pickup(GameObject subject)
        {
            var health = subject.GetComponent<Health>();
            if (health == null) return;
            if (health.GetHealthPoints() >= health.GetMaxHealthPoints()) return;

            health.Heal(healthToRestore);
            StartCoroutine(HideForSeconds(respawnTime));
        }

        private IEnumerator HideForSeconds(float seconds)
        {
            ShowPickup(false);
            yield return new WaitForSeconds(seconds);
            ShowPickup(true);
        }

        private void ShowPickup(bool shouldShow)
        {
            GetComponent<Collider>().enabled = shouldShow;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(shouldShow);
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
