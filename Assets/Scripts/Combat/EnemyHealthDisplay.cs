using System;
using UnityEngine;
using RPG.Attributes;
using TMPro;

namespace RPG.Combat
{
    public class EnemyHealthDisplay : MonoBehaviour
    {
        Fighter fighter;

        private void Awake()
        {
           fighter = GameObject.FindGameObjectWithTag("Player").GetComponent<Fighter>();    
        }

        private void Update() 
        {
            if (fighter.GetTarget() == null)
            {
                 GetComponent<TextMeshProUGUI>().text = "Enemy: N/A";
                 return;
            }
            
            Health health = fighter.GetTarget();
            GetComponent<TextMeshProUGUI>().text = String.Format("Enemy: {0:0}/{1:0}", health.GetHealth(), health.GetMaxHealth());
        }
    }
}