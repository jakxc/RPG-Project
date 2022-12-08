using UnityEngine;

namespace RPG.UI.DamageText
{
    // This class is required so that the parent of the DamageText can be destroyed when spawned
    public class Destroyer : MonoBehaviour
    {
        [SerializeField] GameObject targetToDestroy = null;

        public void DestroyTarget()
        {
            Destroy(targetToDestroy);
        }
    }
}
