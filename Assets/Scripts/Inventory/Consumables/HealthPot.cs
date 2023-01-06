using System.Collections;
using System.Collections.Generic;
using GameDevTV.Inventories;
using RPG.Attributes;
using UnityEngine;

namespace RPG.Inventory.Consumables
{
    [CreateAssetMenu(menuName = ("RPG/Inventory/Action Item/Health Pot"))]
    public class HealthPot : ActionItem
    {
        [SerializeField] float healthToRestore = 0f;
        public override void Use(GameObject user)
        {
            user.GetComponent<Health>().Heal(healthToRestore);
        }
    }    
}
