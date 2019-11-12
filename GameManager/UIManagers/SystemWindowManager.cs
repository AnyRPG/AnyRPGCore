using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
public class SystemWindowManager : MonoBehaviour {

    #region Singleton
    private static SystemWindowManager instance;

    public static SystemWindowManager MyInstance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<SystemWindowManager>();
            }

            return instance;
        }
    }
    #endregion

    protected bool startHasRun = false;
    protected bool eventSubscriptionsInitialized = false;

    public CloseableWindow mainMenuWindow;
    public CloseableWindow inGameMainMenuWindow;
    //public CloseableWindow keyBindMenuWindow;
    public CloseableWindow keyBindConfirmWindow;
    //public CloseableWindow soundMenuWindow;
    //public CloseableWindow graphicsMenuWindow;
    public CloseableWindow playerOptionsMenuWindow;
    public CloseableWindow characterCreatorWindow;
    public CloseableWindow playMenuWindow;
    public CloseableWindow settingsMenuWindow;
    public CloseableWindow exitMenuWindow;
    public CloseableWindow deleteGameMenuWindow;
    public CloseableWindow loadGameWindow;
    public CloseableWindow confirmDestroyMenuWindow;
    public CloseableWindow confirmCancelCutsceneMenuWindow;
    public CloseableWindow nameChangeWindow;
    public CloseableWindow exitToMainMenuWindow;
    public CloseableWindow newGameMenuWindow;


    private void Start() {
        //Debug.Log("PlayerManager.Start()");
        startHasRun = true;
        CreateEventSubscriptions();
    }

    private void CreateEventSubscriptions() {
        ////Debug.Log("PlayerManager.CreateEventSubscriptions()");
        if (eventSubscriptionsInitialized || !startHasRun) {
            return;
        }
        SystemEventManager.MyInstance.OnPlayerConnectionSpawn += SetupDeathPopup;
        SystemEventManager.MyInstance.OnPlayerConnectionDespawn += RemoveDeathPopup;
        eventSubscriptionsInitialized = true;
    }

    private void CleanupEventSubscriptions() {
        ////Debug.Log("PlayerManager.CleanupEventSubscriptions()");
        if (!eventSubscriptionsInitialized) {
            return;
        }
        SystemEventManager.MyInstance.OnPlayerConnectionSpawn += SetupDeathPopup;
        SystemEventManager.MyInstance.OnPlayerConnectionDespawn += RemoveDeathPopup;
        eventSubscriptionsInitialized = false;
    }

    public void OnDisable() {
        ////Debug.Log("PlayerManager.OnDisable()");
        CleanupEventSubscriptions();
    }


    // Update is called once per frame
    void Update() {
        if (mainMenuWindow.enabled == false && settingsMenuWindow.enabled == false) {
            return;
        }

        if (InputManager.MyInstance.KeyBindWasPressed("CANCEL")) {
            settingsMenuWindow.CloseWindow();
            exitMenuWindow.CloseWindow();
            playMenuWindow.CloseWindow();
            deleteGameMenuWindow.CloseWindow();
            confirmDestroyMenuWindow.CloseWindow();
        }

        if (InputManager.MyInstance.KeyBindWasPressed("MAINMENU")) {
            inGameMainMenuWindow.ToggleOpenClose();
        }

        if (InputManager.MyInstance.KeyBindWasPressed("CANCEL")) {
            inGameMainMenuWindow.CloseWindow();
            playerOptionsMenuWindow.CloseWindow();
        }
    }

    public void CloseAllWindows() {
        //Debug.Log("SystemWindowManager.CloseAllWindows()");
        mainMenuWindow.CloseWindow();
        inGameMainMenuWindow.CloseWindow();
        settingsMenuWindow.CloseWindow();
        exitMenuWindow.CloseWindow();
        playMenuWindow.CloseWindow();
        deleteGameMenuWindow.CloseWindow();
        confirmDestroyMenuWindow.CloseWindow();
    }

    public void PlayerDeathHandler(CharacterStats characterStats) {
        //Debug.Log("PopupWindowManager.PlayerDeathHandler()");
        playerOptionsMenuWindow.OpenWindow();
    }

    public void SetupDeathPopup() {
        //Debug.Log("PopupWindowmanager.SetupDeathPopup()");
        PlayerManager.MyInstance.MyCharacter.MyCharacterStats.OnDie += PlayerDeathHandler;
    }

    public void RemoveDeathPopup() {
        //Debug.Log("PopupWindowmanager.RemoveDeathPopup()");
        PlayerManager.MyInstance.MyCharacter.MyCharacterStats.OnDie -= PlayerDeathHandler;
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