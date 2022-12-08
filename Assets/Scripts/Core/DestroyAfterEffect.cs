using UnityEngine;

namespace RPG.Core
{
    public class DestroyAfterEffect : MonoBehaviour
    {
        [SerializeField] GameObject targetToDestroy = null;
       
        // Update is called once per frame
        void Update()
        {
            // If the ParticleSystem has finished playing, destroy the targetToDestroy or the gameObject itself
            if (!GetComponent<ParticleSystem>().IsAlive())
            {
                if (targetToDestroy != null)
                {
                    Destroy(targetToDestroy);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
