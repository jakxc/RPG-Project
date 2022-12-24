using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.UI.DisplayText
{
    public class DisplayTextSpawner : MonoBehaviour
    {
        [SerializeField] DisplayText displayTextPrefab = null;
        private Queue<Action> actions = new Queue<Action>();
        private float last = 0;
        private bool emptyQueue = true;

        private void Update()
        {
            last += Time.deltaTime;
            if (actions.Count > 0 && (emptyQueue || last > .25f))
            {
                var action = actions.Dequeue();
                action.Invoke();
                last = 0;
                emptyQueue = actions.Count == 0;
            }
        }
    
        // Called when TakeDamageEvent<float> is invoked in Health 
        public void SpawnDisplayNumber(float amount)
        {
            if (displayTextPrefab)
            {
                actions.Enqueue(() =>
                {
                    DisplayText instance = Instantiate<DisplayText>(displayTextPrefab, transform);
                    instance.SetValue(amount);
                });            
            }
        }

        public void SpawnDisplayString(string message)
        {
            if (displayTextPrefab)
            {
                actions.Enqueue(() =>
                {
                    DisplayText instance = Instantiate<DisplayText>(displayTextPrefab, transform);
                    instance.SetString(message);
                });        
            }
        }
    }
}
