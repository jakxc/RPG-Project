using RPG.Core;
using RPG.Stats;
using GameDevTV.Saving;
using GameDevTV.Utils;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace RPG.Attributes
{
    // Health is placed in Attributes directory to avoid circular dependecy that could occur when placed in Core namespace
    public class Health : MonoBehaviour, ISaveable
    {
        [SerializeField] float regenerationPercentage = 70f; // % of health that is added when this levels up
        [SerializeField] TakeDamageEvent takeDamage;
        [SerializeField] UnityEvent onDied;

        /* Subclass used because cannot Unity cannot serialize UnityEvent<float> (classes with chevrons). Select Dynamic float in
        Unity Editor to allow the float passed into this event to be used in functions triggered by 
        this event*/ 
        [System.Serializable]
        public class TakeDamageEvent : UnityEvent<float>
        {
        }

        public event Action onHealthUpdated;
        public event Action onNoHealthLeft;

        /* Setting healthpoints as type LazyValue<float> ensures that healthPoints are 
        set to something in Awake so it is not null and set its value when it is first used*/
        LazyValue<float> healthPoints; 
        bool hasDied;
        
        private void Awake() 
        {
            healthPoints = new LazyValue<float>(GetInitialHealth);
        }

        private float GetInitialHealth()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);
        }

        private void Start() 
        { 
            healthPoints.ForceInit(); // Initialise healthpoints if it has not already been before Start() is called
            onHealthUpdated.Invoke();
        }

        private void OnEnable() 
        {
            GetComponent<BaseStats>().onLevelUpdated += RegenerateHealth; // Subscribe to onLevelUp event in OnEnable  
        }

        private void OnDisable() 
        {
            GetComponent<BaseStats>().onLevelUpdated -= RegenerateHealth;  // Unsubscribe to onLevelUp event in OnDisable  
        }

        public bool IsDead()
        {
            return hasDied;
        }

        /* sourceOfDamage passed in as Parameter to determine how much experience reward
         the sourceOfDamage should receive from this*/
        public void TakeDamage(GameObject sourceOfDamage, float damage)
        {
            healthPoints.value = Mathf.Max(healthPoints.value - damage, 0);
            
            if (healthPoints.value == 0)
            {
                onNoHealthLeft.Invoke(); // Invoke onNoHealthLeft event to disable health bar
                onDied.Invoke(); // Trigger all the functions in onDied UnityEvent (e.g audio sound etc.)
                Die();
                RewardExperience(sourceOfDamage);
            }
            else
            {
                onHealthUpdated.Invoke(); // Invoke onNoHealthLeft event to update health bar
                /* Trigger all the functions in takeDamage UnityEvent (e.g spawn damage text etc.).
                The damage float is passed in as arg so DamageTextSpawner can use the correct damage value
                to spawn as text*/
                takeDamage.Invoke(damage); 
            }
        }

        public float GetHealth()
        {
            return healthPoints.value;
        }

        public float GetMaxHealth()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);
        }

        public float GetPercentage()
        {
            return (healthPoints.value / GetComponent<BaseStats>().GetStat(Stat.Health)) * 100;
        }

        public float GetFraction()
        {
            return healthPoints.value / GetComponent<BaseStats>().GetStat(Stat.Health);
        }

        private void Die()
        {
            // If target health is already dead, do nothing
            if (hasDied) return;

            hasDied = true;

            // Trigger death animation
            GetComponent<Animator>().SetTrigger("die");

            // Cancel any action (Fighter, Mover etc) that has been set 
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }
        
        private void RewardExperience(GameObject sourceOfDamage)
        {
            Experience experience = sourceOfDamage.GetComponent<Experience>();
            if (experience == null) return;

            experience.GainExperience(GetComponent<BaseStats>().GetStat(Stat.ExperienceReward));
        }

        private void RegenerateHealth()
        {
            float regenHealthPoints = GetComponent<BaseStats>().GetStat(Stat.Health) * (regenerationPercentage / 100);
            healthPoints.value = Mathf.Max(healthPoints.value, regenHealthPoints);
            onHealthUpdated.Invoke();
        }

        public void Heal(float healthToRestore)
        {
             healthPoints.value = Mathf.Min(healthPoints.value + healthToRestore, GetMaxHealth());
             onHealthUpdated.Invoke();
        }

        public object CaptureState()
        {
            return healthPoints.value;
        }

        public void RestoreState(object state)
        {
            healthPoints.value = (float)state;

            if (healthPoints.value == 0)
            {
                Die();
            }
        }
    }
}
