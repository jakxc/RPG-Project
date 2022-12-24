using RPG.Combat;
using RPG.Attributes;
using UnityEngine;

namespace RPG.UI.HealthUI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] Health health = null;
        [SerializeField] RectTransform foreground = null;
        [SerializeField] Canvas rootCanvas = null;

        Fighter fighter;
        
        private void Awake() 
        {
            fighter = GetComponent<Fighter>();
        }

        private void OnEnable()    
        {
            health.onHealthUpdated += UpdateHealthBar;
            health.onNoHealthLeft += DisableHealthBar;
             
            if (fighter != null) 
            {
                fighter.onNotInCombat += DisableHealthBar;
            }
        }

        private void OnDisable()    
        {
              health.onHealthUpdated -= UpdateHealthBar;
              health.onNoHealthLeft -= DisableHealthBar;
        }
   
        private void UpdateHealthBar()
        {   
            // If health is zero/full, disable health bar
            if (Mathf.Approximately(health.GetFraction(), 0) || Mathf.Approximately(health.GetFraction(), 1)) 
            {
                rootCanvas.enabled = false;
                return;
            }
            
            rootCanvas.enabled = true;
            foreground.localScale = new Vector3(health.GetFraction(), 1.0f, 1.0f);      
        }

        private void DisableHealthBar()
        {
            rootCanvas.enabled = false;
        }
    }
}
