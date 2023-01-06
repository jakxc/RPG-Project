using System;
using System.Collections;
using System.Collections.Generic;
using GameDevTV.Inventories;
using RPG.Attributes;
using UnityEngine;

namespace RPG.Combat
{
    [CreateAssetMenu(fileName = "Weapon", menuName = "Weapons/Make New Weapon", order = 0)]
    public class WeaponConfig : EquipableItem
    {
        [SerializeField] float weaponDamage = 5f;
        [SerializeField] float percentageBonus = 0f; // % bonus that will be added to base damage when equipped 
        [SerializeField] float weaponRange = 2f;
        [SerializeField] bool isRightHanded = true;
        [SerializeField] Projectile projectile = null;
        [SerializeField] AnimatorOverrideController animatorOverride = null; // Animator controller specific to weapon equipped
        [SerializeField] Weapon weaponPrefab = null;

        const string weaponName = "Weapon";

        public Weapon Spawn(Transform rightHand, Transform leftHand, Animator anim)
        {   
            DestroyOldWeapon(rightHand, leftHand);
            
            Weapon weapon = null;

            if (weaponPrefab != null)
            {
                Transform handTransform = GetTransform(rightHand, leftHand); // Determines if weapon is equip on right or left hand
                weapon = Instantiate(weaponPrefab, handTransform);
                weapon.gameObject.name = weaponName; // Set weaponName to WeaponConfig name for saving/loading purposes
            }

            /* Cast as AnimatorOverrideController to be able to set it as default animator controller if
             animatorOverride is null*/
            var overrideController = anim.runtimeAnimatorController as AnimatorOverrideController;
            if (animatorOverride != null)
            {
                anim.runtimeAnimatorController = animatorOverride; // If there is animator override for weapon, set it as runtime animator controller
            }
            /* If the weapon does not have animator override set, set animator controller as default animator controller.
            This prevents this from using previous WeaponConfig animator override controller 
            (e.g fireball using sword animation) */
            else if (overrideController != null) 
            {
                anim.runtimeAnimatorController = overrideController.runtimeAnimatorController;
                
            }

            return weapon;
        }

        private void DestroyOldWeapon(Transform rightHand, Transform leftHand)
        {
            Transform oldWeapon = rightHand.Find(weaponName);
            if (oldWeapon == null)
            {
                oldWeapon = leftHand.Find(weaponName);
            }
            if (oldWeapon == null) return;

            oldWeapon.name = "Destroying";
            Destroy(oldWeapon.gameObject);
        }

        private Transform GetTransform(Transform rightHand, Transform leftHand)
        {
            Transform handTransform;
            if (isRightHanded) handTransform = rightHand;
            else handTransform = leftHand;
            return handTransform;
        }

        public float GetDamage()
        {
            return weaponDamage;
        }

        public float GetPercentageBonus()
        {
            return percentageBonus;
        }

        public float GetRange()
        {
            return weaponRange;
        } 

        public bool HasProjectile()
        {
            return projectile != null;
        }

        public void LaunchProjectile(Transform rightHand, Transform leftHand, Health target, GameObject sourceOfDamage, float calculatedDamage)
        {
            Projectile projectileInstance = Instantiate(projectile, GetTransform(rightHand, leftHand).position, Quaternion.identity);
            projectileInstance.SetTarget(target, calculatedDamage, sourceOfDamage);
        }
    }
}
