using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using RPG.Control;

namespace RPG.SceneManagement
{
    public class Portal : MonoBehaviour
    {
        enum DestinationIdentifier
        {
            A, B, C, D, E
        }

        [SerializeField] int sceneToLoad = -1;
        [SerializeField] float fadeOutTime = 0.5f;
        [SerializeField] float fadeInTime = 1f;
        [SerializeField] float fadeWaitTime = 0.5f;

        [SerializeField] Transform spawnPoint;
        [SerializeField] DestinationIdentifier destination;

        private void OnTriggerEnter(Collider other) 
        {
            if (other.tag == "Player")
            {
                StartCoroutine(Transition());
            }
        }

        IEnumerator Transition()
        {   
            if (sceneToLoad < 0)
            {
                Debug.LogError("Scene to load not set");
                yield break;
            }

            // Do not destroy portal in current scene when another scene is loaded so rest of code can be run
            DontDestroyOnLoad(gameObject);
            
            Fader fader = FindObjectOfType<Fader>();
            SavingWrapper wrapper = FindObjectOfType<SavingWrapper>();
            PlayerController playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            /* Disable PlayerController so player cannot move while entering portal, this prevents player from entering 
            another portal which could cause a coroutine race condition*/
            playerController.enabled = false; 
            yield return fader.FadeOut(fadeOutTime); // Yield return fade out to ensure Fader completes fade out before any further code is run
           
            wrapper.Save(); // Save the state of game in current scene (e.g enemies health, position etc)

            yield return SceneManager.LoadSceneAsync(sceneToLoad); // Load new scene 
            PlayerController newPlayerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
            newPlayerController.enabled = false;

            wrapper.Load(); // Load the state of the game in current scene (e.g enemies remain in same position they were when player left the scene etc)
            
            Portal otherPortal = GetOtherPortal(); // Retrieve the exit portal (the one that shares the same destination enum as the enter portal)
            UpdatePlayer(otherPortal); // Ensure player position and rotation is same as spawn point set on the exit portal
            
            wrapper.Save(); // Save the state of game automatically when exiting portal into new scene so 

            yield return new WaitForSeconds(fadeWaitTime); // Wait for fadeWaitTime before fade in
            fader.FadeIn(fadeInTime); // Do not yield return fade in so player controller is enabled while fading in (player can move around while fading in etc.)

            newPlayerController.enabled = true;
            Destroy(gameObject); // Destroy enter portal (portal from old scene is no longer needed and will be instantiated as PersistantObjectPrefab)
        }

        private void UpdatePlayer(Portal otherPortal)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.GetComponent<NavMeshAgent>().enabled = false; // Disable navmesh agent of player so it sets position of player navmesh agent properly (bug)
            player.GetComponent<NavMeshAgent>().Warp(otherPortal.spawnPoint.position); // Set player position to spawn point of portal in next scene
            player.transform.rotation = otherPortal.spawnPoint.rotation; // Set player rotation to spawn point rotation so it is facing correct way
            player.GetComponent<NavMeshAgent>().enabled = true;
        }

        private Portal GetOtherPortal()
        {
            /*For all portals in scene, if the destination enum is different from current portal,
             set is as other portal*/
            foreach(Portal portal in FindObjectsOfType<Portal>())
            {
                // If portal is not this, do nothing
                if (portal == this) continue;
                // And portal destination enum is not same destination (so exit portal does not share same destination enum), do nothing
                if (portal.destination != destination) continue;
                // Then return the other portal as it is not the one the player enters but the one the player exits
                return portal;
            }

            return null;
        }
    }
}
