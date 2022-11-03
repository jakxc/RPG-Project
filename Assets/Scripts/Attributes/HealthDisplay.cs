using System;
using UnityEngine;
using TMPro;

namespace RPG.Attributes
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