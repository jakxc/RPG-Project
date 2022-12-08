using UnityEngine;

namespace RPG.Core
{
    public class PersistantObjectSpawner : MonoBehaviour
    {
        [SerializeField] GameObject persistantObjectPrefab;

        static bool hasSpawned = false; // static variable so this value is remembered between scenes 
        private void Awake() 
        {
            /* If the persistant objects (Fader, Saving etc) have already been instantianted 
            and/or already in the scene, do nothing*/
            if (hasSpawned) return;

            SpawnPersistantObjects();
            // hasSpawned set to true after spawn so the Persistant Objects do not get instantiated again
            hasSpawned = true;
        }

        private void SpawnPersistantObjects()
        {
            /* This ensures persistant objects are spawned once in the initial scene loaded
             and does not get destroyed when a new scene is loaded */
            GameObject persistantObject = Instantiate(persistantObjectPrefab);
            DontDestroyOnLoad(persistantObject);
        }
    }
}
