using RPG.Core;
using RPG.Stats;
using RPG.Saving;
using RPG.Attributes;
using RPG.Movement;
using UnityEngine;
using System.Collections.Generic;
using GameDevTV.Utils;
using System;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction, ISaveable, IModifierProvider
    {
        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] Transform rightHandTransform = null;
        [SerializeField] Transform leftHandTransform = null;
        [SerializeField] Weapon defaultWeapon = null;
        float timeSinceLastAttack = Mathf.Infinity;
        Health target;
        Mover mover;
        LazyValue<Weapon> currentWeapon;

        private void Awake() 
        {
            mover = GetComponent<Mover>();    
            currentWeapon = new LazyValue<Weapon>(SetUpDefaultWeapon);
        }

        private Weapon SetUpDefaultWeapon()
        {
           EquipWeapon(defaultWeapon);
           return defaultWeapon;
        }

        void Start() 
        {            
            currentWeapon.ForceInit();
        }

        private void Update() 
        {
            timeSinceLastAttack += Time.deltaTime;
            
            if (target == null) return;
            if (target.IsDead()) return; 

            if (!GetIsInRange())
            {
                mover.MoveTo(target.transform.position, 1f);
            }
            else
            {
                mover.Cancel();
                AttackBehaviour();
            }
        }

        public void SetWeapon(Weapon weapon)
        {
            currentWeapon.value = weapon;
            EquipWeapon(weapon);
        }

        private void EquipWeapon(Weapon weapon)
        {
            Animator anim = GetComponent<Animator>();
            weapon.Spawn(rightHandTransform, leftHandTransform, anim);
        }

        public Health GetTarget()
        {
            return target;
        }

        void AttackBehaviour()
        {
            transform.LookAt(target.transform);
            
            // This will trigger Hit() event
            if (timeSinceLastAttack > timeBetweenAttacks)
            {
                TriggerAttack();
                timeSinceLastAttack = 0;
            }
        }

        private void TriggerAttack()
        {
            GetComponent<Animator>().ResetTrigger("stopAttack");
            GetComponent<Animator>().SetTrigger("attack");
        }

        // Animation Event
        void Hit()
        {
            if (target == null) return;

            float damage = GetComponent<BaseStats>().GetStat(Stat.Damage);
            if (currentWeapon.value.HasProjectile())
            {
                currentWeapon.value.LaunchProjectile(rightHandTransform, leftHandTransform, target, gameObject, damage);
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

        bool GetIsInRange()
        {
            return Vector3.Distance(transform.position, target.transform.position) < currentWeapon.value.GetRange();
        }

        
        public bool CanAttack(GameObject combatTarget)
        {
            if (combatTarget == null) return false;
            
            Health target = combatTarget.GetComponent<Health>();
            return target != null && !target.IsDead();
        }


        public void Attack(GameObject combatTarget)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            target = combatTarget.GetComponent<Health>();
        }

        public void Cancel()
        {
            StopAttack();
            GetComponent<Mover>().Cancel();
            target = null;
        }

        private void StopAttack()
        {
            GetComponent<Animator>().ResetTrigger("attack");
            GetComponent<Animator>().SetTrigger("stopAttack");
        }
        
        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return currentWeapon.value.GetDamage();
            }
        }
        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
             if (stat == Stat.Damage)
            {
                yield return currentWeapon.value.GetPercentageBonus();
            }
        }

        public object CaptureState()
        {
            return currentWeapon.value.name;
        }

        public void RestoreState(object state)
        {
            string weaponName = (string)state;
            Weapon weapon = Resources.Load<Weapon>(weaponName);
            EquipWeapon(weapon);
        }
    }
}
