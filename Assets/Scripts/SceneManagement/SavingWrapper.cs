using System.Collections;
using GameDevTV.Saving;
using UnityEngine;

namespace RPG.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        const string defaultSaveFile = "RPG";
        [SerializeField] float fadeInTime = 0.2f;

        private void Awake() 
        {
            StartCoroutine(LoadLastScene()); 
        }
        
        IEnumerator LoadLastScene() 
        {
            Fader fader = FindObjectOfType<Fader>();
            fader.FadeOutImmediate(); // Fade out so scene cannot be viewed
            
            yield return GetComponent<SavingSystem>().LoadLastScene(defaultSaveFile); // Load last scene
            yield return fader.FadeIn(fadeInTime); // Fade in after everything from last scene has been loaded
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
               Load();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                Save();
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                Delete();
            }
        }

        public void Save()
        {
            GetComponent<SavingSystem>().Save(defaultSaveFile);
        }

        public void Load()
        {
            GetComponent<SavingSystem>().LoadLastScene(defaultSaveFile);
        }

        public void Delete()
        {
            GetComponent<SavingSystem>().Delete(defaultSaveFile);
        }
        
    }
}
