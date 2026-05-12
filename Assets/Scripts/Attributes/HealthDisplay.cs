using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace RPG.Attributes
{
    public class HealthDisplay : MonoBehaviour
    {
        Health health;
        void Awake()
        {
            health = GameObject.FindWithTag("Player").GetComponent<Health>();
        }


        // Update is called once per frame
        void Update()
        {
            GetComponent<TextMeshProUGUI>().text =String.Format("{0:0}/{1:0}", health.GetHealthPoints(), health.GetMaxHealthPoints());
            //GetComponent<UnityEngine.UI.Text>().text = String.Format("{0:F0}%", health.GetPercentage());
        }
    }
}
