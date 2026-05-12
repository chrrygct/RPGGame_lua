using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace RPG.Stats
{
    public class ExperienceDisplay : MonoBehaviour
    {
        Experience experience;
        void Awake()
        {
            experience = GameObject.FindWithTag("Player").GetComponent<Experience>();
        }


        // Update is called once per frame
        void Update()
        {
            GetComponent<TextMeshProUGUI>().text =String.Format("{0:F0}", experience.GetExperiencePoints());
            //GetComponent<UnityEngine.UI.Text>().text = String.Format("{0:F0}%", experience.GetPercentage());
        }
    }
}
