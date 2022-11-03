using System;
using UnityEngine;
using TMPro;

namespace RPG.Stats
{
    public class LevelDisplay : MonoBehaviour
    {
        BaseStats baseStats;

        private void Awake()
        {
            baseStats = GameObject.FindGameObjectWithTag("Player").GetComponent<BaseStats>();    
        }

        private void Update() 
        {
           GetComponent<TextMeshProUGUI>().text = String.Format("Level: {0:0}", baseStats.GetLevel());
        }
    }
}