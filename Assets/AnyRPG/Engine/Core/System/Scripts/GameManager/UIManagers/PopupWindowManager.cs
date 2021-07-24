using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PopupWindowManager : MonoBehaviour {

        #region Singleton
        private static PopupWindowManager instance;

        public static PopupWindowManager Instance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<PopupWindowManager>();
                }

                return instance;
            }
        }
        #endregion

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
            InventoryManager.Instance.Close();
        }

        // Update is called once per frame
        void Update() {
            //Debug.Log("PopupWindowManager.Update()");

            if (PlayerManager.MyInstance.PlayerUnitSpawned == false) {
                // if there is no player, these windows shouldn't be open
                return;
            }
            // don't open windows while binding keys
            if (KeyBindManager.MyInstance.MyBindName == string.Empty) {
                if (InputManager.MyInstance.KeyBindWasPressed("INVENTORY")) {
                    InventoryManager.Instance.OpenClose();
                }
                if (InputManager.MyInstance.KeyBindWasPressed("ABILITYBOOK")) {
                    abilityBookWindow.ToggleOpenClose();
                }
                if (InputManager.MyInstance.KeyBindWasPressed("SKILLBOOK")) {
                    skillBookWindow.ToggleOpenClose();
                }
                if (InputManager.MyInstance.KeyBindWasPressed("ACHIEVEMENTBOOK")) {
                    achievementListWindow.ToggleOpenClose();
                }
                if (InputManager.MyInstance.KeyBindWasPressed("REPUTATIONBOOK")) {
                    reputationBookWindow.ToggleOpenClose();
                }
                if (InputManager.MyInstance.KeyBindWasPressed("CURRENCYPANEL")) {
                    currencyListWindow.ToggleOpenClose();
                }
                if (InputManager.MyInstance.KeyBindWasPressed("CHARACTERPANEL")) {
                    characterPanelWindow.ToggleOpenClose();
                }
                if (InputManager.MyInstance.KeyBindWasPressed("QUESTLOG")) {
                    questLogWindow.ToggleOpenClose();
                }
                if (InputManager.MyInstance.KeyBindWasPressed("MAINMAP")) {
                    //Debug.Log("mainmap was pressed");
                    mainMapWindow.ToggleOpenClose();
                }
            }

            if (InputManager.MyInstance.KeyBindWasPressed("CANCEL")) {
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
            InventoryManager.Instance.Close();
        }
    }

}