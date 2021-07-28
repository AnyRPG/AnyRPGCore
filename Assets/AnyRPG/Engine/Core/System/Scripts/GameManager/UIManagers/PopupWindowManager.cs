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
                return instance;
            }
        }

        private void Awake() {
            instance = this;
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

            if (PlayerManager.Instance.PlayerUnitSpawned == false) {
                // if there is no player, these windows shouldn't be open
                return;
            }
            // don't open windows while binding keys
            if (SystemGameManager.Instance.KeyBindManager.MyBindName == string.Empty) {
                if (InputManager.Instance.KeyBindWasPressed("INVENTORY")) {
                    InventoryManager.Instance.OpenClose();
                }
                if (InputManager.Instance.KeyBindWasPressed("ABILITYBOOK")) {
                    abilityBookWindow.ToggleOpenClose();
                }
                if (InputManager.Instance.KeyBindWasPressed("SKILLBOOK")) {
                    skillBookWindow.ToggleOpenClose();
                }
                if (InputManager.Instance.KeyBindWasPressed("ACHIEVEMENTBOOK")) {
                    achievementListWindow.ToggleOpenClose();
                }
                if (InputManager.Instance.KeyBindWasPressed("REPUTATIONBOOK")) {
                    reputationBookWindow.ToggleOpenClose();
                }
                if (InputManager.Instance.KeyBindWasPressed("CURRENCYPANEL")) {
                    currencyListWindow.ToggleOpenClose();
                }
                if (InputManager.Instance.KeyBindWasPressed("CHARACTERPANEL")) {
                    characterPanelWindow.ToggleOpenClose();
                }
                if (InputManager.Instance.KeyBindWasPressed("QUESTLOG")) {
                    questLogWindow.ToggleOpenClose();
                }
                if (InputManager.Instance.KeyBindWasPressed("MAINMAP")) {
                    //Debug.Log("mainmap was pressed");
                    mainMapWindow.ToggleOpenClose();
                }
            }

            if (InputManager.Instance.KeyBindWasPressed("CANCEL")) {
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