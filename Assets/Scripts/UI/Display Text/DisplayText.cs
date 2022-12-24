using UnityEngine;
using TMPro;
using System;

namespace RPG.UI.DisplayText
{
    public class DisplayText : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI displayText = null;
        [SerializeField] Color damageColor = new Color(142f, 0f, 0f, 1f);
        [SerializeField] Color experienceColor = new Color(106f, 255f, 120f, 1f);
      
         public void SetValue(float amount)
        {
            displayText.color = damageColor;
            displayText.text = String.Format("{0:0}", amount);
        }

        public void SetString(string message)
        {
            displayText.color = experienceColor;
            displayText.text = message;
        }
    }
}
