using RPG.Control;
using UnityEngine;

namespace RPG.Dialogue
{
    public class AIConversant : MonoBehaviour, IRaycastable
    {  
        [SerializeField] string conversantName;
        [SerializeField] Dialogue dialogue = null;

        public string GetName()
        {
            return conversantName;
        }

        public CursorType GetCursorType()
        {
            return CursorType.Dialogue;
        }

        public bool HandleRaycast(PlayerController controller)
        {
            if (dialogue == null) return false;
            
            if (Input.GetMouseButtonDown(0))
            {
                controller.GetComponent<PlayerConversant>().StartDialogue(this, dialogue);
            }
            return true;
        }
    }
}
