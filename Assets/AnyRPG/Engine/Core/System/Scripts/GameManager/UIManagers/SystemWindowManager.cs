using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemWindowManager : MonoBehaviour {

        protected bool eventSubscriptionsInitialized = false;

        public CloseableWindow mainMenuWindow;
        public CloseableWindow inGameMainMenuWindow;
        //public CloseableWindow keyBindMenuWindow;
        public CloseableWindow keyBindConfirmWindow;
        //public CloseableWindow soundMenuWindow;
        //public CloseableWindow graphicsMenuWindow;
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


        private void Start() {
            //Debug.Log("PlayerManager.Start()");
            CreateEventSubscriptions();
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

            if (InputManager.Instance.KeyBindWasPressed("CANCEL")) {
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
                if (PlayerManager.Instance.PlayerUnitSpawned == true && PlayerManager.Instance.MyCharacter.CharacterStats.IsAlive != false) {
                    playerOptionsMenuWindow.CloseWindow();
                }
            }

            if (InputManager.Instance.KeyBindWasPressed("MAINMENU")) {
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
            PlayerManager.Instance.MyCharacter.CharacterStats.OnDie += PlayerDeathHandler;
        }

        public void RemoveDeathPopup() {
            //Debug.Log("PopupWindowmanager.RemoveDeathPopup()");
            PlayerManager.Instance.MyCharacter.CharacterStats.OnDie -= PlayerDeathHandler;
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