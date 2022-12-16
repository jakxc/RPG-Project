using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG.Saving
{
    public class SavingSystem : MonoBehaviour
    {
        public IEnumerator LoadLastScene(string saveFile)
        {
            Dictionary<string, object> state = LoadFile(saveFile); // Retrieves state of scene
            int buildIndex = SceneManager.GetActiveScene().buildIndex;
            if (state.ContainsKey("lastSceneBuildIndex"))
            {
                buildIndex = (int)state["lastSceneBuildIndex"]; // Set build index to value that is assigned to key "lastSceneBuildIndex"
            }
            yield return SceneManager.LoadSceneAsync(buildIndex); // Wait until scene is loaded before RestoreState to avoid race conditions
            RestoreState(state); // Restore data from save file into scene
        }

        public void Save(string saveFile)
        {
            Dictionary<string, object> state = LoadFile(saveFile); // Load original state 
            CaptureState(state); // Add the state to the deserialized saveFile, merges all the states
            SaveFile(saveFile, state);
        }

        public void Load(string saveFile)
        {
            RestoreState(LoadFile(saveFile));
        }

        public void Delete(string saveFile)
        {
            File.Delete(GetPathFromSaveFile(saveFile));
        }

        private Dictionary<string, object> LoadFile(string saveFile)
        {
            string path = GetPathFromSaveFile(saveFile);
            if (!File.Exists(path)) // If file with this path does not exist, load data that is from an empty dictionary
            {
                return new Dictionary<string, object>();
            }
            using (FileStream stream = File.Open(path, FileMode.Open)) // Open the file at path
            {
                BinaryFormatter formatter = new BinaryFormatter();
                
                /*Casts deserialized file to Dictionary<string, object> type with key as UUID 
                and value as type of data saved (e.g strings, floats etc)*/
                return (Dictionary<string, object>)formatter.Deserialize(stream); 
            }
        }

        private void SaveFile(string saveFile, object state)
        {
            string path = GetPathFromSaveFile(saveFile);
            print("Saving to " + path);
            
            /*using statement allows FileStream to be closed once it is exited (all code within is run)
            A FileStream is where binary bits can be read/write to*/
            using (FileStream stream = File.Open(path, FileMode.Create)) // Create new file to path, overwrites if file already exists
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, state); // Serialize all data in state to saveFile (e.g player position, health etc)
            }
        }

        private void CaptureState(Dictionary<string, object> state)
        {
            /*Get all SaveableEntity and set the UUID as key in Dictionary<string, object>
            and set value as the data captured in each SaveableEntity (components that have ISaveable interface)*/
            foreach (SaveableEntity saveable in FindObjectsOfType<SaveableEntity>())
            {
                state[saveable.GetUniqueIdentifier()] = saveable.CaptureState();
            }

            // Set key as string "lastSceneBuildIndex" and set value as the current active scene index
            state["lastSceneBuildIndex"] = SceneManager.GetActiveScene().buildIndex;
        }

        private void RestoreState(Dictionary<string, object> state)
        {
              /*Get all SaveableEntity and get UUID. If the save file has this UUID, restore the value of each
               ISaveable component using the values set in RestoreState() of each ISaveable (e.g Mover, Fighter etc.)*/
            foreach (SaveableEntity saveable in FindObjectsOfType<SaveableEntity>())
            {
                string id = saveable.GetUniqueIdentifier();
                if (state.ContainsKey(id)) // Checks if the UUID exists before restoring its value
                {
                    saveable.RestoreState(state[id]);
                }
            }
        }

        private string GetPathFromSaveFile(string saveFile)
        {
            /* Application.persistantDataPath is directory path where data is stored when saved
            Combine the persistant data path and saveFile string (set as "RPG" in SavingWrapper ) with extension .sav*/
            return Path.Combine(Application.persistentDataPath, saveFile + ".sav");
        }
    }
}