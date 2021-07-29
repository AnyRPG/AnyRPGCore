using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PopupWindowManager : MonoBehaviour {

        //public GameObject inventoryUI;
        public PagedWindow abilityBookWindow;
        public PagedWindow skillBookWindow;
        public PagedWindow reputationBookWindow;
        public PagedWindow currencyListWindow;
        public PagedWindow achievementListWindow;
        public CloseableWindow characterPanelWindow;
        public PagedWindow lootWindow;
        public PagedWindow vendorWindow;
        public CloseableWindow chestWindow;
        public CloseableWindow bankWindow;
        public CloseableWindow questLogWindow;
        public CloseableWindow questGiverWindow;
        public CloseableWindow skillTrainerWindow;
        public CloseableWindow musicPlayerWindow;
        public CloseableWindow interactionWindow;
        public CloseableWindow craftingWindow;
        public CloseableWindow mainMapWindow;
        public CloseableWindow dialogWindow;
        public CloseableWindow factionChangeWindow;
        public CloseableWindow classChangeWindow;
        public CloseableWindow specializationChangeWindow;

        void Start() {
            SystemGameManager.Instance.InventoryManager.Close();
        }

        // Update is called once per frame
        void Update() {
            //Debug.Log("PopupWindowManager.Update()");

            if (SystemGameManager.Instance.PlayerManager.PlayerUnitSpawned == false) {
                // if there is no player, these windows shouldn't be open
                return;
            }
            // don't open windows while binding keys
            if (SystemGameManager.Instance.KeyBindManager.MyBindName == string.Empty) {
                if (SystemGameManager.Instance.InputManager.KeyBindWasPressed("INVENTORY")) {
                    SystemGameManager.Instance.InventoryManager.OpenClose();
                }
                if (SystemGameManager.Instance.InputManager.KeyBindWasPressed("ABILITYBOOK")) {
                    abilityBookWindow.ToggleOpenClose();
                }
                if (SystemGameManager.Instance.InputManager.KeyBindWasPressed("SKILLBOOK")) {
                    skillBookWindow.ToggleOpenClose();
                }
                if (SystemGameManager.Instance.InputManager.KeyBindWasPressed("ACHIEVEMENTBOOK")) {
                    achievementListWindow.ToggleOpenClose();
                }
                if (SystemGameManager.Instance.InputManager.KeyBindWasPressed("REPUTATIONBOOK")) {
                    reputationBookWindow.ToggleOpenClose();
                }
                if (SystemGameManager.Instance.InputManager.KeyBindWasPressed("CURRENCYPANEL")) {
                    currencyListWindow.ToggleOpenClose();
                }
                if (SystemGameManager.Instance.InputManager.KeyBindWasPressed("CHARACTERPANEL")) {
                    characterPanelWindow.ToggleOpenClose();
                }
                if (SystemGameManager.Instance.InputManager.KeyBindWasPressed("QUESTLOG")) {
                    questLogWindow.ToggleOpenClose();
                }
                if (SystemGameManager.Instance.InputManager.KeyBindWasPressed("MAINMAP")) {
                    //Debug.Log("mainmap was pressed");
                    mainMapWindow.ToggleOpenClose();
                }
            }

            if (SystemGameManager.Instance.InputManager.KeyBindWasPressed("CANCEL")) {
                CloseAllWindows();
            }
        }

        public void CloseAllWindows() {
            //Debug.Log("PopupWindowManager.CloseAllWindows()");
            abilityBookWindow.CloseWindow();
            skillBookWindow.CloseWindow();
            achievementListWindow.CloseWindow();
            reputationBookWindow.CloseWindow();
            currencyListWindow.CloseWindow();
            characterPanelWindow.CloseWindow();
            lootWindow.CloseWindow();
            vendorWindow.CloseWindow();
            chestWindow.CloseWindow();
            bankWindow.CloseWindow();
            questLogWindow.CloseWindow();
            questGiverWindow.CloseWindow();
            skillTrainerWindow.CloseWindow();
            musicPlayerWindow.CloseWindow();
            interactionWindow.CloseWindow();
            craftingWindow.CloseWindow();
            mainMapWindow.CloseWindow();
            factionChangeWindow.CloseWindow();
            classChangeWindow.CloseWindow();
            specializationChangeWindow.CloseWindow();
            dialogWindow.CloseWindow();
            SystemGameManager.Instance.InventoryManager.Close();
        }
    }

}