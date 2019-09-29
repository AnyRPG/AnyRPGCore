using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : WindowContentController {

    public void PlayMenu() {
        //Debug.Log("MainMenuController.PlayMenu()");
        SystemWindowManager.MyInstance.exitMenuWindow.CloseWindow();
        SystemWindowManager.MyInstance.deleteGameMenuWindow.CloseWindow();
        SystemWindowManager.MyInstance.settingsMenuWindow.CloseWindow();
        SystemWindowManager.MyInstance.playMenuWindow.OpenWindow();
    }

    public void ExitMenu() {
        //Debug.Log("MainMenuController.ExitMenu()");
        SystemWindowManager.MyInstance.playMenuWindow.CloseWindow();
        SystemWindowManager.MyInstance.deleteGameMenuWindow.CloseWindow();
        SystemWindowManager.MyInstance.exitMenuWindow.OpenWindow();
    }

    public void MainMenu() {
        //Debug.Log("MainMenuController.MainMenu()");
        SystemWindowManager.MyInstance.exitToMainMenuWindow.OpenWindow();
    }

    public void SettingsMenu() {
        //Debug.Log("MainMenuController.SettingsMenu()");
        SystemWindowManager.MyInstance.playMenuWindow.CloseWindow();
        SystemWindowManager.MyInstance.deleteGameMenuWindow.CloseWindow();
        //SystemWindowManager.MyInstance.mainMenuWindow.CloseWindow();
        SystemWindowManager.MyInstance.settingsMenuWindow.OpenWindow();
    }

    public void SaveGame() {
        //Debug.Log("MainMenuController.SaveGame()");
        SystemWindowManager.MyInstance.CloseAllWindows();
        SaveManager.MyInstance.SaveGame();
        MessageFeedManager.MyInstance.WriteMessage("Game Saved");
    }

    public void ContinueGame() {
        //Debug.Log("MainMenuController.ContinueGame()");
        SystemWindowManager.MyInstance.CloseAllWindows();
    }

}
