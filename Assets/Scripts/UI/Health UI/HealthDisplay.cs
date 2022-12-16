using System;
using UnityEngine;
using TMPro;
using RPG.Attributes;

namespace RPG.UI.HealthUI
{
    public class HealthDisplay : MonoBehaviour
    {
        Health health;

        private void Awake()
        {
            health = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();    
        }

        private void Update() 
        {
           GetComponent<TextMeshProUGUI>().text = String.Format("Health: {0:0}/{1:0}", health.GetHealth(), health.GetMaxHealth());
        }
    }
}