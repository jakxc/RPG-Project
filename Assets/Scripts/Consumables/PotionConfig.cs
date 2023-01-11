using System.Collections.Generic;
using GameDevTV.Inventories;
using RPG.Attributes;
using RPG.Stats;
using UnityEngine;

namespace RPG.Consumables
{
    [CreateAssetMenu(menuName = ("RPG/Inventory/Action Item/Make New Potionx/"))]
    public class PotionConfig : ActionItem, IModifierProvider
    {
        [Tooltip("Health/mana restored by this potion")]
        [SerializeField] float amountToRestore = 0f; // This represents health to restore soley for now
        [SerializeField] float additiveBuffAmount = 0f; 
        [SerializeField] float percentageBuffAmount = 0f; 
        [SerializeField] Stat statToModify; // The stat the potion modifies (e.g Health, Mana etc)
        public override void Use(GameObject user)
        {
            if (amountToRestore > 0)
            {
                user.GetComponent<Health>().Heal(amountToRestore);
            }
        }

        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            if (stat == statToModify)
            {
                yield return additiveBuffAmount;
            }
        }

        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
            if (stat == statToModify)
            {
                yield return percentageBuffAmount;
            }
        }
     }    
}
