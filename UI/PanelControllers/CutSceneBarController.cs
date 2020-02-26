using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
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

        private int barFadeInTime = 3;

        private int textFadeInTime = 3;

        private int dialogIndex = 0;

        private float maxDialogTime = 300f;

        private Coroutine dialogCoroutine;
        private Coroutine fadeCoroutine;
        private Coroutine barsCoroutine;

        private Dialog currentDialog = null;

        public void ClearCoRoutine() {
            if (dialogCoroutine != null) {
                StopCoroutine(dialogCoroutine);
            }
            if (fadeCoroutine != null) {
                StopCoroutine(fadeCoroutine);
            }
            if (barsCoroutine != null) {
                StopCoroutine(barsCoroutine);
            }
        }

        /*
        public void OnDisable() {
            //Debug.Log("ActionBarController.OnDisable()");
            //RebuildLayout();
            EndCutScene();
        }
        */

        public void StartCutScene(Dialog newDialog) {
            //Debug.Log("CutSceneBarController.StartCutScene(" + caption + ")");
            currentDialog = newDialog;
            captionText.color = new Color32(255, 255, 255, 0);
            gameObject.SetActive(true);
            topBarLayoutElement.preferredHeight = 0;
            bottomBarLayoutElement.preferredHeight = 0;
            captionBarLayoutElement.preferredHeight = cutSceneBarHeight;


            topBar.gameObject.SetActive(true);
            bottomBar.gameObject.SetActive(true);
            captionBar.gameObject.SetActive(true);
            UIManager.MyInstance.ActivatePlayerUI();
            UIManager.MyInstance.MyPlayerInterfaceCanvas.SetActive(false);
            UIManager.MyInstance.MyPopupWindowContainer.SetActive(false);
            UIManager.MyInstance.MyPopupPanelContainer.SetActive(false);
            UIManager.MyInstance.MyCombatTextCanvas.SetActive(false);
            UIManager.MyInstance.MyCutSceneBarsCanvas.SetActive(true);
            barsCoroutine = StartCoroutine(LoadCutSceneBars(cutSceneBarHeight));
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

        public IEnumerator LoadCutSceneBars(int barHeight) {
            //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() Enter Ienumerator");
            float currentTime = 0f;
            float barHeightPerSecond = barHeight / barFadeInTime;
            //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() currentCastTime: " + currentCastTime + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime);
            while (currentTime < barFadeInTime) {
                currentTime += Time.deltaTime;
                float newHeight = currentTime * barHeightPerSecond;
                topBarLayoutElement.preferredHeight = newHeight;
                bottomBarLayoutElement.preferredHeight = newHeight;

                yield return null;
            }
            if (currentDialog.MyAutomatic == true) {
                dialogCoroutine = StartCoroutine(playDialog());
            } else {
                ProcessDialogNode(currentDialog.MyDialogNodes[0]);
                dialogIndex++;
            }
        }

        public void AdvanceDialog() {
            //Debug.Log("CharacterAbilitymanager.AdvanceDialog()");
            if (currentDialog.MyDialogNodes.Count > dialogIndex) {
                ProcessDialogNode(currentDialog.MyDialogNodes[dialogIndex]);
                dialogIndex++;
            }
        }

        private void ProcessDialogNode(DialogNode currentdialogNode) {
            //Debug.Log("CharacterAbilitymanager.ProcessDialogNode()");
            captionText.text = currentdialogNode.MyDescription;
            captionText.color = new Color32(255, 255, 255, 0);
            dialogCoroutine = StartCoroutine(FadeInText());

            currentdialogNode.MyShown = true;
        }


        public IEnumerator playDialog() {
            //Debug.Log("CharacterAbilitymanager.playDialog()");
            float elapsedTime = 0f;
            DialogNode currentdialogNode = null;

            while (currentDialog.TurnedIn == false) {
                foreach (DialogNode dialogNode in currentDialog.MyDialogNodes) {
                    if (dialogNode.MyStartTime <= elapsedTime && dialogNode.MyShown == false) {
                        currentdialogNode = dialogNode;

                        ProcessDialogNode(currentdialogNode);
                        dialogIndex++;
                    }
                }
                /*
                if (dialogIndex >= currentDialog.MyDialogNodes.Count) {
                    currentDialog.TurnedIn = true;
                }
                */
                elapsedTime += Time.deltaTime;

                // circuit breaker
                if (elapsedTime >= maxDialogTime) {
                    break;
                }
                yield return null;
                dialogCoroutine = null;
            }

            //yield return new WaitForSeconds(currentdialogNode.MyShowTime);
        }

        public IEnumerator FadeInText() {
            float currentTime = 0f;
            float alphaPerSecond = 255 / textFadeInTime;
            //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() currentCastTime: " + currentCastTime + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime);
            while (currentTime < textFadeInTime) {
                currentTime += Time.deltaTime;
                captionText.color = new Color32(255, 255, 255, (byte)Mathf.Clamp((int)(currentTime * alphaPerSecond), 0, 255));

                yield return null;
            }
        }

        public void OnDisable() {
            ClearCoRoutine();
            dialogIndex = 0;
        }
    }

}