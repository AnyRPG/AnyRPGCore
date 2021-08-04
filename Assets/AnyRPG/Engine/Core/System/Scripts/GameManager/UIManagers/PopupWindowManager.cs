using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PopupWindowManager : ConfiguredMonoBehaviour {

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

        // game manager references
        PlayerManager playerManager = null;
        InventoryManager inventoryManager = null;
        KeyBindManager keyBindManager = null;
        InputManager inputManager = null;

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);
            
            // set references
            playerManager = systemGameManager.PlayerManager;
            inventoryManager = systemGameManager.InventoryManager;
            keyBindManager = systemGameManager.KeyBindManager;
            inputManager = systemGameManager.InputManager;

            // initialize windows
            abilityBookWindow.Init(systemGameManager);
            skillBookWindow.Init(systemGameManager);
            reputationBookWindow.Init(systemGameManager);
            currencyListWindow.Init(systemGameManager);
            achievementListWindow.Init(systemGameManager);
            characterPanelWindow.Init(systemGameManager);
            lootWindow.Init(systemGameManager);
            vendorWindow.Init(systemGameManager);
            chestWindow.Init(systemGameManager);
            bankWindow.Init(systemGameManager);
            questLogWindow.Init(systemGameManager);
            questGiverWindow.Init(systemGameManager);
            skillTrainerWindow.Init(systemGameManager);
            musicPlayerWindow.Init(systemGameManager);
            interactionWindow.Init(systemGameManager);
            craftingWindow.Init(systemGameManager);
            mainMapWindow.Init(systemGameManager);
            dialogWindow.Init(systemGameManager);
            factionChangeWindow.Init(systemGameManager);
            classChangeWindow.Init(systemGameManager);
            specializationChangeWindow.Init(systemGameManager);
        }

        void Start() {
            inventoryManager.Close();
        }

        // Update is called once per frame
        void Update() {
            //Debug.Log("PopupWindowManager.Update()");

            if (playerManager.PlayerUnitSpawned == false) {
                // if there is no player, these windows shouldn't be open
                return;
            }
            // don't open windows while binding keys
            if (keyBindManager.MyBindName == string.Empty) {
                if (inputManager.KeyBindWasPressed("INVENTORY")) {
                    inventoryManager.OpenClose();
                }
                if (inputManager.KeyBindWasPressed("ABILITYBOOK")) {
                    abilityBookWindow.ToggleOpenClose();
                }
                if (inputManager.KeyBindWasPressed("SKILLBOOK")) {
                    skillBookWindow.ToggleOpenClose();
                }
                if (inputManager.KeyBindWasPressed("ACHIEVEMENTBOOK")) {
                    achievementListWindow.ToggleOpenClose();
                }
                if (inputManager.KeyBindWasPressed("REPUTATIONBOOK")) {
                    reputationBookWindow.ToggleOpenClose();
                }
                if (inputManager.KeyBindWasPressed("CURRENCYPANEL")) {
                    currencyListWindow.ToggleOpenClose();
                }
                if (inputManager.KeyBindWasPressed("CHARACTERPANEL")) {
                    characterPanelWindow.ToggleOpenClose();
                }
                if (inputManager.KeyBindWasPressed("QUESTLOG")) {
                    questLogWindow.ToggleOpenClose();
                }
                if (inputManager.KeyBindWasPressed("MAINMAP")) {
                    //Debug.Log("mainmap was pressed");
                    mainMapWindow.ToggleOpenClose();
                }
            }

            if (inputManager.KeyBindWasPressed("CANCEL")) {
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
            inventoryManager.Close();
        }
    }

}