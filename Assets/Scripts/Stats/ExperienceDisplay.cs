using System;
using UnityEngine;
using TMPro;

namespace RPG.Stats
{
    public class ExperienceDisplay : MonoBehaviour
    {
        Experience experience;

        private void Awake()
        {
            experience = GameObject.FindGameObjectWithTag("Player").GetComponent<Experience>();    
        }

        private void Update() 
        {
           GetComponent<TextMeshProUGUI>().text = String.Format("XP: {0:0}", experience.GetExperience());
        }
    }
}