using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CutSceneBarController : ConfiguredMonoBehaviour {

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

        private int subtitleIndex = 0;

        private float maxSubtitleTime = 300f;

        private Coroutine subtitleCoroutine = null;
        private Coroutine fadeCoroutine = null;
        private Coroutine barsCoroutine = null;

        private Cutscene currentCutscene = null;

        // game manager references
        SystemPlayableDirectorManager systemPlayableDirectorManager = null;
        CameraManager cameraManager = null;
        UIManager uIManager = null;
        LevelManager levelManager = null;
        AudioManager audioManager = null;
        InputManager inputManager = null;

        public Cutscene CurrentCutscene { get => currentCutscene; set => currentCutscene = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            systemPlayableDirectorManager = systemGameManager.SystemPlayableDirectorManager;
            cameraManager = systemGameManager.CameraManager;
            uIManager = systemGameManager.UIManager;
            levelManager = systemGameManager.LevelManager;
            audioManager = systemGameManager.AudioManager;
            inputManager = systemGameManager.InputManager;
        }

        public void ClearCoRoutine() {
            if (subtitleCoroutine != null) {
                StopCoroutine(subtitleCoroutine);
            }
            if (fadeCoroutine != null) {
                StopCoroutine(fadeCoroutine);
            }
            if (barsCoroutine != null) {
                StopCoroutine(barsCoroutine);
            }
        }

        public void ProcessInput() {
            if (inputManager.KeyBindWasPressed("CANCELALL")
                            || inputManager.KeyBindWasPressed("JOYSTICKBUTTON1")) {
                //Debug.Log("AnyRPGCutsceneCameraController.LateUpdate(): open cancel cutscene window");
                uIManager.confirmCancelCutsceneMenuWindow.OpenWindow();
            }
        }


        // ensure that a cutscene can be considered active even if it loads late in the scene load order
        public void AssignCutScene(Cutscene cutscene) {
            currentCutscene = cutscene;
        }

        public void StartCutScene(Cutscene cutscene) {
            //Debug.Log("CutSceneBarController.StartCutScene(" + cutscene.DisplayName + ")");

            if (cutscene.TimelineName != null
                && cutscene.TimelineName != string.Empty
                && systemPlayableDirectorManager.PlayableDirectorDictionary.ContainsKey(cutscene.TimelineName)) {
                systemPlayableDirectorManager.PlayableDirectorDictionary[cutscene.TimelineName].Play();
            }

            cameraManager.DeactivateMainCamera();
            cameraManager.EnableCutsceneCamera();

            currentCutscene = cutscene;
            //currentDialog = cutscene.MyDialog;
            captionText.color = new Color32(255, 255, 255, 0);
            gameObject.SetActive(true);
            topBarLayoutElement.preferredHeight = 0;
            bottomBarLayoutElement.preferredHeight = 0;
            captionBarLayoutElement.preferredHeight = cutSceneBarHeight;


            topBar.gameObject.SetActive(true);
            bottomBar.gameObject.SetActive(true);
            captionBar.gameObject.SetActive(true);

            uIManager.ActivatePlayerUI();
            uIManager.PlayerInterfaceCanvas.SetActive(false);
            uIManager.PopupWindowContainer.SetActive(false);
            uIManager.PopupPanelContainer.SetActive(false);
            uIManager.CombatTextCanvas.SetActive(false);
            uIManager.CutSceneBarsCanvas.SetActive(true);
            barsCoroutine = StartCoroutine(LoadCutSceneBars(cutSceneBarHeight));
        }

        public void EndCutScene() {
            //Debug.Log("CutSceneBarController.EndCutScene()");

            if (currentCutscene != null
                && currentCutscene.TimelineName != null
                && currentCutscene.TimelineName != string.Empty
                && systemPlayableDirectorManager.PlayableDirectorDictionary.ContainsKey(currentCutscene.TimelineName)) {
                systemPlayableDirectorManager.PlayableDirectorDictionary[currentCutscene.TimelineName].Stop();
            }

            topBar.gameObject.SetActive(false);
            bottomBar.gameObject.SetActive(false);
            captionBar.gameObject.SetActive(false);
            uIManager.CutSceneBarsCanvas.SetActive(false);
           
            ClearCoRoutine();
            gameObject.SetActive(false);

            if (currentCutscene != null) {
                currentCutscene.Viewed = true;
            }
            levelManager.EndCutscene(currentCutscene);
            currentCutscene = null;
        }

        public IEnumerator LoadCutSceneBars(int barHeight) {
            //Debug.Log("CutsceneBarController.PerformAbilityCast() Enter Ienumerator");
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
            if (currentCutscene.SubtitleProperties.SubtitleNodes.Count > 0) {
                subtitleIndex = 0;
                if (currentCutscene.AutoAdvanceSubtitles == true) {
                    subtitleCoroutine = StartCoroutine(playSubtitles());
                } else {
                    ProcessSubtitleNode(subtitleIndex);
                    subtitleIndex++;
                }
            }
        }

        public void AdvanceDialog() {
            //Debug.Log("CharacterAbilitymanager.AdvanceDialog()");
            if (currentCutscene.SubtitleProperties.SubtitleNodes.Count > subtitleIndex) {
                ProcessSubtitleNode(subtitleIndex);
                subtitleIndex++;
            }
        }

        private void ProcessSubtitleNode(int subtitleIndex) {
            //Debug.Log("CharacterAbilitymanager.ProcessDialogNode()");
            SubtitleNode currentSubtitleNode = currentCutscene.SubtitleProperties.SubtitleNodes[subtitleIndex];
            captionText.text = currentSubtitleNode.Description;
            captionText.color = new Color32(255, 255, 255, 0);
            subtitleCoroutine = StartCoroutine(FadeInText());

            if (audioManager != null && currentCutscene?.SubtitleProperties.AudioProfile != null && currentCutscene.SubtitleProperties.AudioProfile.AudioClips != null && currentCutscene.SubtitleProperties.AudioProfile.AudioClips.Count > subtitleIndex) {
                audioManager.PlayVoice(currentCutscene.SubtitleProperties.AudioProfile.AudioClips[subtitleIndex]);
            }

            currentSubtitleNode.Shown = true;
        }


        public IEnumerator playSubtitles() {
            //Debug.Log("CharacterAbilitymanager.playDialog()");
            float elapsedTime = 0f;
            SubtitleNode currentSubtitleNode = null;

            // set all nodes to not shown
            currentCutscene.ResetSubtitles();

            while (elapsedTime < maxSubtitleTime) {
                foreach (SubtitleNode subtitleNode in currentCutscene.SubtitleProperties.SubtitleNodes) {
                    if (subtitleNode.StartTime <= elapsedTime && subtitleNode.Shown == false) {
                        currentSubtitleNode = subtitleNode;

                        ProcessSubtitleNode(subtitleIndex);
                        subtitleIndex++;
                    }
                }

                elapsedTime += Time.deltaTime;

                yield return null;
            }
            subtitleCoroutine = null;

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
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            ClearCoRoutine();
            subtitleIndex = 0;
        }
    }

}