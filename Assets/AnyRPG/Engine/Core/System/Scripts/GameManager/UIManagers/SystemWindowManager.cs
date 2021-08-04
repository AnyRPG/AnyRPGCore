using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemWindowManager : ConfiguredMonoBehaviour {

        protected bool eventSubscriptionsInitialized = false;

        // windows
        public CloseableWindow mainMenuWindow;
        public CloseableWindow inGameMainMenuWindow;
        public CloseableWindow keyBindConfirmWindow;
        public CloseableWindow playerOptionsMenuWindow;
        public CloseableWindow characterCreatorWindow;
        public CloseableWindow unitSpawnWindow;
        public CloseableWindow petSpawnWindow;
        public CloseableWindow playMenuWindow;
        public CloseableWindow settingsMenuWindow;
        public CloseableWindow creditsWindow;
        public CloseableWindow exitMenuWindow;
        public CloseableWindow deleteGameMenuWindow;
        public CloseableWindow copyGameMenuWindow;
        public CloseableWindow loadGameWindow;
        public CloseableWindow newGameWindow;
        public CloseableWindow confirmDestroyMenuWindow;
        public CloseableWindow confirmCancelCutsceneMenuWindow;
        public CloseableWindow confirmSellItemMenuWindow;
        public CloseableWindow nameChangeWindow;
        public CloseableWindow exitToMainMenuWindow;
        public CloseableWindow confirmNewGameMenuWindow;

        // game manager references
        InputManager inputManager = null;
        PlayerManager playerManager = null;

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);

            inputManager = systemGameManager.InputManager;
            playerManager = systemGameManager.PlayerManager;

            CreateEventSubscriptions();

            mainMenuWindow.Init(systemGameManager);
            inGameMainMenuWindow.Init(systemGameManager);
            keyBindConfirmWindow.Init(systemGameManager);
            playerOptionsMenuWindow.Init(systemGameManager);
            characterCreatorWindow.Init(systemGameManager);
            unitSpawnWindow.Init(systemGameManager);
            petSpawnWindow.Init(systemGameManager);
            playMenuWindow.Init(systemGameManager);
            settingsMenuWindow.Init(systemGameManager);
            creditsWindow.Init(systemGameManager);
            exitMenuWindow.Init(systemGameManager);
            deleteGameMenuWindow.Init(systemGameManager);
            copyGameMenuWindow.Init(systemGameManager);
            loadGameWindow.Init(systemGameManager);
            newGameWindow.Init(systemGameManager);
            confirmDestroyMenuWindow.Init(systemGameManager);
            confirmCancelCutsceneMenuWindow.Init(systemGameManager);
            confirmSellItemMenuWindow.Init(systemGameManager);
            nameChangeWindow.Init(systemGameManager);
            exitToMainMenuWindow.Init(systemGameManager);
            confirmNewGameMenuWindow.Init(systemGameManager);
        }

        private void CreateEventSubscriptions() {
            ////Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnPlayerConnectionSpawn", handlePlayerConnectionSpawn);
            SystemEventManager.StartListening("OnPlayerConnectionDespawn", handlePlayerConnectionDespawn);
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            ////Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnPlayerConnectionSpawn", handlePlayerConnectionSpawn);
            SystemEventManager.StopListening("OnPlayerConnectionDespawn", handlePlayerConnectionDespawn);
            eventSubscriptionsInitialized = false;
        }

        public void handlePlayerConnectionSpawn(string eventName, EventParamProperties eventParamProperties) {
            SetupDeathPopup();
        }

        public void handlePlayerConnectionDespawn(string eventName, EventParamProperties eventParamProperties) {
            RemoveDeathPopup();
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
        }


        // Update is called once per frame
        void Update() {
            if (mainMenuWindow.enabled == false && settingsMenuWindow.enabled == false) {
                return;
            }

            if (inputManager.KeyBindWasPressed("CANCEL")) {
                settingsMenuWindow.CloseWindow();
                creditsWindow.CloseWindow();
                exitMenuWindow.CloseWindow();
                playMenuWindow.CloseWindow();
                deleteGameMenuWindow.CloseWindow();
                copyGameMenuWindow.CloseWindow();
                confirmDestroyMenuWindow.CloseWindow();
                confirmSellItemMenuWindow.CloseWindow();
                inGameMainMenuWindow.CloseWindow();
                petSpawnWindow.CloseWindow();

                // do not allow accidentally closing this while dead
                if (playerManager.PlayerUnitSpawned == true && playerManager.MyCharacter.CharacterStats.IsAlive != false) {
                    playerOptionsMenuWindow.CloseWindow();
                }
            }

            if (inputManager.KeyBindWasPressed("MAINMENU")) {
                inGameMainMenuWindow.ToggleOpenClose();
            }

        }

        public void CloseAllWindows() {
            //Debug.Log("SystemWindowManager.CloseAllWindows()");
            mainMenuWindow.CloseWindow();
            inGameMainMenuWindow.CloseWindow();
            settingsMenuWindow.CloseWindow();
            creditsWindow.CloseWindow();
            exitMenuWindow.CloseWindow();
            playMenuWindow.CloseWindow();
            deleteGameMenuWindow.CloseWindow();
            copyGameMenuWindow.CloseWindow();
            confirmDestroyMenuWindow.CloseWindow();
            confirmSellItemMenuWindow.CloseWindow();
        }

        public void PlayerDeathHandler(CharacterStats characterStats) {
            //Debug.Log("PopupWindowManager.PlayerDeathHandler()");
            StartCoroutine(PerformDeathWindowDelay());
        }

        public IEnumerator PerformDeathWindowDelay() {
            float timeCount = 0f;
            while (timeCount < 2f) {
                yield return null;
                timeCount += Time.deltaTime;
            }
            playerOptionsMenuWindow.OpenWindow();
        }

        public void SetupDeathPopup() {
            //Debug.Log("PopupWindowmanager.SetupDeathPopup()");
            playerManager.MyCharacter.CharacterStats.OnDie += PlayerDeathHandler;
        }

        public void RemoveDeathPopup() {
            //Debug.Log("PopupWindowmanager.RemoveDeathPopup()");
            playerManager.MyCharacter.CharacterStats.OnDie -= PlayerDeathHandler;
        }

        public void OpenInGameMainMenu() {
            inGameMainMenuWindow.OpenWindow();
        }

        public void ToggleInGameMainMenu() {
            inGameMainMenuWindow.ToggleOpenClose();
        }

        public void OpenMainMenu() {
            //Debug.Log("SystemWindowManager.OpenMainMenu()");
            mainMenuWindow.OpenWindow();
        }

        public void CloseMainMenu() {
            //Debug.Log("SystemWindowManager.CloseMainMenu()");
            mainMenuWindow.CloseWindow();
        }

    }

}