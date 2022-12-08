using UnityEngine;

namespace RPG.Attributes
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] Health health = null;
        [SerializeField] RectTransform foreground = null;
        [SerializeField] Canvas rootCanvas = null;
    
        private void OnEnable()    
        {
              health.onHealthUpdated += UpdateHealthBar;
              health.onNoHealthLeft += DisableHealthBar;
        }

        private void OnDisable()    
        {
              health.onHealthUpdated -= UpdateHealthBar;
              health.onNoHealthLeft -= DisableHealthBar;
        }
   
        private void UpdateHealthBar()
        {   
            // If health is zero or full, do not display health bar
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
