using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutSceneBarController : MonoBehaviour {

    [SerializeField]
    private GameObject topBar;

    [SerializeField]
    private GameObject bottomBar;

    [SerializeField]
    private GameObject captionBar;

    [SerializeField]
    private LayoutElement topBarLayoutElement;

    [SerializeField]
    private LayoutElement bottomBarLayoutElement;

    [SerializeField]
    private LayoutElement captionBarLayoutElement;

    [SerializeField]
    private Text captionText;

    private int cutSceneBarHeight = 100;

    private int cutSceneLoadTime = 3;


    private Coroutine coroutine;

    public void ClearCoRoutine() {
        if (coroutine != null) {
            StopCoroutine(coroutine);
        }
    }

    /*
    public void OnDisable() {
        //Debug.Log("ActionBarController.OnDisable()");
        //RebuildLayout();
        EndCutScene();
    }
    */

    public void StartCutScene(string caption) {
        //Debug.Log("CutSceneBarController.StartCutScene(" + caption + ")");
        gameObject.SetActive(true);
        topBarLayoutElement.preferredHeight = 0;
        bottomBarLayoutElement.preferredHeight = 0;
        captionBarLayoutElement.preferredHeight = cutSceneBarHeight;
        captionText.text = caption;
        captionText.color = new Color32(255, 255, 255, 0);
        topBar.gameObject.SetActive(true);
        bottomBar.gameObject.SetActive(true);
        captionBar.gameObject.SetActive(true);
        UIManager.MyInstance.ActivatePlayerUI();
        UIManager.MyInstance.MyPlayerInterfaceCanvas.SetActive(false);
        UIManager.MyInstance.MyPopupWindowContainer.SetActive(false);
        UIManager.MyInstance.MyPopupPanelContainer.SetActive(false);
        UIManager.MyInstance.MyCombatTextCanvas.SetActive(false);
        UIManager.MyInstance.MyCutSceneBarsCanvas.SetActive(true);
        coroutine = StartCoroutine(LoadCutSceneBars(cutSceneBarHeight, cutSceneLoadTime));
    }

    public void EndCutScene() {
        topBar.gameObject.SetActive(false);
        bottomBar.gameObject.SetActive(false);
        captionBar.gameObject.SetActive(false);
        UIManager.MyInstance.MyCutSceneBarsCanvas.SetActive(false);
        /*
        UIManager.MyInstance.MyPlayerInterfaceCanvas.SetActive(true);
        UIManager.MyInstance.MyPopupWindowContainer.SetActive(true);
        UIManager.MyInstance.MyPopupPanelContainer.SetActive(true);
        UIManager.MyInstance.MyCombatTextCanvas.SetActive(true);
        */
        ClearCoRoutine();
        gameObject.SetActive(false);
        LevelManager.MyInstance.GetActiveSceneNode().MyCutsceneViewed = true;
        LevelManager.MyInstance.ReturnFromCutScene();
    }

    public IEnumerator LoadCutSceneBars(int barHeight, float loadTime) {
        //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() Enter Ienumerator");
        float currentTime = 0f;
        float barHeightPerSecond = barHeight / loadTime;
        //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() currentCastTime: " + currentCastTime + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime);
        while (currentTime < loadTime) {
            currentTime += Time.deltaTime;
            float newHeight = currentTime * barHeightPerSecond;
            topBarLayoutElement.preferredHeight = newHeight;
            bottomBarLayoutElement.preferredHeight = newHeight;

            yield return null;
        }
        coroutine = StartCoroutine(LoadCutSceneText(loadTime));
    }

    public IEnumerator LoadCutSceneText(float loadTime) {
        float currentTime = 0f;
        float alphaPerSecond = 255 / loadTime;
        //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() currentCastTime: " + currentCastTime + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime);
        while (currentTime < loadTime) {
            currentTime += Time.deltaTime;
            captionText.color = new Color32(255, 255, 255, (byte)Mathf.Clamp((int)(currentTime * alphaPerSecond), 0, 255));

            yield return null;
        }
    }

    public void OnDisable() {
        ClearCoRoutine();
    }
}
