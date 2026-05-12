using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Dialogue;
using TMPro;
using UnityEngine.UI;

namespace RPG.UI
{
    public class DialogueUI : MonoBehaviour
    {
        PlayerConversant playerConversant;
        [SerializeField] TextMeshProUGUI AIText;
        [SerializeField] Transform choiceRoot;
        [SerializeField] GameObject choicePrefab;

        [SerializeField] Button quitButton;
        [SerializeField] TextMeshProUGUI conversantName;

        // Start is called before the first frame update
        void Start()
        {
            playerConversant = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerConversant>();
            playerConversant.onConversationUpdated += UpdateUI;

            quitButton.onClick.AddListener(() => playerConversant.Quit());

            UpdateUI();
        }


        void UpdateUI()
        {
            gameObject.SetActive(playerConversant.IsActive());
            if(!playerConversant.IsActive())
            {
                return;
            }
            conversantName.text = playerConversant.GetCurrentConversantName();
            AIText.text = playerConversant.GetText();
            BuildChoiceList();

        }

        //负责生成玩家选择的UI界面
        private void BuildChoiceList()
        {
            choiceRoot.DetachChildren();
            // foreach (Transform item in choiceRoot)
            // {
            //     Destroy(item.gameObject);
            // }
            foreach (DialogueNode choice in playerConversant.GetChoices())
            {
                GameObject choiceInstance = Instantiate(choicePrefab, choiceRoot);
                var textComp = choiceInstance.GetComponentInChildren<TextMeshProUGUI>();
                textComp.text = choice.GetText();

                Button button = choiceInstance.GetComponentInChildren<Button>();

                
                button.onClick.AddListener(() => 
                {
                    playerConversant.SelectChoice(choice);
                    //UpdateUI();
                });
            }
        }
    }
}