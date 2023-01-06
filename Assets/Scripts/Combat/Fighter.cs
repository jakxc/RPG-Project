using RPG.Core;
using RPG.Stats;
using GameDevTV.Saving;
using RPG.Attributes;
using RPG.Movement;
using UnityEngine;
using System.Collections.Generic;
using GameDevTV.Utils;
using System;
using GameDevTV.Inventories;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction, ISaveable
    {
        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] float timeToIndicateNotInCombat = 8f; // Used to disable healthbar outside of combat
        [SerializeField] Transform rightHandTransform = null;
        [SerializeField] Transform leftHandTransform = null;
        [SerializeField] WeaponConfig defaultWeapon = null;
        float timeSinceLastAttack = Mathf.Infinity;
        float timeOutOfCombat = Mathf.Infinity;
        Health target;
        Mover mover;
        Equipment equipment;
        WeaponConfig currentWeaponConfig;
        /* If currentWeapon value has not been assign the first time it is read, then the Initializer is called.
         If, however, the value is assigned before the initializer is called, then there is no need to 
        call the Initializer. This prevents race conditions where values can be overwritten or null when
        it is accessed */
        LazyValue<Weapon> currentWeapon; 
        public event Action onNotInCombat;

        private void Awake() 
        {
            mover = GetComponent<Mover>();    
            equipment = GetComponent<Equipment>();

            if (equipment)
            {
                equipment.equipmentUpdated += UpdateWeapon;
            }
            currentWeaponConfig = defaultWeapon;
            currentWeapon = new LazyValue<Weapon>(SetUpDefaultWeapon);
        }

        private Weapon SetUpDefaultWeapon()
        {
           return EquipWeapon(defaultWeapon);
        }

        void Start() 
        {            
            currentWeapon.ForceInit();
        }

        private void Update() 
        {
            timeSinceLastAttack += Time.deltaTime;
            timeOutOfCombat += Time.deltaTime;
            
            // If target is null or dead, do nothing
            if (target == null) return;
            if (target.IsDead()) return; 

            if (timeOutOfCombat > timeToIndicateNotInCombat)
            {
                onNotInCombat.Invoke();
            }

            // If target is out of weapon range, move to target
            if (!GetIsInRange(target.transform))
            {
                mover.MoveTo(target.transform.position, 1f);
            }
            // else cancel movement and start attacking
            else
            {
                mover.Cancel();
                AttackBehaviour();
            }
        }

        public void SetWeapon(WeaponConfig weapon)
        {
            currentWeaponConfig = weapon;
            currentWeapon.value = EquipWeapon(weapon);
        }

        private Weapon EquipWeapon(WeaponConfig weapon)
        {
            Animator anim = GetComponent<Animator>();
            return weapon.Spawn(rightHandTransform, leftHandTransform, anim);
        }
        private void UpdateWeapon()
        {
            WeaponConfig weapon = equipment.GetItemInSlot(EquipLocation.Weapon) as WeaponConfig;
            if (weapon == null) 
            {
                SetWeapon(defaultWeapon);
            } else
            {
                SetWeapon(weapon);
            }
        }


        public Health GetTarget()
        {
            return target;
        }

        void AttackBehaviour()
        {
            // Reset time out of combat
            timeOutOfCombat = 0f;
            
            transform.LookAt(target.transform);
            
            // Only trigger attack if time since last attack reached cooldown
            if (timeSinceLastAttack > timeBetweenAttacks)
            {
                TriggerAttack();
                timeSinceLastAttack = 0;
            }
        }

        private void TriggerAttack()
        {
            // Ensure stopAttack trigger is off so when Fighter is in another animation state, trigger is definitely off
            GetComponent<Animator>().ResetTrigger("stopAttack");

            // Has exit time, so attack animation will complete even when trigger is set off
            GetComponent<Animator>().SetTrigger("attack");
        }

        // Animation Event
        void Hit()
        {
            if (target == null) return;

            float damage = GetComponent<BaseStats>().GetStat(Stat.Damage);
            
            // Trigger onHit event and call all functions (e.g sound effects etc)
            if (currentWeapon.value != null) 
            {
                currentWeapon.value.OnHit();
            }

            if (currentWeaponConfig.HasProjectile())
            {
                currentWeaponConfig.LaunchProjectile(rightHandTransform, leftHandTransform, target, gameObject, damage);
            }
            else
            {
                target.TakeDamage(gameObject, damage);
            }
        }

        void Shoot()
        {
            Hit();
        }

        bool GetIsInRange(Transform targetTransform)
        {
            // Checks if distance between this and target is within weapon range
            return Vector3.Distance(transform.position, targetTransform.position) < currentWeaponConfig.GetRange();
        }

        public bool CanAttack(GameObject combatTarget)
        {
            if (combatTarget == null) return false;
           
            /*If fighter cannot move to combat target and is not in weapon range, then cannot attack 
            combat target. This means that if this cannot move to combatTarget but has a ranged weapon
            that has sufficient range, then it can still attack*/
            if (!GetComponent<Mover>().CanMoveTo(combatTarget.transform.position) && 
            !GetIsInRange(combatTarget.transform)) 
            {
                return false;
            }
            
            Health target = combatTarget.GetComponent<Health>();

            /*If target is not null and not dead (and can move to/is in range), then can attack 
            the target*/
            return target != null && !target.IsDead();
        }

        public float GetTimeOutOfCombat()
        {
            return timeOutOfCombat;
        }

        /* Does not take in CombatTarget type as arg because both player and AI controller
        is dependent on Fighter (shared interface) and player does not have CombatTarget component*/
        public void Attack(GameObject combatTarget)
        {
            // Set action to attack
            GetComponent<ActionScheduler>().StartAction(this);

            // Set target to be attacked
            target = combatTarget.GetComponent<Health>();
        }

        public void Cancel()
        {
            // Stop attack animation
            StopAttack();
            
            // Cancel fighter movement
            GetComponent<Mover>().Cancel();

            // Unset target 
            target = null;
        }

        private void StopAttack()
        {
            // Ensure attack trigger is off when Fighter stops attack
            GetComponent<Animator>().ResetTrigger("attack");

            // Set stopAttack trigger on 
            GetComponent<Animator>().SetTrigger("stopAttack");
        }
        
        public object CaptureState()
        {
            return currentWeaponConfig.name;
        }

        public void RestoreState(object state)
        {
            string weaponName = (string)state;
            
            // Retrieve Weapon from Resources in Weapons directory as GameObjects in Resources are shared between scenes
            WeaponConfig weapon = UnityEngine.Resources.Load<WeaponConfig>(weaponName);
            SetWeapon(weapon); // Set and equip weapon
        }
    }
}
