using UnityEngine;

public class ToggleActiveUI : MonoBehaviour
{
    [SerializeField] KeyCode toggleKey;
    [SerializeField] GameObject uiContainer;
    
    // Start is called before the first frame update
    void Start()
    {
        uiContainer.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        uiContainer.SetActive(!uiContainer.activeSelf);
    }
}
