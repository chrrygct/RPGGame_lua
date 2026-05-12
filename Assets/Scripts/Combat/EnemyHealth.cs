using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RPG.Attributes;
namespace RPG.Combat
{
    public class EnemyHealth : MonoBehaviour
    {
        Fighter fighter;
        void Awake()
        {
            fighter =GameObject.FindWithTag("Player").GetComponent<Fighter>();
        }


        // Update is called once per frame
        void Update()
        {
            if (fighter.GetTarget() == null) 
            {
                GetComponent<TextMeshProUGUI>().text = "N/A";
                //GetComponent<UnityEngine.UI.Text>().text = "";
                return;
            }
            Health health = fighter.GetTarget();
            GetComponent<TextMeshProUGUI>().text = String.Format("{0:0}/{1:0}", health.GetHealthPoints(), health.GetMaxHealthPoints());
        }
    }
}
