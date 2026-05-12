using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace RPG.Stats
{
    public class LevelDisplay : MonoBehaviour
    {
        BaseStats baseStats;
        void Awake()
        {
            baseStats = GameObject.FindWithTag("Player").GetComponent<BaseStats>();
        }


        // Update is called once per frame
        void Update()
        {
            GetComponent<TextMeshProUGUI>().text =String.Format("{0:F0}", baseStats.GetLevel());
            //GetComponent<UnityEngine.UI.Text>().text = String.Format("{0:F0}%", baseStats.GetExperiencePercentage());
        }
    }
}
