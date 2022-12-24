using UnityEngine;
using UnityEngine.Events;

namespace RPG.Combat
{
    /* This class is required because WeaponConfic is a scriptable object and it cannot store reference
    to audio source (or any scene game object). This class also allows different audio effects 
    for the same WeaponConfig (e.g different types of swords have different audio effects) */
    public class Weapon : MonoBehaviour
    {
        [SerializeField] UnityEvent onHit;
        public void OnHit() 
        {
            onHit.Invoke();
        }
    }
}
