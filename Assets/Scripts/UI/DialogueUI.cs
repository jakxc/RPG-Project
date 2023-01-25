using System;
using UnityEngine;
using TMPro;
using RPG.Dialogue;
using UnityEngine.UI;

namespace RPG.UI
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI conversantName;
        [SerializeField] TextMeshProUGUI dialogueText;
        [SerializeField] Button nextButton;
        [SerializeField] GameObject npcContent;
        [SerializeField] Transform optionRoot;
        [SerializeField] GameObject optionButtonPrefab;
        [SerializeField] Button quitButton;
        PlayerConversant playerConversant;


        // Start is called before the first frame update
        void Start()
        {
            playerConversant = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerConversant>();
            playerConversant.onDialogueUpdated += UpdateUI;
            nextButton.onClick.AddListener(Next);
            quitButton.onClick.AddListener(Quit);

            UpdateUI();          
        }

        void Next()
        {
            playerConversant.NextDialogue();
        }

        void Quit()
        {
            playerConversant.QuitDialogue();
        }

        void UpdateUI()
        {
            gameObject.SetActive(playerConversant.IsActive());
            conversantName.text = playerConversant.GetCurrentConversantName();
            if (!playerConversant.IsActive()) return;

            npcContent.SetActive(!playerConversant.IsSelectingOption());
            optionRoot.gameObject.SetActive(playerConversant.IsSelectingOption());
            if (playerConversant.IsSelectingOption())
            {
                BuildOptionsList();
            }
            else
            {
                dialogueText.text = playerConversant.GetText();
                nextButton.gameObject.SetActive(playerConversant.HasNext());
            }
        }

        private void BuildOptionsList()
        {
            foreach (Transform item in optionRoot)
            {
                Destroy(item.gameObject);
            }

            foreach (DialogueNode option in playerConversant.GetOptions())
            {
                GameObject optionButtonInstance = Instantiate(optionButtonPrefab, optionRoot);
                var buttonText = optionButtonInstance.GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = option.GetText();
                Button button = optionButtonInstance.GetComponentInChildren<Button>();
                button.onClick.AddListener(() => playerConversant.SelectOption(option)); // Function is only called when button is clicked
            }
        }
    }
}
