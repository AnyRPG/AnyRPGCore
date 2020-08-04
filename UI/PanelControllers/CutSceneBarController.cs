using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CutSceneBarController : MonoBehaviour {

        [SerializeField]
        private GameObject topBar = null;

        [SerializeField]
        private GameObject bottomBar = null;

        [SerializeField]
        private GameObject captionBar = null;

        [SerializeField]
        private LayoutElement topBarLayoutElement = null;

        [SerializeField]
        private LayoutElement bottomBarLayoutElement = null;

        [SerializeField]
        private LayoutElement captionBarLayoutElement = null;

        [SerializeField]
        private TextMeshProUGUI captionText = null;

        private int cutSceneBarHeight = 100;

        private int barFadeInTime = 3;

        private int textFadeInTime = 3;

        private int dialogIndex = 0;

        private float maxDialogTime = 300f;

        private Coroutine dialogCoroutine = null;
        private Coroutine fadeCoroutine = null;
        private Coroutine barsCoroutine = null;

        private Dialog currentDialog = null;

        private Cutscene currentCutscene = null;

        public Cutscene MyCurrentCutscene { get => currentCutscene; set => currentCutscene = value; }

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

            // this method exists to ensure that a cutscene can be considered active even if it loads late in the scene load order
        public void AssignCutScene(Cutscene cutscene) {
            currentCutscene = cutscene;
        }

        public void StartCutScene(Cutscene cutscene) {
            //Debug.Log("CutSceneBarController.StartCutScene()");

            if (cutscene.TimelineName != null && cutscene.TimelineName != string.Empty && SystemPlayableDirectorManager.MyInstance.MyPlayableDirectorDictionary.ContainsKey(cutscene.TimelineName)) {
                SystemPlayableDirectorManager.MyInstance.MyPlayableDirectorDictionary[cutscene.TimelineName].Play();
            }

            CameraManager.MyInstance.DeactivateMainCamera();
            CameraManager.MyInstance.EnableCutsceneCamera();

            currentCutscene = cutscene;
            currentDialog = cutscene.MyDialog;
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
            //Debug.Log("CutSceneBarController.EndCutScene()");

            if (currentCutscene != null && currentCutscene.TimelineName != null && currentCutscene.TimelineName != string.Empty && SystemPlayableDirectorManager.MyInstance.MyPlayableDirectorDictionary.ContainsKey(currentCutscene.TimelineName)) {
                SystemPlayableDirectorManager.MyInstance.MyPlayableDirectorDictionary[currentCutscene.TimelineName].Stop();
            }

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
            if (currentCutscene != null) {
                currentCutscene.Viewed = true;
            }
            // if this is not a cutscene that should return, then do not, else do
            //if (currentCutscene.MyUnloadSceneOnEnd) {
            LevelManager.MyInstance.EndCutscene(currentCutscene);
            //}
            currentCutscene = null;
        }

        public IEnumerator LoadCutSceneBars(int barHeight) {
            //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() Enter Ienumerator");
            float currentTime = 0f;
            float barHeightPerSecond = barHeight / barFadeInTime;
            //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() currentCastTime: " + currentCastTime + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime);
            while (currentTime < barFadeInTime) {
                yield return null;
                currentTime += Time.deltaTime;
                float newHeight = currentTime * barHeightPerSecond;
                topBarLayoutElement.preferredHeight = newHeight;
                bottomBarLayoutElement.preferredHeight = newHeight;

            }
            if (currentDialog != null) {
                dialogIndex = 0;
                if (currentDialog.MyAutomatic == true) {
                    dialogCoroutine = StartCoroutine(playDialog());
                } else {
                    ProcessDialogNode(dialogIndex);
                    dialogIndex++;
                }
            }
        }

        public void AdvanceDialog() {
            //Debug.Log("CharacterAbilitymanager.AdvanceDialog()");
            if (currentDialog.MyDialogNodes.Count > dialogIndex) {
                ProcessDialogNode(dialogIndex);
                dialogIndex++;
            }
        }

        private void ProcessDialogNode(int dialogIndex) {
            //Debug.Log("CharacterAbilitymanager.ProcessDialogNode()");
            DialogNode currentDialogNode = currentDialog.MyDialogNodes[dialogIndex];
            captionText.text = currentDialogNode.MyDescription;
            captionText.color = new Color32(255, 255, 255, 0);
            dialogCoroutine = StartCoroutine(FadeInText());

            if (AudioManager.MyInstance != null && currentDialog.MyAudioProfile != null && currentDialog.MyAudioProfile.AudioClips != null && currentDialog.MyAudioProfile.AudioClips.Count > dialogIndex) {
                AudioManager.MyInstance.PlayVoice(currentDialog.MyAudioProfile.AudioClips[dialogIndex]);
            }


            currentDialogNode.Shown = true;
        }


        public IEnumerator playDialog() {
            //Debug.Log("CharacterAbilitymanager.playDialog()");
            float elapsedTime = 0f;
            DialogNode currentdialogNode = null;

            // this needs to be reset to allow for repeatable dialogs to replay
            currentDialog.ResetStatus();

            while (currentDialog.TurnedIn == false) {
                foreach (DialogNode dialogNode in currentDialog.MyDialogNodes) {
                    if (dialogNode.MyStartTime <= elapsedTime && dialogNode.Shown == false) {
                        currentdialogNode = dialogNode;

                        ProcessDialogNode(dialogIndex);
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
                yield return null;
                currentTime += Time.deltaTime;
                captionText.color = new Color32(255, 255, 255, (byte)Mathf.Clamp((int)(currentTime * alphaPerSecond), 0, 255));

            }
        }

        public void OnDisable() {
            ClearCoRoutine();
            dialogIndex = 0;
        }
    }

}