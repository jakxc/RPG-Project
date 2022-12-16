using UnityEngine;

namespace RPG.Saving
{
    [System.Serializable] // This class is used to ensure a Vector3 can be converted to binary (Unity does not serialize Vector3 type)
    public class SerializableVector3
    {
        float x, y, z;

        public SerializableVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        // Changes SerializableVector3 type to Vector3
        public Vector3 ToVector()
        {
            return new Vector3(x, y, z);
        }
    }
}