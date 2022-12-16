using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RPG.Saving
{
    [ExecuteAlways]
    public class SaveableEntity : MonoBehaviour // Should be attached to all GameObjects that have components that implement ISaveable
    {
        [SerializeField] string uniqueIdentifier = ""; // 128 bits. Only set a constant value if this is consistent between scenes (e.g player)
        
        /*This Dictionary stores all this GameObject's data that is captured. This data can be looked up
        using the uniqueIdentifier as the key and value as this SaveableEntity*/
        static Dictionary<string, SaveableEntity> globalLookup = new Dictionary<string, SaveableEntity>();

        public string GetUniqueIdentifier()
        {
            return uniqueIdentifier;
        }

        public object CaptureState()
        {
            Dictionary<string, object> state = new Dictionary<string, object>();
            foreach (ISaveable saveable in GetComponents<ISaveable>())
            {
                /*GetType() returns type of class it is during run-time (e.g Mover, Fighter etc)
                During compile time, saveable type will be ISaveable*/
                state[saveable.GetType().ToString()] = saveable.CaptureState(); 
            }
            return state;
        }

        public void RestoreState(object state)
        {
            Dictionary<string, object> stateDict = (Dictionary<string, object>)state;
            
            /*For all the ISaveable on this, call RestoreState() on the ISaveable by using 
            the class string as the key. The value assigned to this key is the object returned 
            in CaptureState() of th ISaveable*/
            foreach (ISaveable saveable in GetComponents<ISaveable>())
            {
                string typeString = saveable.GetType().ToString();
                if (stateDict.ContainsKey(typeString))
                {
                    saveable.RestoreState(stateDict[typeString]);
                }
            }
        }

/*Exclude code when not in UnityEditor (e.g when building the game) because UnityEditor namespace is not
available when building*/ 
#if UNITY_EDITOR
        private void Update() {
            if (Application.IsPlaying(gameObject)) return; // If in playmode, do nothing (only add UUID key/ set this as value if not in playmode)
            
            /*If gameObject has empty path, means its in prefab and not in scene, do nothing. This allows
            UUID to not be generated for prefabs so when this is placed in scene, it does not have same UUID
            of prefab which leads to multiple objects with same UUID*/
            if (string.IsNullOrEmpty(gameObject.scene.path)) return; 

            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty property = serializedObject.FindProperty("uniqueIdentifier");
            
            // If UUID is empty or not unique (something else has this UUID), generate a new UUID
            if (string.IsNullOrEmpty(property.stringValue) || !IsUnique(property.stringValue))
            {
                property.stringValue = System.Guid.NewGuid().ToString(); // Generate new UUID for this
                serializedObject.ApplyModifiedProperties(); // Apply the newly generated UUID to this serialized object
            }

            globalLookup[property.stringValue] = this; // Add this UUID (key) and set its value to this into globalLookup dictionary
        }
#endif

        private bool IsUnique(string key)
        {
            if (!globalLookup.ContainsKey(key)) return true;

            if (globalLookup[key] == this) return true;

            if (globalLookup[key] == null)
            {
                globalLookup.Remove(key);
                return true;
            }

            if (globalLookup[key].GetUniqueIdentifier() != key)
            {
                globalLookup.Remove(key);
                return true;
            }

            return false;
        }
    }
}