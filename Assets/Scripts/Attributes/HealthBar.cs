using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RPG.Attributes
{

    public class HealthBar : MonoBehaviour
    {
        [SerializeField] Health healthComponent=null;
        [SerializeField] RectTransform foreground = null;
        [SerializeField] GameObject healthBar;

        // Update is called once per frame
        void Update()
        {
            foreground.localScale = new Vector3(healthComponent.GetFraction(), 1, 1);
            if (healthComponent.IsDead())
            {
                healthBar.SetActive(false);
            }
            else
            {
                healthBar.SetActive(true);
            }
        }
    }
}